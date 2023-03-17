using System.Collections.Generic;
using UnityEngine;

public class PlantGenome : MonoBehaviour
{
    private Dictionary<char, string> rules = new Dictionary<char, string> {{ 'X', "[-FX]X[+FX][+F-FX]" }, { 'F', "FF" }
    };

}
