using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PositionManager : NetworkBehaviour 
{
    private static PositionManager _instance;

    public static PositionManager Instance
    {
        get
        {
            if(_instance == null)
            {
                GameObject go = new GameObject("PositionManager");
                go.AddComponent<PositionManager>();
            }
            return _instance;
        }
    }
    public List<string> positions = new List<string>();
    void Awake()
    {
        _instance = this;
    }

    
	
}
