using System.Collections.Generic;
using UnityEngine;

public class EvolutionManager : MonoBehaviour
{
    public enum SelectionMethod { RouletteWheel, Tournament };
    public SelectionMethod selectionMethod;
    public float mutationRate;
    public float crossoverRate;
    public int tournamentSize;
    
    
    public void ApplyEvolutionaryAlgorithm(List<Plant> plantPopulation)
    {
        List<Plant> newPopulation = new List<Plant>();

        while (newPopulation.Count < plantPopulation.Count)
        {
            Plant parent1 = SelectParent(plantPopulation);
            Plant parent2 = SelectParent(plantPopulation);

            Plant child;
            if (Random.value < crossoverRate)
            {
                child = Crossover(parent1, parent2);
            }
            else
            {
                child = new Plant(parent1.plantGenome);
            }

            if (Random.value < mutationRate)
            {
                child.plantGenome.Mutate();
            }

            newPopulation.Add(child);
        }

        plantPopulation.Clear();
        plantPopulation.AddRange(newPopulation);
    }

    private Plant SelectParent(List<Plant> plantPopulation)
    {
        switch (selectionMethod)
        {
            case SelectionMethod.RouletteWheel:
                return RouletteWheelSelection(plantPopulation);
            case SelectionMethod.Tournament:
                return TournamentSelection(plantPopulation, tournamentSize);
            default:
                return null;
        }
    }

    private Plant RouletteWheelSelection(List<Plant> plantPopulation)
    {
        float totalFitness = 0;

        foreach (Plant plant in plantPopulation)
        {
            totalFitness += plant.Fitness;
        }

        float randomValue = Random.Range(0, totalFitness);
        float currentSum = 0;

        foreach (Plant plant in plantPopulation)
        {
            currentSum += plant.Fitness;
            if (currentSum >= randomValue)
            {
                return plant;
            }
        }

        return plantPopulation[plantPopulation.Count - 1];
    }

    private Plant TournamentSelection(List<Plant> plantPopulation, int tournamentSize)
    {
        Plant bestPlant = null;

        for (int i = 0; i < tournamentSize; i++)
        {
            Plant competitor = plantPopulation[Random.Range(0, plantPopulation.Count)];
            if (bestPlant == null || competitor.Fitness > bestPlant.Fitness)
            {
                bestPlant = competitor;
            }
        }

        return bestPlant;
    }

    private Plant Crossover(Plant parent1, Plant parent2)
    {
        PlantGenome childGenome = new PlantGenome();
        
        PlantGenome parent1Genome = parent1.plantGenome;
        PlantGenome parent2Genome = parent2.plantGenome;
        

        // Perform crossover on the parent genomes to create the child genome
        // This can be done by selecting a crossover point and combining the parent L-system rules

        Plant child = new Plant(childGenome);
        return child;
    }
}
