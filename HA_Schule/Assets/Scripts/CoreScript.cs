using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoreScript : MonoBehaviour
{
    bool endGame;
    bool resumeGame;

    ClockScript clock;
    GameMenu menu;
    CanvasGroup menuGroup;
    GameObject gameMenu, gameHUD;

    Transform sheet;
    RectTransform shadow;
    Vector2 diff, dir;

    public float peekLength = 1000;
    public int levelTime;
    public Vector2 levelCenter;
    public float levelSize;

    bool prePeek;
    bool peeking;
    int peekCount;
  
    void Start()
    {
        Transform canvas = GameObject.Find("Canvas").transform;
        gameHUD = canvas.Find("GameHUD").gameObject;
        clock = gameHUD.transform.Find("Clock").GetComponent<ClockScript>();
        shadow = gameHUD.transform.Find("Prof").GetComponent<RectTransform>();

        gameMenu = canvas.Find("GameMenu").gameObject;
        menu = gameMenu.GetComponent<GameMenu>();
        menuGroup = gameMenu.GetComponent<CanvasGroup>();
        sheet = gameMenu.transform.Find("Sheet");
        diff = sheet.transform.position - Camera.main.transform.position;

        endGame = false;
        StartCoroutine(MenuControl());
        clock.SetTime(levelTime);


        //clock.StartTimer();//wird von MenuControl übernommen
    }
        
    public void EndTheGame()
    {
        endGame = true;
    }

    public void ResumeGame() { resumeGame = true; }

    IEnumerator MenuControl()
    {
        int steps = 180;   
        float alphasteps = 1f / steps;
        Vector3 posSteps = Vector3.up * Camera.main.orthographicSize / steps;

        sheet.position = (Vector2)Camera.main.transform.position + diff - Vector2.up * Camera.main.orthographicSize;
        gameMenu.SetActive(true);
        menuGroup.alpha = 0;
        for (int i = 0; i < steps; i++) { menuGroup.alpha += alphasteps; sheet.position += posSteps; yield return new WaitForEndOfFrame(); }

        steps = 60;
        alphasteps = 1f / steps;
        posSteps = Vector3.up * Camera.main.orthographicSize / steps;

        //Spiel startet, nachdem das Menü das erste Mal geschlossen wird:
        yield return new WaitUntil(() => Input.GetKey(KeyCode.Escape) || resumeGame);
        PlayerScript.allies[0].GetComponent<PlayerScript>().StartPlayer();
        resumeGame = true;

        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (Input.GetKey(KeyCode.Z)) Camera.main.GetComponent<CameraScript>().LookAtCenter(levelCenter, levelSize);


            if (peeking)
            {
                if (peekCount <= 0)//Wähle eine Seite
                {
                    int corner = Random.Range(0, 2);
                    shadow.anchorMin = new Vector2(corner, 0);
                    shadow.anchorMax = shadow.anchorMin;
                    shadow.anchoredPosition = new Vector2(Random.Range(0, corner == 0 ? 200 : -200), Random.Range(50, 100));
                    float rotation = (corner == 0 ? -1 : 1) * Random.Range(10, 30);
                    shadow.eulerAngles = new Vector3(0, 0, rotation);
                    dir = MovementScript.RotToVec(rotation + 90)/10;

                    shadow.gameObject.SetActive(true);
                }
                else if (!prePeek) peekCount += 30;
                prePeek = true;


                shadow.GetComponent<Image>().color = new Color(1, 1, 1, peekCount / (peekLength * 2));
                shadow.anchoredPosition += dir;
                peekCount++;
            }
            else if (peekCount > 0)
            {
                peekCount--;
                shadow.GetComponent<Image>().color = new Color(1, 1, 1, peekCount / (peekLength * 2));
                shadow.anchoredPosition -= dir;
            }
            else { shadow.gameObject.SetActive(false); prePeek = false; }


            //Menüaufruf:
            if (Input.GetKey(KeyCode.Escape) || resumeGame || endGame || peekCount >= peekLength )
            {
                resumeGame = false;
                yield return new WaitWhile(() => Input.GetKey(KeyCode.Escape));

                if (!gameMenu.activeSelf || endGame)
                {
                    //GameManager.block = true;

                    clock.StopTime();

                    sheet.position = (Vector2)Camera.main.transform.position + diff - Vector2.up * Camera.main.orthographicSize; 
                    gameMenu.SetActive(true);
                    menuGroup.alpha = 0;
                    for (int i = 0; i < steps; i++) { menuGroup.alpha += alphasteps; sheet.position += posSteps; yield return new WaitForEndOfFrame(); }

                    yield return new WaitForSeconds(1);
                    Debug.Log((peekCount >= peekLength) + ", " + endGame);
                    if (peekCount >= peekLength) { menu.ShowResults(true, true); yield break; }
                    if (endGame) { menu.ShowResults(true, false); yield break; }
                    menu.ShowResults();
                }
                else
                {
                    menu.HideMenu();
                    menuGroup.alpha = 1f;
                    for (int i = 0; i < steps; i++) { menuGroup.alpha -= alphasteps; sheet.position -= posSteps; yield return new WaitForEndOfFrame(); }
                    gameMenu.SetActive(false);

                    clock.StartTimer();
                    //GameManager.block = false;
                }
            }
        }

        //yield break;
    }

    public void SetPeeking(bool _peeking)
    {
        peeking = _peeking;
    }

}
