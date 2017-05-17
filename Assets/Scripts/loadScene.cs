using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loadScene : MonoBehaviour {

    public void nextScene(string scene)
    {
        SceneManager.LoadScene(scene);
        if (scene == "MainMenu")
            Destroy(GameObject.Find("LobbyManager"));
    }
}
