using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FormulaList
{
    public static int numberOfOneToOneTerms = 2;

    public static Dictionary<string, string> nameToAbbr = new Dictionary<string, string>
    {
        { "mass", "_kg"},
        { "size", "_m" },
        { "time", "_t" },
        { "frequency", "_hz" },
        { "velocity", "_v" },
        { "acceleration", "_a" },
        { "puls", "_p" },
        { "force", "_N" },
        { "gravity", "_N" },
        { "friction", "_N" },
        { "spring", "_N" },
        { "energy", "_E" },
        { "kineticEnergy", "_E" },
        { "potentialEnergy", "_E" },
        { "pressure", "_Pa" },
        { "current", "_A" },
        { "2_6", "2" },//Nummer
    };

    public static Dictionary<string, string> abbrToName = new Dictionary<string, string>
    {
        { "_kg", "mass"},
        { "_m", "size"},
        { "_t", "time"},
        { "_hz", "frequency"},
        { "_v", "velocity"},
        { "_a", "acceleration"},
        { "_p", "puls" },
        { "_N", "force"},
        { "_Ng", "gravity"},
        { "_Nr", "friction"},
        { "_Nf", "spring"},
        //{ "_E", "energy"},
        { "_Ekin", "kineticEnergy"},
        { "_E", "potentialEnergy"},
        { "_Pa", "pressure"},
        { "_A", "current"}
    };

    public static List<string> terms = new List<string>()
    {
        "_m/_t",//Geschwindigkeit
        "_v/_t",//Beschleunigung
        "_kg*_v",//puls
        "_kg*_a",//Kraft
        "_my*_kg*_kg/(_m*_m)",//Gravitationskraft
        "_my*_kg",//Reibungskraft
        "_D*_m",//Federkraft
        "_p*_v/2",//Kinetische Energie
        "_p*_v",//Potentielle Energie
        "_kg/_v",//Druck


        "/_t",//Herz
        "/_hz",//Zeit
    };
    public static List<string> results = new List<string>()
    {
        "_v",//Geschwindigkeit
        "_a",//Beschleunigung
        "_p",//kinetic Puls
        "_N",//Kraft
        "_Ng",//Gravitationskraft
        "_Nr",//Reibungskraft
        "_Nf",//Federkraft
        "_Ekin",//Kinetische Energie
        "_E",//(potentielle) Energie
        "_Pa",//impuls



        "_hz",//Herz
        "_t",//zeit
    };
}
