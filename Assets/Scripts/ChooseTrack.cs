using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ChooseTrack : NetworkBehaviour {

    public void OnTrackClicked()
    {
        CmdTrackChange();
    }

    [Command]
    public void CmdTrackChange()
    {
        var lobbyManager = this.GetComponent<NetworkLobbyManager>();
        lobbyManager.playScene = "Level1";
    
            
    }
}
