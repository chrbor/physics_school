using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Char_Trigger : MonoBehaviour
{
    public bool hit;
    private int count;

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Falls Verbündet, dann gebe das Signal zu folgen
        GroupScript groupScript = other.GetComponent<GroupScript>();

        //wenn der Char der Gruppe schon angehört, dann ignorieren:
        if (groupScript.inGroup)
            return;

        //weiße den Char, dem gefolgt werden soll, zu: 
        groupScript.groupNumber = PlayerScript.allies.Count;
        if (PlayerScript.allies.Count == 0)
            groupScript.charToFollow = transform.parent.gameObject;
        else
            groupScript.charToFollow = PlayerScript.allies[PlayerScript.allies.Count - 1];

        other.name = other.GetComponent<SpriteRenderer>().sprite.name;
        PlayerScript.allies.Add(other.gameObject);
        PlayerScript.alliesNames.Add(other.name);
        PlayerScript.collectedAlliesCurrent.Add(other.name);
        PlayerScript.spawnPositions.Add(other.transform.position);
        other.GetComponent<GroupScript>().properties = PlayerScript.allies[0].GetComponent<GroupScript>().properties;//shallow, mehr aber auch nicht vonöten

        //Überschreibe wert mit defaultwert:
        DefaultStats.SetDefaultCharValueByName(other.name);

        //folge dem Char:
        groupScript.inGroup = true;
        groupScript.bottom = true;
        return;
    }
}
