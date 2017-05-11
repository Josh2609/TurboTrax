using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loadScene : MonoBehaviour {

    public static void nextScene(string scene)
    {
        lapCounter.maxLaps = 5;
        SceneManager.LoadScene(scene);
    }
}
