using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScript : MonoBehaviour
{
    //Group:
    [HideInInspector]
    public static List<GameObject> allies = new List<GameObject>();
    [HideInInspector]
    public static List<string> alliesNames = new List<string>();
    [HideInInspector]
    public static List<string> collectedAllies = new List<string>();
    [HideInInspector]
    public static List<string> collectedAlliesCurrent = new List<string>();
    [HideInInspector]
    public static List<Vector2> spawnPositions = new List<Vector2>();

    [HideInInspector]
    public GroupScript gScript;
    private ManipulatorScript manipulatorScript;
    private CoreScript cScript;

    private void Awake()
    {
        name = GetComponent<SpriteRenderer>().sprite.name;
        manipulatorScript = Camera.main.GetComponent<ManipulatorScript>();
        cScript = GameObject.Find("CoreManager").GetComponent<CoreScript>();
        gScript = GetComponent<GroupScript>();
        gScript.inGroup = true;
        if (!enabled) return;
        allies.Clear();
        alliesNames.Clear();
        allies.Add(gameObject);
        alliesNames.Add(name);
        collectedAllies.Add(name);
        collectedAlliesCurrent.Add(name);
        spawnPositions.Add(transform.position);
        //gScript.rb.bodyType = RigidbodyType2D.Static;
        block = true;
        Physics2D.autoSimulation = false;
    }

    public void StartPlayer()
    {
        Physics2D.autoSimulation = true;
        block = false;
        StartCoroutine(gScript.StartAnimation());
    }

    // Update is called once per frame
    void Update()
    {
        //Spieler-Input:
        if (block) return;

        //Reset Level:
        if (Input.GetKey(KeyCode.R)) RestartLevel();
        //Reset Checkpoint:
        if ((Input.GetKey(KeyCode.C) && CheckpointScript.current) || Math.Abs(transform.position.x) > 40 || Mathf.Abs(transform.position.y) > 27) CheckpointScript.current.ReloadCheckpoint();
        //Schummeln:
        if (Input.GetKey(KeyCode.P) && CheckpointScript.current) CheckpointScript.current.StartSchummeln(transform.position);
        //Arbeit frühzeitig abgeben:
        if (Input.GetKey(KeyCode.B)) cScript.EndTheGame();

        gScript.ExecuteInput(Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D), Input.GetKey(KeyCode.W), gScript.properties);
    }

    private void FixedUpdate()
    {
        gScript.FixedUpdate();
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        //setze sprung zurück:
        gScript.ResetJump(collider);
    }

    public List<string> JumpIntoBook(Transform book)
    {
        //Jump into book:
        GameObject obj = new GameObject("temp");
        obj.transform.position = transform.position;
        Camera.main.GetComponent<CameraScript>().ChangeFocus(obj);

        foreach(GameObject ally in allies)
            ally.GetComponent<GroupScript>().JumpIntoBook(book);

        return alliesNames;
    }

    public List<string> JumpOutOfBook()
    {
        foreach (GameObject ally in allies)
            ally.GetComponent<GroupScript>().JumpOutOfBook();

        return alliesNames;
    }
}


