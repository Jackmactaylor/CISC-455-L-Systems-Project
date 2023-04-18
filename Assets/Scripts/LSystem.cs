using System.Collections.Generic;
using System.Text; // Include the System.Text namespace for StringBuilder
using UnityEngine;

public class LSystem
{
    public string Axiom;
    public Dictionary<char, string> Rules;
    public float Angle;
    public float StepSize;

    public LSystem(string axiom, float angle, float stepSize)
    {
        Axiom = axiom;
        Angle = angle;
        StepSize = stepSize;
        Rules = new Dictionary<char, string>();
    }

    public void InitializeRandomRules()
    {
        // Define some possible strings for each symbol
        string[] FStrings =
        {
            "FF",
            "F*F/[-F+F+F]/[+F-F-F]",
            "F[/F*F*F][/F*F*F]/F",

        };

        //TODO: Research good starting axioms for X
        
        //Plants
        string[] XStrings = {
            "[F[+FX][*+FX][/+FX]][F[-FX][*-FX][/-FX]]", // Symmetrical rule 1
            "[F[+FX][*+FX]][F[-FX][*-FX]]", // Symmetrical rule 2
            "F[+FX]F[-FX]F[+FX]F[-FX]", // Symmetrical rule 3
            "[F[+FX][*+FX][/+FX]]", // Taken from the original L-System implementation
            "[*+FX]X[+FX][/+F-FX]", // Taken from the original L-System implementation
            "[F[-X+F[+FX]][*-X+F[+FX]][/-X+F[+FX]-X]]" // Taken from the original L-System implementation
        };
        

        // Randomly choose one string for each symbol
        Rules['F'] = FStrings[Random.Range(0,FStrings.Length)];
        Rules['X'] = XStrings[Random.Range(0,XStrings.Length)];

        // Assign fixed strings for other symbols
        Rules['+'] = "+";
        Rules['-'] = "-";
        Rules['['] = "[";
        Rules[']'] = "]";

        // Randomly choose an Axiom
        Axiom = XStrings[Random.Range(0, XStrings.Length)];
        
        // Randomly choose an Angle
        Angle = Random.Range(25f, 45f);
        
        // Randomly choose a StepSize
        StepSize = Random.Range(2f, 8f);
    }

    public void MutateRules()
    {
        //mutate Rules['X']
    }

    // Apply the production rules to the current state `iterations` number of times
    public string ApplyRules(string currentState , int iterations)
    {
        StringBuilder newState = new StringBuilder(); // Use StringBuilder instead of concatenating with strings
        for (int i = 0; i < iterations; i++)
        {
            foreach (char symbol in currentState)
            {
                newState.Append(Rules.ContainsKey(symbol) ? Rules[symbol] : symbol.ToString()); // Append the rule output to the StringBuilder
            }
            currentState = newState.ToString(); // Set the current state to the new state
            newState.Clear(); // Clear the StringBuilder for future use
        }

        return currentState;
    }

    // Return current axiom as string
    public override string ToString()
    {
        return Axiom;
    }
}