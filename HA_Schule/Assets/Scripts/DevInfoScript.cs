using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevInfoScript : MonoBehaviour
{
    public AudioSource source;
    public Sprite[] devInfoSprites;
    private Image image;

    public static bool devInfo_On = true;
    public static DevInfoScript devInfo;
    public AudioClip audioClip;

    // Start is called before the first frame update
    void Start()
    {
        devInfo = this;
        source = GetComponent<AudioSource>();
        image = GetComponent<Image>();
        image.color = Color.clear;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.M)) DevInfoSwtich();
    }

    /// <summary>
    /// Spielt das Audiofile mit dem pfad "audioName" ab
    /// </summary>
    /// <param name="audioName"></param>
    public void PlayDevInfo(string audioName)
    {
        image.color = Color.white;
        gameObject.SetActive(true);
        StartCoroutine(PlayingDevInfo(audioName));
    }

    IEnumerator PlayingDevInfo(string audioName)
    {

        if (source.isPlaying) source.Stop();
        yield return new WaitForEndOfFrame();

        AudioClip clip = devInfo_On ? Resources.Load<AudioClip>("DevInfo/" + audioName) : null;
        if (clip)
        {
            source.clip = clip;
            source.Play();
            yield return new WaitWhile(() => source.isPlaying);
        }
        else
        {
            //Shake:
            RectTransform tdev = GetComponent<RectTransform>();
            Vector2 startPos = tdev.anchoredPosition;
            for (int i = 60; i > 0; i--) { tdev.anchoredPosition = startPos + UnityEngine.Random.insideUnitCircle * i/80f; yield return new WaitForEndOfFrame(); }
            yield return new WaitForSeconds(0.5f);
            tdev.anchoredPosition = startPos;
        }
        image.color = source.isPlaying? Color.white : Color.clear;
        yield break;
    }

    public void DevInfoSwtich()
    {
        StartCoroutine(SwitchingDevInfo());
    }

    IEnumerator SwitchingDevInfo()
    {
        devInfo_On = !devInfo_On;
        source.Stop();
        image.sprite = devInfoSprites[devInfo_On ? 0 : 1];
        image.color = Color.white;
        yield return new WaitForSeconds(2);
        image.color = source.isPlaying ? Color.white : Color.clear;
        yield break;
    }
}
