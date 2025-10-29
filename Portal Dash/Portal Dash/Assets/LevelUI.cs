using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUI : MonoBehaviour
{
    public string[] levelScenes;
    private static LevelUI Instance;
    
    //
    private void Awake()
    {
        //singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        string currentScene = SceneManager.GetActiveScene().name;
        
        //check if current scene is in levelScenes
        bool isLevel = false;
        foreach (string sceneName in levelScenes)
        {
            if (sceneName == currentScene)
            {
                isLevel = true;
                break;
            }
        }

        //only load canvas in level scenes, and don't destroy on load
        if (isLevel)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
