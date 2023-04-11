using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public PlantGenome plantGenome;
    public LSystemState ShootState { get; private set; }
    public LSystemState RootState { get; private set; }
    public float SunlightCollected { get; private set; }
    public float WaterCollected { get; private set; }

    public GameObject BranchPrefab;
    
    public GameObject LeafPrefab;

    public float branchCost;
    
    private int iterationCount = 0;
    
    private ObjectPooler objectPooler;

    private float sunlightWeight;
    private float waterCollectionWeight;
    private float branchProportionWeight;
    private float symmetryWeight;
    
    public float Fitness
    {
        //To-do: implement bilateral symmetry
        get
        {
            //Count number of branches and calculate proportion
            char stackPush = '[';
            int branchCount = 0;
            string shootString = ShootState.ToString();
            foreach (char c in shootString)
            {
                if (c == stackPush)
                    branchCount++;
            }
            float branchProportion = branchCount / shootString.Length;

            float fitness = (sunlightWeight * SunlightCollected)
                + (waterCollectionWeight * WaterCollected)
                + (branchProportionWeight * branchProportion);

            return fitness;
        }
    }

    private Stack<GameObject> shootBranches;

    public Plant(PlantGenome genome)
    {
        branchCost = 0;
        sunlightWeight = 1;
        waterCollectionWeight = 1;
        branchProportionWeight = 10;
        symmetryWeight = 0;
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
        Debug.Log("name of plant" + gameObject.name);
        Debug.Log("Current Fitness:" + Fitness + " Current Iteration:" + iterationCount);
        Debug.Log("Current Sun:" + SunlightCollected + " Current Water:" + WaterCollected);
        
        //If the plant has enough sunlight and water to grow call the grow functions and iterate the current iteration count
        if (SunlightCollected >= branchCost && WaterCollected >= branchCost)
        {
            GrowShoot(iterationCount);
            GrowRoot(iterationCount);
            SunlightCollected -= branchCost;
            WaterCollected -= branchCost;
            iterationCount++; 
            
        }
    }
    
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

        branchCost++;
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
        leaf.transform.localScale = new Vector3(1, 1, distance);
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
    
}
