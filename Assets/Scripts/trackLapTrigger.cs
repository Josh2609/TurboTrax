using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trackLapTrigger : MonoBehaviour {

    // next trigger in the lap
    public trackLapTrigger next;

    // when an object enters this trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        carController carController = other.gameObject.GetComponent<carController>();
        if (carController)
        {
            Debug.Log("lap trigger " + gameObject.name);
            carController.OnLapTrigger(this);
        }
    }
}
