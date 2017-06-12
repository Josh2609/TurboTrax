using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Prototype.NetworkLobby;
using UnityEngine.UI;


public class NetworkGameManager : NetworkBehaviour
{
    static public List<NetworkCar> sCars = new List<NetworkCar>();
    static public List<NetworkCar> FinishPositions = new List<NetworkCar>();

    
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
        for (int i = 0; i < sCars.Count; i++)
        {
            if (sCars[i].finished && !PlayerRanks.Contains(sCars[i].playerName))
            {  
                PlayerRanks.Add(sCars[i].playerName);
                sCars[i].PlayerRanks = PlayerRanks;//
            }
        }
        if (!_running || sCars.Count == 0)
            return;

        bool allDestroyed = true;

        for (int i = 0; i < sCars.Count; ++i)
        {
            allDestroyed &= (sCars[i].lapCount == 0);
        }

        if (allDestroyed)
        {
            displayRanks();
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
        Leaderboard = (Text)GameObject.Find("Leaderboard").GetComponent(typeof(Text));
        for (int i = 0; i < MPFinish.finishPositions.Count; i++)
        {
            
            Leaderboard.text += "1st " + MPFinish.finishPositions[i];
        }
    }

    IEnumerator ReturnToLobby()
    {
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
