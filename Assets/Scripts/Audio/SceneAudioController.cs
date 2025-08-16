using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneAudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip mix1;
    [SerializeField] private AudioClip mix2;

    // Static field to track which mix was last played
    private static bool wasLastMix1 = true;

    private void Start()
    {
        // List of scenes that should alternate between mix1 and mix2
        string[] mixScenes = { "main", "home", "gallery", "studio", "work", "social" };
        string currentScene = SceneManager.GetActiveScene().name.ToLower();

        // Check if this is a scene that should play mix music
        if (System.Array.Exists(mixScenes, scene => scene == currentScene))
        {
            PlayNextMix();
        }
    }

    private void PlayNextMix()
    {
        if (wasLastMix1)
        {
            musicSource.clip = mix2;
            wasLastMix1 = false;
            Debug.Log("Playing Mix2");
        }
        else
        {
            musicSource.clip = mix1;
            wasLastMix1 = true;
            Debug.Log("Playing Mix1");
        }

        musicSource.Play();
    }
} 