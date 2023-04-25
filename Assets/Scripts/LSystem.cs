using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; // Include the System.Text namespace for StringBuilder
using UnityEngine;
using Random = UnityEngine.Random;

public class LSystem
{
    public string Axiom;
    public Dictionary<char, string> Rules;
    public float Angle;
    public float StepSize;

    private string[] alphabet = {"F", "X", "[", "]", "+", "-", "*", "/"}; // Define the alphabet of symbols
    
    // Define some possible strings for each symbol

    string[] FStrings = {"FF",  // two consecutive branches
        "F[+F][-F][*F][/F]", //branch with four branches
        "F+F-F", //a branch that splits into two branches at an angle of 120 degrees, forming a "Y" shape
        "F[+F]F[-F]F", //two branches at angles of 45 degrees to the right and left, with additional straight line segments or branches in between.
        "F[*+F][-F][*F]", //splits into two branches, an additional straight line segment or branch
        "F[/F*F*F][/F*F*F]/F"
    };

        
    //Plants
    string[] XStrings = {
        "[F[+FX][*+FX]][F[-FX][*-FX]]", // Symmetrical rule 2
        "[F[+FX][*+FX][/+FX]]", // Taken from the original L-System implementation
        "[*+FX]X[+FX][/+F-FX]", // Taken from the original L-System implementation
        "[F[*+FX][+FX][/-FX]]", //v structure
        

    };

    public LSystem(string axiom, float angle, float stepSize)
    {
        Axiom = axiom;
        Angle = angle;
        StepSize = stepSize;
        Rules = new Dictionary<char, string>();
    }
    
    public string SymmetricalOnePointCrossover(string parent1, string parent2)
    {
        int length = Math.Min(parent1.Length, parent2.Length) / 2;
        int crossoverPoint = Random.Range(0, length);
        string firstHalf = parent1.Substring(0, crossoverPoint) + parent2.Substring(parent2.Length - length + crossoverPoint);
        
        //Mirror the first half and replace '[' with ']' and vice versa to form the child string
        StringBuilder secondHalf = new StringBuilder(firstHalf);
        char[] secondHalfArray = secondHalf.ToString().ToCharArray();
        Array.Reverse(secondHalfArray);
        secondHalf = new StringBuilder(new string(secondHalfArray));
        
        for(int i = 0; i < secondHalf.Length; i++) {
            if (secondHalf[i] == '[') {
                secondHalf[i] = ']';
            } else if (secondHalf[i] == ']') {
                secondHalf[i] = '[';
            }
        }

        string child = firstHalf + secondHalf;
        
        return child;
    }
    
    public string NPointCrossover(string parent1, string parent2, int n)
    {
        
        int genomeLength = Math.Min(parent1.Length, parent2.Length);
        string child = "";
        int[] crossoverPoints = new int[n];


        // Insert substrings from parent2 into parent1 at n evenly spaced points
        for (int i = 0; i < n; i++)
        {
            int point = UnityEngine.Random.Range(1, genomeLength);
            while (crossoverPoints.Contains(point))
            {
                point = UnityEngine.Random.Range(1, genomeLength);
            }
            crossoverPoints[i] = point;
        }

        Array.Sort(crossoverPoints);
        bool swap = false;
        int currentIndex = 0;
        for (int i = 0; i <= n; i++)
        {
            int endpoint = i == n ? genomeLength : crossoverPoints[i];
            if (swap)
            {
                
                child += parent2.Substring(currentIndex, endpoint - currentIndex);
                if (i % 1 == 0)
                {
                    child = EnsureBalanced(child);
                }


            }
            else
            {
                child += parent1.Substring(currentIndex, endpoint - currentIndex);
                if (i % 2 == 0)
                {
                    child = EnsureBalanced(child);
                }
                

            }
            swap = !swap;
            currentIndex = endpoint;
        }
        child = EnsureBalanced(child);

        return child.ToString();
    }

    private static string EnsureBalanced(string genome)
    {
        int openCount = 0;
        int closeCount = 0;

        foreach (char c in genome)
        {
            if (c == '[')
            {
                openCount++;
            }
            else if (c == ']')
            {
                closeCount++;
            }
        }

        if (openCount > closeCount)
        {
            genome += new string(']', openCount - closeCount);
        }
        else if (closeCount > openCount)
        {
            genome = new string('[', closeCount - openCount) + genome;
        }

        return genome;
    }

    //performs crossover for half the parents string and then mirrors it to create a symmetrical child string
    public string SymmetricalNPointCrossover(string parent1, string parent2, int n)
    {
        bool flip = Random.value < 0.5f;
        int lengthOfHalf = parent1.Length / 2;
        int spacing = lengthOfHalf / (n + 1);
        int maxVariance = 3;

        StringBuilder child = new StringBuilder(parent1);
        for (int i = 1; i <= n; i++)
        {
            int insertIndex = i * spacing;
            if (flip)
            {
                insertIndex += lengthOfHalf;
            }
            if (insertIndex >= lengthOfHalf) continue;

            int length = Math.Min(parent2.Length - insertIndex, lengthOfHalf - insertIndex);
            if (length <= 0) continue;

            if (insertIndex + length > parent1.Length / 2)
            {
                length = parent1.Length / 2 - insertIndex;
            }
            if (length <= 0) continue;

            child.Remove(insertIndex, length);
            child.Insert(insertIndex, parent2.Substring(insertIndex, length));
        }

        int idealLength = (parent1.Length + parent2.Length) / 4;
        int variance = Random.Range(-maxVariance, maxVariance + 1);
        while (child.Length > idealLength + variance)
        {
            child.Remove(Random.Range(0, child.Length), 1);
        }

        string leftHalf = child.ToString().Substring(0, lengthOfHalf);
        StringBuilder mirroredString = new StringBuilder(leftHalf.Length);
        foreach (char c in leftHalf.Reverse())
        {
            char mirror = (c == '[') ? ']' : ((c == ']') ? '[' : c);
            mirroredString.Append(mirror);
        }

        if (flip)
        {
            child.Length = lengthOfHalf;
            child.Append(mirroredString.ToString());
        }
        else
        {
            child.Insert(0, mirroredString.ToString());
        }

        Debug.Log($"Parent 1: {parent1}");
        Debug.Log($"Parent 2: {parent2}");
        Debug.Log("Symmetrical crossover:");

        return child.ToString();
    }
    
    //Warning, leads to highly random and complex rules, use with caution
    public string GenerateRandomRule(List<string> strings, int maxLength = 40, int numSubstrings = 3)
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
    
    public void InitializeRandomRules(bool completelyRandom = true)
    {

        // Randomize the F and X rules
        if (completelyRandom)
        {
            Rules['F'] = GenerateRandomRule(FStrings.ToList(), Random.Range(2, 20));
            Rules['X'] = GenerateRandomRule(XStrings.ToList(), Random.Range(8, 40));
        }
        else
        {
            Rules['F'] = FStrings[Random.Range(0,FStrings.Length)];
            Rules['X'] = XStrings[Random.Range(0,XStrings.Length)];
        } 
        //Debug.Log("Rules['F'] = " + Rules['F']);
       //Debug.Log("Rules['X'] = " + Rules['X']);
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

    public void MutateParameters(float maxAngleChange = 10f, float maxStepSizeChange = 0.5f)
    {
        float currAngle = Angle;
        float currStepsize = StepSize;
        //Randomly mutate the angle of the ShootLSystem by +- maxAngleChange and step size by +- maxStepSizeChange
        Angle = Random.Range(currAngle - maxAngleChange, currAngle + maxAngleChange);
        StepSize = Random.Range(currStepsize - maxStepSizeChange, currStepsize + maxStepSizeChange);
        
    }

    public void MutateRules(float angleProb = 0.05f, float pushPopProb = 0.05f, float fProb = 0.2f, float xProb = 0.2f)
    {
        //Low chance to introduce double rotation or double push/pop which drastically changes plant structure
        Rules['['] = (Random.value < pushPopProb) ? "[" : "[["; 
        Rules[']'] = (Random.value < pushPopProb) ? "]" : "]]"; 
        Rules['+'] = (Random.value < angleProb) ? "+" : "++"; 
        Rules['-'] = (Random.value < angleProb) ? "-" : "--"; 
        
        // Randomly choose a character to mutate for F and X rules and its index at a higher probability as this mutation is less destructive
        char mutateChar = (Random.value < fProb) ? 'F' : (Random.value < fProb + xProb) ? 'X' : ' '; 
        if(mutateChar == 'F' || mutateChar == 'X')
        {
            int index = Random.Range(0, Rules[mutateChar].Length);
            StringBuilder newString = new StringBuilder(Rules[mutateChar]);

            // Perform single point mutation symmetrically across the mid point to maintain plant like structure
            if (index < newString.Length / 2)
            {
                newString[index] = newString[newString.Length - index - 1];
            }
            else
            {
                newString[newString.Length - index - 1] = newString[index];
            }

            // Update the rules dictionary with the mutated string
            Rules[mutateChar] = newString.ToString();
        }
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