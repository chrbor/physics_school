using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CheckpointScript;

public class GameMenu : MonoBehaviour
{
    [Header("Prefabs, nicht anfassen!!!")]
    public GameObject exercise;
    public GameObject result;
    public Sprite[] id_sprites;
    public Sprite[] div_sprites;
    public Sprite[] mark_sprite;
    public Sprite[] stamp_sprite;
    public GameObject line;

    //immer zugreifbar:
    private GameObject sheet, menu;
    private GameObject exerciseList;
    public GameObject startButton, restartButton, nextButton, quitButton, resumeButton;

    //innerhalb der Aufgabe:
    private GameObject lineHolder, id_object, div_object;
    private Animator anim_num1, anim_num2, anim_num3, anim_tick, anim_menu;

    //helper
    public class CheckpointData
    {
        public CheckpointData(int _id, int _points)
        {
            id = _id;
            points = _points;
        }

        public GameObject gameObject;
        public GameObject divObject, idObject;
        public int id;
        public int points;
        public bool reached = false;
    }
    private List<CheckpointData> cData = new List<CheckpointData>();
    private int pointSum;
    private float pointsReached;
    private int mark;

    private void Awake()
    {
        sheet = transform.Find("Sheet").gameObject;
        exerciseList = sheet.transform.Find("AufgabenListe").gameObject;
        menu = transform.GetChild(1).gameObject;
        anim_menu = menu.GetComponent<Animator>();
        startButton = menu.transform.GetChild(0).Find("StartButton").gameObject;
        restartButton = menu.transform.GetChild(0).Find("RestartButton").gameObject;
        nextButton = menu.transform.GetChild(0).Find("NextButton").gameObject;
        quitButton = menu.transform.GetChild(0).Find("QuitButton").gameObject;
        resumeButton = menu.transform.GetChild(0). Find("ContinueButton").gameObject;
 
        pointSum = 0;
        pointsReached = 0;
        StartCoroutine(FillLines());
        StartCoroutine(ShowingMenu(MenuState.start));
    }

    public void ShowMenu()
    {
        StartCoroutine(ShowingMenu(mark > 0? (mark < 5 ? MenuState.suceeded : MenuState.failed) : MenuState.game));
    }

    public void HideMenu()
    {
        anim_menu.SetTrigger("hide");
        startButton.SetActive(false);
        resumeButton.SetActive(false);
        restartButton.SetActive(false);
        quitButton.SetActive(false);
        nextButton.SetActive(false);
    }

    public enum MenuState { start, game, failed, suceeded}
    IEnumerator ShowingMenu(MenuState state)
    {
        yield return new WaitForSeconds(1);
        anim_menu.SetTrigger("show");
        yield return new WaitForSeconds(1);

        switch (state)
        {
            case MenuState.start:
                startButton.SetActive(true);
                break;
            case MenuState.game:
                resumeButton.SetActive(true);
                restartButton.SetActive(true);
                break;
            case MenuState.failed:
                nextButton.SetActive(true);
                restartButton.SetActive(true);
                break;
            case MenuState.suceeded:
                nextButton.SetActive(true);
                restartButton.SetActive(true);
                break;
        }
        quitButton.SetActive(true);
    }

    IEnumerator FillLines()
    {
        //Innehalb von Awake muss jeder Checkpoint die Daten an GameMenu senden
        yield return new WaitForFixedUpdate();
        FillingLines();
        yield break;
    }

    /// <summary>
    /// Füllt LineHolder mit Linien und fügt Result hinzu
    /// </summary>
    private void FillingLines()
    {
        int count = 0;
        foreach(CheckpointData data in cData)
        {
            lineHolder = exerciseList.transform.GetChild(count++).Find("LineHolder").gameObject;
            for(int i = 0; i < 2 + (data.id+2) % 3; i++)
            {
                Instantiate(line, lineHolder.transform);
            }
            GameObject obj = Instantiate(line, lineHolder.transform);
            obj.GetComponent<RectTransform>().localScale = new Vector3(7 * (1+data.id) / 10f - 7 * (1+data.id) / 10, 1, 1);
        }
            //füge Result hinzu
            Instantiate(result, exerciseList.transform);
    }

    public void InsertExercise(int _id, int _points)
    {
        int count = 0;
        for(; count < cData.Count; count++)
            if(_id < cData[count].id) { cData.Insert(count, new CheckpointData(_id, _points)); break; }
        if (count == cData.Count) { cData.Add(new CheckpointData(_id, _points)); }

        cData[count].gameObject = Instantiate(exercise, exerciseList.transform);
        cData[count].gameObject.transform.SetSiblingIndex(count);

        //Setze die Sprites:
        cData[count].idObject = cData[count].gameObject.transform.Find("ID").gameObject;
        cData[count].idObject.GetComponent<Image>().sprite = id_sprites[_id];
        cData[count].divObject = cData[count].gameObject.transform.Find("Div").gameObject;
        cData[count].divObject.GetComponent<Image>().sprite = div_sprites[_points <= 20 ? _points / 5 - 1 : 3 + _points / 50];

        pointSum += _points;
    }

    /// <summary>
    /// setzt den Haken beim nächsten Öffnen des Menüs und addiert die Punkte der Aufgabe
    /// </summary>
    /// <param name="_id"></param>
    public void UpdateExercise(int _id)
    {
        int ptr = 0;
        bool done = true;
        for (int i = 0; i < cData.Count; i++)
        {
            if (cData[i].id == _id) { ptr = i; cData[i].reached = true; }
            done &= cData[i].reached;
        }
        if (ptr == cData.Count) return;

        pointsReached += cData[ptr].points;
        if (done) GameObject.Find("CoreManager").GetComponent<CoreScript>().EndTheGame();
    }

    /// <summary>
    /// Setzt den Haken und schreibt bei showMark = true die Punkte zu den Aufgaben und zeigt entsprechend der Gesamtpunktzahl die Note
    /// </summary>
    public void ShowResults(bool showMark = false, bool failed = false)
    {
        float percent = pointsReached/pointSum;
        mark = showMark? (percent > 0.5f && !failed ? (int)((1.05f - percent) / 0.15f)+1 : 5) : -1;

        ShowMenu();
        StartCoroutine(ShowingResults(mark));
    }

    IEnumerator MoveSheetAlong()
    {
        yield return new WaitForSeconds(1);

        int steps = 60;
        Vector3 stepPos = Vector3.up * 0.01f;
        for (int i = 0; i < steps * cData.Count; i++) { sheet.transform.position += stepPos; yield return new WaitForEndOfFrame(); }
        yield break;
    }

    IEnumerator ShowingResults(int mark)
    {
        int steps;
        if(mark > 0) StartCoroutine(MoveSheetAlong());

        foreach(CheckpointData data in cData)
        {
            if(data.reached)
                data.gameObject.transform.Find("Tick").GetComponent<Animator>().SetTrigger("tick");
            
            if (mark < 1) continue;
            yield return new WaitForSeconds(1);


            if (!data.reached) data.points = 0;

            anim_num1 = data.divObject.transform.Find("Number_1").GetComponent<Animator>();
            anim_num2 = data.divObject.transform.Find("Number_2").GetComponent<Animator>();
            anim_num3 = data.divObject.transform.Find("Number_3").GetComponent<Animator>();

            string sPoints = data.points.ToString();

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

            anim_num1.SetTrigger("draw");
            anim_num2.SetTrigger("draw");
            anim_num3.SetTrigger("draw");
        }
        if (mark < 1) yield break;
        Debug.Log("mark: " + mark);
        yield return new WaitForSeconds(1);

        Transform tResult = exerciseList.transform.GetChild(exerciseList.transform.childCount-1);

        GameObject markObj = tResult.Find("Mark").gameObject;
        Image markSprite = markObj.GetComponent<Image>();
        markSprite.sprite = mark_sprite[mark - 1];
        markSprite.color = new Color(1, 1, 1, 0);
        markObj.SetActive(true);


        //Mark Animation:
        steps = 30;
        Color stepColor = new Color(0,0,0, 1f / steps);
        for (int i = 0; i < steps; i++) { markSprite.color += stepColor; yield return new WaitForEndOfFrame(); }

        GameObject stampObj = tResult.Find("Stamp").gameObject;
        Image stampSprite = stampObj.GetComponent<Image>();
        stampSprite.sprite = stamp_sprite[mark < 5 ? 0 : 1];
        stampSprite.color = new Color(1, 1, 1, 0);
        stampObj.SetActive(true);

        //Stamp Animation:
        steps = 60;
        stepColor = new Color(0, 0, 0, 1f / steps);

        Vector3 realScale = transform.localScale;

        float b = -steps / 2f;
        float a = -4f / (steps * steps);
        float factor;

        for (int i = 0; i <= steps; i++)
        {
            factor = a * (i + b) * (i + b) + 1;
            stampObj.transform.localScale = realScale + (Vector3)Vector2.one * factor;
            stampSprite.color += stepColor;

            yield return new WaitForEndOfFrame();
        }
        yield break;
    }
}
