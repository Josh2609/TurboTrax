using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trackLapTrigger : MonoBehaviour {

    public trackLapTrigger next;

    void OnTriggerEnter2D(Collider2D other)
    {
        NetworkCar car = other.gameObject.GetComponent<NetworkCar>();
        if (car)
        {
            Debug.Log("lap trigger " + gameObject.name);
            car.OnLapTrigger(this);
        }
    }
    public string getCheckpointName()
    {
        return gameObject.name;
    }
}
