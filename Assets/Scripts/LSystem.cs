using System.Collections.Generic;
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
        string[] FStrings = {"FF"};
        string[] XStrings = {"X", "[-FX][+FX][FX]", "[-FX]X[+FX][+F-FX]", "[FF[+XF-F+FX]--F+F-FX]"};

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
    }

    public void MutateRules()
    {

    }

    public string ApplyRules(string currentState)
    {
        string newState = "";

        foreach (char symbol in currentState)
        {
            newState += Rules.ContainsKey(symbol) ? Rules[symbol] : symbol.ToString();
        }

        return newState;
    }
}