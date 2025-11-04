using UnityEngine;
using XCharts.Runtime;

public class GraphManager : MonoBehaviour
{
    [Header("Chart References")]
    public LineChart velocityChart;
    public LineChart accelerationChart;
    public LineChart displacementChart; // Optional

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
    private const int SERIES_CV = 0;
    private const int SERIES_UA = 1;

    // Timing
    private float sampleTimer = 0f;
    private float sampleInterval;
    private float simulationStartTime;

    // Mode tracking
    private bool isMultipleCarMode = false;

    void Start()
    {
        sampleInterval = 1f / sampleRateHz;
        simulationStartTime = Time.time;

        InitializeCharts();
        UpdateChartsVisibility();
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

            // Set Y-axis range
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

            if (!useAutoYRange)
            {
                var yAxis = accelerationChart.EnsureChartComponent<YAxis>();
                yAxis.minMaxType = Axis.AxisMinMaxType.Custom;
                yAxis.min = -2;
                yAxis.max = yMaxAcceleration;
            }

            var xAxis = accelerationChart.EnsureChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Value;
        }

        // Displacement Chart Setup (Optional)
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

            if (!useAutoYRange)
            {
                var yAxis = displacementChart.EnsureChartComponent<YAxis>();
                yAxis.minMaxType = Axis.AxisMinMaxType.Custom;
                yAxis.min = 0;
                yAxis.max = yMaxDisplacement;
            }

            var xAxis = displacementChart.EnsureChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Value;
        }
    }

    void Update()
    {
        // Don't sample when paused or menu is open
        if (PauseManager.isSimulationPaused) return;
        if (menuManager != null && menuManager.IsMenuOpen) return;

        // Update mode tracking
        UpdateMode();

        // Update chart visibility based on mode
        UpdateChartsVisibility();

        // Sample data at fixed intervals
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
        if (isMultipleCarMode)
        {
            // Multiple Car Mode: Show velocity and acceleration
            if (velocityChart != null) velocityChart.gameObject.SetActive(true);
            if (accelerationChart != null) accelerationChart.gameObject.SetActive(true);

            // Show both series
            if (velocityChart != null)
            {
                velocityChart.series[SERIES_CV].show = true;
                velocityChart.series[SERIES_UA].show = true;
            }
            if (accelerationChart != null)
            {
                accelerationChart.series[SERIES_CV].show = true;
                accelerationChart.series[SERIES_UA].show = true;
            }
            if (displacementChart != null)
            {
                displacementChart.series[SERIES_CV].show = true;
                displacementChart.series[SERIES_UA].show = true;
            }
        }
        else
        {
            // Single Car Mode: Only show velocity chart
            if (velocityChart != null) velocityChart.gameObject.SetActive(true);
            if (accelerationChart != null) accelerationChart.gameObject.SetActive(false);

            // Show only the active car's series
            bool cvActive = cvCar != null && cvCar.gameObject.activeSelf;
            bool uaActive = uaCar != null && uaCar.gameObject.activeSelf;

            if (velocityChart != null)
            {
                velocityChart.series[SERIES_CV].show = cvActive;
                velocityChart.series[SERIES_UA].show = uaActive;
            }
            if (displacementChart != null)
            {
                displacementChart.series[SERIES_CV].show = cvActive;
                displacementChart.series[SERIES_UA].show = uaActive;
            }
        }
    }

    void SampleAndAddData()
    {
        float currentTime = Time.time - simulationStartTime;

        // Sample CV Car
        if (cvCar != null && cvCar.gameObject.activeSelf)
        {
            float cvVelocity = cvCar.velocity;
            float cvAcceleration = 0f;
            float cvDisplacement = cvCar.velocity * cvCar.timeElapsed;

            AddDataPoint(SERIES_CV, currentTime, cvVelocity, cvAcceleration, cvDisplacement);
        }

        // Sample UA Car
        if (uaCar != null && uaCar.gameObject.activeSelf)
        {
            float uaTime = uaCar.timeElapsed;
            float uaVelocity = uaCar.initialVelocity + uaCar.acceleration * uaTime;
            float uaAcceleration = uaCar.acceleration;
            float uaDisplacement = uaCar.initialVelocity * uaTime + 0.5f * uaCar.acceleration * uaTime * uaTime;

            AddDataPoint(SERIES_UA, currentTime, uaVelocity, uaAcceleration, uaDisplacement);
        }
    }

    void AddDataPoint(int seriesIndex, float time, float velocity, float acceleration, float displacement)
    {
        // Try multiple possible method names for different XCharts versions

        // Add to Velocity Chart
        if (velocityChart != null && velocityChart.series.Count > seriesIndex)
        {
            // Create a list with x and y values
            velocityChart.AddData(seriesIndex, new double[] { time, velocity });
            LimitDataPoints(velocityChart, seriesIndex);
        }

        // Add to Acceleration Chart
        if (accelerationChart != null && accelerationChart.series.Count > seriesIndex)
        {
            accelerationChart.AddData(seriesIndex, new double[] { time, acceleration });
            LimitDataPoints(accelerationChart, seriesIndex);
        }

        // Add to Displacement Chart (Optional)
        if (displacementChart != null && displacementChart.series.Count > seriesIndex)
        {
            displacementChart.AddData(seriesIndex, new double[] { time, displacement });
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

    // Public method to reset all graphs
    public void ResetGraphs()
    {
        simulationStartTime = Time.time;
        sampleTimer = 0f;

        if (velocityChart != null) velocityChart.ClearData();
        if (accelerationChart != null) accelerationChart.ClearData();
        if (displacementChart != null) displacementChart.ClearData();

        InitializeCharts();
        UpdateChartsVisibility();

        Debug.Log("Graphs reset");
    }
}