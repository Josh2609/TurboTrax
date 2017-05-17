using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(Rigidbody2D))]
public class NetworkCar : NetworkBehaviour {

    public delegate void changeLapCounterUI(int currentLap, int lapCount);
    public event changeLapCounterUI onLapChange;

    public delegate void changePowerUpUI(int powerup);
    public event changePowerUpUI onPowerUpChange;

    public delegate void changePowerUpTimerUI(float _powerUpTimer);
    public event changePowerUpTimerUI onPowerUpTimerChange;

    Camera MainCamera;
    SpawnManager spawnManager;
    public Text Leaderboard;
    static int powerUp;
    float _powerUpTimer;

    public bool finished = false;

    //**
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public GameObject rocketPrefab;
    public Transform rocketSpawn;
    public GameObject minePrefab;
    public Transform mineSpawn;
    //**
    public Camera camera;

    [SyncVar]
    public string playerName = "player";
    [SyncVar]
    public Color color = Color.blue;
    [SyncVar(hook = "OnLapChanged")]
    public int lapCount;

    protected Rigidbody2D _rigidbody2D;
    protected Collider2D _collider2D;
    protected Text _lapText;


    protected float _shootingTimer = 0;

    protected bool _canControl = true;

    protected bool _wasInit = false;

    //**
    int bullets;
    public trackLapTrigger first;
    trackLapTrigger next;
    int currentLap = 1;
    public Text lapCounterUI;
    //**
    Sprite[] carSprites;

    PlayerView playerView;
    void Awake()
    {
        //register the spaceship in the gamemanager, that will allow to loop on it.
        carSprites = Resources.LoadAll<Sprite>("Car");
        NetworkGameManager.sCars.Add(this);
        MainCamera = (Camera)GameObject.Find("MainCamera").GetComponent(typeof(Camera));
        MainCamera.enabled = false;
        camera.enabled = false;
        Leaderboard = (Text)GameObject.Find("Leaderboard").GetComponent(typeof(Text));
        playerView = gameObject.GetComponent<PlayerView>();
        playerView.manualStart();
    }
    
    public override void OnStartLocalPlayer()
    {
        camera.enabled = true;
    }

    public Transform spawnPoint;
    void Start()
    {
        spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        powerUp = -1;
        _powerUpTimer = 8.0f;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();

        if (onLapChange != null) { onLapChange(currentLap, lapCount); } // informs any observers
        first = (trackLapTrigger)GameObject.Find("StartFinish").GetComponent(typeof(trackLapTrigger));
        SetNextTrigger(first);

        setColor();
        //Leaderboard.enabled = false;
        ClientScene.RegisterPrefab(bulletPrefab);
        ClientScene.RegisterPrefab(rocketPrefab);
        ClientScene.RegisterPrefab(minePrefab);
        if (NetworkGameManager.sInstance != null)
        {//we MAY be awake late (see comment on _wasInit above), so if the instance is already there we init
            Init();
        }
        Debug.Log(Network.player.ipAddress);////
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
    }

    void OnDestroy()
    {
        NetworkGameManager.sCars.Remove(this);
    }


    Vector2 ForwardVelocity()
    {
        return transform.up * Vector2.Dot(GetComponent<Rigidbody2D>().velocity, transform.up);
    }

    Vector2 RightVelocity()
    {
        return transform.right * Vector2.Dot(GetComponent<Rigidbody2D>().velocity, transform.right);
    }

    float speedForce = 3f;
    float torqueForce = -200f;
    float driftFactorSticky = 0.9f;
    float driftFactorSlippy = 0.6f;
    float maxStickyVelocity = 2.5f;

   public List<string> PlayerRanks = new List<string>();

    [ClientCallback]
    void Update()
    {

        if (!isLocalPlayer || !_canControl)
            return;

        // ********** MOVEMENT START **********
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        float driftFactor = driftFactorSticky;
        if (RightVelocity().magnitude > maxStickyVelocity)
        {
            driftFactor = driftFactorSlippy;
        }

        rb.velocity = ForwardVelocity() + RightVelocity() * driftFactor;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            rb.AddForce(transform.up * speedForce);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            rb.AddForce(transform.up * -speedForce / 2f);
        }

        float tf = Mathf.Lerp(0, torqueForce, rb.velocity.magnitude / 2);
        rb.angularVelocity = Input.GetAxis("Horizontal") * tf;
        // ********** MOVEMENT END **********

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
            if (onPowerUpTimerChange != null) { onPowerUpTimerChange(_powerUpTimer); } // informs any observers
            _powerUpTimer -= Time.deltaTime;
        }

    }

    [ClientCallback]
    void FixedUpdate()
    {
        if (!hasAuthority)
            return;
    }

    public int setPowerUp()
    {
        int randomNumber = UnityEngine.Random.Range(0, 7);
        if (randomNumber == 3)
        {
            randomNumber = 0;
        }
        Debug.Log("Powerup == " + randomNumber);
        if (randomNumber == 0)
        {
            bullets = 15;
        }
        if(onPowerUpChange != null) { onPowerUpChange(randomNumber); } //informs any observers
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
            }
            if (_shootingTimer > 0)
                _shootingTimer -= Time.deltaTime;
        } else if (powerUp == 1)
        {
            CmdDropMine();
            powerUp = -1;
            _powerUpTimer = 8.0f;
        } else if (powerUp == 2)
        {
            Debug.Log("RefillHealth");
            CmdRefillHealth();
            powerUp = -1;
            _powerUpTimer = 8.0f;
        }
        else if (powerUp == 4)
        {
            CmdFireRocket();
            powerUp = -1;
            _powerUpTimer = 8.0f;
        }
        if (onPowerUpChange != null) { onPowerUpChange(powerUp); } // informs any observers
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
                    camera.enabled = false;
                    MainCamera.enabled = true;
                    Kill();
                }
                currentLap++;
                if (onLapChange != null) { onLapChange(currentLap, lapCount); } // informs any observers
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
        lapCount = 0;
        RpcDestroyed();
        EnableCar(false);
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
    {//
        var bullet = spawnManager.GetFromPool(bulletSpawn.transform.position);
        bullet.GetComponent<Rigidbody2D>().transform.rotation = bulletSpawn.transform.rotation;
        bullet.GetComponent<Rigidbody2D>().velocity = transform.up * 16;
        NetworkServer.Spawn(bullet, spawnManager.assetId);

        StartCoroutine(Destroy(bullet, 2.0f));
    }

    public IEnumerator Destroy(GameObject go, float timer)
    {
        yield return new WaitForSeconds(timer);
        spawnManager.UnSpawnObject(go);
        NetworkServer.UnSpawn(go);
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
