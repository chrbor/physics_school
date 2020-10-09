using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SchummelExScript : MonoBehaviour
{
    private SchummelScript sScript;
    private List<string> alliesNames = new List<string>();
    private List<GameObject> allies = new List<GameObject>();
    List<GameObject> activeObjects = new List<GameObject>();

    List<SchummelScript.SchummelEvent> sEvents;

    private bool stop;
    private bool block;

    /// <summary>
    /// Lädt das Script für das Verhalten der Geister
    /// </summary>
    /// <param name="path"></param>
    public void SetSchummelScript(string path)
    {
        if (block) return;
        SchummelScript temp = Resources.Load<SchummelScript>("SchummelScripts/" + path);
        if (temp) sScript = temp;
        else Debug.Log("SchummelScripts/" + path + " not found!");
    }

    public void StartSchummeln(Vector2 playerPos, Vector2 startPos)
    {
        if (block || !sScript) return;
        block = true;
        int startPtr = FindEntry(playerPos, startPos);
        if (startPtr == -1) { Debug.Log("Spieler außerhalb der Reichweite"); block = false; return; }

        StartCoroutine(StartSchummelScript(startPtr));
    }

    /// <summary>
    /// Finde den am nächsten liegenden Punkt zum Spieler
    /// </summary>
    /// <param name="playerPos"></param>
    private int FindEntry(Vector2 playerPos, Vector2 startPos)
    {
        if (!sScript) return -1;

        //initialisiere Anzahl an col. Allies
        List<string> tempList = new List<string>(PlayerScript.collectedAllies);
        alliesNames.Clear();

        //Finde den am nächsten liegenden Punkt zum Spieler: 
        float diff_nearest = Camera.main.orthographicSize * 2 * Camera.main.aspect;
        float diff;
        int nearestNum = -1;


        sEvents = new List<SchummelScript.SchummelEvent>(sScript.events);
        for(int count = 0; count < sEvents.Count; count++)
        {
            //setze startPos:
            if (count == 0) sEvents[count].startPosition = startPos;
            else
            {
                sEvents[count].startPosition = sEvents[count - 1].endPosition;
                if (sEvents[count].eventType != SchummelScript.EventType.add
                    && sEvents[count].eventType != SchummelScript.EventType.move
                    && sEvents[count].eventType != SchummelScript.EventType.jump)
                    sEvents[count].endPosition = sEvents[count].startPosition;

                if (sEvents[count].eventType == SchummelScript.EventType.spawn
                    || sEvents[count].eventType == SchummelScript.EventType.despawn) continue;


            }

            if(sEvents[count].eventType == SchummelScript.EventType.addSavePoint || count == 0)
            {
                diff = (playerPos - sEvents[count].startPosition).magnitude;
                if(diff < diff_nearest)
                {
                    nearestNum = count;
                    diff_nearest = diff;
                    alliesNames = new List<string>(tempList);
                }
                if (sEvents[count].eventType == SchummelScript.EventType.addSavePoint)
                { sEvents.RemoveAt(count--); continue; }
            }

            //aktualisiere tempList:
            switch(sScript.events[count].eventType)
            {
                case SchummelScript.EventType.add:
                    tempList.Add(sScript.events[count].focus[0]);
                    break;
                case SchummelScript.EventType.combine:
                    tempList.Add(sScript.events[count].focus[0]);
                    for (int i = 1; i < sScript.events[count].focus.Length; i++)
                        tempList.Remove(sScript.events[count].focus[i]);
                    break;
            }
        }
        return nearestNum;
    }



    IEnumerator WaitForRelease()
    {
        yield return new WaitUntil(() => (!Input.GetKey(KeyCode.P)));
        Debug.Log("stop: " + stop);
        if (stop) yield break;
        stop = true;
        block = false;
        foreach (GameObject unit in allies) Destroy(unit);
        foreach (GameObject obj in activeObjects) Destroy(obj);
        GetComponent<CoreScript>().SetPeeking(false);
        yield break;
    }



    IEnumerator StartSchummelScript(int ptr = 0)
    {
        //Physik läuft weiter, der Spieler kann dem Geist hinterherlaufen

        stop = false;
        GetComponent<CoreScript>().SetPeeking(true);

        //spawne Einheiten:
        Debug.Log("Spawne Ghosts");
        allies.Clear();
        GameObject obj, prevObj = null;
        int count = 0;
        foreach(string unitName in alliesNames)
        {
            obj = Resources.Load<GameObject>("Units/" + unitName);
            obj = Instantiate(obj, sScript.events[ptr].startPosition + Vector2.left * 0.2f * count, Quaternion.identity);
            obj.transform.GetChild(2).gameObject.layer = 17;//Ghost-Layer für Trigger
            obj.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);//unsichtbar

            allies.Add(obj);

            if (count++ == 0)
            {
                obj.tag = "Player";//ermöglicht es den character durch die Gegend zu steuern ohne vom Spieler beeinflusst zu werden 
                obj.layer = 18;//mainGhost
            }
            else
            {
                obj.tag = "Ghost";
                obj.layer = 17;//Ghost
                obj.GetComponent<GroupScript>().charToFollow = prevObj;
                obj.GetComponent<GroupScript>().inGroup = true;
            }
            prevObj = obj;
        }

        int steps = 60;
        Color stepColor = new Color(0, 0, 0, 0.005f);
        for(int i = 0; i < steps; i++)
        {
            foreach (GameObject unit in allies) unit.GetComponent<SpriteRenderer>().color += stepColor;
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(WaitForRelease());
        StartCoroutine(RunSchummelScript(ptr));
        yield break;
    }

    IEnumerator RunSchummelScript(int ptr)
    {

        Debug.Log("Starte SchummelScript");
        int steps = 30;
        Color stepColor;
        GameObject obj;
        SchummelScript.SchummelEvent sEvent;
        GroupScript gScript = allies[0].GetComponent<GroupScript>();
        while (ptr < sEvents.Count)
        {
            sEvent = sEvents[ptr];

            gScript.rb.bodyType = RigidbodyType2D.Static;
            if (ptr > 0)
                if (sScript.events[ptr].eventType != SchummelScript.EventType.add
                    && sScript.events[ptr].eventType != SchummelScript.EventType.move)
                    gScript.rb.velocity = Vector2.zero;

            //Warte auf Trigger:
            switch (sEvent.trigger)
            {
                case SchummelScript.TriggerType.none:

                    break;
                case SchummelScript.TriggerType.time:
                    if(sEvent.triggerTime > 0) yield return new WaitForSeconds(sEvent.triggerTime);
                    break;
            }
            if (stop) yield break;


            //Ausführung der Aktion:
            MovementScript mScript;
            bool goRight, changeObj;
            int arrayPos;
            Debug.Log("active Event: " + sEvent.title);
            switch (sEvent.eventType)
            {
                case SchummelScript.EventType.spawn:
                    //spawne ein SpielObjekt in der Geisterwelt:
                    Debug.Log("Spawn Object");
                    obj = Resources.Load<GameObject>("Prefabs/Objects/" + sEvent.focus[0]);
                    obj = Instantiate(obj, sEvent.endPosition, Quaternion.identity);
                    obj.layer = 19;//Geisterobjekt
                    obj.GetComponent<SpriteRenderer>().color = new Color(1,1,1,0);
                    activeObjects.Add(obj);
                    Debug.Log(obj);

                    StartCoroutine(FadeIn(obj));
                    yield return new WaitForSeconds(3);
                    break;



                case SchummelScript.EventType.despawn:
                    //despawne ein Objekt in der Geisterwelt
                    if (!int.TryParse(sEvent.focus[0], out arrayPos)) break;
                    obj = activeObjects[arrayPos];

                    steps = 30;
                    stepColor = new Color(0, 0, 0, -0.01f);
                    for (int i = 0; i < steps; i++)
                    {
                        obj.GetComponent<SpriteRenderer>().color += stepColor;
                        yield return new WaitForEndOfFrame();
                        if (stop) yield break;
                    }
                    activeObjects.RemoveAt(arrayPos);
                    Destroy(obj);
                    break;



                case SchummelScript.EventType.wait:
                    //Warte auf Event (noch nicht implementiert)

                    break;



                case SchummelScript.EventType.add:
                    //füge Einheit zur Gruppe hinzu, wie move, bloß mit gespawnter Einheit

                    //spawne Einheit:
                    obj = Resources.Load<GameObject>("Units/" + sEvent.focus[0]);
                    obj = Instantiate(obj, sEvent.endPosition, Quaternion.identity);
                    obj.transform.GetChild(2).gameObject.layer = 17;//Ghost-Layer für Trigger
                    allies.Add(obj);
                    alliesNames.Add(obj.GetComponent<SpriteRenderer>().sprite.name);
                    obj.tag = "Ghost";
                    obj.layer = 18;//mainGhost, bleibt auf Boden stehen
                    obj.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);//unsichtbar
                    StartCoroutine(FadeIn(obj));

                    //move zur Einheit:
                    gScript.rb.bodyType = RigidbodyType2D.Dynamic;
                    goRight = (sEvent.endPosition - (Vector2)gScript.transform.position).x > 0;
                    while (((sEvent.endPosition - (Vector2)gScript.transform.position).x > 0) == goRight)
                    {
                        gScript.ExecuteInput(!goRight, goRight, false, gScript.properties);
                        yield return new WaitForEndOfFrame();
                        if (stop) yield break;
                    }

                    //Einheit hinzufügen:
                    obj.GetComponent<GroupScript>().charToFollow = allies[allies.Count-2];
                    obj.GetComponent<GroupScript>().inGroup = true;
                    obj.layer = 17;//Ghost
                    break;



                case SchummelScript.EventType.combine:
                    //vereinige Einheiten zu einer neuen Einheit

                    //Einheiten die vereinigt werden sollen springen hoch, treffen sich in der Luft und werden gelöscht:
                    int count = 0;
                    float number = sEvent.focus.Length;
                    foreach(string unitName in sEvent.focus)
                    {
                        if (count++ == 0) continue;

                        StartCoroutine(Merge(alliesNames.IndexOf(unitName), sEvent.endPosition, (count / number - 0.5f) * 3));
                    }
                    yield return new WaitForSeconds(2f);
                    if (stop) yield break;

                    //spawne Einheit:
                    obj = Resources.Load<GameObject>("Units/" + sEvent.focus[0]);
                    obj = Instantiate(obj, sEvent.endPosition + Vector2.up, Quaternion.identity);
                    obj.transform.GetChild(2).gameObject.layer = 17;//Ghost-Layer für Trigger
                    allies.Add(obj);
                    alliesNames.Add(obj.GetComponent<SpriteRenderer>().sprite.name);
                    obj.tag = "Ghost";
                    obj.layer = 17;//Ghost
                    obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                    obj.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);//unsichtbar
                    obj.GetComponent<GroupScript>().enabled = false;

                    //Erscheinen:
                    steps = 30;
                    stepColor = new Color(0, 0, 0, 0.01f);
                    for (int i = 0; i < steps; i++)
                    {
                        obj.GetComponent<SpriteRenderer>().color += stepColor;
                        yield return new WaitForEndOfFrame();
                        if (stop) yield break;
                    }
                   
                    //Auf Boden Gleiten
                    steps = 30;
                    Vector3 stepPos = ((Vector3)sEvent.endPosition - obj.transform.position)/steps;
                    for (int i = 0; i < steps; i++)
                    {
                        obj.transform.position += stepPos;
                        yield return new WaitForEndOfFrame();
                        if (stop) yield break;
                    }

                    //Einheit hinzufügen:
                    obj.GetComponent<GroupScript>().enabled = true;
                    obj.GetComponent<GroupScript>().charToFollow = allies[allies.Count - 2];
                    obj.GetComponent<GroupScript>().inGroup = true;
                    break;



                case SchummelScript.EventType.move:
                    //bewege Einheiten (ohne zu springen)
                    gScript.rb.bodyType = RigidbodyType2D.Dynamic;
                    goRight = (sEvent.endPosition - (Vector2)allies[0].transform.position).x > 0;
                    while (((sEvent.endPosition - (Vector2)allies[0].transform.position).x > 0) == goRight)
                    {
                        Debug.Log("moving");
                        gScript.ExecuteInput(!goRight, goRight, false, gScript.properties);
                        yield return new WaitForEndOfFrame();
                        if (stop) yield break;
                    }
                    break;



                case SchummelScript.EventType.jump:
                    //bewege Einheiten mit Springanimation über eine Dauer von ValueSet

                    if (!gScript) yield break;
                    gScript.enabled = false;//gScript updates squish (scale) -> deactivation 

                    steps = 30;
                    Vector3 realPos = gScript.transform.position;
                    Vector3 realScale = Vector2.one * gScript.properties.size;
                    Vector3 posStep = (sEvent.endPosition - (Vector2)gScript.transform.position) / sEvent.valueSet;
                    Vector3 angleStep = -gScript.transform.eulerAngles / steps;

                    float b = -steps / 2f;
                    float a = -8f / (steps * steps);
                    float factor;

                    for (int i = 0; i <= steps; i++)
                    {
                        realPos += posStep;

                        factor = a * (i + b) * (i + b) + 2;
                        gScript.transform.position = realPos + Vector3.up * factor;
                        gScript.transform.localScale = realScale + (Vector3)new Vector2(-0.1f, 0.05f) * factor;
                        gScript.transform.eulerAngles += angleStep;

                        gScript.UpdatePastPos();
                        yield return new WaitForEndOfFrame();
                        if (stop) yield break;
                    }

                    gScript.enabled = true;
                    break;



                case SchummelScript.EventType.set_Mass:
                    changeObj = int.TryParse(sEvent.focus[0], out arrayPos);
                    obj = changeObj ? activeObjects[arrayPos] : gScript.gameObject;
                    mScript = obj.GetComponent<MovementScript>();

                    mScript.rb.mass = sEvent.valueSet;
                    break;



                case SchummelScript.EventType.set_Size:

                    changeObj = int.TryParse(sEvent.focus[0], out arrayPos);
                    obj = changeObj ? activeObjects[arrayPos] : gScript.gameObject;
                    mScript = obj.GetComponent<MovementScript>();

                    float diff = sEvent.valueSet - obj.transform.localScale.x;
                    obj.transform.localScale = Vector2.one * sEvent.valueSet;
                    mScript.properties.size = obj.transform.localScale.x;

                    obj.transform.position += Vector3.up * diff / 2;
                    break;



                case SchummelScript.EventType.change_mass:

                    break;



                case SchummelScript.EventType.change_size:

                    changeObj = int.TryParse(sEvent.focus[0], out arrayPos);
                    obj = changeObj ? activeObjects[arrayPos] : gScript.gameObject;
                    mScript = obj.GetComponent<MovementScript>();

                    bool add = sEvent.valueSet > gScript.properties.size;
                    while (Mathf.Abs(sEvent.valueSet - mScript.properties.size) > 0.15f)
                    {
                        //Debug.Log(Mathf.Abs(sEvent.valueSet - gScript.properties.size));
                        yield return new WaitForSeconds(0.5f);
                        if (stop) yield break;

                            obj = activeObjects[arrayPos];
                            obj.transform.localScale += (Vector3)Vector2.one * (add ? 0.1f : -0.1f);
                            obj.GetComponent<ObjectScript>().properties.size = obj.transform.localScale.x;

                            obj.transform.position += Vector3.up * (add ? 0.05f : -0.05f);
                    }
                    yield return new WaitForSeconds(0.5f);
                    break;


                case SchummelScript.EventType.change_vel:

                    break;


                case SchummelScript.EventType.change_puls:

                    break;


                case SchummelScript.EventType.change_force:

                    break;


                case SchummelScript.EventType.change_energy:

                    break;
            }

            ptr++;
        }

        //despawne Einheiten
        steps = 100;
        stepColor = new Color(0, 0, 0, -0.01f);
        for (int i = 0; i < steps; i++)
        {
            foreach (GameObject unit in allies) unit.GetComponent<SpriteRenderer>().color += stepColor;
            yield return new WaitForEndOfFrame();
            if (stop) yield break;
        }
        foreach (GameObject unit in allies) Destroy(unit);
        foreach (GameObject active_obj in activeObjects) Destroy(active_obj);
        stop = true;
        GetComponent<CoreScript>().SetPeeking(false);
        yield break;
    }


    IEnumerator FadeIn(GameObject obj)
    {
        int steps = 30;
        Color stepColor = new Color(0, 0, 0, 0.01f);
        for (int i = 0; i < steps; i++)
        {
            obj.GetComponent<SpriteRenderer>().color += stepColor;
            yield return new WaitForEndOfFrame();
            if (stop) yield break;
        }
        yield break;
    }

    /// <summary>
    /// Hebt die Einheiten hervor, schiebt sie anschließend zusammen und löscht sie danach
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="endPos"></param>
    /// <returns></returns>
    IEnumerator Merge(int  index, Vector2 endPos, float idPos)
    {
        GameObject obj = allies[index];

        //Schritt 1: Auffächern:
        int steps = 60;
        Vector3 realPos = obj.transform.position;
        Vector3 posStep = (endPos + new Vector2(idPos, 1) - (Vector2)obj.transform.position) / steps;
        Vector3 angleStep = -obj.transform.eulerAngles / steps;

        float b = -steps / 2f;
        float a = -4f / (steps * steps);
        float factor;

        for (int i = 0; i <= steps; i++)
        {
            realPos += posStep;

            factor = a * (i + b) * (i + b) + 1;
            obj.transform.position = realPos + Vector3.up * factor;
            obj.transform.eulerAngles += angleStep;

            yield return new WaitForEndOfFrame();
            if (stop) yield break;
        }

        yield return new WaitForSeconds(1);
        if (stop) yield break;

        //Schritt 2: Zusammenziehen:
        steps = 30;
        posStep = new Vector3(-idPos / steps, 0);
        Color colorStep = new Color(0, 0, 0, -0.01f);
        for(int i = 0; i < steps; i++)
        {
            obj.transform.position += posStep;
            obj.GetComponent<SpriteRenderer>().color += colorStep;
            yield return new WaitForEndOfFrame();
            if (stop) yield break;
        }

        allies.RemoveAt(index);
        alliesNames.RemoveAt(index);
        if (index == 0)
        {
            GroupScript gScript = allies[0].GetComponent<GroupScript>();
            gScript.tag = "Player";
            gScript.gameObject.layer = 18;//mainGhost
            gScript.charToFollow = null;
            gScript.inGroup = false;
            gScript.rb.bodyType = RigidbodyType2D.Static;
            gScript.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        Destroy(obj);
        yield break;
    }
}
