using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterMenu : MonoBehaviour
{
    private bool block;
    private Vector2 diff;
    private Animator anim;
    private PlayerScript pScript;

    //backup-Daten:
    private List<string> backupNames;
    public static List<Vector2> startPos;
    public static List<float> startAngle;
    static MovementScript.Properties startStats;

    private List<GameObject> divident = new List<GameObject>();
    private List<GameObject> divisor = new List<GameObject>();

    private int activeField;//Feld innerhalb der Formel, die vom Spieler aufgestellt wird
    private Vector2 plane;//aktive Ebene und Anzahl der Ebenen
    private char[] sep = new char[]
    {
        '_',
    };

    private GameObject sepLine;

    private GameObject gegeben;
    private GameObject selection;
    private GameObject divisionLine;
    private GameObject oneDivident;
    private GameObject oneDivisor;
    private GameObject wrong;
    public GameObject factorSign;

    //Buttons:
    private GameObject equalSign;
    private GameObject divideButton;
    private GameObject resetButton;
    private GameObject backButton;
    private GameObject formulaButton;
    private GameObject unitButton;
    private GameObject calcButton;

    //InfoFields:
    private GameObject instructions;
    private GameObject formulaList;
    private GameObject unitList;

    //Zusätzliche Einheit die immer verfügbar ist:
    public GameObject _2_prefab;
    private GameObject _2;

    public void CreateMenu()
    {
        if (block) return;
        block = true;

        Physics2D.autoSimulation = false;

        backupNames = new List<string>(PlayerScript.alliesNames);
        startPos = new List<Vector2>(); startAngle = new List<float>();
        foreach (GameObject unit in PlayerScript.allies) { startPos.Add(unit.transform.position); startAngle.Add(unit.transform.eulerAngles.z); }

        if (GameObject.FindGameObjectWithTag("Player"))
        {
            pScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
            pScript.transform.GetChild(2).gameObject.SetActive(false);
            pScript.tag = "Ally";
        }
        else Debug.Log("Warnung: Player- Tag nicht gefunden. StartStats können abweichen");

        startStats = pScript.gScript.properties;

        GameManager.block = true;
        anim = GetComponent<Animator>();
        Vector2 diff = -Vector2.one * Camera.main.orthographicSize - Vector2.up * 3;
        transform.position = (Vector2)Camera.main.transform.position + diff;
        gameObject.SetActive(true);

        sepLine = transform.Find("SeparationLine").gameObject;
        equalSign = transform.Find("EqualSign").gameObject;
        divideButton = transform.Find("DivideButton").gameObject;
        resetButton = transform.Find("ResetButton").gameObject;
        backButton = transform.Find("BackButton").gameObject;
        gegeben = transform.Find("Gegeben").gameObject;
        selection = transform.Find("Selection").gameObject;
        divisionLine = transform.Find("DivisionLine").gameObject;
        oneDivident = transform.Find("OneDivident").gameObject;
        oneDivisor = transform.Find("OneDivisor").gameObject;
        wrong = transform.Find("Wrong").gameObject;
        instructions = transform.Find("Instructions").gameObject;
        formulaButton = transform.Find("FormulaButton").gameObject;
        unitButton = transform.Find("UnitButton").gameObject;
        calcButton = transform.Find("CalcButton").gameObject;
        formulaList = transform.Find("FormulaList").gameObject;
        unitList = transform.Find("UnitList").gameObject;
        SetFormulaUI(false);
        formulaButton.SetActive(true);
        unitButton.SetActive(true);
        calcButton.SetActive(true);

        StartCoroutine(MenuCreation(diff: diff));
    }


    IEnumerator MenuCreation(Vector2 diff, int steps = 50)
    {
        //Move Formula into View:
        Vector2 endPos = (Vector2)transform.position - diff;
        endPos += new Vector2(1 * Camera.main.aspect, -1) * Camera.main.orthographicSize + new Vector2(-5f, 2.5f);

        Vector2 a = diff / (steps * steps);

        for (int i = 0; i <= steps; i++)
        {
            transform.position = endPos + a * (i - steps) * (i - steps);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.5f);
        anim.SetTrigger("open");

        //JumpIn- Animation:
        pScript.JumpIntoBook(transform);
        yield return new WaitForSeconds(0.75f);
        SetFormulaUI(true);
        selection.SetActive(false);
        wrong.SetActive(false);
        formulaList.SetActive(false);
        unitList.SetActive(false);
        calcButton.SetActive(false);

        //Spawn _2
        _2 = Instantiate(_2_prefab, transform);
        _2.name = "_2";
        _2.transform.localPosition = new Vector3(-4.25f, 4);
        _2.transform.parent = null;
        _2.transform.rotation = Quaternion.identity;
        yield break;
    }

    /// <summary>
    /// Schließt das Menü, lässt die Einheiten ins Spiel zurückspringen und lässt das Spiel fortfahren.
    /// </summary>
    public void ExitMenu()
    {
        Debug.Log("exiting charmenu");
        StartCoroutine(ExitingMenu());
    }

    IEnumerator ExitingMenu(int steps = 50)
    {
        //hole alle Einheiten, die noch in der Formel stecken in die Gruppe rein:
        ResetEquation(false);

        anim.SetTrigger("close");
        SetFormulaUI(false);
        Destroy(_2);

        pScript.JumpOutOfBook();
        yield return new WaitForSeconds(1);

        Vector2 diff = (Vector2)transform.position + new Vector2(-20, -20);

        Vector3 c = diff * 3 / (2 * steps);
        Vector3 a = c / (steps * steps);

        for (int i = 0; i <= steps; i++)
        {
            transform.position += a * (i - steps) * (i - steps) + c;
            yield return new WaitForEndOfFrame();
        }

        PlayerScript.allies[0].transform.GetChild(2).gameObject.SetActive(true);
        PlayerScript.allies[0].GetComponent<GroupScript>().rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        //GameManager.block = false;
        Camera.main.GetComponent<ManipulatorScript>().RemoveBlock();
        Destroy(gameObject);
        yield break;
    }

    private void SetFormulaUI(bool isActive)
    {
        sepLine.SetActive(isActive);
        equalSign.SetActive(isActive);
        divideButton.SetActive(isActive);
        resetButton.SetActive(isActive);
        backButton.SetActive(isActive);
        gegeben.SetActive(isActive);
        selection.SetActive(isActive);
        divisionLine.SetActive(isActive);
        oneDivident.SetActive(isActive);
        oneDivisor.SetActive(isActive);
        wrong.SetActive(isActive);
        instructions.SetActive(isActive);
        formulaList.SetActive(isActive);
        unitList.SetActive(isActive);
        formulaButton.SetActive(isActive);
        unitButton.SetActive(isActive);
        calcButton.SetActive(isActive);

        Transform obj;
        for (int i = 1; i < divident.Count; i++)
            if (obj = transform.Find("u_" + i.ToString())) Destroy(obj.gameObject);
            else break;
        for (int i = 1; i < divisor.Count; i++)
            if (obj = transform.Find("l_" + i.ToString())) Destroy(obj.gameObject);
            else break;
    }

    private void SetBlockInteractables(bool block)
    {
        equalSign.GetComponent<FormulaButton>().SetBlock(block);
        divideButton.GetComponent<FormulaButton>().SetBlock(block);
        resetButton.GetComponent<FormulaButton>().SetBlock(block);
        backButton.GetComponent<FormulaButton>().SetBlock(block);
        formulaButton.GetComponent<FormulaButton>().SetBlock(block);
        unitButton.GetComponent<FormulaButton>().SetBlock(block);
        calcButton.GetComponent<FormulaButton>().SetBlock(block);
    }

    /// <summary>
    /// Löscht die Gleichung
    /// </summary>
    public void ResetEquation(bool jumpBackAnim = true)
    {
        List<GameObject> dividentCopy = new List<GameObject>(divident);//gefährliche Angelegenheit, gelöschte Referenzen werden jedoch nicht mehr genutzt
        List<GameObject> divisorCopy = new List<GameObject>(divisor);

        foreach (GameObject obj in dividentCopy)
            SubstractUnit(obj, jumpBackAnim);
        foreach (GameObject obj in divisorCopy)
            SubstractUnit(obj, jumpBackAnim);
    }

    /// <summary>
    /// Geht in das Stadium zurück, in dem das Menü geöffnet wurde
    /// </summary>
    public void ResetAll()
    {
        StartCoroutine(ResetingAll());
    }

    IEnumerator ResetingAll()
    {
        Debug.Log("Backup");
        foreach (string n in backupNames) Debug.Log(n);
        //vernichte alles:
        StartCoroutine(FadeTermUnits(0.2f));

        int steps = 30;
        Color stepColor = new Color(0, 0, 0, 1f / steps);
        Color realColor = Color.white;
        List<GameObject> objects = new List<GameObject>(PlayerScript.allies);
        for (int i = 0; i < steps; i++)
        {
            realColor -= stepColor;
            foreach (GameObject obj in objects) obj.GetComponent<GroupScript>().SetColor(realColor);
            yield return new WaitForEndOfFrame();
        }

        foreach (GameObject obj in objects) Destroy(obj);

        yield return new WaitForSeconds(0.5f);
        PlayerScript.allies.Clear();
        PlayerScript.alliesNames.Clear();

        //erstelle neu:
        int count = 0;
        foreach(string unitName in backupNames)
        {
            GameObject obj = SpawnUnit(unitName, new Vector2(-4.5f + 0.75f * (count % 5), 2.75f - 1f * (count / 5))/0.4f, false);
            obj.GetComponent<GroupScript>().properties = startStats;
            count++;
            yield return new WaitForEndOfFrame();
        }
        yield break;
    }


    public void GoToCalculation()
    {
        bool goBack = true;
        StartCoroutine(ChangingPage(goBack, changePageType.moveToCalc));
    }

    public void GoToFormulaList()
    {
        bool goBack = unitList;
        StartCoroutine(ChangingPage(goBack, changePageType.moveToFormula));
    }

    public void GoToUnitList()
    {
        bool goBack = false;
        StartCoroutine(ChangingPage(goBack, changePageType.moveToUnit));
    }


    public enum changePageType { moveToCalc, moveToFormula, moveToUnit }
    IEnumerator ChangingPage(bool goBack, changePageType type )
    {
        SetFormulaUI(false);
        anim.SetTrigger(goBack ? "back" : "next");

        foreach (GameObject unit in PlayerScript.allies) unit.SetActive(false);
        _2.SetActive(false);
        yield return new WaitForSeconds(0.75f);

        switch (type)
        {
            case changePageType.moveToCalc:
                foreach (GameObject unit in PlayerScript.allies) { unit.SetActive(true); unit.GetComponent<MovementScript>().StartEyes(); }
                formulaButton.transform.localPosition = new Vector3(Math.Abs(formulaButton.transform.localPosition.x), formulaButton.transform.localPosition.y);
                SetFormulaUI(true);
                unitList.SetActive(false);
                formulaList.SetActive(false);
                calcButton.SetActive(false);
                wrong.SetActive(false);
                _2.SetActive(true);
                break;
            case changePageType.moveToFormula:
                calcButton.SetActive(true);
                unitButton.SetActive(true);
                formulaList.SetActive(true);
                _2.SetActive(false);
                break;
            case changePageType.moveToUnit:
                formulaButton.transform.localPosition = new Vector3(-Math.Abs(formulaButton.transform.localPosition.x), formulaButton.transform.localPosition.y);
                formulaButton.SetActive(true);
                calcButton.SetActive(true);
                unitList.SetActive(true);
                _2.SetActive(false);
                break;
        }

        yield break;
    }

    public bool CheckUnit(GameObject unit)
    {
        bool isInTerm = divident.Contains(unit);
        isInTerm |= divisor.Contains(unit);
        return isInTerm;
    }

    /// <summary>
    /// Fügt eine Einheit der Gleichung hinzu.
    /// </summary>
    /// <param name="unit">GameObject der Einheit</param>
    public void AddUnit(GameObject unit, bool moveAnim = true)//(string unit, int position, bool divisor)
    {
        bool is_2 = unit.name == "_2";

        if (!selection.activeSelf
            || selection.transform.localPosition.x > -4.5f
            || selection.transform.localPosition.x < -12)
        { if (is_2) Move_2Back(); else unit.GetComponent<GroupScript>().JumpIntoBook(transform); selection.SetActive(false); SetBlockInteractables(false); return; }

        if (selection.transform.localPosition.y > 0.5f)
        {
            //Divident
            if(divident.Count > 5) { if (is_2) Move_2Back(); else unit.GetComponent<GroupScript>().JumpIntoBook(transform); return; }
            if (divident.Count == 0)
                oneDivident.SetActive(false);
            divident.Add(unit);
        }
        else
        {
            //Divisor
            if(divisor.Count > 5) { if (is_2) Move_2Back(); else unit.GetComponent<GroupScript>().JumpIntoBook(transform); return; }
            if (divisor.Count == 0)
                oneDivisor.SetActive(false);
            divisor.Add(unit);
        }

        if(!is_2) RepositionUnits(unit, moveAnim);
        UpdateTerm();

        selection.SetActive(false);
        SetBlockInteractables(false);
    }

    public void SubstractUnit(GameObject unit, bool moveAnim = true)
    {
        Debug.Log("Abziehen");

        if (unit.transform.localPosition.y > 0)
        { if (divident.IndexOf(unit) >= 0) divident.RemoveAt(divident.IndexOf(unit)); else return; }
        else
        { if (divisor.IndexOf(unit) >= 0) divisor.RemoveAt(divisor.IndexOf(unit)); else return; }

        oneDivident.SetActive(divident.Count == 0);
        oneDivisor.SetActive(divisor.Count == 0);

        if (unit.name == "_2") Move_2Back(); else RepositionUnits(unit, moveAnim);
        UpdateTerm();
    }

    private void Move_2Back()
    {
        StartCoroutine(Moving_2Back());
    }

    IEnumerator Moving_2Back()
    {
        _2.GetComponent<Collider2D>().enabled = false;
        _2.transform.parent = transform;
        Vector3 realPos = _2.transform.localPosition;
        Vector3 posStep;

        int steps = 30;
        posStep = (new Vector3(-4.25f, 4) - _2.transform.localPosition) / steps;

        float b = -steps / 2f;
        float a = -2f / (steps * steps);
        float factor;

        for (int i = 0; i < steps; i++)
        {
            realPos += posStep;

            factor = a * (i + b) * (i + b) + 0.5f;
            _2.transform.localPosition = realPos + Vector3.up * factor;

            yield return new WaitForEndOfFrame();
        }

        _2.transform.parent = null;
        _2.GetComponent<Collider2D>().enabled = true;
        yield break;
    }

    /// <summary>
    /// Positioniert die Einheiten bei "gegeben" neu und ordnet die zu folgenden Einheiten neu zu. Zudem aktualisiert diese Methode die Allies-Listen
    /// </summary>
    private void RepositionUnits(GameObject unit, bool moveAnim = true)
    {
        Debug.Log("positioniere neu.");

        List<GameObject> objects = PlayerScript.allies;
        List<string> names = PlayerScript.alliesNames;
        int ptr = objects.IndexOf(unit);

        if(ptr < 0)//Hinzufügen:
        {
            objects.Add(unit);
            names.Add(unit.name);


            GroupScript gScript = unit.GetComponent<GroupScript>();
            gScript.groupNumber = objects.Count - 1;
            gScript.inGroup = true;
            if (objects.Count > 1)
            {
                gScript.charToFollow = objects[objects.Count - 2];
                unit.GetComponent<PlayerScript>().enabled = false;
                gScript.enabled = true;
                unit.layer = 16;//defaultOnly
            }
            else
            {
                unit.GetComponent<PlayerScript>().enabled = true;
                gScript.enabled = false;
                unit.layer = 8;//character
            }
        }
        else//Abziehen:
        {
            if(objects.Count == 1) { objects.RemoveAt(0); names.Clear(); return; }

            if (ptr == 0)//Einheit, die vom Spieler gesteuert wird
            {
                objects[0].GetComponent<PlayerScript>().enabled = false;
                objects[0].GetComponent<GroupScript>().enabled = true;
                objects[0].layer = 16;//defaultOnly
                objects[1].GetComponent<PlayerScript>().enabled = true;
                objects[1].GetComponent<GroupScript>().enabled = false;
                objects[1].layer = 8;//character
                pScript = objects[1].GetComponent<PlayerScript>();
            }

            objects.RemoveAt(ptr);
            for (int i = ptr; i < objects.Count; i++)
            {
                objects[i].GetComponent<GroupScript>().groupNumber--;
                if(i > 0) objects[i].GetComponent<GroupScript>().charToFollow = objects[i-1];
            }
        }

        names.Clear();
        int count = 0;
        foreach(GameObject obj in objects)
        {
            names.Add(obj.name);
            //Bewege Einheiten zu der neuen Position:
            if (!moveAnim
                || (ptr >= 0 && count < ptr)//wegnehmen
                || (ptr < 0 && count < objects.Count-1))//anhängen
            {
                count++;
                continue;
            }
            obj.GetComponent<GroupScript>().MoveToNewPosition(transform);
        }
    }

    /// <summary>
    /// Positioniert alle Einheiten innerhalb des Terms
    /// </summary>
    private void UpdateTerm()
    {
        int i = 0;
        Transform factor;
        foreach (GameObject member in divident)
        {
            member.transform.parent = transform;
            member.transform.localPosition = new Vector3(-8 + 1.7f * (i - (divident.Count - 1) / 2f), 1.5f);
            if(i != 0)
            {
                factor = transform.Find("u_" + i.ToString());
                if (!factor) { factor = Instantiate(factorSign, transform).transform; factor.gameObject.name = "u_" + i.ToString(); }
                factor.localPosition = new Vector3(-8.8f + 1.7f * (i - (divident.Count - 1) / 2f), 1.5f);
            }
            i++;
        }
        factor = transform.Find("u_" + i.ToString());
        if (factor) Destroy(factor.gameObject);

        i = 0;
        foreach (GameObject member in divisor)
        {
            member.transform.parent = transform;
            member.transform.localPosition = new Vector3(-8 + 1.7f * (i - (divisor.Count - 1) / 2f), -0.5f);
            if (i != 0)
            {
                factor = transform.Find("l_" + i.ToString());
                if (!factor) { factor = Instantiate(factorSign, transform).transform; factor.gameObject.name = "l_" + i.ToString(); }
                factor.localPosition = new Vector3(-8.8f + 1.7f * (i - (divident.Count - 1) / 2f), -0.5f);
            }
            i++;
        }
        factor = transform.Find("l_" + i.ToString());
        if (factor) Destroy(factor.gameObject);
    }

    /// <summary>
    /// Zeigt entsprechend der Position der Einheit die Selektion an
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public bool UpdateSelection(GameObject unit)
    {
        SetBlockInteractables(true);

        unit.transform.parent = transform;
        float y_pos = unit.transform.localPosition.y;
        if ( y_pos > 2.5f || y_pos < -2f) { selection.SetActive(false); return false; }

        selection.SetActive(true);
        if (y_pos > 0.5f)
            selection.transform.localPosition = new Vector3(-8, 1.5f);
        else
            selection.transform.localPosition = new Vector3(-8, -0.5f);

        unit.transform.parent = null;
        return true;
    }

    private GameObject SpawnUnit(string unitName, Vector2 position, bool moveAnim = true)
    {
        GameObject obj = Resources.Load<GameObject>("Units/" + unitName);
        obj = Instantiate(obj, transform);

        obj.name = obj.GetComponent<SpriteRenderer>().sprite.name;
        obj.transform.localPosition = position;
        obj.transform.parent = null;
        obj.transform.localScale = new Vector3(0.1f, 0.1f);
        obj.GetComponent<CapsuleCollider2D>().enabled = false;
        obj.transform.GetChild(2).gameObject.SetActive(false);
        obj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        obj.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        obj.GetComponent<GroupScript>().properties = DefaultStats.DeepClone(PlayerScript.allies.Count > 0 ? PlayerScript.allies[0].GetComponent<GroupScript>().properties : DefaultStats.defaultChar);
        StartCoroutine(SpawnAnimation(obj, moveAnim));
        return obj;
    }

    IEnumerator SpawnAnimation(GameObject unit, bool moveAnim)
    {
        int steps = 30;
        Vector3 scaleStep;
        Color colorStep;

        SpriteRenderer sprite = unit.GetComponent<SpriteRenderer>();
        sprite.sortingLayerName = "UI";
        sprite.sortingOrder = 1;
        unit.GetComponent<GroupScript>().SetEyeOrder("UI", 2);

        //expand:
        scaleStep = ((Vector2)unit.transform.localScale - new Vector2(0.6f, 0.6f)) / steps;
        colorStep = new Color(0, 0, 0, 1f / steps);
        for (int i = 0; i < steps; i++)
        {
            unit.transform.localScale -= scaleStep;
            sprite.color += colorStep;
            yield return new WaitForEndOfFrame();
        }

        //shrink:
        steps = 20;
        scaleStep = ((Vector2)unit.transform.localScale - new Vector2(0.3f, 0.3f)) / steps;
        for (int i = 0; i < steps; i++)
        {
            unit.transform.localScale -= scaleStep;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1);
        RepositionUnits(unit, moveAnim);
        yield break;
    }

    /// <summary>
    /// Erstellt neue Einheit aus der Gleichung heraus
    /// </summary>
    public void CreateUnit()
    {
        string term = "";
        string abbr;

        foreach(GameObject unit in divident)
        {
            abbr = "";
            FormulaList.nameToAbbr.TryGetValue(unit.GetComponent<SpriteRenderer>().sprite.name, out abbr);
            term += abbr;
        }  
        term += "/(";
        foreach (GameObject unit in divisor)
        {
            abbr = "";
            FormulaList.nameToAbbr.TryGetValue(unit.GetComponent<SpriteRenderer>().sprite.name, out abbr);
            term += abbr;
        }       
        term += ")";
        Debug.Log(term);
        string sDivident = "";
        string sDivisor = "";
        string unitName = "";
        DoConvertion(term, out sDivident, out sDivisor);
        if(sDivisor.Length > 0 || !FormulaList.abbrToName.TryGetValue(sDivident, out unitName))
        {
            StartCoroutine(WrongAnimation());
        }
        else
        {
            //Spawne neue Einheit:
            wrong.SetActive(false);
            block = true;

            //langsam:
            DefaultStats.SetDefaultCharValueByName(unitName);
            foreach (GameObject unit in divident) DefaultStats.SetDefaultCharValueByName(unit.name);
            foreach (GameObject unit in divisor) DefaultStats.SetDefaultCharValueByName(unit.name);

            //Debug.Log(PlayerScript.allies[0].GetComponent<GroupScript>().properties.size);//Es muss dafür noch mindestens eine Einheit außerhalb der Gleichung sein

            StartCoroutine(FadeTermUnits());
            SpawnUnit(unitName, wrong.transform.localPosition);
        }
    }

    IEnumerator WrongAnimation()
    {
        Camera.main.GetComponent<ManipulatorScript>().dragBlock = true;
        wrong.SetActive(true);
        Vector2 startPos = wrong.transform.localPosition;
        int steps = 50;
        for (int i = 0; i < steps; i++)
        {
            wrong.transform.localPosition = startPos + UnityEngine.Random.insideUnitCircle * steps / 200;
            yield return new WaitForEndOfFrame();
        }
        wrong.transform.localPosition = startPos;
        Camera.main.GetComponent<ManipulatorScript>().dragBlock = false;
        yield return new WaitForSeconds(1);
        wrong.SetActive(false);
        yield break;
    }

    IEnumerator FadeTermUnits(float waitTime = 1)
    {
        Camera.main.GetComponent<ManipulatorScript>().dragBlock = true;
        yield return new WaitForSeconds(waitTime);

        int steps = 30;
        Color stepColor = new Color(0, 0, 0, 1f / steps);
        Color realColor = Color.white;
        for(int i = 0; i < steps; i++)
        {
            realColor -= stepColor;
            foreach (GameObject obj in divident) if (obj.name != "_2") obj.GetComponent<GroupScript>().SetColor(realColor);
            foreach (GameObject obj in divisor) if (obj.name != "_2")  obj.GetComponent<GroupScript>().SetColor(realColor);
            yield return new WaitForEndOfFrame();
        }

        foreach (GameObject obj in divident) { if (obj.name == "_2") Move_2Back(); else Destroy(obj); } //RepositionUnits(obj, false);
        foreach (GameObject obj in divisor) { if (obj.name == "_2") Move_2Back(); else Destroy(obj); }// RepositionUnits(obj, false);
        divident.Clear();
        divisor.Clear();
        oneDivident.SetActive(true);
        oneDivisor.SetActive(true);

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Factor")) Destroy(obj);

        Camera.main.GetComponent<ManipulatorScript>().dragBlock = false;
        yield break;
    }

    /// <summary>
    /// Konvertiert den Eingangsterm in eine Einheit. 
    /// 
    /// <para>Es sind nur die Operatoren * und / sowie eine Klammer unter- und eine oberhalb der Division erlaubt. Es ist max. ein '/' erlaubt.</para>
    /// <para>Damit lassen sich 99.9% aller existierenden Formeln bilden.</para>
    /// </summary>
    /// <param name="input">Term, der in eine Einheit umgewandelt werden soll</param>
    public void DoConvertion(string input, out  string divident, out string divisor)
    {
        int index = 0;
        string[] division;
        string[] term = new string[2];

        string[] formulaDivision = new string[1];//default, wird bei der preparation überschrieben
        string[][][] formulaElements = new string[2][][];

        //prep der Formeln:
        formulaElements[0] = new string[FormulaList.terms.Count][];
        formulaElements[1] = new string[FormulaList.terms.Count][];

        for (int i = 0; i < FormulaList.terms.Count; i++)
        {
            formulaDivision = FormulaList.terms[i].Split('/');//splitte in Divident und Divisor auf

            //splitte Komponenten vom Divident und Divisor in einzelne Komponenten auf
            formulaElements[0][i] = formulaDivision[0].Split('*');
            if (formulaDivision.Length == 1) { formulaElements[1][i] = new string[] { "" }; continue; }
            if(formulaDivision[1][0] == '(')
                formulaDivision[1] = formulaDivision[1].Substring(1, formulaDivision[1].Length - 2);
            formulaElements[1][i] = formulaDivision[1].Split('*');
        }

        //Ordne Eingabe in Divident und Divisor:
        if (!input.Contains("/")) input += "/1";
        division = input.Split('/');
        if(division[0].Length != 0)
            if (division[0][division[0].Length - 1] == ')')
            {
                division[0] = division[0].Remove(division[0].Length - 1);
                index = division[0].IndexOf('(');
                division[0] = division[0].Remove(index, 1);
            }

        if (division.Length == 2)
        {           
            //füge alle nachstehenden Multiplikationen an Dividenten an:
            if (division[1][0] == '(')
            {
                division[1] = division[1].Remove(0, 1);
                index = division[1].IndexOf(')');
                division[1] = division[1].Remove(index, 1);
                if(index < division[1].Length)
                {
                    division[0] += division[1].Substring(index);
                    division[1] = division[1].Remove(index);
                }
            }
        }

        Debug.Log("input:");
        Debug.Log(division[0] + "/" + division[1]);

        //eleminiere alle Malzeichen:
        for (int i = 0; i < 2; i++)
        {
            string divTerm = "";
            foreach (string el in division[i].Split('*'))
                divTerm += el;
            division[i] = divTerm;
        }

        //Breche Term in die Grundeinheiten auf:
        Debug.Log("breche term auf...");
        int complete = 1;
        while (complete == 1)
        {
            int formulaResult = 0;

            complete = 0;
            term[0] = division[0];
            term[1] = division[1];
            for (int i = 0; i < 2; i++)//behandle divident/divisor des inputs
            {
                term[i] = division[i];
                formulaResult = -1;
                foreach (string element in FormulaList.results)//laufe alle herleitbaren elemente ab
                {
                    formulaResult++;
                    if (!term[i].Contains(element) || formulaResult >= FormulaList.results.Count - FormulaList.numberOfOneToOneTerms)
                        continue;

                    term[i] = term[i].Remove(term[i].IndexOf(element), element.Length);
                    complete = 1;
                    break;
                }

                if (complete == 1)
                {
                    Debug.Log(formulaResult);
                    division[0] = term[0];
                    division[1] = term[1];
                    foreach (string element in formulaElements[i][formulaResult]) division[0] +=  element;
                    foreach (string element in formulaElements[(i+1)%2][formulaResult]) division[1] += element;
                    Debug.Log(division[0] + "/" + division[1]);
                    break;
                }
            }
        }

        //kürze Term:
        Debug.Log("starte Kürzen...");
        complete = 1;
        while (complete == 1)
        {
            complete = 0;
            string[] elements = division[0].Split(sep, StringSplitOptions.RemoveEmptyEntries);
            foreach(string element in elements)
            {
                if (!division[1].Contains(element))
                    continue;

                division[0] = division[0].Remove(division[0].IndexOf(element) - 1, element.Length + 1);
                division[1] = division[1].Remove(division[1].IndexOf(element) - 1, element.Length + 1);

                Debug.Log(division[0] + "/" + division[1]);
                complete = 1;
                break;
            } 
        }

        //Vereinfache Term:
        Debug.Log("starte Vereinfachen...");
        complete = 2;
        while (complete == 2)
        {
            //oldDivident = division[0];
            int count;
            int formulaResult = 0;
            for (; formulaResult < FormulaList.terms.Count; formulaResult++)//betrachte jede formel        
            {
                complete = 0;
                for (int i = 0; i < 2; i++)//behandle divident/divisor der formel
                {
                    term[i] = division[i];
                    count = 0;
                    foreach (string element in formulaElements[i][formulaResult])//laufe alle elemente des dividenten/divisors ab
                    {
                        if (!term[i].Contains(element))
                            break;
                        if (++count == formulaElements[i][formulaResult].Length)
                            complete++;
                        term[i] = term[i].Remove(term[i].IndexOf(element), element.Length);
                    }
                    if (complete == 0)
                        break;
                }

                if (complete == 2)
                    break;
            }

            if (complete == 2)
            {
                division[0] = term[0] + FormulaList.results[formulaResult];
                division[1] = term[1];
                if (formulaResult >= formulaElements[0].Length - FormulaList.numberOfOneToOneTerms)//letzte Konvertierungen sind 1:1 , um hin und herschwanken zu verhindern wird hier abgebrochen
                    complete = 0;
            }
            Debug.Log(division[0] + "/" + division[1]);
        }

        if(division[0].Length != 0)
            while (division[0][0] == '*')
                division[0] = division[0].Remove(0, 1);
        if(division[1].Length != 0)
            while (division[1][0] == '*')
                division[1] = division[1].Remove(0, 1);

        Debug.Log("Result:");
        Debug.Log(division[0]);
        Debug.Log(division[1]);
        divident = division[0];
        divisor = division[1];
    }
}
