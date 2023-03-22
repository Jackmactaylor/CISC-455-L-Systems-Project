using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    //public PlantGenome PlantGenome { get; private set; }
    //public PlantGenome plantGenome;
    public LSystemState ShootState { get; private set; }
    public LSystemState RootState { get; private set; }
    public float SunlightCollected { get; private set; }
    public float WaterCollected { get; private set; }
    
    public PlantGenome plantGenome;
    
    public GameObject BranchPrefab;
    
    public GameObject LeafPrefab;

    public float branchCost;
    
    
    public float Fitness
    {
        get
        {
            // Fitness could be a combo of sun and water weighted to select for larger roots/shoots
            return 2 * SunlightCollected + WaterCollected;
            // Could also be some measure of its total size
        }
    }

    private Stack<GameObject> shootBranches;

    public Plant(PlantGenome genome)
    {
        plantGenome = genome;
        ShootState = new LSystemState(Vector3.zero, Quaternion.identity);
        RootState = new LSystemState(Vector3.zero, Quaternion.identity);
        shootBranches = new Stack<GameObject>();
    }

    private void Awake()
    {
        if(plantGenome == null) plantGenome = new PlantGenome();
        ShootState = new LSystemState(transform.position, Quaternion.identity);
        RootState = new LSystemState(transform.position, Quaternion.identity);
        shootBranches = new Stack<GameObject>();
    }
    
    public void GrowShoot()
    {
        string shootInstructions = plantGenome.ShootLSystem.ApplyRules(plantGenome.ShootLSystem.Axiom);
        Debug.Log(shootInstructions);
        foreach (char symbol in shootInstructions)
        {
            switch (symbol)
            {
                case 'F':
                    Vector3 startPosition = ShootState.Position;
                    ShootState.MoveForward(plantGenome.ShootLSystem.StepSize);
                    Vector3 endPosition = ShootState.Position;
                    // Add a branch to the shoot
                    CreateBranch(startPosition, endPosition, BranchPrefab);
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
            }
        }
    }

    public void GrowRoot()
    {
        //Will be similar to GrowShoot but testing that first before implementing this

    }

    private void CreateBranch(Vector3 startPosition, Vector3 endPosition, GameObject branchPrefab)
    {
        Vector3 offset = endPosition - startPosition;
        float distance = offset.magnitude;
        Vector3 direction = offset.normalized;

        GameObject branch = Instantiate(branchPrefab, startPosition, Quaternion.LookRotation(Vector3.forward, direction));
        branch.transform.localScale = new Vector3(branch.transform.localScale.x, distance / branch.transform.localScale.y, branch.transform.localScale.z);
        branch.transform.position += branch.transform.up * distance / 2;
        branch.transform.SetParent(transform);
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
