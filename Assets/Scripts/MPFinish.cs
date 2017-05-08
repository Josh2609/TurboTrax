using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPFinish : MonoBehaviour {

    public static List<string> finishPositions = new List<string>();
    public static List<string> playerList = new List<string>();

    public static void playerFinished(string playerName)
    {
        Debug.Log("Test");
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
}
