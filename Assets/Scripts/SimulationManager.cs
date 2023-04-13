using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public GameObject PlantPrefab;
    public EvolutionManager EvolutionManager;
    public Sun Sun;
    public int PlantPopulationSize;
    public float WaterPerIteration;
    public int DayDuration; // in growth iterations

    private List<Plant> plantPopulation;
    private int currentIteration;

    private void Start()
    {
        InitializePlantPopulation();
        currentIteration = 0;
    }

    private void Update()
    {
        if (currentIteration < DayDuration)
        {
            ApplyGrowthIteration();
            currentIteration++;
        }
        else
        {
            EvolutionManager.ApplyEvolutionaryAlgorithm(plantPopulation);
            currentIteration = 0;
        }
    }

    private void InitializePlantPopulation()
    {
        plantPopulation = new List<Plant>();

        for (int i = 0; i < PlantPopulationSize; i++)
        {
            GameObject plantObject = Instantiate(PlantPrefab);
            Plant plant = plantObject.GetComponent<Plant>();
            PlantGenome genome = new PlantGenome();
            genome.InitializeRandomGenome();
            plant = new Plant(genome);
            plantPopulation.Add(plant);
        }
    }

    //need to rework this method and likely whole class
    private void ApplyGrowthIteration()
    {
        foreach (Plant plant in plantPopulation)
        {
            //float sunlight = Sun.RaycastSunlight(plant.transform.position);
            //plant.AddSunlight(sunlight);
            plant.AddWater(WaterPerIteration);
            plant.Grow();
        }
    }
}
