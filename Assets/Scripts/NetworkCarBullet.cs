using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCarBullet : MonoBehaviour {

    public Vector3 originalDirection;

    //The spaceship that shoot that bullet, use to attribute point correctly
    public NetworkCar owner;

    void Start()
    {
        Destroy(gameObject, 3.0f);
        GetComponent<Rigidbody2D>().velocity = originalDirection * 200.0f;
        transform.forward = originalDirection;
    }

    void OnCollisionEnter()
    {
        Destroy(gameObject);
    }
}
