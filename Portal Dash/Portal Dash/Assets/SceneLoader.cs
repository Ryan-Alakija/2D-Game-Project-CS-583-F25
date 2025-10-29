using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    public bool waitForInputOnSceneEnd = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        //if the current scene is PostGame, wait for input to load next scene
        if (sceneName == "PostGame")
            waitForInputOnSceneEnd = true;
        else
            waitForInputOnSceneEnd = false;

        if (waitForInputOnSceneEnd && Input.anyKeyDown)
        {
            LoadNextScene();
        }
    }

    public void LoadNextScene()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        //check if we are on the final scene
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        //reload the first scene if we are at the end
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    public void ReloadCurrentScene()
    {
        Debug.Log("Reloading current scene, index: " + SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
