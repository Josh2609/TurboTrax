using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class lapCounter : MonoBehaviour {

    public trackLapTrigger first;
    public TextMesh currentLapMesh;
    public Text lapText;
    //public TextMesh currentLapTimeMesh;
    //public TextMesh raceTimeMesh;
    //public TextMesh bestLapMesh;

    float currentLapTime = 0f;
    float bestLapTime = 0f;
    float raceTime = 0f;
    public static int maxLaps;

    trackLapTrigger next;

    int currentLap = 0;

    // Use this for initialization
    void Start()
    {
       // currentLapMesh = (TextMesh)GameObject.Find("LapCounter").GetComponentInChildren(typeof(TextMesh));
        first = (trackLapTrigger)GameObject.Find("StartFinish").GetComponent(typeof(trackLapTrigger));
        SetNextTrigger(first);
        UpdateText();
    }

    public void Update()
    {
        currentLapTime += Time.deltaTime;
        raceTime += Time.deltaTime;
       // currentLapTimeMesh.text = timeFloatToString(currentLapTime);
        //raceTimeMesh.text = timeFloatToString(raceTime);
    }

    // update lap counter text
    void UpdateText()
    {
        if (currentLapMesh)
        {
            currentLapMesh.text = string.Format("Lap {0}/{1}", currentLap, maxLaps);
        }
        //bestLapMesh.text = timeFloatToString(bestLapTime);
    }

    // when lap trigger is entered
    public void OnLapTrigger(trackLapTrigger trigger)
    {
        if (trigger == next)
        {
            if (first == next)
            {
                if (bestLapTime == 0)
                {
                    bestLapTime = currentLapTime;
                } else if (currentLapTime < bestLapTime)
                {
                    bestLapTime = currentLapTime;
                }
                currentLap++;
                currentLapTime = 0f;
                UpdateText();
            }
            SetNextTrigger(next);
        }
    }

    public String timeFloatToString(float floatTime)
    {
        TimeSpan time;
        time = TimeSpan.FromSeconds(floatTime);
        string answer = string.Format("{0:D2}:{1:D2}:{2:D3}",
                time.Minutes,
                time.Seconds,
                time.Milliseconds);
        return answer;
    }

    void SetNextTrigger(trackLapTrigger trigger)
    {
        next = trigger.next;
        SendMessage("OnNextTrigger", next, SendMessageOptions.DontRequireReceiver);
    }
}
