using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class carController : NetworkBehaviour{


    public trackLapTrigger first;
    public TextMesh currentLapMesh;
    int _lap;

    public float acceleration = 3;
    public float maxSpeed = 10;
    public float turning = 2;
    public float friction = 3;
    public Vector2 currentSpeed;
    Rigidbody2D rigidbody2D;
    Sprite[] carSprites;

    public static int maxLaps = 3;

    trackLapTrigger next;

    public Camera camera;

    void Awake()
    {
        carSprites = Resources.LoadAll<Sprite>("Car");
        camera.enabled = false;
    }

    void Start()
    {
    }

    void UpdateText()
    {

            currentLapMesh.text = string.Format("Lap {0}/{1}", _lap, maxLaps);

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

    public override void OnStartLocalPlayer()
    {
        camera.enabled = true;
        this.GetComponent<SpriteRenderer>().sprite = carSprites[0];
        rigidbody2D = GetComponent<Rigidbody2D>();
        currentLapMesh = (TextMesh)GameObject.Find("LapCounter").GetComponentInChildren(typeof(TextMesh));
        first = (trackLapTrigger)GameObject.Find("StartFinish").GetComponent(typeof(trackLapTrigger));
        _lap = 0;
        SetNextTrigger(first);
        UpdateText();   
    }

    void FixedUpdate()
    {

        if (!isLocalPlayer)
        {
            return;
        }

        // ********** MOVEMENT START **********
        currentSpeed = new Vector2(rigidbody2D.velocity.x, rigidbody2D.velocity.y);

        if (currentSpeed.magnitude > maxSpeed)
        {
            currentSpeed = currentSpeed.normalized;
            currentSpeed *= maxSpeed;
        }

        if (Input.GetKey(KeyCode.W))
        {
            rigidbody2D.AddForce(transform.up * acceleration);
            rigidbody2D.drag = friction;
        }

        if (Input.GetKey(KeyCode.S))
        {
            rigidbody2D.AddForce(-(transform.up) * (acceleration / 2));
            rigidbody2D.drag = friction;
        }

        if (Input.GetKey(KeyCode.A) && currentSpeed.magnitude > 0)
        {
            transform.Rotate(Vector3.forward * turning);
        }

        if (Input.GetKey(KeyCode.D) && currentSpeed.magnitude > 0)
        {
            transform.Rotate(Vector3.forward * -turning);
        }

        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
        {
            rigidbody2D.drag = friction * 2;
        }
        // ********** MOVEMENT END **********
    }

    

}
