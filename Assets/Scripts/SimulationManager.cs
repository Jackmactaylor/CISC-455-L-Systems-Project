using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour
{
    public GameObject PlantPrefab;
    public EvolutionManager EvolutionManager;
    public Sun Sun;
    public BoxCollider groundPlane;
    public int PlantPopulationSize;
    public float WaterPerIteration;
    public int DayDuration; // in growth iterations
    
    //UI stuff
    public TextMeshProUGUI DayText;
    public TextMeshProUGUI HourText;
    public TextMeshProUGUI AvgFitnessText;
    public TextMeshProUGUI HighestFitnessText;
    public TextMeshProUGUI LowestFitnessText;
    
    //UI slider reference
    public Slider hourSlider;

    private List<Plant> plantPopulation;
    private int currentIteration;
    private int currentDay;

    private void Start()
    {
        InitializePlantPopulation();
        Sun.groundPlane = groundPlane;
        currentIteration = 0;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextStep();
        }
    }


    public void NextStep()
    {
        
        if (currentIteration < DayDuration)
        {
            Sun.NextHour();
            ApplyGrowthIteration();
            currentIteration++;
        }
        else
        {
            EvolutionManager.ApplyEvolutionaryAlgorithm(plantPopulation);
            currentIteration = 0;
            currentDay++;
        }
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        DayText.text = "Day: " + currentDay;
        HourText.text = "Hour: " + currentIteration;
        hourSlider.maxValue = DayDuration;
        hourSlider.value = currentIteration;
        AvgFitnessText.text = "Avg Fitness: " + GetAverageFitness();
        HighestFitnessText.text = "Highest Fitness: " + GetHighestFitness();
        LowestFitnessText.text = "Lowest Fitness: " + GetLowestFitness();
    }
    
    private float GetAverageFitness()
    {
        float totalFitness = 0;
        foreach (Plant plant in plantPopulation)
        {
            totalFitness += plant.Fitness;
        }

        return totalFitness / plantPopulation.Count;
    }
    
    private float GetHighestFitness()
    {
        float highestFitness = 0;
        foreach (Plant plant in plantPopulation)
        {
            if (plant.Fitness > highestFitness)
            {
                highestFitness = plant.Fitness;
            }
        }

        return highestFitness;
    }
    
    private float GetLowestFitness()
    {
        float lowestFitness = 100000;
        foreach (Plant plant in plantPopulation)
        {
            if (plant.Fitness < lowestFitness)
            {
                lowestFitness = plant.Fitness;
            }
        }

        return lowestFitness;
    }
    
    private void InitializePlantPopulation()
    {
        plantPopulation = new List<Plant>();
        Vector3[] randomPositions = GetValidRandomPositions(PlantPopulationSize);
        
        for (int i = 0; i < PlantPopulationSize; i++)
        {
            Vector3 randomRotation = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
            GameObject plantObject = Instantiate(PlantPrefab, randomPositions[i], Quaternion.LookRotation(randomRotation));
            Plant plant = plantObject.GetComponent<Plant>();
            plant.RandomizeGenome();
            plantPopulation.Add(plant);
        }
    }

    private Vector3[] GetValidRandomPositions(int numPositions)
    {
        Vector3 randomPoint;
        Vector3[] randomPositions = new Vector3[numPositions];
        for (int i = 0; i < numPositions; i++)
        {
            do
            {
                randomPoint = new Vector3(
                    Random.Range(groundPlane.bounds.min.x, groundPlane.bounds.max.x),
                    100,
                    Random.Range(groundPlane.bounds.min.z, groundPlane.bounds.max.z)
                );
                //perform a raycast down to get the y position and only store random positions that are on the land
                RaycastHit hit;
                if (Physics.Raycast(randomPoint, Vector3.down, out hit, Mathf.Infinity, 
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && !hit.collider.CompareTag("Water"))
                {
                    randomPositions[i] = new Vector3(randomPoint.x, hit.point.y, randomPoint.z);
                }
            } while (randomPositions[i] == Vector3.zero);
        }

        return randomPositions;
    }

    private void ApplyGrowthIteration()
    {
        foreach (Plant plant in plantPopulation)
        {
            plant.AddWater(WaterPerIteration);
            plant.Grow();
            plant.age++;
        }
    }
}
