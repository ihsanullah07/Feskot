using UnityEngine;
using UnityEditor;
using System.Threading;
 
[InitializeOnLoad]
public class Startup {
    static Startup() {
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-us");
    }
}