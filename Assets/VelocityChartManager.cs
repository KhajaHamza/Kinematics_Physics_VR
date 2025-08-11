using UnityEngine;
using XCharts.Runtime;

public class VelocityChartManager : MonoBehaviour
{
    public LineChart lineChart;
    public ConstantVelocityCar constantCar;
    public AcceleratedCarMovementWithTicks acceleratedCar;

    private float timeElapsed = 0f;
    public float updateInterval = 0.1f;
    private float nextUpdateTime = 0f;

    void Start()
    {
        lineChart.ClearData();

        // Add two series by name
        lineChart.AddSerie<Line>("Velocity Constant");
        lineChart.AddSerie<Line>("Velocity Accelerated");
    }

    void Update()
    {
        if (PauseManager.isSimulationPaused)
            return;

        timeElapsed += Time.deltaTime;

        if (timeElapsed >= nextUpdateTime)
        {
            float vConstant = constantCar.velocity;
            float vAccelerated = acceleratedCar.initialVelocity + (acceleratedCar.acceleration * timeElapsed);

            lineChart.AddData("Velocity Constant", timeElapsed, vConstant);
            lineChart.AddData("Velocity Accelerated", timeElapsed, vAccelerated);

            nextUpdateTime += updateInterval;
        }
    }
}
