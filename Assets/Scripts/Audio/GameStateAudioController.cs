using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip victorySound;

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name.ToLower();

        if (currentScene == "gameover" && gameOverSound != null)
        {
            sfxSource.clip = gameOverSound;
            sfxSource.Play();
        }
        else if (currentScene == "gamevictory" && victorySound != null)
        {
            sfxSource.clip = victorySound;
            sfxSource.Play();
        }
    }
} 