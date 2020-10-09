using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject focus;
    public bool block;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (block)
            return;

        //transform.position = new Vector3(focus.transform.position.x, focus.transform.position.y, -10);//starre Kamera
        //transform.position += (Vector3)((Vector2)(focus.transform.position - transform.position) / 4);
        if(focus)
            transform.position += new Vector3((focus.transform.position.x - transform.position.x), (focus.transform.position.y - transform.position.y) / 6, 0);
    }

    public void LookAtCenter( Vector2 center, float size)
    {
        StartCoroutine(LookingAtCenter(center, size));
    }

    IEnumerator LookingAtCenter(Vector2 center, float size)
    {
        block = true;
        while (Input.GetKey(KeyCode.Z))
        {
            transform.position += new Vector3((center.x - transform.position.x), (center.y - transform.position.y) / 6, 0);
            GetComponent<Camera>().orthographicSize = size;
            yield return new WaitForFixedUpdate();
        }
        GetComponent<Camera>().orthographicSize = 4;
        block = false;
        yield break;
    }

    public void ChangeFocus(GameObject newFocus, int steps = 20)
    {
        StartCoroutine(sweepPosition(newFocus, steps));
    }

    /// <summary>
    /// sweepPosition gleitet in steps zu newFocus und setzt dann focus auf newFocus
    /// </summary>
    /// <param name="newFocus">Das neue GameObjekt, auf das die Kamera zentriert wird</param>
    /// <param name="steps">Anzahl der Schritte, in denen die Kamera zu newFocus gleitet</param>
    /// <returns></returns>
    public IEnumerator sweepPosition(GameObject newFocus, int steps = 20)
    {
        if (!focus) focus = gameObject;
        GameObject oldFocus = focus;
        focus = newFocus;
        Vector2 diff_steps = (newFocus.transform.position - oldFocus.transform.position)/steps;

        block = true;
        for(int i = 0; i < steps; i++)
        {
            transform.position += (Vector3)diff_steps;
            yield return new WaitForEndOfFrame();
        }

        block = false;
        if (newFocus.name == "temp") Destroy(newFocus);
        if(GameManager.manager)
            GameManager.manager.OnTriggerMove(0);
        yield break;
    }
}
