using System.Collections.Generic;
using UnityEngine;

public class LSystem
{
    public string Axiom;
    public Dictionary<char, string> Rules;
    public float Angle;
    public float StepSize;
    public float GrowthRate = 0.5f; // Time in seconds to complete one growth iteration


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
        string[] FStrings = {"FF",  // two consecutive branches
                             "F", //straight branch
                             "F[+F][-F]", //branch with two branches
                             "F[+F][-F][*F][/F]", //branch with four branches
                             "F+F-F", //a branch that splits into two branches at an angle of 120 degrees, forming a "Y" shape
                              "F[+F][-F]", //a branch that splits into two branches at angles of 45 degrees to the right and left, forming a "V" shape.
                              "F[+F]F[-F]F", //two branches at angles of 45 degrees to the right and left, with additional straight line segments or branches in between.
                              "F[*+F][-F][*F]" //splits into two branches, an additional straight line segment or branch 
        };

        
        //Plants
        string[] XStrings = {
            "[F[+FX][*+FX][/+FX]][F[-FX][*-FX][/-FX]]", // Symmetrical rule 1
            "[F[+FX][*+FX]][F[-FX][*-FX]]", // Symmetrical rule 2
            "F[+FX]F[-FX]F[+FX]F[-FX]", // Symmetrical rule 3
            "[F[+FX][*+FX][/+FX]]", // Taken from the original L-System implementation
            "[*+FX]X[+FX][/+F-FX]", // Taken from the original L-System implementation
            "[F[-X+F[+FX]][*-X+F[+FX]][/-X+F[+FX]-X]]",// Taken from the original L-System implementation
            "F[+F[+FX][-FX]][-F[-FX][+FX]]", //asyemtrical structure
            "[F[+F[-FX][+F/X]]][F[-F[+FX][-F*X]]]", //asyemmetry
            "[F[-F[-FX]][+F[+FX]]][F[+F[+FX]][-F[-FX]]]",//asyemmetry
            "[F[*+FX][+FX][-FX]]", //v structure
            "F[+FX][*+FX][-F[-FX]][/-F[-FX]]" //y structure

        };
        

        // Randomly choose one string for each symbol
        Rules['F'] = FStrings[Random.Range(0,FStrings.Length)];
        Rules['X'] = XStrings[Random.Range(0,XStrings.Length)];

        // Assign fixed strings for other symbols
        // rules below keep the current system
        Rules['+'] = "+";
        Rules['-'] = "-";
        Rules['['] = "[";
        Rules[']'] = "]";

        // Randomly choose an Axiom
        Axiom = XStrings[Random.Range(0, XStrings.Length)];
        
        // Randomly choose an Angle
        Angle = Random.Range(25f, 45f);
        
        // Randomly choose a StepSize
        StepSize = Random.Range(2f, 5f);
    }

    public void MutateRules( float angleProb, float pushPopProb, float fProb. float xProb)
    {
        Dictionary<char, string> additionRules = new Dictionary<char, string>();
        Dictionary<char, string> deletionRules = new Dictionary<char, string>();
        Dictionary<char, string> axiomChangeRules = new Dictionary<char, string>();

        //rules below add a new rule, or keeps the current 
        additionRules['F'] = (Random.value < pushPopProb) ? "F" : "FF";
        additionRules['['] = (Random.value < pushPopProb) ? "[" : "[["; // 15%? chance for a single push or double push
        additionRules[']'] = (Random.value < pushPopProb) ? "]" : "]]"; // 15%? chance for a single pop or double pop
        additionRules['+'] = (Random.value < angleProb) ? "+" : "++"; // 30%? chance for a single rotation or double rotation
        additionRules['-'] = (Random.value < angleProb) ? "-" : "--"; // 30%? chance for a single rotation or double rotation



        //rule below will delete
        deletionRules['F'] = (Random.value < fProb) ? "" : "F"; // 10%? chance for deletion, 90%? chance for keeping the branch
        deletionRules['['] = (Random.value < pushPopProb) ? "[" : ""; // pushPopProb chance for adding, pushPopProb chance for keeping, and (1 - pushPopProb) chance for deleting
        deletionRules[']'] = (Random.value < pushPopProb) ? "]" : ""; // pushPopProb chance for adding, pushPopProb chance for keeping, and (1 - pushPopProb) chance for deleting
        deletionRules['+'] = (Random.value < angleProb) ? "+" : ""; // angleProb chance for adding, angleProb chance for keeping, and (1 - angleProb) chance for deleting
        deletionRules['-'] = (Random.value < angleProb) ? "-" : ""; // angleProb chance for adding, angleProb chance for keeping, and (1 - angleProb) chance for deleting

        // 5% chance for changing the axiom to a random string from XStrings, otherwise keep the axiom as is
        axiomChangeRules['X'] = (Random.value < xProb)) ? XStrings[Random.Range(0, XStrings.Length)] : "X"; 
        
    }


    public string ApplyMutateRules(string currentState,, bool additionYN, bool deletionYN, bool axiomChangeYN)
    {
        string nextState = "";

        for (int i = 0; i < currentState.Length; i++)
        {
            char currentChar = currentState[i];
            string mutatedChar = "";

            if (additionYN && additionRules.ContainsKey(currentChar))
            {
                mutatedChar = additionRules[currentChar];
            }
            else if (deletionYN && deletionRules.ContainsKey(currentChar))
            {
                mutatedChar = deletionRules[currentChar];
            }
            else if (axiomChangeYN && axiomChangeRules.ContainsKey(currentChar))
            {
                mutatedChar = axiomChangeRules[currentChar];
            }

            // If the character is not found in any of the mutation rules, keep it unchanged
            if (string.IsNullOrEmpty(mutatedChar))
            {
                nextState += currentChar.ToString();
            }
            else
            {
                nextState += mutatedChar;
            }
        }

        return nextState;
    }

    public string ApplyRules(string currentState , int iterations)
    {
        string newState = "";
        
        for (int i = 0; i < iterations; i++)
        {
            foreach (char symbol in currentState)
            {
                newState += Rules.ContainsKey(symbol) ? Rules[symbol] : symbol.ToString();
            }
        }

        return newState;
    }
}