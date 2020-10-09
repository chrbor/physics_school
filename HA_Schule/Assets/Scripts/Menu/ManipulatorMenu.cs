using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManipulatorMenu : MonoBehaviour
{
    private float maxValue = 20;
    private float minValue = 2;

    public GameObject manipulatorButton;

    [HideInInspector]
    public GameObject focus;//focus, damit die Objekte manitpuliert werden können
    private Transform scrollContent;//das GameObject, das die Buttons beinhaltet
    private PlayerScript pScript;//Spieler, damit nach den Einheiten abgefragt werden kann
    private bool isChar;

    private List<Sprite> sprites = new List<Sprite>();
    private List<string> units = new List<string>();


    public bool isActive;
    public bool isSliding;
    public bool isAdding;
    private int numberOfUsableUnits;
    public int activeUnit;
    private string activeUnitName;
    private Transform activeUnitField;

    private GameObject scrollButton;
    private VerticalLayoutGroup verticalGroup;

    private MovementScript moveScript;
    GameObject arrow;
    private Vector2 arrowDir;
     /* zum einfügen einer weiteren unit:
     * 0. in MovementScript.Properties Einheit hinzufügen 
     * 1. unit.Add() bei Awake() hinzufügen
     * 2. Change_<Einheit> - funktion einfügen
     * 
     */



    void Awake()
    {
        pScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

        //Kommentiere hier ein oder -aus, um Einheiten hinzuzufügen/rauszunehmen:
        units.Add("acceleration");
        units.Add("mass");
        units.Add("velocity");
        //units.Add("work");
        //units.Add("current");
        units.Add("size");
        //units.Add("time");
        //units.Add("pressure");
        units.Add("kineticEnergy");
        units.Add("potentialEnergy");
        units.Add("puls");
        //units.Add("lumination");
        units.Add("force");
        //units.Add("temperature");

        //lade Sprites zum Darstellen der Einheiten
        sprites.Clear();
        units.ForEach(LoadSprite);
        //scrollContent = transform.Find("Scroll View").Find("Viewport").Find("Content");
        scrollContent = transform.Find("Slider");
        verticalGroup = scrollContent.GetComponent<VerticalLayoutGroup>();

        arrow = transform.Find("Arrow").gameObject;
    }

    /// <summary>
    /// Delegate- Funktion zum Laden der Sprites
    /// </summary>
    /// <param name="name">Name des Sprites, das geladen werden soll</param>
    private void LoadSprite(string name)
    {
        sprites.Add(Resources.Load<Sprite>("Sprites/Units/" + name));
    }

    /// <summary>
    /// Löscht alle Buttons, in denen Einstellungen des Objektes getätigt werden können.
    /// </summary>
    private void DeleteAllButtons()
    {
        for(int i = 0; i < scrollContent.childCount; i++)
            Destroy(scrollContent.GetChild(i).gameObject);
    }

    /// <summary>
    /// Löscht alle Buttons und erstellt das Menü neu
    /// </summary>
    public void CreateMenu(GameObject menuFocus)
    {
        if (GameManager.block) return;
        GameManager.block = true;

        gameObject.SetActive(true);
        DeleteAllButtons();
        arrow.SetActive(false);

        moveScript = null;
        if(isChar = menuFocus.CompareTag("Player") || menuFocus.CompareTag("Ally"))
        {
            focus = GameObject.FindGameObjectWithTag("Player");
            moveScript = focus.GetComponent<GroupScript>();
            if (menuFocus.CompareTag("Ally"))
                if (!menuFocus.GetComponent<GroupScript>().inGroup) moveScript = null;
                else
                    foreach(GameObject unit in PlayerScript.allies)
                    {
                        unit.GetComponent<SpriteRenderer>().sortingLayerName = "Forground";
                        unit.GetComponent<SpriteRenderer>().sortingOrder = 20;
                        unit.GetComponent<GroupScript>().SetEyeOrder("Forground", 21);
                    }
        }
        else
        {
            focus = menuFocus;
            if(moveScript = focus.GetComponent<MovementScript>())
            {
                focus.GetComponent<SpriteRenderer>().sortingLayerName = "Forground";
                focus.GetComponent<SpriteRenderer>().sortingOrder = 20;
            }
        }

        if (moveScript == null)
        {
            Debug.Log("Fehler: Objekt hat kein ObjectScript");
            ExitMenu(true);
            return;
        }

        Camera.main.GetComponent<CameraScript>().ChangeFocus(focus);

        //Fülle das Menü (Entscheidung, ob Eignenschaft dabei ist wird in AddButton getroffen):
        numberOfUsableUnits = 0;
        sprites.ForEach(AddButton);
        if (numberOfUsableUnits == 0) { Debug.Log("Keine nutzbaren Einheiten gefunden!"); ExitMenu(); return; }

        Physics2D.autoSimulation = false;
        StartCoroutine(UpdateSlider());
    }


    /// <summary>
    /// Verlässt das Manipulatormenü und startet das Spiel
    /// </summary>
    public void ExitMenu(bool withoutMovescript = false)
    {
        foreach (GameObject unit in PlayerScript.allies)
        {
            unit.GetComponent<SpriteRenderer>().sortingLayerName = "physObject";
            unit.GetComponent<SpriteRenderer>().sortingOrder = 0;
            unit.GetComponent<GroupScript>().SetEyeOrder("physObject", 1);
        }

        focus.GetComponent<SpriteRenderer>().sortingLayerName = "physObject";
        focus.GetComponent<SpriteRenderer>().sortingOrder = 0;

        DeleteAllButtons();
        isActive = false;
        isChar = false;
        Camera.main.GetComponent<CameraScript>().ChangeFocus(GameObject.FindGameObjectWithTag("Player"), steps: 10);
        Camera.main.GetComponent<ManipulatorScript>().RemoveBlock();
        if(!withoutMovescript) moveScript.preVel = Vector2.one;
        Physics2D.autoSimulation = true;
        gameObject.SetActive(false);
        GameManager.block = false;
    }
    
    /// <summary>
    /// Fügt ein Feld in scrollContent mit dem Sprite unit ein. Der Wert wird automatisch vom Objekt entnommen.
    /// </summary>
    /// <param name="unit">Sprite, das die Einheit darstellt.</param>
    private void AddButton(Sprite unit)
    {
        GameObject newButton;

        //Reflection: wird die Einheit genutzt?
        Debug.Log("using_" + unit.name);
        Debug.Log((bool)moveScript.properties.GetType().GetField("using_" + unit.name).GetValue(moveScript.properties));
        if (!((bool)moveScript.properties.GetType().GetField("using_" + unit.name).GetValue(moveScript.properties) && PlayerScript.alliesNames.Contains(unit.name)))
            return;

        newButton = Instantiate(manipulatorButton, scrollContent);
        newButton.transform.SetSiblingIndex(numberOfUsableUnits);
        newButton.transform.Find("Unit").GetComponent<Image>().sprite = unit;

        string txt = "";
        switch (unit.name)
        {
            case "force":
                txt = new Vector2(moveScript.properties.force_x, moveScript.properties.force_y).magnitude.ToString();
                break;
            case "puls":
                txt = (Mathf.Rad2Deg * Mathf.Atan2(moveScript.rb.velocity.y, moveScript.rb.velocity.x)).ToString();
                break;
            case "kineticEnergy":
                float temp = Mathf.RoundToInt(moveScript.rb.velocity.magnitude);
                txt = (temp > maxValue ? maxValue : temp).ToString();
                break;
            case "potentialEnergy":
                txt = moveScript.GetHeight().ToString();
                break;
            default:
                txt = ((float)moveScript.properties.GetType().GetField(unit.name).GetValue(moveScript.properties)).ToString();
                break;
        }

        string[] txt_split = txt.Split('.');
        txt = txt_split.Length > 1 ? txt_split[0] + "." + txt_split[1][0] : txt_split[0];//umständlicher gings leider nicht :P
        newButton.transform.Find("Value").GetComponent<Text>().text = txt;

        newButton.transform.Find("Unit").GetComponent<Image>().color = new Color(1, 1, 1, 1 - numberOfUsableUnits * 0.4f);
        newButton.transform.Find("Value").GetComponent<Text>().color = new Color(0, 0, 0, 1 - numberOfUsableUnits * 0.4f);

        numberOfUsableUnits++;
    }

    protected IEnumerator UpdateSlider()
    {
        isActive = true;
        isSliding = false;
        activeUnit = 0;
        activeUnitField = scrollContent.GetChild(activeUnit);
        activeUnitName = activeUnitField.Find("Unit").GetComponent<Image>().sprite.name;
        verticalGroup.padding.top = -25; //75 * numberOfUsableUnits;

        SetArrow();

        while (isActive)
        {
            if (isSliding) { yield return new WaitForFixedUpdate(); continue; }

            if (Input.GetKey(KeyCode.W) && activeUnit > 0)                      StartCoroutine(Sliding(75));
            if (Input.GetKey(KeyCode.S) && activeUnit < numberOfUsableUnits-1)  StartCoroutine(Sliding(-75));
            if (Input.GetKey(KeyCode.D)) { Plus(); yield return new WaitForSeconds(0.3f); }
            if (Input.GetKey(KeyCode.A)) { Minus(); yield return new WaitForSeconds(0.3f);}

            yield return new WaitForFixedUpdate();
        }
        yield break;
    }

    IEnumerator SettingArrow()
    {
        Vector2 diff;
        while (activeUnitName == "force" || activeUnitName == "puls" || activeUnitName == "kineticEnergy")
        {
            diff = Input.mousePosition - new Vector3(Screen.width, Screen.height) / 2;
            if(Input.GetMouseButton(0) && diff.magnitude < 200)
            {
                arrow.transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg);
                arrowDir = diff.normalized;
                switch (activeUnitName)
                {
                    case "force":
                        float factor = new Vector2(moveScript.properties.force_x, moveScript.properties.force_y).magnitude;
                        moveScript.properties.force_x = arrowDir.x * factor;
                        moveScript.properties.force_y = arrowDir.y * factor;
                        break;
                    case "puls":
                        moveScript.rb.velocity = arrowDir * moveScript.rb.velocity.magnitude;
                        break;
                    case "kineticEnergy":
                        moveScript.rb.velocity = arrowDir * moveScript.rb.velocity.magnitude;
                        break;
                }
            }
            yield return new WaitForEndOfFrame();
        }

        yield break;
    }

    //Navigation:
    public void AddOne()
    {
        if (activeUnit > 0 && !isSliding)
            StartCoroutine(Sliding(75));
    }

    public void SubstractOne()
    {
        if (activeUnit < numberOfUsableUnits-1 && !isSliding)
            StartCoroutine(Sliding(-75));
    }

    IEnumerator Sliding(float relPos, int vel = 20)
    {
        float step = relPos / vel;
        float fChanged = 0;
        int pos = verticalGroup.padding.top;

        isSliding = true;
        for(int i = 0; i < vel; i++)
        {
            fChanged += step; 
            verticalGroup.padding.top = pos + (int)fChanged;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollContent.transform);

            float currentPos = (verticalGroup.padding.top + 25f) / 75;
            Color whiteClear = Color.white - Color.black;
            Color resColor;
            for(int j = 0; j < numberOfUsableUnits; j++)
            {
                if (Mathf.Abs(j + currentPos) > 3)
                    resColor = Color.clear;
                else
                    resColor = whiteClear + Color.black * (1 - Mathf.Abs(j + currentPos) * 0.4f);
                
                scrollContent.GetChild(j).Find("Unit").GetComponent<Image>().color = resColor;
                scrollContent.GetChild(j).Find("Value").GetComponent<Text>().color = Color.black * (1 - Mathf.Abs(j + currentPos) * 0.4f);
            }

            yield return new WaitForEndOfFrame();
        }
        activeUnit -= (int)Mathf.Sign(relPos);
        activeUnitField = scrollContent.GetChild(activeUnit);
        activeUnitName = activeUnitField.Find("Unit").GetComponent<Image>().sprite.name;
        SetArrow();

        Debug.Log(activeUnitName);

        isSliding = false;
        yield break;
    }

    private void SetArrow()
    {
        switch (activeUnitName)
        {
            case "force":
                arrow.SetActive(!moveScript.changeForce(0));
                arrow.transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(moveScript.properties.force_y, moveScript.properties.force_x));
                StartCoroutine(SettingArrow());
                break;
            case "puls":
                arrow.SetActive(true);
                arrow.transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(moveScript.rb.velocity.y, moveScript.rb.velocity.x));
                StartCoroutine(SettingArrow());
                break;
            case "kineticEnergy":
                arrow.SetActive(true);
                arrow.transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(moveScript.rb.velocity.y, moveScript.rb.velocity.x));
                StartCoroutine(SettingArrow());
                break;
            default: arrow.SetActive(false);Debug.Log(activeUnitName); break;
        }
    }

    /// <summary>
    /// Methode um aktive Einheit um +0.1 zu ändern. Der Einstellbereich liegt bei [1, 0.1]
    /// </summary>
    public void Plus()
    {
        if (isAdding) return;
        float step = 0.1f;

        //Teste Schranken:
        float curVal = 0;
        switch (activeUnitName)
        {
            case "force": curVal = new Vector2(moveScript.properties.force_x, moveScript.properties.force_y).magnitude * 4.5f + 2; break;
            case "puls": curVal = maxValue - 1; break;
            case "kineticEnergy": curVal = moveScript.rb.velocity.magnitude + 2; break;
            case "potentialEnergy": curVal = moveScript.GetHeight() + 2; break;
            case "velocity": curVal = (moveScript.properties.velocity + 6) * 2; break;
            default: curVal = Mathf.RoundToInt(10 * ((float)moveScript.properties.GetType().GetField(activeUnitName).GetValue(moveScript.properties) + step)); break;
        }
        //curVal = Mathf.RoundToInt(10 * (curVal + step));
        if (curVal > maxValue) return;

        //Rufe durch Reflektion die Funktion auf:
        var unit = GetType().GetMethod("Change_" + activeUnitName).Invoke(this, new object[] { step });

        //Ändere Wert im Anzeigefeld:
        string txt = Mathf.Round((float)unit * 10).ToString();
        txt = txt.Length > 1 ? txt[0] + "." + txt[1] : "0." + txt;//umständlicher gings leider nicht :P
        activeUnitField.Find("Value").GetComponent<Text>().text = txt;
    }

    /// <summary>
    /// Methode, um aktive Einheit um -0.1 zu ändern. Der Einstellbereich liegt bei [1, 0.1]
    /// </summary>
    public void Minus()
    {
        if (isAdding) return;
        float step = -0.1f;

        //Teste Schranken:
        float curVal = 0;
        switch (activeUnitName)
        {
            case "force": curVal = new Vector2(moveScript.properties.force_x, moveScript.properties.force_y).magnitude * 4.5f + 2; break;
            case "puls": curVal = maxValue - 1; break;
            case "kineticEnergy": curVal = moveScript.rb.velocity.magnitude + 2; break;
            case "potentialEnergy": curVal = moveScript.GetHeight() + 2; Debug.Log("potentialEnergy:" + curVal); break;
            case "velocity": curVal = (moveScript.properties.velocity + 6)*2; break;
            default: curVal = Mathf.RoundToInt(10 * ((float)moveScript.properties.GetType().GetField(activeUnitName).GetValue(moveScript.properties) + step)); break;
        }
        //curVal = Mathf.RoundToInt(10 * (curVal + step));
        if (curVal < minValue) return;

        //Call through Reflection:
        var unit = GetType().GetMethod("Change_" + activeUnitName).Invoke(this, new object[] { step });
        //Ändere Wert im Anzeigefeld:
        string txt = Mathf.Round((float)unit * 10).ToString();
        txt = txt.Length > 1 ? txt[0] + "." + txt[1] : "0." + txt;//umständlicher gings leider nicht :P
        activeUnitField.Find("Value").GetComponent<Text>().text = txt;
    }



    //Nachfolgend sind die Funktionen eingefügt, mit denen die Objekte manipuliert werden können:

    /// <summary>
    /// Ändert die x- und y- Größe des Objektes um change. x- und y- Größe sind gekoppelt.
    /// </summary>
    public float Change_size(float change)
    {
        if (moveScript.changeSize(change)) return moveScript.properties.size;

        if (isChar)
        {
            //focus.GetComponent<BoxCollider2D>().enabled = false;
            focus.transform.position += Vector3.up * 0.3f;

            foreach(GameObject unit in PlayerScript.allies)
            {
                unit.GetComponent<BoxCollider2D>().enabled = false;
                unit.GetComponent<CapsuleCollider2D>().enabled = false;
                unit.transform.localScale = Vector2.one * (moveScript.properties.size + change);
                unit.transform.position += Vector3.up * change;
            }
            focus.GetComponent<CapsuleCollider2D>().enabled = true;
        }
        else
        {
            focus.transform.position += Vector3.up * 0.3f;

            focus.transform.localScale = Vector2.one * (moveScript.properties.size + change);
            focus.transform.position += Vector3.up * change / 2;
        }

        //teste, ob sich der Collider mit anderen collidern überschneidet:
        bool contact;
        List<Collider2D> cols = new List<Collider2D>();
        if (isChar) contact = focus.GetComponent<CapsuleCollider2D>().OverlapCollider(new ContactFilter2D(), cols) > 0;
        else contact = focus.GetComponent<Collider2D>().OverlapCollider(new ContactFilter2D(), new List<Collider2D>()) > 0;
        if (contact)
        {
            foreach(Collider2D col in cols)
                Debug.Log("Kollision mit: " + col.name);

            if (isChar)
            {
                foreach (GameObject unit in PlayerScript.allies)
                {
                    unit.GetComponent<MovementScript>().PulsColor(new Color(1, 0.75f, 0.75f));
                    unit.transform.localScale = Vector2.one * moveScript.properties.size;
                    unit.transform.position -= Vector3.up * change;
                }
            }
            else
            {
                focus.GetComponent<MovementScript>().PulsColor(new Color(1, 0.75f, 0.75f));
                focus.transform.localScale = Vector2.one * moveScript.properties.size;
                focus.transform.position -= Vector3.up * change / 2;
            }

        }

        if (isChar)
        {
            //focus.GetComponent<BoxCollider2D>().enabled = true;
            focus.transform.position -= Vector3.up * 0.3f;

            foreach (GameObject unit in PlayerScript.allies)
            {
                unit.GetComponent<BoxCollider2D>().enabled = true;
                unit.GetComponent<CapsuleCollider2D>().enabled = true;
                unit.GetComponent<GroupScript>().properties.size = focus.transform.localScale.x;
            }
        }
        else
        {
            focus.transform.position -= Vector3.up * 0.3f;
            moveScript.properties.size = focus.transform.localScale.x;
        }

        return moveScript.properties.size;
    }

    /// <summary>
    /// Ändert die Masse des Objektes um change.
    /// </summary>
    public float Change_mass(float change)
    {
        moveScript.rb.mass += change;
        moveScript.properties.mass = moveScript.rb.mass;
        if(isChar)
            foreach (var unit in PlayerScript.allies)
            {
                unit.GetComponent<GroupScript>().properties.mass = moveScript.properties.mass;
                unit.GetComponent<Rigidbody2D>().mass = moveScript.properties.mass;
            }
        return moveScript.properties.mass;
    }

    /// <summary>
    /// Ändert die x- und y- Größe des Objektes um change. x- und y- Größe sind gekoppelt.
    /// </summary>
    public float Change_velocity(float change)
    {
        //focus.GetComponent<Rigidbody2D>().velocity += new Vector2(change, change);
        moveScript.properties.velocity += change * 5f;
        if(isChar)
            foreach (var unit in PlayerScript.allies)
                unit.GetComponent<GroupScript>().properties.velocity = moveScript.properties.velocity;
        return moveScript.properties.velocity;
    }

    /// <summary>
    /// Ändert die x- und y- Größe des Objektes um change. x- und y- Größe sind gekoppelt.
    /// </summary>
    public float Change_acceleration(float change)
    {
        moveScript.properties.acceleration += change;
        foreach (var unit in PlayerScript.allies)
            unit.GetComponent<GroupScript>().properties.acceleration = moveScript.properties.acceleration;
        return moveScript.properties.acceleration;
    }

    /// <summary>
    /// Ändert die x- und y- Größe des Objektes um change. x- und y- Größe sind gekoppelt.
    /// </summary>
    public float Change_force(float change)
    {
        moveScript.properties.force_x += arrowDir.x * change * 4f;
        moveScript.properties.force_y += arrowDir.y * change * 4f;
        return new Vector2(moveScript.properties.force_x, moveScript.properties.force_y).magnitude;

    }

    /// <summary>
    /// Ändert die Geschwindigkeitsrichtung
    /// </summary>
    public float Change_puls(float change)
    {
        float dir = Mathf.Atan2(arrowDir.y, arrowDir.x) * Mathf.Rad2Deg + change * 18;
        arrow.transform.eulerAngles = new Vector3(0, 0, dir);
        moveScript.rb.velocity = MovementScript.RotToVec(dir) * moveScript.rb.velocity.magnitude;

        return dir;
    }
    
    /// <summary>
    /// Setzt die Geschwindigkeit vom Objekt
    /// </summary>
    public float Change_kineticEnergy(float change)
    {
        moveScript.rb.velocity += arrowDir * change * 7.5f;
        return moveScript.rb.velocity.magnitude;
    }

    /// <summary>
    /// Ändert die Höhe des Objektes
    /// </summary>
    /// <param name="change"></param>
    /// <returns></returns>
    public float Change_potentialEnergy(float change)
    {
        Debug.Log("change pot energy");
        focus.transform.position += Vector3.up * change * 5;

        List<Collider2D> cols = new List<Collider2D>();
        focus.GetComponent<Collider2D>().OverlapCollider(new ContactFilter2D(), cols);
        if (cols.Count > 0)
        {
            moveScript.rb.position -= Vector2.up * change;

            if (isChar)
            {
                foreach (GameObject unit in PlayerScript.allies)
                {
                    unit.GetComponent<MovementScript>().PulsColor(new Color(1, 0.75f, 0.75f));
                    unit.transform.localScale = Vector2.one * moveScript.properties.size;
                    unit.transform.position -= Vector3.up * change * 5;
                }
            }
            else
            {
                focus.GetComponent<MovementScript>().PulsColor(new Color(1, 0.75f, 0.75f));
                focus.transform.localScale = Vector2.one * moveScript.properties.size;
                focus.transform.position -= Vector3.up * change * 5;
            }
        }
        return moveScript.GetHeight();
    }
}
