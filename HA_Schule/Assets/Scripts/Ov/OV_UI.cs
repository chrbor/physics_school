using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static GameManager;

public class OV_UI : MonoBehaviour
{
    [HideInInspector]
    public DevInfoScript devInfo;
    private Image iCover;
    private GameObject textFields;
    private GameObject speakField, nameField, faceField, decisionField, cover;
    private Text textfield;
    private bool stop;
    //private char[] sep = new char[] { '\n' };

    private void Start()
    {
        devInfo = transform.Find("DevInfo").GetComponent<DevInfoScript>();

        cover = transform.Find("Cover").gameObject;
        textFields = transform.Find("TextFields").gameObject;
        speakField = textFields.transform.GetChild(0).gameObject;
        nameField = textFields.transform.GetChild(1).gameObject;
        faceField = textFields.transform.GetChild(2).gameObject;
        decisionField = textFields.transform.GetChild(3).gameObject;
        textfield = speakField.transform.GetChild(0).GetComponent<Text>();
        HideEverything();
        cover.SetActive(true);
    }

    private void HideEverything()
    {
        cover.SetActive(false);
        speakField.SetActive(false);
        faceField.SetActive(false);
        nameField.SetActive(false);
        decisionField.SetActive(false);
    }

    public void Speak(LevelScript.LevelEvent levelEvent)
    {
        textFields.SetActive(true);
        textfield.text = "";

        string expression = "";
        switch (levelEvent.expression)
        {
            case LevelScript.Expression.none:   expression = "none"; break;
            case LevelScript.Expression.happy:  expression = "happy"; break;
            case LevelScript.Expression.angry:  expression = "angry"; break;
            case LevelScript.Expression.annoyed:expression = "annoyed"; break;
            case LevelScript.Expression.sad:    expression = "sad"; break;
        }

        Sprite face = Resources.Load<Sprite>("Faces/" + expression + "/" + levelEvent.focus);
        if (face)
        {
            faceField.transform.GetChild(0).GetComponent<Image>().sprite = face;
            faceField.SetActive(true);
            nameField.transform.GetChild(0).GetComponent<Text>().text = levelEvent.focus;
        }
        else
        {
            faceField.SetActive(false);
            nameField.SetActive(true);
            nameField.transform.GetChild(0).GetComponent<Text>().text = levelEvent.focus;
        }
        speakField.SetActive(true);
        gameObject.SetActive(true);

        StartCoroutine(WriteTextfield(levelEvent));
    }

    public void closeTextFields()
    {
        textFields.SetActive(false);
        stop = true;
    }

    IEnumerator WriteTextfield(LevelScript.LevelEvent levelEvent)
    {
        stop = true;
        yield return new WaitForSeconds(0.1f);
        stop = false;

        textfield.text = "";
        foreach(char letter in levelEvent.spokenText)
        {
            if (stop) break;
            textfield.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        if(levelEvent.title == "Decision")
        {
            int val = -1;
            while(val < 0)
            {
                if (Input.GetKey(KeyCode.Alpha1)) val = 1;
                if (Input.GetKey(KeyCode.Alpha2)) val = 2;
                if (Input.GetKey(KeyCode.Alpha3) && levelEvent.eventVal > 2) val = 2;
                yield return new WaitForEndOfFrame();
            }
            manager.StartExecutingEvents("decision_" + SequenceCount + "_" + val.ToString());
        }

        manager.OnTriggerMove(0);
        yield break;
    }


    
    public void StartCoreGame(int scene)
    {
        HideEverything();
        gameObject.SetActive(true);
        StartCoroutine(LoadCoreGame(scene));
    }

    IEnumerator LoadCoreGame(int scene)
    {
        Image img = cover.GetComponent<Image>();
        img.color = Color.clear;
        cover.SetActive(true);
        Color stepcolor = Color.black / 20;
        for (int i = 0; i < 20; i++)
        {
            img.color += stepcolor;
            yield return new WaitForEndOfFrame();
        }
        LoadLevel(scene);
        yield break;
    }


    public void RevealScene()
    {
        StartCoroutine(RevealCover());
    }

    IEnumerator RevealCover()
    {
        Image img = cover.GetComponent<Image>();
        img.color = Color.black;
        cover.SetActive(true);
        Color stepcolor = Color.black / 40;
        for (int i = 0; i < 40; i++)
        {
            img.color -= stepcolor;
            yield return new WaitForEndOfFrame();
        }
        cover.SetActive(false);
        yield break;
    }
}
