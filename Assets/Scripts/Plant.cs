using System.Collections.Generic;
using UnityEngine;
using System;

public class Plant : MonoBehaviour
{
    public PlantGenome plantGenome;
    public LSystemState ShootState { get; private set; }
    public LSystemState RootState { get; private set; }
    public float SunlightCollected { get; private set; }
    public float WaterCollected { get; private set; }

    public GameObject BranchPrefab;
    
    public GameObject LeafPrefab;

    public float maxGrowthIterations = 4;
    public float branchCost = 0;
    
    
    private int iterationCount = 0;
    private int branchCount;

    private ObjectPooler objectPooler;

    private float sunlightWeight = 1;
    private float waterCollectionWeight = 1;
    private float branchProportionWeight = 10;
    private float symmetryWeight = 0;
    private float branchCountWeight = 0.125f;
    
    //age in hours of plant since creation
    public int age = 0;

    public float Fitness
    {
        //To-do: implement bilateral symmetry
        get
        {
            //Count number of branches and calculate proportion
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

            float fitness = ((sunlightWeight * SunlightCollected)
                + (waterCollectionWeight * WaterCollected)
                + (branchProportionWeight * branchProportion)) - ((float)Math.Pow(branchCountWeight * branchCount, 2));
            Debug.Log("Fitness: " + fitness);
            Debug.Log("Sunlight: " + sunlightWeight * SunlightCollected);
            Debug.Log("Water: " + waterCollectionWeight * WaterCollected);
            Debug.Log("Branch Proportion: " + branchProportionWeight * branchProportion);
            
            return fitness;
        }
    }

    private Stack<GameObject> shootBranches;

    public Plant(PlantGenome genome)
    {
        plantGenome = genome;
        ShootState = new LSystemState(transform.position, Quaternion.Euler(-90, 0, 0));
        RootState = new LSystemState(transform.position, Quaternion.Euler(-90, 0, 0));
        shootBranches = new Stack<GameObject>();
    }

    private void Awake()
    {
        if(plantGenome == null) plantGenome = new PlantGenome();
        ShootState = new LSystemState(transform.position, Quaternion.Euler(-90, 0, 0));
        RootState = new LSystemState(transform.position, Quaternion.Euler(-90, 0, 0));
        shootBranches = new Stack<GameObject>();
        
        objectPooler = FindObjectOfType<ObjectPooler>();
        
    }
    
    private string GenerateShootInstructions(int iteration)
    {
        string prevInstructions = plantGenome.ShootLSystem.Axiom;
        string newInstructions = "";

        for (int i = 0; i < iteration; i++)
        {
            newInstructions = plantGenome.ShootLSystem.ApplyRules(prevInstructions, 1);
            prevInstructions = newInstructions;
        }

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
            if (SunlightCollected >= (branchCost * iterationCount + branchCost) &&
                WaterCollected >= (branchCost * iterationCount * .5 + branchCost))
            {
                SunlightCollected -= branchCost * iterationCount + branchCost;
                WaterCollected -= branchCost * iterationCount * .5f + branchCost;
                GrowShoot(iterationCount);
                GrowRoot(iterationCount);
                iterationCount++;
            }
        }

    }

    //TODO Create functions for calculating branchCost based on things like LSystem StepSize
    
    
    public void GrowShoot(int iterations)
    {
        string shootInstructions = GenerateShootInstructions(iterations);
        //Debug.Log(shootInstructions);
        for (int i = 0; i < shootInstructions.Length; i++)
        {
            char symbol = shootInstructions[i];
            switch (symbol)
            {
                case 'F':
                    Vector3 startPosition = ShootState.Position;
                    ShootState.MoveForward(plantGenome.ShootLSystem.StepSize);
                    Vector3 endPosition = ShootState.Position;
                    // Add a branch or a leaf based on the conditions
                    bool shouldCreateLeaf =
                        shootInstructions[(i + 1) % shootInstructions.Length] == 'X' ||
                        shootInstructions[(i + 3) % shootInstructions.Length] == 'F' &&
                        shootInstructions[(i + 4) % shootInstructions.Length] == 'X';
                    //Without object pooling
                    // GameObject prefabToUse = shouldCreateLeaf ? LeafPrefab : BranchPrefab;
                    // CreateBranch(startPosition, endPosition, prefabToUse);
                    //With object pooling
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
                    // Add a leaf to the shoot
                    GameObject leaf = Instantiate(LeafPrefab, ShootState.Position, ShootState.Orientation);
                    leaf.transform.SetParent(transform);
                    shootBranches.Push(leaf);
                    break;
                case ']':
                    ShootState.PopState();
                    if (shootBranches.Count > 0)
                    {
                        GameObject poppedLeaf = shootBranches.Pop();
                        Destroy(poppedLeaf);
                    }
                    break;
                case '/':
                    ShootState.PitchDown(plantGenome.ShootLSystem.Angle);
                    break;
                case '*':
                    ShootState.RotateUp(plantGenome.ShootLSystem.Angle);
                    break;
            }
        }
    }

    public void GrowRoot(int iterations)
    {
        //Will be similar to GrowShoot but testing that first before implementing this

    }

    private void CreateBranch(Vector3 startPosition, Vector3 endPosition, GameObject branchPrefab)
    {
        Vector3 offset = endPosition - startPosition;
        float distance = offset.magnitude;
        Vector3 direction = offset.normalized;

        GameObject branch = objectPooler.GetBranch();
        if (branch == null) return;
        branch.transform.position = startPosition;
        branch.transform.rotation = Quaternion.LookRotation(direction);
        branch.transform.localScale = new Vector3(1, 1, distance);
        branch.transform.position += branch.transform.forward * distance / 2;
        branch.transform.SetParent(transform);

        branchCount++;
    }

    private void CreateLeaf(Vector3 startPosition, Vector3 endPosition, GameObject leafPrefab)
    {
        Vector3 offset = endPosition - startPosition;
        float distance = offset.magnitude;
        Vector3 direction = offset.normalized;

        GameObject leaf = objectPooler.GetLeaf();
        if (leaf == null) return;
        leaf.transform.position = startPosition;
        leaf.transform.rotation = Quaternion.LookRotation(direction);
        leaf.transform.localScale = new Vector3(2, 2, distance);
        leaf.transform.position += leaf.transform.forward * distance / 2;
        leaf.transform.SetParent(transform);
        //TODO Make color of leaf dependent on height

    }
    
    public void AddSunlight(float sunlight)
    {
        SunlightCollected += sunlight;
    }

    public void AddWater(float water)
    {
        WaterCollected += water;
    }
    
    public void Reset()
    {
        SunlightCollected = 0;
        WaterCollected = 0;
    }
    
    public void RandomizeGenome()
    {
        plantGenome = new PlantGenome();
        plantGenome.InitializeRandomGenome();
    }
    
    //on draw gizmos draw a vertical line from the plant indicating water collected and a vertical line indicating sun collected
    private void OnDrawGizmos()
    {
        //Sunlight
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * SunlightCollected);
        
        //Water
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.right, transform.position + Vector3.right + Vector3.up * WaterCollected);
        
        //Fitness visualized using a sphere
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + Vector3.up * 2, Fitness);
    }
}
