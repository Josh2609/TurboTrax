using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(Rigidbody2D))]
public class NetworkCar : NetworkBehaviour {

    public float acceleration = 5f;
    public float maxSpeed = 7.0f;
    public float speed = 7.0f;
    public float turning = 1.0f;
    public float friction = 2.0f;
    public Vector2 currentSpeed;

    public Text Leaderboard;
    static int powerUp;
    float _powerUpTimer;

    //**
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public GameObject rocketPrefab;
    public Transform rocketSpawn;
    public GameObject minePrefab;
    public Transform mineSpawn;
    //**
    public Camera camera;

   // public GameObject bulletPrefab;

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

    bool finished = false;
    //**
    int bullets;
    public trackLapTrigger first;
    trackLapTrigger next;
    float currentLapTime = 0f;
    float raceTime = 0f;
    int currentLap = 1;
    public Text lapCounterUI;
    //**
    Sprite[] carSprites;


    void Awake()
    {
        //register the spaceship in the gamemanager, that will allow to loop on it.
        carSprites = Resources.LoadAll<Sprite>("Car");
        NetworkGameManager.sCars.Add(this);
        
        camera.enabled = false;
        Leaderboard = (Text)GameObject.Find("Leaderboard").GetComponent(typeof(Text));
       
    }
    
    public override void OnStartLocalPlayer()
    {
        camera.enabled = true;
    }

    public Transform spawnPoint;
    void Start()
    {
        powerUp = -1;
        _powerUpTimer = 8.0f;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();

        lapCounterUI.text = currentLap + "/" + lapCount;
        first = (trackLapTrigger)GameObject.Find("StartFinish").GetComponent(typeof(trackLapTrigger));
        SetNextTrigger(first);
        UpdateText();

        setColor();
        //Leaderboard.enabled = false;
        ClientScene.RegisterPrefab(bulletPrefab);
        ClientScene.RegisterPrefab(rocketPrefab);
        ClientScene.RegisterPrefab(minePrefab);
        if (NetworkGameManager.sInstance != null)
        {//we MAY be awake late (see comment on _wasInit above), so if the instance is already there we init
            Init();
        }
        MPFinish.addToPlayerList(playerName);
        int joinPosition = MPFinish.getIndexOfPlayer(playerName);
        Debug.Log("joinPosition. = " + joinPosition);
        spawnPoint = (Transform)GameObject.Find("Spawn" + joinPosition).GetComponent(typeof(Transform));
        this.transform.position = spawnPoint.position;
        this.transform.rotation = spawnPoint.rotation;
    }

    void setColor()
    {
        if (color == Color.blue)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[0];
        else if (color == Color.cyan)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[1];
        else if (color == Color.green)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[2];
        else if (color == Color.magenta)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[6];
        else if (color == Color.red)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[8];
        else if (color == Color.white)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[9];
        else if (color == Color.yellow)
            this.GetComponent<SpriteRenderer>().sprite = carSprites[10];
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
        //if (!NetworkGameManager.PlayerNames.Contains(playerName))
        //{

        //    NetworkGameManager.PlayerNames.Add(playerName);
        //}
        //Leaderboard.text = "";
        //for (int i = 0; i < NetworkGameManager.PlayerNames.Count; i++)
        //{
        //    Leaderboard.text += i + ": " + NetworkGameManager.PlayerNames[i] + "\n";
        //}

        if (!isLocalPlayer || !_canControl)
            return;
        // ********** MOVEMENT START **********
        float h = -Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
 
         Vector2 speed = transform.up * (v * acceleration);
         _rigidbody2D.AddForce(speed);
 
         float direction = Vector2.Dot(_rigidbody2D.velocity, _rigidbody2D.GetRelativeVector(Vector2.up));
         if(direction >= 0.0f) {
             //_rigidbody2D.rotation += h * turning * (_rigidbody2D.velocity.magnitude / 5.0f);
             _rigidbody2D.AddTorque((h * turning) * (_rigidbody2D.velocity.magnitude / 10.0f));
         } else {
             //_rigidbody2D.rotation -= h * turning * (_rigidbody2D.velocity.magnitude / 5.0f);
             _rigidbody2D.AddTorque((-h * turning) * (_rigidbody2D.velocity.magnitude / 10.0f));
         }
 
         Vector2 forward = new Vector2(0.0f, 0.5f);
         float steeringRightAngle;
         if(_rigidbody2D.angularVelocity > 0) {
             steeringRightAngle = -90;
         } else {
             steeringRightAngle = 90;
         }
 
         Vector2 rightAngleFromForward = Quaternion.AngleAxis(steeringRightAngle, Vector3.forward) * forward;
         Debug.DrawLine((Vector3)_rigidbody2D.position, (Vector3)_rigidbody2D.GetRelativePoint(rightAngleFromForward), Color.green);
 
         float driftForce = Vector2.Dot(_rigidbody2D.velocity, _rigidbody2D.GetRelativeVector(rightAngleFromForward.normalized));
 
         Vector2 relativeForce = (rightAngleFromForward.normalized * -1.0f) * (driftForce * 10.0f);
 
 
         Debug.DrawLine((Vector3)_rigidbody2D.position, (Vector3)_rigidbody2D.GetRelativePoint(relativeForce), Color.red);
 
         _rigidbody2D.AddForce(_rigidbody2D.GetRelativeVector(relativeForce));
     
        // ********** MOVEMENT END **********

         if (Input.GetKeyDown(KeyCode.G))
         {
             powerUp = 2;//setPowerUp();
             Debug.Log("Powerup Update == " + powerUp);
         }
         if (Input.GetKeyDown(KeyCode.Y))
         {
             powerUp = 4;//setPowerUp();
             Debug.Log("Powerup Update == " + powerUp);
         }

         if (Input.GetKeyDown(KeyCode.H))
         {
             powerUp = -1;//setPowerUp();
             _powerUpTimer = 0.0f;
         }
        if (powerUp == -1 && _powerUpTimer <= 0.0f)
        {
            powerUp = setPowerUp();
        } else if(powerUp >= 0 && powerUp <= 7)
        {
            if (Input.GetButton("Jump"))
            {
                powerUp = usePowerUp(powerUp);
            }
        } else if (_powerUpTimer > 0.0f)
        {
            _powerUpTimer -= Time.deltaTime;
        }

    }

    [ClientCallback]
    void FixedUpdate()
    {
        if (!hasAuthority)
            return;
    }

    void UpdateText()
    {
        lapCounterUI.text = string.Format("Lap {0}/{1}", currentLap, lapCount);
    }

    public int setPowerUp()
    {
        /* Randomly generated number corresponds to certain power up
         * 0 = guns(50 ammo?)
         * 1 = mine
         * 2 = health refill
         * 3 = temp (how long?) speed boost
         * 4 = rocket (instant kill)
         * 5 = tacks? speed debuff for car hit
         * 6 = armor?
         */
        int randomNumber = 4;//Random.Range(0, 7);
        Debug.Log("Powerup == " + randomNumber);
        if (randomNumber == 0)
        {
            bullets = 10;
        }
        NetworkGameManager.PlayerNames.Add("Num = " + randomNumber);

        return randomNumber;
    }

    public int usePowerUp(int powerUp)
    {
        if (powerUp == 0) // * Shooting
        {
            if (_shootingTimer <= 0 && bullets > 0)
            {
                _shootingTimer = 0.2f;
                CmdFireGun();
                bullets--;
                Debug.Log("Bullets == " + bullets);
            }else if (bullets <= 0)
            {
                Debug.Log("Bullets 000");
                powerUp = -1;
                _powerUpTimer = 8.0f;
                Debug.Log("powerUp in bullets == " + powerUp);
                return powerUp;
            }
            if (_shootingTimer > 0)
                _shootingTimer -= Time.deltaTime;
        } else if (powerUp == 1)
        {
            CmdDropMine();
            powerUp = -1;
            _powerUpTimer = 8.0f;
            return powerUp;
        } else if (powerUp == 2)
        {
            Debug.Log("RefillHealth");
            CmdRefillHealth();
            powerUp = -1;
            _powerUpTimer = 8.0f;
            return powerUp;
        }
        else if (powerUp == 4)
        {
            CmdFireRocket();
            powerUp = -1;
            _powerUpTimer = 8.0f;
            return powerUp;
        }
        return powerUp;
    }

    public string lastCheckpoint;
    public void OnLapTrigger(trackLapTrigger trigger)
    {
        Debug.Log("PlayerName trig = " + playerName);
        if (trigger == next)
        {
            lastCheckpoint = trigger.getCheckpointName();
            if (first == next)
            {
                if (currentLap == lapCount)
                {
                    finished = true;
                    MPFinish finish = gameObject.GetComponent<MPFinish>();
                    finish.RpcPlayerFinished(playerName);
                    //NetworkGameManager.PlayerRanks.Add(playerName);
                    //MPFinish.finishPositions.Add(playerName);
                    //for (int i = 0; i < MPFinish.finishPositions.Count; i++)
                    //{
                     //   Debug.Log("Finished " + i + " " + MPFinish.finishPositions[i]);
                        //Leaderboard.text += "\nDicks: " + NetworkGameManager.PlayerRanks[i];
                   // }
                    //Leaderboard.text += "\nDicks:" + playerName;
                    //Leaderboard.enabled = true;
                    //NetworkGameManager.PlayerRanks.Add(playerName);
                    //Debug.Log("Dongers: " + NetworkGameManager.PlayerRanks.Count);
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
        _collider2D.enabled = enable;
        //_collider2D.enabled = isServer && enable;
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

    [Command]
    void CmdRefillHealth()
    {
        var health = this.GetComponent<Health>();
        health.refillHealth();
    }

    [Command]
    void CmdFireGun()
    {
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * 6;
        NetworkServer.Spawn(bullet);
        Destroy(bullet, 2.0f);
    }

    [Command]
    void CmdFireRocket()
    {
        var rocket = (GameObject)Instantiate(
            rocketPrefab,
            rocketSpawn.position,
            rocketSpawn.rotation);

        rocket.GetComponent<Rigidbody2D>().velocity = rocket.transform.up * 6;
        NetworkServer.Spawn(rocket);
        Destroy(rocket, 2.0f);
    }

    [Command]
    void CmdDropMine()
    {
        var mine = (GameObject)Instantiate(
            minePrefab,
            mineSpawn.position,
            mineSpawn.rotation);

        NetworkServer.Spawn(mine);
        //Destroy(mine, 2.0f);
    }

    [Command]
    void CmdSpeedBoost()
    {
        //Destroy(mine, 2.0f);
    }


    public void CreateBullets()
    {
        //Vector3[] vectorBase = { _rigidbody2D.rotation * Vector3.right, _rigidbody2D.rotation * Vector3.up, _rigidbody2D.rotation * Vector3.forward };
        //Vector3[] offsets = { -1.5f * vectorBase[0] + -0.5f * vectorBase[2], 1.5f * vectorBase[0] + -0.5f * vectorBase[2] };

        //for (int i = 0; i < 2; ++i)
        //{
        //    GameObject bullet = Instantiate(bulletPrefab, _rigidbody2D.position + offsets[i], Quaternion.identity) as GameObject;
        //    NetworkCarBullet bulletScript = bullet.GetComponent<NetworkCarBullet>();

        //    bulletScript.originalDirection = vectorBase[2];
        //    bulletScript.owner = this;

        //    NetworkServer.SpawnWithClientAuthority(bullet, connectionToClient);
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
        //this.transform.rotation = lastCheckpoint.transform.rotation;
        //EnableCar(true);
    }
}
