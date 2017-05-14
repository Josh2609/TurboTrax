using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MPFinish : NetworkBehaviour {

    public static List<string> finishPositions = new List<string>();
    static List<string> playerList = new List<string>();

    [ClientRpc]
    public void RpcPlayerFinished(string playerName)
    {
        finishPositions.Add(playerName);
        Debug.Log("finishPositions.length = " + finishPositions.Count);
        for (int i = 0; i < finishPositions.Count; i++)
        {
            Debug.Log("finishPositions. = " + finishPositions[i]);
        }
    }

    public static void addToPlayerList(string playerName)
    {
        playerList.Add(playerName);
    }

    public static int getIndexOfPlayer(string playerName)
    {
        return playerList.IndexOf(playerName); // returns -1 if not found
    }

    public static bool checkIfPlayerFinished(string playerName) // only works for host for some reason
    {
        return finishPositions.Contains(playerName);
    }

    public static bool checkIfAllFinished()
    {
        return playerList.Count == finishPositions.Count;
    }


}
