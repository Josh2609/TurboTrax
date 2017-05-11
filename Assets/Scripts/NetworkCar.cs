﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(Rigidbody2D))]
public class NetworkCar : NetworkBehaviour {

    public float acceleration = 6f;
    public float maxSpeed = 10.0f;
    public float speed = 10.0f;
    public float turning = 2.0f;
    public float friction = 1f;
    public Vector2 currentSpeed;

    public Camera camera;

    public GameObject bulletPrefab;

    [SyncVar]
    public string playerName = "player";
    [SyncVar]
    public Color color = Color.blue;
    [SyncVar(hook = "OnLapChanged")]
    public int lapCount;

    protected Rigidbody2D _rigidbody2D;
    protected Collider2D _collider2D;
    protected Text _lapText;

    protected float _rotation = 0;
    protected float _acceleration = 0;

    protected float _shootingTimer = 0;

    protected bool _canControl = true;

    protected bool _wasInit = false;

    //**
    public trackLapTrigger first;
    trackLapTrigger next;
    float currentLapTime = 0f;
    float raceTime = 0f;
    int currentLap = 1;
    public Text lapCounterUI;
    //**

    void Awake()
    {
        //register the spaceship in the gamemanager, that will allow to loop on it.
        NetworkGameManager.sCars.Add(this);
        camera.enabled = false;
    }
    
    public override void OnStartLocalPlayer()
    {
        camera.enabled = true;
    }

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();

        lapCounterUI.text = currentLap + "/" + lapCount;
        first = (trackLapTrigger)GameObject.Find("StartFinish").GetComponent(typeof(trackLapTrigger));
        SetNextTrigger(first);
        UpdateText();

        Renderer[] rends = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
            r.material.color = color;

         //We don't want to handle collision on client, so disable collider there
        _collider2D.enabled = isServer;

        if (NetworkGameManager.sInstance != null)
        {//we MAY be awake late (see comment on _wasInit above), so if the instance is already there we init
            Init();
        }
    }

    public void Init()
    {
        if (_wasInit)
            return;

        GameObject lapGO = new GameObject(playerName + "lap");
        lapGO.transform.SetParent(NetworkGameManager.sInstance.uiScoreZone.transform, false);
        _lapText = lapGO.AddComponent<Text>();
        _lapText.alignment = TextAnchor.MiddleCenter;
        _lapText.font = NetworkGameManager.sInstance.uiScoreFont;
        _lapText.resizeTextForBestFit = true;
        _lapText.color = color;
        _wasInit = true;

        UpdateLapText();
    }

    void OnDestroy()
    {
        NetworkGameManager.sCars.Remove(this);
    }

    [ClientCallback]
    void Update()
    {
        if (!isLocalPlayer || !_canControl)
            return;
        // ********** MOVEMENT START **********
        currentSpeed = new Vector2(_rigidbody2D.velocity.x, _rigidbody2D.velocity.y);

        if (currentSpeed.magnitude > maxSpeed)
        {
            currentSpeed = currentSpeed.normalized;
            currentSpeed *= maxSpeed;
        }

        if (Input.GetKey(KeyCode.W))
        {
            _rigidbody2D.AddForce(transform.up * acceleration);
            _rigidbody2D.drag = friction;
        }

        if (Input.GetKey(KeyCode.S))
        {
            _rigidbody2D.AddForce(-(transform.up) * (acceleration / 2));
            _rigidbody2D.drag = friction;
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
            _rigidbody2D.drag = friction * 2;
        }
        // ********** MOVEMENT END **********

        if (Input.GetButton("Jump") && _shootingTimer <= 0)
        {
            _shootingTimer = 0.2f;
            //we call a Command, that will be executed only on server, to spawn a new bullet
            //we pass the position&forward to be sure to shoot from the right one (server can lag one frame behind)
            CmdFire(transform.position, transform.forward, _rigidbody2D.velocity);
        }

        if (_shootingTimer > 0)
            _shootingTimer -= Time.deltaTime;
    }

    [ClientCallback]
    void FixedUpdate()
    {
        if (!hasAuthority)
            return;

        if (!_canControl)
        {//if we can't control, mean we're destroyed, so make sure the ship stay in spawn place
            //_rigidbody.rotation = Quaternion.identity;
            //_rigidbody.position = Vector3.zero;
            //_rigidbody.velocity = Vector3.zero;
            //_rigidbody.angularVelocity = Vector3.zero;
        }
        else
        {
            //Quaternion rotation = _rigidbody.rotation * Quaternion.Euler(0, _rotation * turning * Time.fixedDeltaTime, 0);
            //_rigidbody.MoveRotation(rotation);

            //_rigidbody.AddForce((rotation * Vector3.forward) * _acceleration * 1000.0f * /*?*/speed * Time.deltaTime);

            //if (_rigidbody.velocity.magnitude > maxSpeed * 1000.0f)
            //{
              //  _rigidbody.velocity = _rigidbody.velocity.normalized * maxSpeed * 1000.0f;
            //}


            CheckExitScreen();
        }
    }

    void UpdateText()
    {
        lapCounterUI.text = string.Format("Lap {0}/{1}", currentLap, lapCount);
    }

    public void OnLapTrigger(trackLapTrigger trigger)
    {
        Debug.Log("PlayerName trig = " + playerName);
        if (trigger == next)
        {
            if (first == next)
            {
                if (currentLap == lapCount)
                {
                    Kill();
                }
                currentLap++;
                currentLapTime = 0f;
                UpdateText();
            }
            SetNextTrigger(next);
        }
    }

    void SetNextTrigger(trackLapTrigger trigger)
    {
        Debug.Log("TRIGGERED!");
        next = trigger.next;
        SendMessage("OnNextTrigger", next, SendMessageOptions.DontRequireReceiver);
    }
    void CheckExitScreen()
    {
        //if (Camera.main == null)
        //    return;

        //if (Mathf.Abs(_rigidbody.position.x) > Camera.main.orthographicSize * Camera.main.aspect)
        //{
        //    _rigidbody.position = new Vector3(-Mathf.Sign(_rigidbody.position.x) * Camera.main.orthographicSize * Camera.main.aspect, 0, _rigidbody.position.z);
        //    _rigidbody.position -= _rigidbody.position.normalized * 0.1f; // offset a little bit to avoid looping back & forth between the 2 edges 
        //}

        //if (Mathf.Abs(_rigidbody.position.z) > Camera.main.orthographicSize)
        //{
        //    _rigidbody.position = new Vector3(_rigidbody.position.x, _rigidbody.position.y, -Mathf.Sign(_rigidbody.position.z) * Camera.main.orthographicSize);
        //    _rigidbody.position -= _rigidbody.position.normalized * 0.1f; // offset a little bit to avoid looping back & forth between the 2 edges 
        //}
    }

    void OnLapChanged(int newValue)
    {
        lapCount = newValue;
        UpdateLapText();
    }

    void UpdateLapText()
    {
        if (_lapText != null)
        {
            _lapText.text = playerName + lapCount + "/3";
        }
    }

    public void EnableCar(bool enable)
    {
        GetComponent<Renderer>().enabled = enable;
        _collider2D.enabled = isServer && enable;
        //trailGameobject.SetActive(enable);

        _canControl = enable;
    }

    [Client]
    public void LocalDestroy()
    {
        if (!_canControl)
            return;//already destroyed, happen if destroyed Locally, Rpc will call that later

        EnableCar(false);
    }

    [Server]
    public void Kill()
    {
        lapCount -= 1;

        RpcDestroyed();
        EnableCar(false);

        if (lapCount > 0)
        {
            //we start the coroutine on the manager, as disabling a gameobject stop ALL coroutine started by it
            NetworkGameManager.sInstance.StartCoroutine(NetworkGameManager.sInstance.WaitForRespawn(this));
        }
    }

    [Server]
    public void Respawn()
    {
        EnableCar(true);
        RpcRespawn();
    }

    public void CreateBullets()
    {
        //Vector3[] vectorBase = { _rigidbody.rotation * Vector3.right, _rigidbody.rotation * Vector3.up, _rigidbody.rotation * Vector3.forward };
        //Vector3[] offsets = { -1.5f * vectorBase[0] + -0.5f * vectorBase[2], 1.5f * vectorBase[0] + -0.5f * vectorBase[2] };

        //for (int i = 0; i < 2; ++i)
        //{
        //    GameObject bullet = Instantiate(bulletPrefab, _rigidbody.position + offsets[i], Quaternion.identity) as GameObject;
        //    NetworkCarBullet bulletScript = bullet.GetComponent<NetworkCarBullet>();

        //    bulletScript.originalDirection = vectorBase[2];
        //    bulletScript.owner = this;

        //    //NetworkServer.SpawnWithClientAuthority(bullet, connectionToClient);
        //}
    }

    [Command]
    public void CmdFire(Vector3 position, Vector3 forward, Vector3 startingVelocity)
    {
        if (!isClient) //avoid to create bullet twice (here & in Rpc call) on hosting client
            CreateBullets();

        RpcFire();
    }

    [ClientRpc]
    public void RpcFire()
    {
        CreateBullets();
    }

    [ClientRpc]
    void RpcDestroyed()
    {
        LocalDestroy();
    }

    [ClientRpc]
    void RpcRespawn()
    {
        EnableCar(true);
    }
}
