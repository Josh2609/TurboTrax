using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Prototype.NetworkLobby;
using UnityEngine.UI;


public class NetworkGameManager : NetworkBehaviour
{
    static public List<NetworkCar> sCars = new List<NetworkCar>();
    static public List<string> PlayerRanks = new List<string>();
    static public NetworkGameManager sInstance = null;

    public GameObject uiScoreZone;
    public Font uiScoreFont;

    [Space]

    protected bool _running = true;

    void Awake()
    {
        sInstance = this;
    }

    void Start()
    {
        for (int i = 0; i < sCars.Count; ++i)
        {
            sCars[i].Init();
        }
    }

    [ServerCallback]
    void Update()
    {
        if (!_running || sCars.Count == 0)
            return;

        bool allDestroyed = true;

        for (int i = 0; i < sCars.Count; ++i)
        {
            allDestroyed &= (sCars[i].lapCount == 0);
        }

        if (allDestroyed)
        {
            StartCoroutine(ReturnToLobby());
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public Text Leaderboard;

    public void displayRanks()
    {
        Leaderboard.text = "1st " + PlayerRanks[0];
    }

    IEnumerator ReturnToLobby()
    {
        displayRanks();
        _running = false;
        yield return new WaitForSeconds(5.0f);
        LobbyManager.s_Singleton.ServerReturnToLobby();
    }

    public IEnumerator WaitForRespawn(NetworkCar car)
    {
        yield return new WaitForSeconds(4.0f);

        car.Respawn();
    }
}
