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

    private void ApplyGrowthIteration()
    {
        foreach (Plant plant in plantPopulation)
        {
            float sunlight = Sun.RaycastSunlight(plant.transform.position);
            plant.AddSunlight(sunlight);
            plant.AddWater(WaterPerIteration);
            plant.Grow();
        }
    }
}

public class Sun: MonoBehaviour
{
    //function to raycast sunlight to plants
    public float RaycastSunlight(Vector3 plantPosition)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, plantPosition - transform.position, out hit))
        {
            if (hit.collider.gameObject.tag == "Plant")
            {
                return 1;
            }
        }
        return 0;
    }
    
}