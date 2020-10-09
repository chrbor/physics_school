using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupScript : MovementScript
{
    private Vector2 endPosition;    //Position, die angestrebt wird

    //Variablen zur Bestimmung der Gruppe:
    public bool inGroup;            //gibt an ob der Char in der Gruppe des Spielers ist
    public int groupNumber;         //gibt an, an welcher Gruppenstelle sich der Char befindet
    public GameObject charToFollow; //der Char, dem gefolgt werden soll. Der erste Char folgt dem Spieler, der zweite dem ersten Char usw.

    //private float umkreis = 1f;
    private bool left, right, up;   //Steuereingaben vom Algorithmus für das Spiel

    private bool moving;
    private int moveCount = 0;

    new public void FixedUpdate()
    {
        if (GameManager.block) return;

        UpdatePastPos();

        trigger.SetActive(CompareTag("Player"));

        //Gravitation wirken lassen:
        rb.velocity += new Vector2(properties.force_x, properties.force_y) / 20;

        if (!trigger.activeSelf)
        {
            //Falls in der Gruppe, dann folge dem zugewiesenen Char
            //Char zugewiesen, dem gefolgt werden soll?
            if (!charToFollow)
            {
                //->warteanimation hier einfügen
                UpdateSquish();
                RotateToGround();
                inGroup = false;
                return;
            }

            FollowByList();
        }
        else
        {
            UpdateSquish();
            RotateToGround();
        }
    }

    public void UpdatePastPos()
    {
        pastPositions.Add(new TransformSaved(transform.position, transform.rotation, transform.localScale));
        if (pastPositions.Count > 25) pastPositions.RemoveAt(0);  
    }

    private void FollowByList()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        gameObject.layer = 16;//DefaultOnly

        if(moveCount >= 0) { moveCount--; return; }

        //bei nähe nicht bewegen
        GroupScript gScript = charToFollow.GetComponent<GroupScript>();
        if (Mathf.Abs((transform.position - charToFollow.transform.position).magnitude) < 0.5f * (moving ? 1.75f : 2) && bottom) { moving = false; transform.localScale = Vector2.one * properties.size; return; }
        //Debug.Log(name + ": " + bottom);

        if (gScript.pastPositions.Count == 0) return;
        TransformSaved t;// = gScript.pastPositions[0];
        if (moving)
        {
            t = gScript.pastPositions[0];
        }
        else//restart
        {
            //suche ersten wert, dessen position zwischen char und charToFollow liegt 
            moveCount = 0;
            foreach(TransformSaved position in gScript.pastPositions)
            {
                Vector2 diff_1 = charToFollow.transform.position - transform.position;
                Vector2 diff_2 = position.position - (Vector2)transform.position;
                //wenn               abstand zwischen charToFollow und char      größer als         abstand zwischen pastPosition und char       dann break
                if (diff_1.magnitude > diff_2.magnitude
                    && Mathf.Sign(diff_1.x) == Mathf.Sign(diff_2.x)
                    && Mathf.Sign(diff_1.y) == Mathf.Sign(diff_2.y))
                    break;
                moveCount++;
            }

            if (moveCount > 0) { moveCount--; return; }
            //Debug.Log(name + " restarts at: " + moveCount);
            t = gScript.pastPositions[moveCount];
        }

        transform.position = t.position;
        transform.rotation = t.rotation;
        transform.localScale = t.scale;
        moving = true;
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        //setze sprung zurück:
        ResetJump(collider);
    }

    public void JumpIntoBook(Transform book)
    {
        mainCol.enabled = false;
        transform.parent = null;
        GetComponent<SpriteRenderer>().sortingLayerName = "UI";
        GetComponent<SpriteRenderer>().sortingOrder =  groupNumber+1;
        SetEyeOrder("UI", groupNumber+2);

        StartCoroutine(BookJump(book));
    }

    IEnumerator BookJump(Transform book)
    {
        //yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds((groupNumber+1)/20f);

        Vector3 scaleStep, posStep, angleStep;

        //squish:
        scaleStep = ((Vector2)transform.localScale - new Vector2(0.6f, 0.25f)) / 10;
        for (int i = 0; i < 10; i++)
        {
            transform.localScale -= scaleStep;
            yield return new WaitForEndOfFrame();
        }

        //Sprunganimation:
        int steps = 60;
        scaleStep = (new Vector2(0.3f, 0.3f) - (Vector2)transform.localScale) / steps;
        posStep = (((Vector2)book.position + RotToVec(book.eulerAngles.z) * (-4.5f + 0.75f * (groupNumber % 5)) + RotToVec(book.eulerAngles.z + 90) * (2.75f - 1f * (groupNumber / 5))) - (Vector2)transform.position) / steps;
        angleStep = new Vector3(0, 0, book.eulerAngles.z - transform.eulerAngles.z) / steps;

        Vector3 realPos = transform.position;
        Vector3 realScale = transform.localScale;

        float b = -steps / 2f;
        float a = -4f / (steps * steps);
        float factor;

        for (int i = 0; i <= steps; i++)
        {
            realPos += posStep;
            realScale += scaleStep;

            factor = a * (i + b) * (i + b) + 1;
            transform.position = realPos + Vector3.up * factor;
            transform.localScale = realScale + (Vector3)Vector2.one * factor;
            transform.eulerAngles += angleStep;

            yield return new WaitForEndOfFrame();
        }

        //Fungiere als Button:
        mainCol.isTrigger = true;
        mainCol.enabled = true;
        bottom = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(1);
        Physics2D.autoSimulation = true;


        yield break;
    }

    public void JumpOutOfBook()
    {
        transform.parent = null;
        StartCoroutine(JumpOutBook());
    }

    IEnumerator JumpOutBook()
    {
        //Deaktiviere Button:
        mainCol.isTrigger = false;
        mainCol.enabled = false;
        bottom = false;

        yield return new WaitForSeconds((groupNumber + 1) / 20f);

        Vector3 scaleStep, posStep, angleStep;

        //squish:
        scaleStep = ((Vector2)transform.localScale - new Vector2(0.6f, 0.25f)) / 10;
        for (int i = 0; i < 10; i++)
        {
            transform.localScale -= scaleStep;
            yield return new WaitForEndOfFrame();
        }

        //Sprunganimation:
        int steps = 60;
        scaleStep = (Vector2.one * properties.size - (Vector2)transform.localScale) / steps;
        posStep = (CharacterMenu.startPos[groupNumber] - (Vector2)transform.position) / steps;
        angleStep = new Vector3(0, 0, CharacterMenu.startAngle[groupNumber] - transform.eulerAngles.z) / steps;

        Vector3 realPos = transform.position;
        Vector3 realScale = transform.localScale;

        float b = -steps / 2f;
        float a = -4f / (steps * steps);
        float factor;

        for (int i = 0; i <= steps; i++)
        {
            realPos += posStep;
            realScale += scaleStep;

            factor = a * (i + b) * (i + b) + 1;
            transform.position = realPos + Vector3.up * factor;
            transform.localScale = realScale + (Vector3)Vector2.one * factor;
            transform.eulerAngles += angleStep;

            yield return new WaitForEndOfFrame();
        }


        if (groupNumber == 0) Camera.main.GetComponent<CameraScript>().ChangeFocus(gameObject); 

        scaleStep = ((Vector2)transform.localScale - new Vector2(0.6f, 0.25f)) / 10;
        for (int i = 0; i < 10; i++)
        {
            transform.localScale -= scaleStep;
            yield return new WaitForEndOfFrame();
        }
        scaleStep = ((Vector2)transform.localScale - Vector2.one * properties.size) / 10;
        for (int i = 0; i < 10; i++)
        {
            transform.localScale -= scaleStep;
            yield return new WaitForEndOfFrame();
        }


        GetComponent<SpriteRenderer>().sortingLayerName = "Char_Layer";
        GetComponent<SpriteRenderer>().sortingOrder = 0;
        SetEyeOrder("Char_Layer", 1);
        mainCol.enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = Vector2.zero;
        GameManager.block = false;
        if (groupNumber == 0) { tag = "Player"; }
        yield return new WaitForSeconds(1);
        Physics2D.autoSimulation = true;

        yield break;
    }

    public void MoveToNewPosition(Transform book)
    {
        transform.parent = null;
        mainCol.enabled = false;
        StartCoroutine(SetNewPosition(book));
    }

    IEnumerator SetNewPosition(Transform book)
    {
        yield return new WaitForSeconds((groupNumber + 1) / 20f);
        Vector3 posStep, angleStep;

        int steps = 30;
        posStep = (((Vector2)book.position + RotToVec(book.eulerAngles.z) * (-4.5f + 0.75f * (groupNumber % 5)) + RotToVec(book.eulerAngles.z + 90) * (2.75f - 1f * (groupNumber / 5))) - (Vector2)transform.position) / steps;
        angleStep = new Vector3(0, 0, book.eulerAngles.z - transform.eulerAngles.z) / steps;

        Vector3 realPos = transform.position;

        float b = -steps / 2f;
        float a = -2f / (steps * steps);
        float factor;

        for (int i = 0; i < steps; i++)
        {
            realPos += posStep;

            factor = a * (i + b) * (i + b) + 0.5f;
            transform.position = realPos + Vector3.up * factor;
            transform.eulerAngles += angleStep;

            yield return new WaitForEndOfFrame();
        }

        mainCol.enabled = true;
        yield break;
    }
}
