
using Assets.Scripts;
using UnityEngine;

public class Plant : MonoBehaviour
{
    int totalSun = 0;
    int totalWater = 0;
    PlantGenome genome;
    
    public void AddSun(int sun)
    {
        totalSun += sun;
    }
    
    public void AddWater(int water)
    {
        totalWater += water;
    }

    // Called by SimulationManager after each hour, the plant will attempt to 
    public void Grow()
    {
        
    }
}
