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
    //Use presets at start for faster convergence or completely randomize from substrings
    //(Completely randomize leads to very weird plants)
    public bool completelyRandomizeStartGenomes = false;
    
    //UI stuff
    public TextMeshProUGUI DayText;
    public TextMeshProUGUI HourText;
    public TextMeshProUGUI AvgFitnessText;
    public TextMeshProUGUI HighestFitnessText;
    public TextMeshProUGUI LowestFitnessText;
    
    //UI slider reference
    public Slider hourSlider;

    private List<Plant> oldPlantPopulation;
    private List<Plant> currentPlantPopulation;
    private int currentIteration;
    private int currentDay;

    private void Awake()
    {
        InitializePlantPopulation();
        Sun.groundPlane = groundPlane;
        Sun.hoursInDay = DayDuration;
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
        
        //Collect data
        
        if (currentIteration < DayDuration)
        {
            Sun.NextHour();
            ApplyGrowthIteration();
            currentIteration++;
        }
        else
        {
            //Where we apply the evolutionary algorithm
            UpdatePlantPopulation();
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
        foreach (Plant plant in currentPlantPopulation)
        {
            totalFitness += plant.Fitness;
        }

        return totalFitness / currentPlantPopulation.Count;
    }
    
    private float GetHighestFitness()
    {
        float highestFitness = 0;
        foreach (Plant plant in currentPlantPopulation)
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
        foreach (Plant plant in currentPlantPopulation)
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
        currentPlantPopulation = new List<Plant>();
        Vector3[] randomPositions = GetValidRandomPositions(PlantPopulationSize);
        
        for (int i = 0; i < PlantPopulationSize; i++)
        {
            Vector3 randomRotation = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
            GameObject plantObject = Instantiate(PlantPrefab, randomPositions[i], Quaternion.LookRotation(randomRotation));
            plantObject.name = "Plant " + i;
            Plant plant = plantObject.GetComponent<Plant>();
            
            Debug.Log("Plant Name: " + plantObject.name);
            plant.RandomizeGenome(completelyRandomizeStartGenomes);
            //Give them one step of growth so they can collect sunlight the first hour and transform from a seed
            plant.Grow();
            currentPlantPopulation.Add(plant);
        }
    }

    private void UpdatePlantPopulation()
    {
        oldPlantPopulation = currentPlantPopulation;
        currentPlantPopulation = EvolutionManager.ApplyEvolutionaryAlgorithm(oldPlantPopulation, currentDay);
        
        //DEALING WITH OLD PLANTS
        int numFromOldInNew = 0;
        //Destroy old plants if their age is > 1 and they are not in the current plant population otherwise reset their resources
        foreach (Plant plant in oldPlantPopulation)
        {
            if (!currentPlantPopulation.Contains(plant))
            {
                Destroy(plant.gameObject);
            }
            else
            {
                numFromOldInNew++;
                plant.ResetResources();
            }
        }
        
        //DEALING WITH NEW PLANTS
        //get a new set of random positions for the new plants
        Vector3[] randomPositions = GetValidRandomPositions(currentPlantPopulation.Count - numFromOldInNew);

        int positionIndex = 0;
        foreach (Plant plant in currentPlantPopulation)
        {
            //If the plant is not in the old give it a randomized position otherwise leave it alone
            if (!oldPlantPopulation.Contains(plant))
            {
                PlantGenome genome = plant.plantGenome;
                Vector3 randomRotation = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
                GameObject plantObject = Instantiate(PlantPrefab, randomPositions[positionIndex], Quaternion.LookRotation(randomRotation));
                plantObject.name = "Plant " + positionIndex;
                Plant instantiatedPlant = plantObject.GetComponent<Plant>();
                instantiatedPlant.plantGenome = genome;
                
                positionIndex++;
            }
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
        foreach (Plant plant in currentPlantPopulation)
        {
            plant.AddWater(WaterPerIteration);
            plant.Grow();
            plant.age++;
        }
    }

    public void SaveData()
    {
        //Export CSV for analysis
    }
    
}
