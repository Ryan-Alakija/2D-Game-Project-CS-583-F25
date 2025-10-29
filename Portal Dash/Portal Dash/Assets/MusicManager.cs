using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    public class SceneMusic
    {
        public string[] sceneNames;
        public AudioClip musicClip;
    }

    public SceneMusic[] musicMappings;
    public float fadeInDuration = 0.2f;
    public float fadeOutDuration = 1.0f;

    private AudioSource audioSource;
    private static MusicManager Instance;
    private AudioClip currentClip;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        //singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip newClip = GetClipForScene(sceneName);

        Debug.Log("MusicManager: Scene loaded: " + sceneName + ", playing clip: " + (newClip != null ? newClip.name : "none"));

        //if some clip is already playing, don't restart it
        if (newClip == currentClip) return;

        //fade out old clip and fade in new clip
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeToNewClip(newClip));
    }

    private AudioClip GetClipForScene(string sceneName)
    {
        foreach (var mapping in musicMappings)
        {
            foreach (var name in mapping.sceneNames)
            {
                if (name == sceneName)
                {
                    return mapping.musicClip;
                }
            }
        }

        return null;
    }

    private IEnumerator FadeToNewClip(AudioClip newClip)
    {
        //fade out
        float startVolume = audioSource.volume;
        
        for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        currentClip = newClip;

        if (newClip != null)
        {
            audioSource.Play();
            //fade in
            for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(0f, 1f, t / fadeInDuration);
                yield return null;
            }
        }

        audioSource.volume = 1f;
    }
}
