﻿using System.Collections.Generic;
using System.Linq;
using System.Text; // Include the System.Text namespace for StringBuilder
using UnityEngine;

public class LSystem
{
    public string Axiom;
    public Dictionary<char, string> Rules;
    public float Angle;
    public float StepSize;
    
    // Define some possible strings for each symbol

    string[] FStrings = {"FF",  // two consecutive branches
        "F[+F][-F]", //branch with two branches
        "F[+F][-F][*F][/F]", //branch with four branches
        "F+F-F", //a branch that splits into two branches at an angle of 120 degrees, forming a "Y" shape
        "F[+F][-F]", //a branch that splits into two branches at angles of 45 degrees to the right and left, forming a "V" shape.
        "F[+F]F[-F]F", //two branches at angles of 45 degrees to the right and left, with additional straight line segments or branches in between.
        "F[*+F][-F][*F]", //splits into two branches, an additional straight line segment or branch
        "F*F/[-F+F+F]/[+F-F-F]",
        "F[/F*F*F][/F*F*F]/F"
    };

        
    //Plants
    string[] XStrings = {
        "[F[+FX][*+FX][/+FX]][F[-FX][*-FX][/-FX]]", // Symmetrical rule 1
        "[F[+FX][*+FX]][F[-FX][*-FX]]", // Symmetrical rule 2
        "F[+FX]F[-FX]F[+FX]F[-FX]", // Symmetrical rule 3
        "[F[+FX][*+FX][/+FX]]", // Taken from the original L-System implementation
        "[*+FX]X[+FX][/+F-FX]", // Taken from the original L-System implementation
        "[F[-X+F[+FX]][*-X+F[+FX]][/-X+F[+FX]-X]]",// Taken from the original L-System implementation
        "[F[+F[-FX][+F/X]]][F[-F[+FX][-F*X]]]", //asyemmetry
        "[F[*+FX][+FX][-FX]]", //v structure
        "F[+FX][*+FX][-F[-FX]][/-F[-FX]]" //y structure

    };

    public LSystem(string axiom, float angle, float stepSize)
    {
        Axiom = axiom;
        Angle = angle;
        StepSize = stepSize;
        Rules = new Dictionary<char, string>();
    }
    
    string GenerateRandomRule(List<string> strings, int maxLength = 40, int numSubstrings = 3)
    {
        StringBuilder sb = new StringBuilder();
    
        // Generate half-length strings
        List<string> halfStrings = new List<string>();
        for (int i = 0; i < numSubstrings; i++)
        {
            halfStrings.Add(strings[Random.Range(0, strings.Count)]);
        }
        int lengthSum = halfStrings.Sum(s => Mathf.Min(s.Length, maxLength / (2 * numSubstrings)));
    
        // Concatenate the selected substrings
        int remainingLength = maxLength - lengthSum;
        foreach (string halfString in halfStrings)
        {
            int length = Mathf.Min(halfString.Length, remainingLength / (numSubstrings--));
            sb.Append(halfString.Substring(0, length));
            remainingLength -= length;
        }
    
        // Mirror and append
        string mirroredString = new string(sb.ToString().Reverse().Select(c => {
            switch (c) {
                case '[': return ']';
                case ']': return '[';
                default: return c;
            }
        }).ToArray());
        sb.Append(mirroredString);
    
        return sb.ToString();
    }
    
    public void InitializeRandomRules()
    {

        // Randomize the F and X rules
        Rules['F'] = FStrings[Random.Range(0,FStrings.Length)];
        Rules['X'] = XStrings[Random.Range(0,XStrings.Length)];
        //Rules['F'] = GenerateRandomRule(FStrings.ToList(), Random.Range(2, 40));
        //Rules['X'] = GenerateRandomRule(XStrings.ToList(), Random.Range(2, 40));
        Debug.Log("Rules['F'] = " + Rules['F']);
        Debug.Log("Rules['X'] = " + Rules['X']);

        // Assign fixed strings for other symbols
        // rules below keep the current system
        Rules['+'] = "+";
        Rules['-'] = "-";
        Rules['['] = "[";
        Rules[']'] = "]";
        Rules['*'] = "*";
        Rules['/'] = "/";

        
        // Randomly choose an Angle
        Angle = Random.Range(25f, 45f);
        
        // Randomly choose a StepSize
        StepSize = Random.Range(2f, 8f);

    }

    public void MutateRules( float angleProb = 0.1f, float pushPopProb = 0.1f, float fProb = 0.1f, float xProb = 0.1f)
    {
        //
        Rules['['] = (Random.value < pushPopProb) ? "[" : "[["; // 15%? chance for a single push or double push
        Rules[']'] = (Random.value < pushPopProb) ? "]" : "]]"; // 15%? chance for a single pop or double pop
        Rules['+'] = (Random.value < angleProb) ? "+" : "++"; // 30%? chance for a single rotation or double rotation
        Rules['-'] = (Random.value < angleProb) ? "-" : "--"; // 30%? chance for a single rotation or double rotation
        
        Rules['['] = (Random.value < pushPopProb) ? "[" : ""; // pushPopProb chance for adding, pushPopProb chance for keeping, and (1 - pushPopProb) chance for deleting
        Rules[']'] = (Random.value < pushPopProb) ? "]" : ""; // pushPopProb chance for adding, pushPopProb chance for keeping, and (1 - pushPopProb) chance for deleting
        Rules['+'] = (Random.value < angleProb) ? "+" : ""; // angleProb chance for adding, angleProb chance for keeping, and (1 - angleProb) chance for deleting
        Rules['-'] = (Random.value < angleProb) ? "-" : ""; // angleProb chance for adding, angleProb chance for keeping, and (1 - angleProb) chance for deleting

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