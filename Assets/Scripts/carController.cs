using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;



public class carController : NetworkBehaviour{

    [SyncVar]
    public string playerName = "player";

    [SyncVar]
    public Color playerColor = Color.blue;
   
    //**
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    //**
    public float acceleration = 3;
    public float maxSpeed = 10;
    public float turning = 2;
    public float friction = 3;
    public Vector2 currentSpeed;
    Rigidbody2D rigidbody2D;
    Sprite[] carSprites;
    public TextMesh playerNameMesh;

    public Transform spawnPoint;


    public Camera camera;

    void Awake()
    {
        carSprites = Resources.LoadAll<Sprite>("Car");
        camera.enabled = false;
    }

    void Start()
    {
        MPFinish.addToPlayerList(playerName);
        int joinPosition = MPFinish.getIndexOfPlayer(playerName);
        Debug.Log("joinPosition. = " + joinPosition);
        spawnPoint = (Transform)GameObject.Find("Spawn" + joinPosition).GetComponent(typeof(Transform));
        this.transform.position = spawnPoint.position;


        playerNameMesh.text = playerName;
         setColor();
        //this.transform.position = ;
    }


    public override void OnStartLocalPlayer()
    {
        
        playerNameMesh.text = playerName;

        
        //first = (trackLapTrigger)GameObject.Find("StartFinish").GetComponent(typeof(trackLapTrigger));


        camera.enabled = true;
        
        rigidbody2D = GetComponent<Rigidbody2D>();  
    }

    void setColor()
    {
        if (playerColor == Color.blue)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[0];
        else if (playerColor == Color.cyan)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[1];
        else if (playerColor == Color.green)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[2];
        else if (playerColor == Color.magenta)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[6];
        else if (playerColor == Color.red)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[8];
        else if (playerColor == Color.white)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[9];
        else if (playerColor == Color.yellow)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[10];
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

        // ********** POWER UP START **********
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireGun();
        }
        // ********** POWER UP END **********
    }

    void FireGun()
    {
        // Create the Bullet from the Bullet Prefab
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * 6;

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 2.0f);
    }

    public void playerFinish()
    {

    }

}
