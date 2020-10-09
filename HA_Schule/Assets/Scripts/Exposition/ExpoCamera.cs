using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExpoCamera : MonoBehaviour
{
    GameObject blende, blende2;
    SpriteRenderer sprite, sprite2;

    DevInfoScript devInfo;
    GameObject textField;
    // Start is called before the first frame update
    void Start()
    {
        blende = GameObject.Find("Blende");
        blende2 = GameObject.Find("Blende_2");
        textField = GameObject.Find("Canvas").transform.Find("Image").gameObject;

        if (!blende || !blende2)
            Destroy(this);

        devInfo = GameObject.Find("Canvas").transform.Find("DevInfo").GetComponent<DevInfoScript>();
        sprite = blende.GetComponent<SpriteRenderer>();
        sprite2 = blende2.GetComponent<SpriteRenderer>();
        StartCoroutine(ExpositionFunction());
    }


    IEnumerator ExpositionFunction()
    {
        yield return new WaitUntil(() => Input.anyKey);
        textField.transform.GetChild(0).GetComponent<Text>().text = "Viel Spaß!";
        yield return new WaitForSeconds(1);
        Destroy(textField);
        devInfo.PlayDevInfo("Intro");
        yield return new WaitForSeconds(5);


        //gehe zu Hintergrundfarbe über:
        for (int i = 0; i < 40; i++)
        {
            sprite2.color -= Color.black * 0.025f;
            yield return new WaitForSeconds(0.1f);
        }
        sprite2.color = Color.clear;

        yield return new WaitForSeconds(2);

        //zeige Setting:
        for (int i = 0; i < 40; i++)
        {
            sprite.color -= Color.black * 0.025f;
            yield return new WaitForSeconds(0.1f);
        }
        sprite.color = Color.clear;

        yield return new WaitWhile(()=>devInfo.source.isPlaying);

        yield return new WaitForSeconds(2);
        devInfo.PlayDevInfo("AudioComments");


        GameObject.Find("Coach").GetComponent<ExpoCoach>().runCoach = true;

        //Gibt dem Spieler Zeit zu lesen:
        yield return new WaitForSeconds(3);

        //Kameraschwenk:
        for(int i = 0; i < 190; i++)
        {
            transform.position += Vector3.down * 0.1f;
            yield return new WaitForFixedUpdate();
        }

        //Gib dem Spieler Zeit:
        yield return new WaitForSeconds(7);

        //Blende ins Spiel über:
        blende.transform.position = (Vector2)transform.position;
        blende.transform.localScale = new Vector3(3, 3);
        sprite.sortingOrder = 7;
        for (int i = 0; i < 20; i++)
        {
            sprite.color += Color.black * 0.05f;
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitWhile(() => devInfo.source.isPlaying);
        SceneManager.LoadScene("Overworld");
        yield break;
    }
}
