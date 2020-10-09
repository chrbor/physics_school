using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClockScript : MonoBehaviour
{
    [System.Serializable]
    public class OnEvent : UnityEvent { };

    public int currentTime = 0;
    public OnEvent TimeUp;
    private bool running, wasRunning, settingTime;

    private GameObject SecPointer;
    private GameObject MinPointer;


    private void Awake()
    {
        SecPointer = transform.Find("Second").gameObject;
        MinPointer = transform.Find("Minute").gameObject;
    }

    public void StartTimer()
    {
        StartCoroutine(RunTime());
    }

    public void StopTime()
    {
        running = false;
    }

    public void SetTime(int timeSet)
    {
        StartCoroutine(SettingTime(0, timeSet));
    }

    IEnumerator RunTime()
    {
        if(settingTime) { wasRunning = true; yield break; }
        running = true;

        for (; currentTime > 0; currentTime--)
        {
            yield return new WaitForSeconds(1);
            if (!running) break;
            SecPointer.transform.eulerAngles -= Vector3.forward * 6;
            MinPointer.transform.eulerAngles -= Vector3.forward / 2f;// /60 * 6 * 5
        }
        if (!running) yield break;
        
        TimeUp.Invoke();
        running = false;
        yield break;
    }

    IEnumerator SettingTime(int oldTime, int newTime, int steps = 400)
    {
        yield return new WaitForEndOfFrame();
        wasRunning = running;
        running = false;
        settingTime = true;

        int diff = newTime - oldTime;
        currentTime += diff;
        float diffSecond = (diff * 6f) / steps;
        float diffMinute = (diff / 2f) / steps;
        float oldMin = oldTime / 2f;
        oldTime = oldTime * 6 + 180;
        
        SecPointer.transform.eulerAngles = Vector3.forward * oldTime;
        MinPointer.transform.eulerAngles = Vector3.forward * oldMin;

        for (int i = 0; i < steps; i++)
        {
            SecPointer.transform.eulerAngles = Vector3.forward * (oldTime + diffSecond * i);
            MinPointer.transform.eulerAngles = Vector3.forward * (oldMin  + diffMinute * i);
            yield return new WaitForEndOfFrame();
        }

        running = wasRunning;
        settingTime = false;
        if (wasRunning) StartCoroutine(RunTime());
        yield break;
    }

    public void AddTime(int addedTime)
    {
        StartCoroutine(SettingTime(currentTime, currentTime + addedTime, 30));
    }
}
