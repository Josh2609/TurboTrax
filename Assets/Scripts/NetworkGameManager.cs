using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Prototype.NetworkLobby;


public class NetworkGameManager : NetworkBehaviour
{
    static public List<NetworkCar> sCars = new List<NetworkCar>();
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

    IEnumerator ReturnToLobby()
    {
        _running = false;
        yield return new WaitForSeconds(3.0f);
        LobbyManager.s_Singleton.ServerReturnToLobby();
    }

    public IEnumerator WaitForRespawn(NetworkCar car)
    {
        yield return new WaitForSeconds(4.0f);

        car.Respawn();
    }
}
