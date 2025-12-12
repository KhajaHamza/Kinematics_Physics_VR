using UnityEngine;
using XCharts.Runtime;

/// <summary>
/// GraphManager handles real-time data visualization for the physics simulation.
/// 
/// KEY FEATURES:
/// 1. THREE GRAPHS: Velocity vs Time, Acceleration vs Time, and Displacement vs Time
/// 2. CONTINUOUS BACKGROUND SAMPLING: Data is collected whenever simulation is running (not paused)
/// 3. TWO-MODE SUPPORT: Single car mode (one car active) and Multiple car mode (two cars racing)
/// 
/// CRITICAL FIX - Background Data Collection:
/// Previous Issue: Graphs only started collecting data when visible AND menu was closed, causing 
///                 missing data from t=0 to when graphs were opened.
/// Current Fix: Data collection happens continuously in Update() whenever simulation is RUNNING
///              (i.e., when PauseManager.isSimulationPaused = false). Graph visibility and menu 
///              state DO NOT affect data collection. This ensures when user opens graphs at any 
///              time (e.g., t=5s), they see the complete history from t=0 to t=5s.
/// 
/// DISPLACEMENT CALCULATION:
/// - Displacement is measured from the car's initial position at t=0
/// - Uses actual car position: Displacement = (currentPosition - startPosition).magnitude
/// - For CV car: displacement ≈ velocity × time (linear)
/// - For UA car: displacement ≈ u×t + 0.5×a×t² (quadratic)
/// 
/// DATA FLOW:
/// 1. Update() → Samples car data at fixed intervals (default 10 Hz) when sim is running
/// 2. SampleAndAddData() → Reads car.timeElapsed, car.Displacement, velocity, acceleration
/// 3. AddDataPoint() → Adds data to all three charts (velocity, acceleration, displacement)
/// 4. Charts continuously update in memory whether visible or not
/// 5. When user toggles visibility (Y button), complete data from t=0 is immediately displayed
/// </summary>
public class GraphManager : MonoBehaviour
{
    [Header("Chart References")]
    public LineChart velocityChart;
    public LineChart accelerationChart;
    public LineChart displacementChart; // Optional

    [Header("Canvas Reference")]
    [Tooltip("The parent canvas GameObject that contains all the charts. Should have GraphCanvasFollower component.")]
    public GameObject graphCanvas;

    [Header("Car References")]
    public ConstantVelocityCar cvCar;
    public AcceleratedCarMovementWithTicks uaCar;

    [Header("Menu Manager")]
    public MenuManager menuManager;

    [Header("Sampling Settings")]
    [Tooltip("How many data points per second")]
    public float sampleRateHz = 10f;

    [Tooltip("Maximum number of data points to keep (prevents memory issues)")]
    public int maxDataPoints = 200;

    [Header("Y-Axis Range Settings")]
    public bool useAutoYRange = true;
    public float yMaxVelocity = 10f;
    public float yMaxAcceleration = 5f;
    public float yMaxDisplacement = 50f;

    [Header("Series Indices")]
    private const int SERIES_CV = 0;  // Constant Velocity Car
    private const int SERIES_UA = 1;  // Uniformly Accelerated Car

    // Timing
    private float sampleTimer = 0f;
    private float sampleInterval;

    // Mode tracking
    private bool isMultipleCarMode = false;
    private bool lastMultipleCarMode = false;

    // Debug tracking
    private bool lastCvActive = false;
    private bool lastUaActive = false;

    // Graph visibility state - graphs always collect data in background, visibility only affects display
    private bool graphsVisible = false;
    private GraphCanvasFollower canvasFollower;

    void Start()
    {
        sampleInterval = 1f / sampleRateHz;

        // Get or find the canvas follower component
        if (graphCanvas != null)
        {
            canvasFollower = graphCanvas.GetComponent<GraphCanvasFollower>();
            if (canvasFollower == null)
            {
                canvasFollower = graphCanvas.AddComponent<GraphCanvasFollower>();
                Debug.Log("[GraphManager] Added GraphCanvasFollower component to graph canvas.");
            }
        }
        else
        {
            // Try to find canvas by name
            GameObject foundCanvas = GameObject.Find("Charts_Canvas");
            if (foundCanvas != null)
            {
                graphCanvas = foundCanvas;
                canvasFollower = graphCanvas.GetComponent<GraphCanvasFollower>();
                if (canvasFollower == null)
                {
                    canvasFollower = graphCanvas.AddComponent<GraphCanvasFollower>();
                }
                Debug.Log("[GraphManager] Auto-found graph canvas: Charts_Canvas");
            }
        }

        InitializeCharts();
        UpdateChartsVisibility();

        // Hide graphs by default
        SetGraphsVisible(false);
    }

    void InitializeCharts()
    {
        // Velocity Chart Setup
        if (velocityChart != null)
        {
            velocityChart.ClearData();

            // Ensure we have 2 series
            while (velocityChart.series.Count < 2)
            {
                var serie = velocityChart.AddSerie<Line>("Series " + velocityChart.series.Count);
                serie.symbol.show = true;
                serie.symbol.size = 4;
                serie.lineStyle.width = 3;
            }

            velocityChart.series[SERIES_CV].serieName = "CV Car";
            velocityChart.series[SERIES_UA].serieName = "UA Car";

            // Set colors
            velocityChart.series[SERIES_CV].itemStyle.color = Color.red;      // Red for constant velocity
            velocityChart.series[SERIES_UA].itemStyle.color = Color.yellow;   // Yellow for accelerated

            // IMPORTANT: Refresh the chart to apply colors
            velocityChart.RefreshChart();

            if (!useAutoYRange)
            {
                var yAxis = velocityChart.EnsureChartComponent<YAxis>();
                yAxis.minMaxType = Axis.AxisMinMaxType.Custom;
                yAxis.min = -2;
                yAxis.max = yMaxVelocity;
            }

            // Set X-axis to Value type
            var xAxis = velocityChart.EnsureChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Value;

            Debug.Log($"Velocity Chart initialized: Serie[0]={velocityChart.series[0].serieName}, Serie[1]={velocityChart.series[1].serieName}");
        }

        // Acceleration Chart Setup
        if (accelerationChart != null)
        {
            accelerationChart.ClearData();

            while (accelerationChart.series.Count < 2)
            {
                var serie = accelerationChart.AddSerie<Line>("Series " + accelerationChart.series.Count);
                serie.symbol.show = true;
                serie.symbol.size = 4;
                serie.lineStyle.width = 3;
            }

            accelerationChart.series[SERIES_CV].serieName = "CV Car";
            accelerationChart.series[SERIES_UA].serieName = "UA Car";

            accelerationChart.series[SERIES_CV].itemStyle.color = Color.red;      // Red for constant velocity (0 acceleration)
            accelerationChart.series[SERIES_UA].itemStyle.color = Color.yellow;   // Yellow for accelerated movement

            accelerationChart.RefreshChart();

            if (!useAutoYRange)
            {
                var yAxis = accelerationChart.EnsureChartComponent<YAxis>();
                yAxis.minMaxType = Axis.AxisMinMaxType.Custom;
                yAxis.min = -2;
                yAxis.max = yMaxAcceleration;
            }

            var xAxis = accelerationChart.EnsureChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Value;

            Debug.Log($"Acceleration Chart initialized: Serie[0]={accelerationChart.series[0].serieName}, Serie[1]={accelerationChart.series[1].serieName}");
        }

        // Displacement Chart Setup
        if (displacementChart != null)
        {
            displacementChart.ClearData();

            while (displacementChart.series.Count < 2)
            {
                var serie = displacementChart.AddSerie<Line>("Series " + displacementChart.series.Count);
                serie.symbol.show = true;
                serie.symbol.size = 4;
                serie.lineStyle.width = 3;
            }

            displacementChart.series[SERIES_CV].serieName = "CV Car";
            displacementChart.series[SERIES_UA].serieName = "UA Car";

            // Set colors to match velocity/acceleration charts
            displacementChart.series[SERIES_CV].itemStyle.color = Color.red;      // Red for constant velocity
            displacementChart.series[SERIES_UA].itemStyle.color = Color.yellow;   // Yellow for accelerated

            // IMPORTANT: Refresh the chart to apply colors
            displacementChart.RefreshChart();

            if (!useAutoYRange)
            {
                var yAxis = displacementChart.EnsureChartComponent<YAxis>();
                yAxis.minMaxType = Axis.AxisMinMaxType.Custom;
                yAxis.min = 0;
                yAxis.max = yMaxDisplacement;
            }

            var xAxis = displacementChart.EnsureChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Value;

            Debug.Log($"Displacement Chart initialized: Serie[0]={displacementChart.series[0].serieName}, Serie[1]={displacementChart.series[1].serieName}");
        }
    }

    void Update()
    {
        // CRITICAL: ALWAYS collect data when simulation is running, even if graphs are hidden or menu is open
        // This ensures when user opens graphs at t=5s, they see complete history from t=0 to t=5s

        // Only stop sampling when simulation is paused
        if (PauseManager.isSimulationPaused)
        {
            // Optionally log every 60 frames to avoid spam
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log("[GraphManager] Sampling BLOCKED - Simulation is paused");
            }
            return;
        }

        // Update mode tracking
        UpdateMode();

        // Check if mode changed - if so, clear data and reset visibility
        if (isMultipleCarMode != lastMultipleCarMode)
        {
            ClearAllChartData();
            lastMultipleCarMode = isMultipleCarMode;
        }

        // Update chart visibility based on mode
        UpdateChartsVisibility();

        // CRITICAL FIX: Sample data continuously at fixed intervals, regardless of graph visibility
        // This ensures graphs always have complete data from t=0 when user chooses to view them
        sampleTimer += Time.deltaTime;
        if (sampleTimer >= sampleInterval)
        {
            SampleAndAddData();
            sampleTimer = 0f;
        }
    }

    void UpdateMode()
    {
        // Determine if we're in multiple car mode
        bool cvActive = cvCar != null && cvCar.gameObject.activeSelf;
        bool uaActive = uaCar != null && uaCar.gameObject.activeSelf;

        isMultipleCarMode = cvActive && uaActive;
    }

    void UpdateChartsVisibility()
    {
        bool cvActive = cvCar != null && cvCar.gameObject.activeSelf;
        bool uaActive = uaCar != null && uaCar.gameObject.activeSelf;

        // DEBUG: Log only when car states change
        if (cvActive != lastCvActive || uaActive != lastUaActive)
        {
            Debug.Log($"UpdateChartsVisibility: isMultipleCarMode={isMultipleCarMode}, cvActive={cvActive}, uaActive={uaActive}");
            lastCvActive = cvActive;
            lastUaActive = uaActive;
        }

        if (isMultipleCarMode)
        {
            // Multiple Car Mode: Show velocity and acceleration charts
            if (velocityChart != null)
            {
                velocityChart.gameObject.SetActive(true);
                velocityChart.series[SERIES_CV].show = true;
                velocityChart.series[SERIES_UA].show = true;
                velocityChart.RefreshChart(); // Refresh to update visibility
            }

            if (accelerationChart != null)
            {
                accelerationChart.gameObject.SetActive(true);
                accelerationChart.series[SERIES_CV].show = true;
                accelerationChart.series[SERIES_UA].show = true;
                accelerationChart.RefreshChart(); // Refresh to update visibility
            }

            if (displacementChart != null)
            {
                displacementChart.gameObject.SetActive(true);
                displacementChart.series[SERIES_CV].show = true;
                displacementChart.series[SERIES_UA].show = true;
                displacementChart.RefreshChart(); // Refresh to update visibility
            }
        }
        else
        {
            // Single Car Mode: Show velocity chart and acceleration chart (for UA car only)
            if (velocityChart != null)
            {
                velocityChart.gameObject.SetActive(true);
                velocityChart.series[SERIES_CV].show = cvActive;
                velocityChart.series[SERIES_UA].show = uaActive;

                // DEBUG: Log series visibility only when car states change
                if (cvActive != lastCvActive || uaActive != lastUaActive)
                {
                    Debug.Log($"Single Car Mode - Velocity Chart: Series[CV].show={cvActive}, Series[UA].show={uaActive}, " +
                             $"CV DataCount={velocityChart.series[SERIES_CV].data.Count}, " +
                             $"UA DataCount={velocityChart.series[SERIES_UA].data.Count}");
                }
            }

            // Show acceleration chart in single car mode (only for UA car)
            if (accelerationChart != null)
            {
                accelerationChart.gameObject.SetActive(true);
                accelerationChart.series[SERIES_CV].show = false; // CV car has 0 acceleration
                accelerationChart.series[SERIES_UA].show = uaActive; // Show only if UA car is active
                accelerationChart.RefreshChart(); // Refresh to update visibility
            }

            if (displacementChart != null)
            {
                displacementChart.gameObject.SetActive(true);
                displacementChart.series[SERIES_CV].show = cvActive;
                displacementChart.series[SERIES_UA].show = uaActive;
            }
        }
    }

    void SampleAndAddData()
    {
        // Check which cars are active
        bool cvActive = cvCar != null && cvCar.gameObject.activeSelf;
        bool uaActive = uaCar != null && uaCar.gameObject.activeSelf;

        // Sample CV Car (only if active)
        if (cvActive)
        {
            float cvTime = cvCar.timeElapsed;
            float cvVelocity = cvCar.velocity;
            float cvAcceleration = 0f; // Constant velocity = 0 acceleration

            // Use actual displacement from car's current position relative to start position
            float cvDisplacement = cvCar.Displacement;

            AddDataPoint(SERIES_CV, cvTime, cvVelocity, cvAcceleration, cvDisplacement);
        }

        // Sample UA Car (only if active)
        // CRITICAL FIX: Sample data even when car hasn't started moving yet
        // This ensures we capture t=0 data point and any pre-trigger state
        if (uaActive)
        {
            float uaTime = uaCar.timeElapsed;
            float uaVelocity = uaCar.initialVelocity + uaCar.acceleration * uaTime;
            float uaAcceleration = uaCar.acceleration;

            // Use actual displacement from car's current position relative to start position
            float uaDisplacement = uaCar.Displacement;

            AddDataPoint(SERIES_UA, uaTime, uaVelocity, uaAcceleration, uaDisplacement);
        }
    }

    void AddDataPoint(int seriesIndex, float time, float velocity, float acceleration, float displacement)
    {
        // Add to Velocity Chart
        if (velocityChart != null && velocityChart.series.Count > seriesIndex)
        {
            // DEBUG: Check series visibility and data
            bool seriesVisible = velocityChart.series[seriesIndex].show;
            int dataCountBefore = velocityChart.series[seriesIndex].data.Count;

            velocityChart.AddData(seriesIndex, time, velocity);
            LimitDataPoints(velocityChart, seriesIndex);

            int dataCountAfter = velocityChart.series[seriesIndex].data.Count;

            // Log every 10th data point to avoid spam
            if (dataCountAfter % 10 == 0)
            {
                Debug.Log($"Velocity Chart - Series[{seriesIndex}] '{velocityChart.series[seriesIndex].serieName}': " +
                         $"Visible={seriesVisible}, DataPoints={dataCountAfter}, Time={time:F2}, Velocity={velocity:F2}");
            }
        }

        // Add to Acceleration Chart
        // In multiple car mode: show both CV (0) and UA (acceleration)
        // In single car mode: show only UA car acceleration (if active)
        if (accelerationChart != null && accelerationChart.series.Count > seriesIndex)
        {
            // Only add data if chart is active and series should be visible
            bool shouldAddData = false;

            if (isMultipleCarMode)
            {
                // Multiple mode: add data for both cars
                shouldAddData = true;
            }
            else
            {
                // Single mode: only add data for UA car (seriesIndex == SERIES_UA)
                shouldAddData = (seriesIndex == SERIES_UA && accelerationChart.series[seriesIndex].show);
            }

            if (shouldAddData && accelerationChart.gameObject.activeSelf)
            {
                bool seriesVisible = accelerationChart.series[seriesIndex].show;
                accelerationChart.AddData(seriesIndex, time, acceleration);
                LimitDataPoints(accelerationChart, seriesIndex);

                // Log every 10th data point
                if (accelerationChart.series[seriesIndex].data.Count % 10 == 0)
                {
                    Debug.Log($"Acceleration Chart - Series[{seriesIndex}] '{accelerationChart.series[seriesIndex].serieName}': " +
                             $"Visible={seriesVisible}, DataPoints={accelerationChart.series[seriesIndex].data.Count}, " +
                             $"Time={time:F2}, Acceleration={acceleration:F2}, Mode={(isMultipleCarMode ? "Multiple" : "Single")}");
                }
            }
        }

        // Add to Displacement Chart (Optional)
        if (displacementChart != null && displacementChart.series.Count > seriesIndex)
        {
            displacementChart.AddData(seriesIndex, time, displacement);
            LimitDataPoints(displacementChart, seriesIndex);
        }
    }

    void LimitDataPoints(LineChart chart, int seriesIndex)
    {
        if (chart.series[seriesIndex].data.Count > maxDataPoints)
        {
            chart.series[seriesIndex].data.RemoveAt(0);
        }
    }

    void ClearAllChartData()
    {
        if (velocityChart != null)
        {
            velocityChart.series[SERIES_CV].ClearData();
            velocityChart.series[SERIES_UA].ClearData();
        }

        if (accelerationChart != null)
        {
            accelerationChart.series[SERIES_CV].ClearData();
            accelerationChart.series[SERIES_UA].ClearData();
        }

        if (displacementChart != null)
        {
            displacementChart.series[SERIES_CV].ClearData();
            displacementChart.series[SERIES_UA].ClearData();
        }
    }

    // Called when UI or trigger wants to start/restart data collection
    // This method now only handles clearing existing data - sampling happens continuously in Update()
    public void StartSimulation(bool clearExistingData = true)
    {
        if (clearExistingData)
        {
            ClearAllChartData();
            sampleTimer = 0f; // Reset sample timer to start fresh
            Debug.Log($"[GraphManager] StartSimulation called - Data CLEARED, ready to collect from t=0");
        }
        else
        {
            // Don't clear data, just continue sampling (for trigger scenario)
            Debug.Log($"[GraphManager] StartSimulation called - Data PRESERVED, continuing collection");
        }
    }

    // Public method to reset all graphs
    public void ResetGraphs()
    {
        sampleTimer = 0f;

        ClearAllChartData();
        InitializeCharts();
        UpdateChartsVisibility();

        Debug.Log("Graphs reset - data cleared, sampling continues automatically");
    }

    /// <summary>
    /// Toggle graph visibility. Shows graphs in front of VR camera when enabled.
    /// </summary>
    public void ToggleGraphs()
    {
        SetGraphsVisible(!graphsVisible);
    }

    /// <summary>
    /// Set graph visibility state.
    /// </summary>
    public void SetGraphsVisible(bool visible)
    {
        graphsVisible = visible;

        if (canvasFollower != null)
        {
            if (visible)
            {
                canvasFollower.ShowGraphs();

                // Ensure charts are properly visible and refreshed when shown
                UpdateChartsVisibility();

                // Refresh all charts to ensure they display correctly
                if (velocityChart != null && velocityChart.gameObject.activeSelf)
                {
                    velocityChart.RefreshChart();
                }
                if (accelerationChart != null && accelerationChart.gameObject.activeSelf)
                {
                    accelerationChart.RefreshChart();
                }
                if (displacementChart != null && displacementChart.gameObject.activeSelf)
                {
                    displacementChart.RefreshChart();
                }

                Debug.Log("[GraphManager] Graphs shown in front of VR camera.");
            }
            else
            {
                canvasFollower.HideGraphs();
                Debug.Log("[GraphManager] Graphs hidden.");
            }
        }
        else if (graphCanvas != null)
        {
            // Fallback: just toggle active state if no follower component
            graphCanvas.SetActive(visible);
            if (visible)
            {
                UpdateChartsVisibility();
            }
            Debug.Log($"[GraphManager] Graph canvas {(visible ? "shown" : "hidden")} (no GraphCanvasFollower component).");
        }
        else
        {
            Debug.LogWarning("[GraphManager] Cannot toggle graphs - no graph canvas reference assigned!");
        }
    }

    /// <summary>
    /// Check if graphs are currently visible.
    /// </summary>
    public bool AreGraphsVisible()
    {
        return graphsVisible;
    }
}