﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class lapCounter : MonoBehaviour {

    public string playerName = "player";

    public trackLapTrigger first;
    trackLapTrigger next;
    float currentLapTime = 0f;
    float raceTime = 0f;
    public static int maxLaps = 1;
    int currentLap = 1;

    public Text lapCounterUI;
    // Use this for initialization
    void Start()
    {
        lapCounterUI.text = currentLap + "/" + maxLaps;
        first = (trackLapTrigger)GameObject.Find("StartFinish").GetComponent(typeof(trackLapTrigger));
        SetNextTrigger(first);
        UpdateText();
    }

    public void Update()
    {
        currentLapTime += Time.deltaTime;
        raceTime += Time.deltaTime;
    }

    // update lap counter text
    void UpdateText()
    {
        lapCounterUI.text = string.Format("Lap {0}/{1}", currentLap, maxLaps);
    }

    // when lap trigger is entered
    public void OnLapTrigger(trackLapTrigger trigger)
    {
        Debug.Log("PlayerName trig = " + playerName);
        if (trigger == next)
        {
            if (first == next)
            {
                if (currentLap == maxLaps)
                {
                    //MPFinish.playerFinished(playerName);
                }
                currentLap++;
                currentLapTime = 0f;
                UpdateText();
            }
            SetNextTrigger(next);
        }
    }

    public string timeFloatToString(float floatTime)
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
        Debug.Log("TRIGGERED!");
        next = trigger.next;
        SendMessage("OnNextTrigger", next, SendMessageOptions.DontRequireReceiver);
    }
}
