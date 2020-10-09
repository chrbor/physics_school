using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Managet das Spiel. Steuert Storyevents
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager manager;
    public static int activeBuildIndex;
    public static int level;

    public static int SequenceCount;
    //public static int subSequenceCount;

    public static bool eventLock;
    public static int eventID;
    static int eventIDCount;

    static int triggerID;
    static int triggerCount;
    static bool triggerMove;

    private GameObject focus;
    private OV_UI ov_ui;
    public static bool block;

    public enum GameStateType { sequence, menu, coreGame, hub}
    public static GameStateType stateType = GameStateType.coreGame;//startet immer mit einer zwischensequenz oder einer Blende

    public Texture2D cursor, cursorPressed;
    private Vector2 cursorOff = new Vector2(64, 0);

    public string LevelScriptFile;
    public string LevelScriptName;



    private void Awake()
    {
        //singleton:
        if (manager)
            Destroy(gameObject);
        else
        {
            manager = this;
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += OnLevelLoaded;
    }

    private void Update()
    {
        if (stateType == GameStateType.coreGame)
            Cursor.SetCursor(Input.GetMouseButton(0) ? cursorPressed : cursor, cursorOff, CursorMode.ForceSoftware);
    }

    public void OnTriggerEvent(int _triggerID) { triggerID = _triggerID; }

    public void OnTriggerSignal(){ triggerCount++; }

    public void OnTriggerMove(int id){ eventID = id; eventLock = true; triggerMove = true; OnTriggerSignal(); StartCoroutine(ResetMoveTrigger()); }

    protected IEnumerator ResetMoveTrigger()
    {
        yield return new WaitForEndOfFrame();
        triggerMove = false;
        eventID = 0;
        eventLock = false;
        yield break;
    }

    protected IEnumerator ExecuteEvents(LevelScript.LevelEvent[] events)
    {
        Cursor.visible = false;
        Debug.Log("Go through " + events.Length + " events");
        triggerID = 0;
        eventIDCount = 0;
        eventID = 0;
        eventLock = false;


        while(triggerID < events.Length)
        {
            //Warte darauf, dass das Event ausgelöst wird
            int _triggerID = triggerID;
            if (events[triggerID].trigger == LevelScript.EventTrigger.waitForTime)
                yield return new WaitForSeconds(events[triggerID].triggerVal);
            else if (events[triggerID].trigger == LevelScript.EventTrigger.waitForMoveTrigger)
            {
                triggerMove = false;
                triggerCount = 0;
                yield return new WaitUntil(() => (triggerMove && eventID == 0) || triggerID != _triggerID);
                //triggerMove = false;
                //eventLock = false;
            }
            else if(events[triggerID].trigger == LevelScript.EventTrigger.waitForTriggerCollect)
            {
                triggerCount = 0;
                yield return new WaitUntil(() => triggerCount >= events[triggerID].triggerVal || triggerID != _triggerID);
            }
            else if (events[_triggerID].trigger == LevelScript.EventTrigger.waitForUserInput)
            {
                yield return new WaitUntil(() => !(Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space)));
                yield return new WaitUntil(() => Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space));
            }



            triggerID = RunEvent(triggerID, events);
            yield return new WaitForFixedUpdate();
        }

        Cursor.visible = stateType == GameStateType.coreGame;
        yield break;
    }

    private int RunEvent(int _triggerID, LevelScript.LevelEvent[] events, int id = 0)
    {
        focus = GameObject.Find(events[_triggerID].focus);
        OV_NPC npc = focus ? focus.GetComponent<OV_NPC>() : null;
        if (npc) npc.eventID = id;

        //schließe vorhergehenden Dialog:
        if (_triggerID > 0)
            if (events[_triggerID].type != LevelScript.EventType.textBox && events[_triggerID - 1].type == LevelScript.EventType.textBox)
                ov_ui.closeTextFields();

        //events:
        switch (events[_triggerID].type)
        {
            case LevelScript.EventType.wait:
                break;

            case LevelScript.EventType.jumpEvent:
                _triggerID += (int)events[_triggerID].eventVal;
                break;

            case LevelScript.EventType.startParallelEvent:
                ++eventIDCount;
                StartCoroutine(ParallelEvent(_triggerID + 1, (int)events[_triggerID].eventVal, eventIDCount, events));
                _triggerID += (int)events[_triggerID].eventVal;
                break;

            case LevelScript.EventType.cameraPosRelativ:
                if (!focus) { focus = new GameObject(); focus.transform.position = (Vector2)Camera.main.transform.position + events[_triggerID].position; focus.name = "temp"; }
                Camera.main.GetComponent<CameraScript>().ChangeFocus(focus, (int)events[_triggerID].eventVal);
                break;

            case LevelScript.EventType.cameraPosAbsolut:
                if (!focus) { focus = new GameObject(); focus.transform.position = events[_triggerID].position; focus.name = "temp"; }
                Camera.main.GetComponent<CameraScript>().ChangeFocus(focus, (int)events[_triggerID].eventVal);
                break;

            case LevelScript.EventType.spawn:
                GameObject obj = Resources.Load<GameObject>("Prefabs/Chars/" + events[_triggerID].focus);
                string objName = obj.name;
                obj = Instantiate(obj, events[_triggerID].position, Quaternion.identity);
                obj.name = objName;

                obj.GetComponent<OV_NPC>().Set_Spawn((int)events[_triggerID].eventVal);
                obj.GetComponent<OV_NPC>().Set_Stand(events[_triggerID].direction);
                if (obj.CompareTag("Player")) OV_Player.block = true;
                break;

            case LevelScript.EventType.stand:
                npc.Set_Stand(events[_triggerID].direction);
                break;

            case LevelScript.EventType.jump:
                npc.Set_Jump();
                break;

            case LevelScript.EventType.sit:
                npc.Set_Sit(events[_triggerID].position, events[_triggerID].direction);
                break;

            case LevelScript.EventType.fall:
                npc.Set_Fall();
                break;

            case LevelScript.EventType.chat:
                npc.Set_Chat();
                break;

            case LevelScript.EventType.continousChat:
                npc.Set_ContinousChat();
                break;

            case LevelScript.EventType.move:
                npc.Set_Move(events[_triggerID].position);
                break;

            case LevelScript.EventType.run:
                npc.Set_Move(events[_triggerID].position, true);
                break;

            case LevelScript.EventType.stayInArea:
                npc.Set_StayInArea(events[_triggerID].eventVal);
                break;

            case LevelScript.EventType.textBox:
                ov_ui.Speak(events[_triggerID]);               
                break;

            case LevelScript.EventType.stopSequence:
                _triggerID = 99999999;
                break;

            case LevelScript.EventType.startHub:
                OV_Player.block = false;
                stateType = GameStateType.hub;
                break;

            case LevelScript.EventType.startCoreGame:
                stateType = GameStateType.coreGame;
                ov_ui.StartCoreGame((int)events[_triggerID].eventVal);
                break;

            case LevelScript.EventType.devInfo:
                DevInfoScript.devInfo.PlayDevInfo(events[_triggerID].focus);
                break;

            case LevelScript.EventType.setSequence:
                SequenceCount = (int)events[_triggerID].eventVal;
                break;

            case LevelScript.EventType.revealScene:
                ov_ui.RevealScene();
                break;

            case LevelScript.EventType.stopGame:
                Debug.Log("Game stopped");
                QuitGame();
                break;
        }
        return ++_triggerID;
    }

    protected IEnumerator ParallelEvent(int start, int length, int id, LevelScript.LevelEvent[] events)
    {
        int end = start + length;
        int _triggerID = start;
        while(_triggerID < end)
        {
            //Warte darauf, dass das Event ausgelöst wird
            if (events[_triggerID].trigger == LevelScript.EventTrigger.waitForTime)
                yield return new WaitForSeconds(events[_triggerID].triggerVal);
            else if (events[_triggerID].trigger == LevelScript.EventTrigger.waitForMoveTrigger)
            {

                triggerMove = false;
                yield return new WaitUntil(() => triggerMove && id == eventID);
                //triggerMove = false;
                //eventID = 0;
                //eventLock = false;
            }
            else if (events[_triggerID].trigger == LevelScript.EventTrigger.waitForTriggerCollect)
            {
                triggerCount = 0;//rausnehmen oder drinnen lassen?
                yield return new WaitUntil(() => triggerCount >= events[_triggerID].triggerVal);
            }
            else if (events[_triggerID].trigger == LevelScript.EventTrigger.waitForUserInput)
                yield return new WaitUntil(() => Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space));


            _triggerID = RunEvent(_triggerID, events, id);
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }

    private void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        Scene active = SceneManager.GetActiveScene();
        activeBuildIndex = active.buildIndex;
        //if(active.name[0] == 'o' && active.name[1] == 'v')
        ov_ui = GameObject.Find("Canvas").GetComponent<OV_UI>();
        SequenceCount = 0;
        Debug.Log(stateType);

        Cursor.visible = stateType == GameStateType.coreGame;

        StartExecutingEvents("Level");
    }

    public bool StartExecutingEvents(string scriptName)
    {
        LevelScript script = Resources.Load<LevelScript>("LevelScripts/lvl_" + activeBuildIndex.ToString() + "/" + scriptName);
        if (!script)
        {
            Debug.Log("Script" + scriptName + "wurde nicht gefunden.");
            return false;
        }
        StartCoroutine(ExecuteEvents(script.events));
        return true;
    }


    //Methoden, die abgerufen werden können:

    /// <summary>
    /// Beendet das Programm
    /// </summary>
    public static void QuitGame()
    {
        Application.Quit(0);
    }

    /// <summary>
    /// Lädt das Level komplett neu
    /// </summary>
    public static void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Lädt aktuellen Szenenindex + sceneDiff
    /// </summary>
    /// <param name="sceneDiff"></param>
    public static void LoadLevelRelative(int sceneDiff)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + sceneDiff);
    }

    public static void LoadLevel(int scene)
    {
        SceneManager.LoadScene(scene);
    }
}
