using UnityEngine;

public class PlantGenome : MonoBehaviour
{
    public LSystem ShootLSystem { get; private set; }
    public LSystem RootLSystem { get; private set; }

    public PlantGenome()
    {
        ShootLSystem = new LSystem("X", 25f, 1f); // Initialize with proper angle and stepSize values
        RootLSystem = new LSystem("X", 25f, 1f); // Initialize with proper angle and stepSize values
    }

    public PlantGenome(PlantGenome other)
    {
        // Perform a deep copy of the LSystem rules from the other PlantGenome
        ShootLSystem = new LSystem(other.ShootLSystem.Axiom, other.ShootLSystem.Angle, other.ShootLSystem.StepSize);
        foreach (var rule in other.ShootLSystem.Rules)
        {
            ShootLSystem.Rules[rule.Key] = rule.Value;
        }

        RootLSystem = new LSystem(other.RootLSystem.Axiom, other.RootLSystem.Angle, other.RootLSystem.StepSize);
        foreach (var rule in other.RootLSystem.Rules)
        {
            RootLSystem.Rules[rule.Key] = rule.Value;
        }
    }

    public void InitializeRandomGenome(bool completelyRandom = true)
    {
        ShootLSystem.InitializeRandomRules(completelyRandom);
        //Roots were cut for now due to scope of course and project
        //RootLSystem.InitializeRandomRules(completelyRandom);
    }

    public void Mutate()
    {
        //Mutate both the Rules and Parameters of the LSystems
        ShootLSystem.MutateRules();
        ShootLSystem.MutateParameters();
        //RootLSystem.MutateRules();
       // RootLSystem.MutateParameters();
    }
}
