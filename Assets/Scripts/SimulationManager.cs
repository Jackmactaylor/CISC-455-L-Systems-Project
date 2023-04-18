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
    }
    
    private void InitializePlantPopulation()
    {
        plantPopulation = new List<Plant>();

        for (int i = 0; i < PlantPopulationSize; i++)
        {
            // Pick random point on ground to place plant
            Vector3 randomPoint = new Vector3(
                Random.Range(groundPlane.bounds.min.x, groundPlane.bounds.max.x),
                groundPlane.transform.position.y,
                Random.Range(groundPlane.bounds.min.z, groundPlane.bounds.max.z)
                );
            GameObject plantObject = Instantiate(PlantPrefab, randomPoint, Quaternion.identity);
            Plant plant = plantObject.GetComponent<Plant>();
            //PlantGenome genome = new PlantGenome();
            //genome.InitializeRandomGenome();
            //plant = new Plant(genome);
            plant.RandomizeGenome();
            plantPopulation.Add(plant);
        }
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
