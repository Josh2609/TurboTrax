using Prototype.NetworkLobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkLobbyHook : LobbyHook {

    public override void OnLobbyServerSceneLoadedForPlayer(UnityEngine.Networking.NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        carController localPlayer = gamePlayer.GetComponent<carController>();
        lapCounter playerLapCounter = gamePlayer.GetComponent<lapCounter>();


        localPlayer.playerName = lobby.playerName;
        playerLapCounter.playerName = lobby.playerName;
        localPlayer.playerColor = lobby.playerColor;
    }
}
