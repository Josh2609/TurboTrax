using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lapCounter : MonoBehaviour {

    public trackLapTrigger first;
    public TextMesh textMesh;

    trackLapTrigger next;

    int _lap;

    // Use this for initialization
    void Start()
    {
        _lap = 0;
        SetNextTrigger(first);
        UpdateText();
    }

    // update lap counter text
    void UpdateText()
    {
        if (textMesh)
        {
            textMesh.text = string.Format("Lap {0}", _lap);
        }
    }

    // when lap trigger is entered
    public void OnLapTrigger(trackLapTrigger trigger)
    {
        if (trigger == next)
        {
            if (first == next)
            {
                _lap++;
                UpdateText();
            }
            SetNextTrigger(next);
        }
    }

    void SetNextTrigger(trackLapTrigger trigger)
    {
        next = trigger.next;
        SendMessage("OnNextTrigger", next, SendMessageOptions.DontRequireReceiver);
    }
}
