using System.Collections.Generic;
using UnityEngine;

public class EvolutionManager : MonoBehaviour
{
    public enum SelectionMethod { RouletteWheel, Tournament };
    public enum SelectionType { Elitism };

    public SelectionMethod selectionMethod;
    public SelectionType selectionType;
    public float mutationRate = 0.1f;
    public float crossoverRate = 0.8f;
    public int tournamentSize;
    
    public bool useAgism = true;
    public int numOfDaysForAgism = 3;
    public float percentOldestToRemove = 0.2f;

    public List<Plant> ApplyEvolutionaryAlgorithm(List<Plant> plantPopulation, int currentDay, float percentageToReplace = 0.20f )
    {
        Debug.Log("plant population: " + plantPopulation);
        
        List<Plant> newPopulation = new List<Plant>();

        int lambda = (int)(plantPopulation.Count * (1 - percentageToReplace));
        int mu = (int)((float)plantPopulation.Count * percentageToReplace);

        for (int i = 0; i < mu; i++)
        {
            Plant parent1 = SelectParent(plantPopulation);
            Plant parent2 = SelectParent(plantPopulation);

            Debug.Log("Parent 1: " + parent1 + "Parent 1 X Rule" + parent1.plantGenome.ShootLSystem.Rules['X']);
            Debug.Log("Parent 2: " + parent2 + "Parent 2 X Rule" + parent2.plantGenome.ShootLSystem.Rules['X']);
            
            Plant child;
            //Sexual reproduction
            if (Random.value > crossoverRate)
            {
                child = Crossover(parent1, parent2);
                Debug.Log("Child from crossover X rule: " + child.plantGenome.ShootLSystem.Rules['X']); 
            }
            //Asexual reproduction
            else
            {
                
                //50 50 the child gets the genome of either parent
                child = new Plant(Random.value < 0.5f ? parent1.plantGenome : parent2.plantGenome);
                Debug.Log("Child from asexual X rule: " + child.plantGenome.ShootLSystem.Rules['X']); 
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
            
            Debug.Log("Before Agism application");
            foreach (Plant plant in plantPopulation)
            {
                Debug.Log("Plant: " + plant + "age:" + plant.age);
            }
            
            int numToRemove = (int)(plantPopulation.Count * percentOldestToRemove);
            plantPopulation.RemoveRange(plantPopulation.Count - numToRemove - 1, numToRemove);
            
            Debug.Log("After Agism application");
            foreach (Plant plant in plantPopulation)
            {
                Debug.Log("Plant: " + plant + "age:" + plant.age);
            }
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

        Dictionary<char, List<string>> parentRules = new Dictionary<char, List<string>>();
        
        //for each char representing a Rule in each parents Dictionary<char, string> Rules, add the Rule to the List<string> of the Dictionary<char, List<string>> parentRules
        foreach (KeyValuePair<char, string> rule in parent1Genome.ShootLSystem.Rules)
        {
            if (!parentRules.ContainsKey(rule.Key))
            {
                parentRules.Add(rule.Key, new List<string>());
            }
            parentRules[rule.Key].Add(rule.Value);
        }
        foreach (KeyValuePair<char, string> rule in parent2Genome.ShootLSystem.Rules)
        {
            if (!parentRules.ContainsKey(rule.Key))
            {
                parentRules.Add(rule.Key, new List<string>());
            }
            parentRules[rule.Key].Add(rule.Value);
        }
        
        
        //foreach rule in parentRules GenerateRandomRules that is the avg of parent1 and parent2 max length for that rule +- 1)
        foreach (KeyValuePair<char, List<string>> rule in parentRules)
        {
            float childRuleLength = 0f;
            foreach (string parentRule in rule.Value)
            {
                   childRuleLength += parentRule.Length;
            }
            childRuleLength /= rule.Value.Count;
            //Add some variation into the rule length + - 3 range for additional characters to be selected
            int ruleVariation = Random.Range(-3, 3);
            
            if(childRuleLength + ruleVariation > 0)
            {
                childRuleLength += ruleVariation;
            }

            //PlantObject.name, Rule Key, Rule Value, RuleLength
            Debug.Log("Parent names: " + parent1 + ", " + parent2);
            Debug.Log("Rule Key: " + rule.Key + ", Rule Value: " + rule.Value + ", Rule Length: " + childRuleLength);
            
            string newRule = childGenome.ShootLSystem.GenerateRandomRule(rule.Value, (int)childRuleLength);
            //Give the child the new rule
            childGenome.ShootLSystem.Rules[rule.Key] = newRule;
        }

        // Perform uniform crossover on the parents genome to make the child genome, select features of the l-system : rules, axiom, stepsize, angle and growth rate
        // Shoot LSystem
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
