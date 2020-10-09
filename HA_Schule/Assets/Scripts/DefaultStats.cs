using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class DefaultStats
{
    /// <summary>
    /// Funktion, um deep-Copies zu machen
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T DeepClone<T>(this T obj)
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T)formatter.Deserialize(ms);
        }
    }

    /// <summary>
    /// Setzt die Variable mit dem angegebenen namen auf den Defaultwert
    /// </summary>
    /// <param name="unitName">Name der Variable, die auf default gesetzt werden soll</param>
    public static void SetDefaultCharValueByName(string unitName)
    {
        if (PlayerScript.allies.Count == 0) return;
        if (unitName.Equals("puls") || unitName.Equals("kineticEnergy") || unitName.Equals("potentialEnergy") || unitName.Equals("time")) return;
        MovementScript.Properties props = PlayerScript.allies[0].GetComponent<GroupScript>().properties;

        if(unitName.Equals("force"))
        {
            props.GetType().GetField(unitName + "_x").SetValue(props, (float)defaultChar.GetType().GetField(unitName + "_x").GetValue(defaultChar));//reflection
            foreach (GameObject unit in PlayerScript.allies) { unit.GetComponent<GroupScript>().properties = props; Debug.Log("reflect: " + props.size); }
            props.GetType().GetField(unitName + "_y").SetValue(props, (float)defaultChar.GetType().GetField(unitName + "_y").GetValue(defaultChar));//reflection
            foreach (GameObject unit in PlayerScript.allies) { unit.GetComponent<GroupScript>().properties = props; Debug.Log("reflect: " + props.size); }
            return;
        }

        //Debug.Log(unitName);
        props.GetType().GetField(unitName).SetValue(props, (float)defaultChar.GetType().GetField(unitName).GetValue(defaultChar));//reflection
        foreach (GameObject unit in PlayerScript.allies) { unit.GetComponent<GroupScript>().properties = props; }
    }

    public static MovementScript.Properties defaultChar = new MovementScript.Properties
    {
        using_acceleration = true,
        using_velocity = true,
        using_kineticEnergy = true,
        using_potentialEnergy = true,
        using_size = true,
        using_force = true,
        using_mass = true,
        using_puls = true,
        using_jumpForce = true,
        using_friction = true,

        force_x = 0,
        force_y = 0,
        mass = 1,
        size = 0.4f,
        velocity = 0.5f,
        velocity_Damping = 0.1f,
        acceleration = 1,
        jumpForce = 2.1f,
        initialDrag = 0.0f,
        current = 1,
        luminosity = 0,
        temperature = 1
    };

    public static MovementScript.Properties defaultphysObj = new MovementScript.Properties
    {
        using_puls = true,
        using_force = true,
        using_potentialEnergy = true,
        using_kineticEnergy = true,
        using_mass = true,
        using_initialDrag = true,
        using_luminosity = true,

        force_x = 0,
        force_y = 0,
        mass = 1,
        velocity = 0.5f,
        velocity_Damping = 0.2f,
        acceleration = 1,
        jumpForce = 3.5f,
        initialDrag = 0.05f,
        current = 1,
        luminosity = 0,
        temperature = 1
    };
}
