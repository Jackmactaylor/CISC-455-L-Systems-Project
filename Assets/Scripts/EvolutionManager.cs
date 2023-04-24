using System.Collections.Generic;
using UnityEngine;

public class EvolutionManager : MonoBehaviour
{
    public enum SelectionMethod { RouletteWheel, Tournament };
    public enum SelectionType { Elitism };
    
    public enum CrossoverMethod { OnePoint, NPoint, NPointSymmetrical, HighlyRandom};

    public SelectionMethod selectionMethod;
    public SelectionType selectionType;
    public CrossoverMethod crossoverMethod = CrossoverMethod.OnePoint;

    public float mutationRate = 0.1f;
    public float crossoverRate = 0.2f;
    public int crossoverPoints = 1;
    public int tournamentSize;
    
    public bool useAgism = true;
    public int numOfDaysForAgism = 3;
    public float percentOldestToRemove = 0.2f;

    public List<Plant> ApplyEvolutionaryAlgorithm(List<Plant> plantPopulation, int currentDay, float percentageToReplace = 0.20f )
    {
        //Debug.Log("plant population: " + plantPopulation);
        
        List<Plant> newPopulation = new List<Plant>();

        int lambda = Mathf.CeilToInt(plantPopulation.Count * (1 - percentageToReplace));
        int mu = (int)((float)plantPopulation.Count * percentageToReplace);

        for (int i = 0; i < mu; i++)
        {
            Plant parent1 = SelectParent(plantPopulation);
            Plant parent2 = SelectParent(plantPopulation);
            Plant child;
            
            //Sexual reproduction
            if (Random.value < crossoverRate)
            {
                child = Crossover(parent1, parent2);
            }
            
            //Asexual reproduction
            else
            {
                //50 50 the child gets the genome of either parent
                child = new Plant(Random.value < 0.5f ? parent1.plantGenome : parent2.plantGenome);
            }

            if (Random.value < mutationRate)
            {
                Mutate(child.plantGenome);
            }

            newPopulation.Add(child);
        }

        //sort by age remove % of oldest specified by percentOldestToRemove, then sort by fittest when using agism
        if (useAgism && currentDay >= numOfDaysForAgism)
        {
            plantPopulation.Sort((x, y) => y.age.CompareTo(x.age));
            
            /*Debug.Log("Before Agism application");
            foreach (Plant plant in plantPopulation)
            {
                Debug.Log("Plant: " + plant + "age:" + plant.age);
            }*/
            
            int numToRemove = (int)(plantPopulation.Count * percentOldestToRemove);
            plantPopulation.RemoveRange(0, numToRemove);
            
            /*Debug.Log("After Agism application");
            foreach (Plant plant in plantPopulation)
            {
                Debug.Log("Plant: " + plant + "age:" + plant.age);
            }*/
        }

        //Selecting lamba - mu parents to include in the new population from PlantPopulation
        //If plantPopulation.count - mu is less than lambda, then randomly add copies of the parents to the new population until lambda is reached
        
        //Ensure that the new population is at least lambda
        if(plantPopulation.Count < lambda)
        {
            int numToAdd = lambda - plantPopulation.Count;
            for(int i = 0; i < numToAdd; i++){
                Plant parent = SelectParent(plantPopulation);
                plantPopulation.Add(new Plant(parent.plantGenome));
            }
        }
        
        //adding lambda to new population
        List<Plant> lambdaPlants = SelectTrees(plantPopulation, lambda);
        newPopulation.AddRange(lambdaPlants);

        return newPopulation;

    }


    private List<Plant> SelectTrees(List<Plant> plantPopulation, int treesNeeded)
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
    private List<Plant> ElitismSelection(List<Plant> plantPopulation, int treesNeeded)
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

        if (crossoverMethod == CrossoverMethod.OnePoint)
        {
            childGenome.ShootLSystem.Rules['F'] = childGenome.ShootLSystem.SymmetricalOnePointCrossover(
                parent1Genome.ShootLSystem.Rules['F'], parent2Genome.ShootLSystem.Rules['F']);
            childGenome.ShootLSystem.Rules['X'] = childGenome.ShootLSystem.SymmetricalOnePointCrossover(
                parent1Genome.ShootLSystem.Rules['X'], parent2Genome.ShootLSystem.Rules['X']);
        }
        
        if (crossoverMethod == CrossoverMethod.NPoint)
        {
            childGenome.ShootLSystem.Rules['F'] = childGenome.ShootLSystem.NPointCrossover(
                parent1Genome.ShootLSystem.Rules['F'], parent2Genome.ShootLSystem.Rules['F'], crossoverPoints);
            childGenome.ShootLSystem.Rules['X'] = childGenome.ShootLSystem.NPointCrossover(
                parent1Genome.ShootLSystem.Rules['X'], parent2Genome.ShootLSystem.Rules['X'], crossoverPoints);
        }

        if (crossoverMethod == CrossoverMethod.NPointSymmetrical)
        {
            childGenome.ShootLSystem.Rules['F'] = childGenome.ShootLSystem.SymmetricalNPointCrossover(
                parent1Genome.ShootLSystem.Rules['F'], parent2Genome.ShootLSystem.Rules['F'], crossoverPoints);
            childGenome.ShootLSystem.Rules['X'] = childGenome.ShootLSystem.SymmetricalNPointCrossover(
                parent1Genome.ShootLSystem.Rules['X'], parent2Genome.ShootLSystem.Rules['X'], crossoverPoints);
        }
        
        //Add additional crossoverMethod for just selecting the genes from one parent and then the parameters from the other
        
        //Picking a random parent to take step size and angle from
        childGenome.ShootLSystem.StepSize = Random.value < 0.5f ? parent1Genome.ShootLSystem.StepSize : parent2Genome.ShootLSystem.StepSize;
        childGenome.ShootLSystem.Angle = Random.value < 0.5f ? parent1Genome.ShootLSystem.Angle : parent2Genome.ShootLSystem.Angle;

        Plant child = new Plant(childGenome);
        return child;
    }
    
    private void Mutate(PlantGenome genome)
    {
        genome.Mutate();
    }

}
