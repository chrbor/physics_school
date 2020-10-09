using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Dieses Script
/// </summary>
public class CheckpointScript : MonoBehaviour
{
    public static CheckpointScript current;
    private SchummelExScript sScript;

    [Header("Nur Points teilbar durch 5 einsetzen!")]
    public int id_number;
    public int points;
    public bool startPoint = false;
    [Header("Objekte, die beim Checkpoint zurückgesetzt werden:")]
    public List<GameObject> areaObjects;

    [System.Serializable]
    public class OnEvent : UnityEvent { };
    [Header("Aktion, die bei Erreichen des Checkpoints ausgelöst wird:")]
    public OnEvent CheckpointReached;


    public enum State { ready, current, reached}
    [HideInInspector]
    public State state = State.ready;
    [Header("List of sprites (dont change):")]
    public Sprite[] id;
    public Sprite[] div;


    public class ObjectState
    {
        public string name;
        public MovementScript.Properties properties;
        public Vector2 position;
        public Vector2 scale;
        public Quaternion rotation;
        public float mass;

        //Constructor:
        public ObjectState(GameObject obj)
        {
            properties = DefaultStats.DeepClone(obj.GetComponent<ObjectScript>().properties);

            name = obj.GetComponent<SpriteRenderer>().sprite.name;
            position = new Vector2(obj.transform.position.x, obj.transform.position.y);
            rotation = obj.transform.rotation;
            scale = obj.transform.localScale;
            mass = obj.GetComponent<Rigidbody2D>().mass;
        }
    }
    private List<ObjectState> objectStates = new List<ObjectState>();
    private GameMenu menu;
    
    //private SpriteRenderer sprite_ID;
    private Animator anim_num1, anim_num2, anim_num3, anim_tick;

    private bool block;

    private void Start()
    {
        sScript = GameObject.Find("CoreManager").GetComponent<SchummelExScript>();

        //erstelle Liste von ObjectStates:
        foreach (GameObject obj in areaObjects) objectStates.Add(new ObjectState(obj));

        if (startPoint) { state = State.reached; current = this; return; }

        //sende Daten an GameMenu:
        menu = GameObject.Find("Canvas").transform.Find("GameMenu").GetComponent<GameMenu>();
        menu.InsertExercise(id_number, points);

        state = State.ready;
        anim_num1 = transform.GetChild(2).GetComponent<Animator>();
        anim_num2 = transform.GetChild(3).GetComponent<Animator>();
        anim_num3 = transform.GetChild(4).GetComponent<Animator>();
        anim_tick = transform.GetChild(5).GetComponent<Animator>();

        //Sprite id und points muss vorher bestimmt werden
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = id[id_number];
        transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = div[points <= 20 ? points/5 -1 : 3 + points / 50];

        //Setze GameObject Number_1, Number_2 und Number_3:
        string sPoints = points.ToString();

        if (sPoints.Length == 1)
        {
            anim_num1.SetInteger("number", (int)char.GetNumericValue(sPoints[0]));
            anim_num2.gameObject.SetActive(false);
            anim_num3.gameObject.SetActive(false);
        }
        else if (sPoints.Length == 2)
        {
            anim_num1.SetInteger("number", (int)char.GetNumericValue(sPoints[1]));
            anim_num2.SetInteger("number", (int)char.GetNumericValue(sPoints[0]));
            anim_num3.gameObject.SetActive(false);
        }
        else
        {
            anim_num1.SetInteger("number", (int)char.GetNumericValue(sPoints[2]));
            anim_num2.SetInteger("number", (int)char.GetNumericValue(sPoints[1]));
            anim_num3.SetInteger("number", (int)char.GetNumericValue(sPoints[0]));
        }
    }

    /// <summary>
    /// Aktiviert den Checkpoint
    /// </summary>
    public void SetCheckpoint()
    {
        GetComponent<BoxCollider2D>().enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (state == State.ready)
        {
            if(current) current.ChangeToReached();
            current = this;
            state = State.current;
            PlayerScript.collectedAllies.Clear();
            foreach (string unitName in PlayerScript.collectedAlliesCurrent)
                PlayerScript.collectedAllies.Add(unitName);
            CheckpointReached.Invoke();
            StartCoroutine(ChangeCheckpoint());
            menu.UpdateExercise(id_number);
        }
    }

    IEnumerator ChangeCheckpoint()
    {
        anim_tick.SetTrigger("tick");
        yield return new WaitForSeconds(0.5f);
        anim_num3.SetTrigger("draw");
        yield return new WaitForSeconds(0.5f);
        anim_num2.SetTrigger("draw");
        yield return new WaitForSeconds(0.5f);
        anim_num1.SetTrigger("draw");
        yield break;
    }

    public void StartSchummeln(Vector2 playerPos)
    {
        sScript.SetSchummelScript("lvl_" + GameManager.activeBuildIndex + "/checkpoint_" + (id_number+1).ToString());
        sScript.StartSchummeln(playerPos, transform.position);
    }

    public void ChangeToReached()
    {
        state = State.reached;
        if (startPoint) return;
        anim_tick.GetComponent<SpriteRenderer>().color = Color.black;
    }

    public void ReloadCheckpoint()
    {
        if (block) return;
        block = true;
        foreach (GameObject ally in PlayerScript.allies) Debug.Log("entering: " + ally.name);
        StartCoroutine(ReloadingCheckpoint());
    }

    public IEnumerator ReloadingCheckpoint()
    {
        GameObject obj;
        CameraScript cScript = Camera.main.GetComponent<CameraScript>();
        GroupScript gScript;

        //Setze die Position der Objekte im Spielabschnitt zurück:
        int i = 0;
        foreach (GameObject areaObj in areaObjects)
        {
            //falls zerstört, dann spawne das Objekt neu:
            obj = areaObj ? areaObj : Instantiate(Resources.Load<GameObject>("Prefabs/Objects/" + objectStates[i].name));
            obj.transform.position = objectStates[i].position;
            obj.transform.rotation = objectStates[i].rotation;
            obj.transform.localScale = objectStates[i].scale;
            obj.GetComponent<ObjectScript>().properties = objectStates[i].properties;
            obj.GetComponent<ObjectScript>().preVel = Vector2.up;
            obj.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            if(objectStates[i].properties.using_mass) obj.GetComponent<Rigidbody2D>().mass = objectStates[i].mass;
            i++;
        }

        //foreach (string u in PlayerScript.collectedAllies) Debug.Log(u);

        //spawne neu dazugekommene Einheiten an ihren Plätzen neu:
        int countAllies = PlayerScript.collectedAllies.Count;
        for (int j = countAllies; j < PlayerScript.collectedAlliesCurrent.Count; j++)
        {
            Instantiate(Resources.Load<GameObject>("Units/" + PlayerScript.collectedAlliesCurrent[j]), PlayerScript.spawnPositions[countAllies], Quaternion.identity);
            PlayerScript.spawnPositions.RemoveAt(countAllies);
        }


        //lösche Gruppe und ersetze sie durch die eingesammelten Einheiten:
        foreach (GameObject unit in PlayerScript.allies) { Destroy(unit); }
        PlayerScript.allies.Clear();
        PlayerScript.alliesNames.Clear();
        PlayerScript.collectedAlliesCurrent.Clear();

        i = 0;
        foreach (string unitName in PlayerScript.collectedAllies)//laufe alle einheiten am anfang dieses Checkpoints durch und spawne sie neu
        {
            //Spawn:
            obj = Resources.Load<GameObject>("Units/" + unitName);
            obj = Instantiate(obj, transform.position, transform.rotation);
            gScript = obj.GetComponent<GroupScript>();
            gScript.groupNumber = i;
            gScript.inGroup = true;

            PlayerScript.allies.Add(obj);
            PlayerScript.alliesNames.Add(obj.name);
            PlayerScript.collectedAlliesCurrent.Add(obj.name);

            if (i == 0)
            {
                obj.tag = "Player";
                obj.GetComponent<PlayerScript>().enabled = true;
                obj.GetComponent<GroupScript>().enabled = false;
                Camera.main.GetComponent<CameraScript>().ChangeFocus(obj,1);
            }
            else
            {
                gScript.charToFollow = PlayerScript.allies[i - 1];
            }

            i++;
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitWhile(()=>cScript.block);
        block = false;
        yield break;
    }
}
