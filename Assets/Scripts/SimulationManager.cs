using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour
{
    public GameObject PlantPrefab;
    public Plant.FitnessFunction fitnessFunction = Plant.FitnessFunction.Sunlight;
    public EvolutionManager EvolutionManager;
    public Sun Sun;
    public BoxCollider groundPlane;
    public int PlantPopulationSize;
    public float WaterPerIteration;
    public int DayDuration; // in growth iterations
    
    public bool autoNextHour = false;
    public bool autoNextDay = false;
    //Delay between nextStep calls when auto is on
    public float autoStepDelay = 1f;
    
    public int numDaysToSimulate = 10;
    
    //File data export
    public bool saveData = false;
    public string fileName = "PlantData";
    
    //Use presets at start for faster convergence or completely randomize from substrings
    public bool completelyRandomizeStartGenomes = false;     //(Completely randomize leads to very weird plants)
    public bool maintainPreviousGrowth = false;

    //UI 
    public TextMeshProUGUI AutoSimulationText;
    public TextMeshProUGUI DayText;
    public TextMeshProUGUI HourText;
    public TextMeshProUGUI AvgFitnessText;
    public TextMeshProUGUI HighestFitnessText;
    public TextMeshProUGUI LowestFitnessText;

    public Slider hourSlider;

    private List<Plant> oldPlantPopulation;
    private List<Plant> currentPlantPopulation;
    private int currentIteration;
    private int currentDay;
    
    private ObjectPooler objectPooler;
    
    private List<string[]> dataRows = new List<string[]>();

    private void Awake()
    {
        objectPooler = FindObjectOfType<ObjectPooler>();
        InitializePlantPopulation();
        Sun.groundPlane = groundPlane;
        Sun.hoursInDay = DayDuration;
        currentIteration = 0;
        
        // Add header row to data collection
        dataRows.Insert(0, new string[] {
            "Tree ID",
            "Day",
            "Hour",
            "Total Sunlight Collected over Day",
            "Sunlight Collected",
            "Total Cell Count",
            "Branch Cell Count",
            "Leaf Cell Count",
            "Width of Tree",
            "Height of Tree",
            "Sunlight over Total Cells",
            "Sunlight over Total Leaf Cells",
            "Age",
            "Growth Iterations",
            "Fitness"
        });
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextStep();
        }
    }
    
    public void ToggleAutoSimulation()
    {
        autoNextDay = !autoNextDay;
        autoNextHour = !autoNextHour;
    }
    

    //Coroutine NextStep to wait for a second before calling NextStep
    private IEnumerator NextStepCoroutine()
    {
        yield return new WaitForSeconds(autoStepDelay);
        NextStep();
    }


    public void NextStep()
    {
        if (currentIteration < DayDuration)
        {
            Sun.NextHour();
            
            //Save data for each plant in currentPlantPopulation
            foreach (Plant plant in currentPlantPopulation)
            {
                float sunlightOverTotalCells = 0;
                float sunlightOverTotalLeafCount = 0;
                
                if(plant.branchCount + plant.leafCount > 0) sunlightOverTotalCells = plant.sunlightCollected / (plant.branchCount + plant.leafCount);
                if(plant.leafCount > 0) sunlightOverTotalLeafCount = plant.sunlightCollected / plant.leafCount;
                
                
                string[] dataRow =
                {
                    plant.gameObject.name,
                    currentDay.ToString(),
                    currentIteration.ToString(),
                    plant.totalSunlightCollected.ToString(),
                    plant.sunlightCollected.ToString(),
                    (plant.branchCount + plant.leafCount).ToString(),
                    plant.branchCount.ToString(),
                    plant.leafCount.ToString(),
                    plant.width.ToString(),
                    plant.height.ToString(),
                    sunlightOverTotalCells.ToString(),
                    sunlightOverTotalLeafCount.ToString(),
                    plant.age.ToString(),
                    plant.iterationCount.ToString(),
                    plant.Fitness.ToString()
                };
                //Debug.Log(dataRow.ToString());
                AddDataRow(dataRow);
            }
            //Growth after data collection
            ApplyGrowthIteration();
            currentIteration++;
            
            if(autoNextHour) StartCoroutine(NextStepCoroutine());
        }
        else
        {
            
            //Important to update day before the population for naming purposes and age reasons
            currentDay++;
            
            if(currentDay == numDaysToSimulate)
            {
                //Save data to file
                if (saveData)
                {
                    SaveData();
                }
                return;
            }
            //Where we apply the evolutionary algorithm
            UpdatePlantPopulation();
            currentIteration = 0;
            
            if(autoNextDay && currentDay < numDaysToSimulate) StartCoroutine(NextStepCoroutine());
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
        
        if(autoNextDay || autoNextHour) AutoSimulationText.text = "Auto Simulation: ON";
        else AutoSimulationText.text = "Auto Simulation: OFF";
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
            plantObject.name = "Plant " + i + "_"+ currentDay;
            Plant plant = plantObject.GetComponent<Plant>();
            plant.fitnessFunction  = fitnessFunction;
            
            //Debug.Log("Plant Name: " + plantObject.name);
            plant.RandomizeGenome(completelyRandomizeStartGenomes);
            //Give them one step of growth so they can collect sunlight the first hour and transform from a seed
            plant.Grow();
            currentPlantPopulation.Add(plant);
        }
    }

    private void UpdatePlantPopulation()
    {
        //set oldPlantPopulation to a copy of the currentPlantPopulation
        oldPlantPopulation = new List<Plant>(currentPlantPopulation);
        //get the new plant population from the evolutionary algorithm
        currentPlantPopulation = EvolutionManager.ApplyEvolutionaryAlgorithm(oldPlantPopulation, currentDay);
        List<Plant> plantsToRemove = GetPlantsToRemove(oldPlantPopulation, currentPlantPopulation);

        RemoveOldPlants(plantsToRemove);
        CreateNewPlants(currentPlantPopulation);
        ResetExistingPlants(currentPlantPopulation);

        //Function to remove any plants that weren't caught in the remove old plants function due to time constraints
        RemoveStragglerPlants(currentPlantPopulation);
    }
    
    public void RemoveStragglerPlants(List<Plant> plantsToKeep)
    {
        List<Plant> plantsToRemove = new List<Plant>();
        List<Plant> allPlants = new List<Plant>(FindObjectsOfType<Plant>());

        foreach (Plant plant in allPlants)
        {
            if (!plantsToKeep.Contains(plant))
            {
                plantsToRemove.Add(plant);
            }
        }

        foreach (Plant plant in plantsToRemove)
        {
            //Debug.Log("Removing plant: " + plant.name);
            currentPlantPopulation.Remove(plant);
            Destroy(plant.gameObject);
        }
    }

    private List<Plant> GetPlantsToRemove(List<Plant> oldPlants, List<Plant> currentPlants)
    {
        List<Plant> plantsToRemove = new List<Plant>();
        foreach (Plant plant in oldPlants)
        {
            if (!currentPlants.Contains(plant))
            {
                plantsToRemove.Add(plant);
            }
        }
        return plantsToRemove;
    }

    private void RemoveOldPlants(List<Plant> plantsToRemove)
    {
        objectPooler.ReturnAllToPool();
        foreach (Plant plant in plantsToRemove)
        {
            Destroy(plant.gameObject);
        }
    }

    private void ResetExistingPlants(List<Plant> currentPlants)
    {
        for (int i = 0; i < currentPlants.Count; i++)
        {
            Plant plant = currentPlants[i];
            plant.ResetResources();
            int growthIterations = plant.iterationCount;
            plant.iterationCount = 0;
            plant.ShootState.Position = plant.transform.position;
            //If we want to maintain the previous growth, we need to force grow the plant up to its previous iteration count
            if (maintainPreviousGrowth)
            {
                for (int j = 0; j < growthIterations; j++)
                {
                    plant.ForceGrow();
                }
            }
        }
    }

    private void CreateNewPlants(List<Plant> currentPlants)
    {
        Vector3[] randomPositions = GetValidRandomPositions(currentPlants.Count);
        int positionIndex = 0;
        for (int i = 0; i < currentPlants.Count; i++)
        {
            Plant plant = currentPlants[i];
            //If the plant is new, we need to instantiate it
            if (plant.iterationCount == 0)
            {
                PlantGenome genome = plant.plantGenome;
                Vector3 randomRotation = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
                GameObject plantObject = Instantiate(PlantPrefab, randomPositions[positionIndex],
                    Quaternion.LookRotation(randomRotation));
                plantObject.name = $"Plant {positionIndex}_{currentDay}";
                Plant instantiatedPlant = plantObject.GetComponent<Plant>();
                instantiatedPlant.plantGenome = genome;
                instantiatedPlant.fitnessFunction = fitnessFunction;
                currentPlants[i] = instantiatedPlant;
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


    public void AddDataRow(string[] rowData)
    {
        dataRows.Add(rowData);
    }

    public void SaveData()
    {
        Debug.Log("At start of SaveData");
        // Write data to CSV file
        string filePath = Application.dataPath + "/" + fileName + ".csv";
        StreamWriter writer = new StreamWriter(filePath);
        foreach (string[] rowDataArray in dataRows)
        {
            writer.WriteLine(string.Join(",", rowDataArray));
        }
        writer.Close();
    
        Debug.Log("Data saved to " + filePath);
    }
    
}
