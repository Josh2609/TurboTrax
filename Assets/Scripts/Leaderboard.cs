using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Leaderboard : NetworkBehaviour
{

    Text FinishPositions;
	// Use this for initialization
	void Start () {
        FinishPositions = (Text)GameObject.Find("PlayerPositions").GetComponent(typeof(Text));
	}
	// Update is called once per frame
	void Update () {

        FinishPositions.text = "";
        for (int i = 0; i < NetworkGameManager.FinishPositionsName.Count; i++)
        {
            FinishPositions.text += "Finished " + i + " " + NetworkGameManager.FinishPositionsName[i];
            Debug.Log("Finished " + i + " " + NetworkGameManager.FinishPositionsName[i]);
        }
	}
}
