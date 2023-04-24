using System.Collections.Generic;
using UnityEngine;
using System;

public class Plant : MonoBehaviour
{
    public PlantGenome plantGenome;
    public LSystemState ShootState { get; private set; }
    public LSystemState RootState { get; private set; }

    public float waterCollected;
    public float sunlightCollected;
    
    public float totalWaterCollected;
    public float totalSunlightCollected;

    public GameObject BranchPrefab;
    
    public GameObject LeafPrefab;

    public float maxGrowthIterations = 4;
    public float branchCost = 1;
    
    public int iterationCount = 0;
    //Incremented whenever create branch is called
    public int branchCount { get; private set; }
    //Incremented whenever create leaf is called
    public int leafCount { get; private set; }


    public ObjectPooler objectPooler;

    public enum FitnessFunction
    {
        Sunlight,
        SunlightAndBranchProportion
    };
    
    public FitnessFunction fitnessFunction = FitnessFunction.Sunlight;

    //age in hours of plant since creation
    public int age = 0;
    
    // Initialize variables to keep track of bounds
    private Vector3 minCorner;
    private Vector3 maxCorner;
    public float width { get; private set; }
    public float height{ get; private set; }

    public float Fitness
    {
        get
        {
            switch (fitnessFunction)
            {
                case FitnessFunction.Sunlight:
                    return totalSunlightCollected;
                
                case FitnessFunction.SunlightAndBranchProportion:
                    
                    float sunlightWeight = 1f;
                    float branchProportionWeight = 10f;
                    float branchCountWeight = 0.125f;
                    
                    char stackPush = '[';
                    float localBranchCount = 0;
                    string shootString = ShootState.ToString();
                    foreach (char c in shootString)
                    {
                        if (c == stackPush)
                            localBranchCount++;
                    }
                    //Count branches relative to length of whole shoot
                    //Plants that have more branches would have more leafs and in theory more sunlight 
                    float branchProportion = localBranchCount / (float)shootString.Length;

                    float fitness = ((sunlightWeight * totalSunlightCollected)
                                     + (branchProportionWeight * branchProportion)) - ((float)Math.Pow(branchCountWeight * branchCount, 2));

                    return fitness;
                default:
                    return -1f;
            }

        }
    }

    
    

    public Plant(PlantGenome genome)
    {
        plantGenome = genome;
        ShootState = new LSystemState(Vector3.zero, Quaternion.Euler(-90, 0, 0));
        RootState = new LSystemState(Vector3.zero,  Quaternion.Euler(-90, 0, 0));
    }

    private void Awake()
    {
        if(plantGenome == null) plantGenome = new PlantGenome();
        ShootState = new LSystemState(transform.position, Quaternion.Euler(-90, 0, 0));
        RootState = new LSystemState(transform.position, Quaternion.Euler(-90, 0, 0));

        objectPooler = FindObjectOfType<ObjectPooler>();
        
    }
    
    
    
    private string GenerateShootInstructions(int iteration)
    {
        string prevInstructions = plantGenome.ShootLSystem.Axiom;
        string newInstructions = "";

        newInstructions = plantGenome.ShootLSystem.ApplyRules(prevInstructions, iteration);

        return newInstructions;
    }

    //For testing purposes
    public void ForceGrow()
    {
        GrowShoot(iterationCount);
        GrowRoot(iterationCount);
        iterationCount++; 

    }
    
    public void Grow()
    {
        //Grow the plant from seed stage if it is the first iteration
        if (iterationCount <= 1)
        {
            GrowShoot(iterationCount);
            GrowRoot(iterationCount);
            iterationCount++;
        }
        //If the plant has enough sunlight and water to grow call the grow functions and iterate the current iteration count
        //grow the plant using sunlight resources collected
        else if (iterationCount < maxGrowthIterations)
        {

            //Based on X and F rules, determine the number of branches (F) that would be created in the next iteration

            float consecutiveBranchCost = 0;
            consecutiveBranchCost += CalculateSquaredBranchCost(plantGenome.ShootLSystem.Rules['F']);
            consecutiveBranchCost += CalculateSquaredBranchCost(plantGenome.ShootLSystem.Rules['X']);
            //At end multiply the cost by the StepSize to determine final cost
            consecutiveBranchCost *= plantGenome.ShootLSystem.StepSize;
            
            //This comes out to something between 1.5 for no consecutives and 12 for FFFF
            //Debug.Log("Consecutive Branch Cost for Plant " + gameObject.name + ": " + consecutiveBranchCost);
            
            if (sunlightCollected >= (branchCost * iterationCount + consecutiveBranchCost) &&
    waterCollected >= branchCost * iterationCount * .5)
            { 
                sunlightCollected -= branchCost * iterationCount + branchCost;
                waterCollected -= branchCost * iterationCount * .5f + branchCost;
                GrowShoot(iterationCount);
                GrowRoot(iterationCount);
                iterationCount++;
            }
        }

    }
    
    float CalculateSquaredBranchCost(string rule)
    {
        float totalFBranches = 0;
        float branchCost = 0;

        //Count concurrent F's and create a increasingly more expensive branch cost as the number of consecutive branches increases
        for (int i = 0; i < rule.Length; i++)
        {
            //If the current character is an F, increment the branch count
            if (rule[i] == 'F') totalFBranches++;
            //If the current character is not an F, add the cost of the current branch to the total cost and reset the branch count
            else
            {
                //The cost of a branch is the square of the number of consecutive branches
                branchCost += totalFBranches * totalFBranches;
                totalFBranches = 0;
            }
        }
        //divide the branchCost by the length of the rule to get the average cost of a branch
        branchCost /= rule.Length;
        
        return branchCost;
    }
    
    
    public void GrowShoot(int iterations)
{
    // Initialize variables to keep track of bounds
    minCorner = ShootState.Position;
    maxCorner = ShootState.Position;

    string shootInstructions = GenerateShootInstructions(iterations);
    float stepSize = plantGenome.ShootLSystem.StepSize;

    for (int i = 0; i < shootInstructions.Length; i++)
    {
        char symbol = shootInstructions[i];
        switch (symbol)
        {
            case 'F':
                Vector3 startPosition = ShootState.Position;
                ShootState.MoveForward(stepSize);
                Vector3 endPosition = ShootState.Position;
                
                // Update bounds based on start and end positions
                minCorner = Vector3.Min(minCorner, startPosition);
                minCorner = Vector3.Min(minCorner, endPosition);
                maxCorner = Vector3.Max(maxCorner, startPosition);
                maxCorner = Vector3.Max(maxCorner, endPosition);

                // Add a branch or a leaf based on the conditions
                bool shouldCreateLeaf =
                    shootInstructions[(i + 1) % shootInstructions.Length] == 'X' ||
                    shootInstructions[(i + 3) % shootInstructions.Length] == 'F' &&
                    shootInstructions[(i + 4) % shootInstructions.Length] == 'X';

                if (shouldCreateLeaf)
                {
                    CreateLeaf(startPosition, endPosition, LeafPrefab);
                }
                else
                {
                    CreateBranch(startPosition, endPosition, BranchPrefab);
                }
                break;
            case '+':
                ShootState.RotateLeft(plantGenome.ShootLSystem.Angle);
                break;
            case '-':
                ShootState.RotateRight(plantGenome.ShootLSystem.Angle);
                break;
            case '[':
                ShootState.PushState();
                break;
            case ']':

                ShootState.PopState();
                break;
            case '/':
                ShootState.PitchDown(plantGenome.ShootLSystem.Angle);
                break;
            case '*':
                ShootState.RotateUp(plantGenome.ShootLSystem.Angle);
                break;
        }
    }

    // Calculate width and height
    
    //Width is the abs difference between the x and z values of the min and max corners averaged
    width = (Mathf.Abs(maxCorner.x - minCorner.x) + Mathf.Abs(maxCorner.z - minCorner.z)) / 2;
    //Height is the abs difference between the y values of the min and max corners
    height = Mathf.Abs(maxCorner.y - minCorner.y);
    
    //Debug.Log("Plant: " + gameObject.name + "Width: " + width + " Height: " + height);
}

    public void GrowRoot(int iterations)
    {
        //Will be similar to GrowShoot but testing that first before implementing this

    }

    private void CreateBranch(Vector3 startPosition, Vector3 endPosition, GameObject branchPrefab)
    {
        if(objectPooler == null) objectPooler = FindObjectOfType<ObjectPooler>();
        
        Vector3 offset = endPosition - startPosition;
        float distance = offset.magnitude;
        Vector3 direction = offset.normalized;

        GameObject branch = objectPooler.GetBranch();
        if (branch == null) return;
        branch.transform.position = startPosition;
        branch.transform.rotation = Quaternion.LookRotation(direction);
        branch.transform.localScale = new Vector3(1, 1, distance);
        branch.transform.position += branch.transform.forward * distance / 2;
        //Debug.Log("Branch being created name: " + branch.name);
        branch.transform.SetParent(transform);

        branchCount++;
    }

    private void CreateLeaf(Vector3 startPosition, Vector3 endPosition, GameObject leafPrefab)
    {
        if(objectPooler == null) objectPooler = FindObjectOfType<ObjectPooler>();
        
        Vector3 offset = endPosition - startPosition;
        float distance = offset.magnitude;
        Vector3 direction = offset.normalized;

        GameObject leaf = objectPooler.GetLeaf();
        if (leaf == null) return;
        leaf.transform.position = startPosition;
        leaf.transform.rotation = Quaternion.LookRotation(direction);
        leaf.transform.localScale = new Vector3(2, 2, distance);
        leaf.transform.position += leaf.transform.forward * distance / 2;
       // Debug.Log("Leaf being created name: " + leaf.name);
        //Debug.Log("Leaf being created parent: " + transform.name);
        leaf.transform.SetParent(transform);
        //TODO Make color of leaf dependent on height

    }
    
    public void AddSunlight(float sunlight)
    {
        sunlightCollected += sunlight;
        totalSunlightCollected += sunlight;
    }

    public void AddWater(float water)
    {
        waterCollected += water;
        totalWaterCollected += water;
    }
    
    public void ResetResources()
    {
        sunlightCollected = 0;
        waterCollected = 0;
        totalSunlightCollected = 0;
        totalWaterCollected = 0;
    }

    public void ResetGrowth()
    {
        iterationCount = 0;
        age = 0;
        branchCount = 0;
        leafCount = 0;
        Grow();
    }
    
    
    public void RandomizeGenome(bool completelyRandom = true)
    {
        plantGenome = new PlantGenome();
        plantGenome.InitializeRandomGenome(completelyRandom);
    }
    
    //on draw gizmos draw a vertical line from the plant indicating water collected and a vertical line indicating sun collected
    private void OnDrawGizmos()
    {
        //Draw box between points minCorner and maxCorner
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((minCorner + maxCorner) / 2, new Vector3(maxCorner.x - minCorner.x, maxCorner.y - minCorner.y, maxCorner.z - minCorner.z));
        
        
        //Sunlight
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * sunlightCollected);
        
        //Water
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.right, transform.position + Vector3.right + Vector3.up * waterCollected);
        
        //Fitness
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + Vector3.right * 2, transform.position + Vector3.right * 2 + Vector3.up * Fitness);
    }
}
