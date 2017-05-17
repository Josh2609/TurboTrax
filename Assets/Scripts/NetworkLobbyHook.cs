using Prototype.NetworkLobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkLobbyHook : LobbyHook {

    public override void OnLobbyServerSceneLoadedForPlayer(UnityEngine.Networking.NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {

        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        NetworkCar car = gamePlayer.GetComponent<NetworkCar>();
        lapCounter playerLapCounter = gamePlayer.GetComponent<lapCounter>();

        car.playerName = lobby.playerName;
        //playerLapCounter.playerName = lobby.playerName;
        car.color = lobby.playerColor;
        car.lapCount = 3;
    }
}
