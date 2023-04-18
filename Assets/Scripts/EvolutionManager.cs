using System.Collections.Generic;
using UnityEngine;

public class EvolutionManager : MonoBehaviour
{
    public enum SelectionMethod { RouletteWheel, Tournament };
    public enum SelectionType { Elitism };

    public SelectionMethod selectionMethod;
    public SelectionType selectionType;
    public float mutationRate;
    public float crossoverRate;
    public int tournamentSize;
    
    
    public void ApplyEvolutionaryAlgorithm(List<Plant> plantPopulation)
    {
        List<Plant> newPopulation = new List<Plant>();
        float newPlants = plantPopulation.Count / 3;
        float treesNeeded = plantPopulation.Count - newPlants;

        while (newPopulation.Count < newPlants)
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

        List<Plant> survivngTrees = SelectTrees(plantPopulation, treesNeeded);
        newPopulation.AddRange(survivngTrees);

        plantPopulation.Clear();
        plantPopulation.AddRange(newPopulation);
    }


    private List<Plant> SelectTrees(List<Plant> plantPopulation, float treesNeeded)
    {
        switch (selectionType)
        {
            case SelectionType.Elitism:
                return ElitismSelection(plantPopulation, treesNeeded);
            default:
                return null;
        }
    }




    //a method that applies the selection method of Elitism, using fitness values, and returns the list of the fittest trees
    private List<Plant> ElitismSelection(List<Plant> plantPopulation, float treesNeeded)
    {
        List<Plant> fittestTrees = new List<Plant>();
        plantPopulation.Sort((x, y) => y.Fitness.CompareTo(x.Fitness));
        for (int i = 0; i < treesNeeded; i++)
        {
            fittestTrees.Add(plantPopulation[i]);
        }
        return fittestTrees;
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


        // Perform uniform crossover on the parents genome to make the child genome, select features of the l-system : rules, axiom, stepsize, angle and growth rate
        // Shoot LSystem
        childGenome.ShootLSystem.Axiom = Random.value < 0.5f ? parent1Genome.ShootLSystem.Axiom : parent2Genome.ShootLSystem.Axiom;
        childGenome.ShootLSystem.StepSize = Random.value < 0.5f ? parent1Genome.ShootLSystem.StepSize : parent2Genome.ShootLSystem.StepSize;
        childGenome.ShootLSystem.Angle = Random.value < 0.5f ? parent1Genome.ShootLSystem.Angle : parent2Genome.ShootLSystem.Angle;
        childGenome.ShootLSystem.Rules = new Dictionary<char, string>();
        foreach (KeyValuePair<char, string> rule in parent1Genome.ShootLSystem.Rules)
        {
            childGenome.ShootLSystem.Rules.Add(rule.Key, Random.value < 0.5f ? rule.Value : parent2Genome.ShootLSystem.Rules[rule.Key]);
        }
        // Root LSystem
        childGenome.RootLSystem.Axiom = Random.value < 0.5f ? parent1Genome.RootLSystem.Axiom : parent2Genome.RootLSystem.Axiom;
        childGenome.RootLSystem.StepSize = Random.value < 0.5f ? parent1Genome.RootLSystem.StepSize : parent2Genome.RootLSystem.StepSize;
        childGenome.RootLSystem.Angle = Random.value < 0.5f ? parent1Genome.RootLSystem.Angle : parent2Genome.RootLSystem.Angle;
        childGenome.RootLSystem.Rules = new Dictionary<char, string>();
        foreach (KeyValuePair<char, string> rule in parent1Genome.RootLSystem.Rules)
        {
            childGenome.RootLSystem.Rules.Add(rule.Key, Random.value < 0.5f ? rule.Value : parent2Genome.RootLSystem.Rules[rule.Key]);
        }

        Plant child = new Plant(childGenome);
        return child;
    }

    private void SimpleMutation(Plant tree)
    {
        PlantGenome treeGenome = tree.plantGenome;

        float currAngle = treeGenome.ShootLSystem.Angle;
        float currStepsize = treeGenome.ShootLSystem.StepSize;
        treeGenome.ShootLSystem.Angle = Random.value < 0.1f ? Random.Range(20f, 100f) : currAngle;
        treeGenome.ShootLSystem.StepSize = Random.value < 0.1f ? Random.Range(2f, 5f) : currStepsize;

        float currAngleR = treeGenome.RootLSystem.Angle;
        float currStepsizeR = treeGenome.RootLSystem.StepSize;
        treeGenome.RootLSystem.Angle = Random.value < 0.1f ? Random.Range(20f, 100f) : currAngleR;
        treeGenome.RootLSystem.StepSize = Random.value < 0.1f ? Random.Range(2f, 5f) : currStepsizeR;
    }
        

}
