using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Setup")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float maxVolume = 1f;

    [Header("Scene Music")]
    [SerializeField] private AudioClip mainGameTrack;
    [SerializeField] private AudioClip gameOverTrack;
    [SerializeField] private AudioClip victoryTrack;
    [SerializeField] private AudioClip menuTrack;
    [SerializeField] private AudioClip galleryTrack;
    [SerializeField] private AudioClip chanceCardTrack;

    [Header("Audio Controls")]
    [SerializeField] private KeyCode m_MuteKey = KeyCode.M;
    private bool m_IsMuted;
    private float m_LastVolume;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSource();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSource()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0f;

        // Start playing the main track when game first loads
        PlayTrack(mainGameTrack, randomStart: true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only change music for game over, victory, and start scenes
        switch (scene.name)
        {
            case "GameOver":
                PlayTrack(gameOverTrack, randomStart: false);
                break;
            case "GameVictory":
                PlayTrack(victoryTrack, randomStart: false);
                break;
            case "StartScene":
            case "Start":
                PlayTrack(mainGameTrack, randomStart: true);
                break;
            case "Home":
            case "MainMenu":
                PlayTrack(menuTrack, randomStart: false);
                break;
            case "Gallery":
                PlayTrack(galleryTrack, randomStart: false);
                break;
            case "ChanceCard":
                PlayTrack(chanceCardTrack, randomStart: false);
                break;
            default:
                // Don't change music for other scenes
                break;
        }
    }

    private void PlayTrack(AudioClip track, bool randomStart)
    {
        if (track == null)
        {
            Debug.LogError($"No track assigned for current scene!");
            return;
        }

        // Don't restart if it's already playing this track
        if (musicSource.clip == track && musicSource.isPlaying)
        {
            return;
        }

        // Stop any current fade and music
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        musicSource.Stop();

        // Set up the track
        musicSource.clip = track;
        
        if (randomStart)
        {
            // Calculate random start time (avoiding the very end)
            float safeEndTime = 30f; // Don't start in last 30 seconds
            float maxStartTime = Mathf.Max(0, track.length - safeEndTime);
            float randomStartTime = Random.Range(0f, maxStartTime);
            musicSource.time = randomStartTime;
            Debug.Log($"Started music at {randomStartTime:F1}s / {track.length:F1}s");
        }
        else
        {
            // Start from beginning
            musicSource.time = 0f;
            Debug.Log($"Started music from beginning: {track.name}");
        }
        
        // Check if we're in GameOver or Victory scene for abrupt transition
        string currentScene = SceneManager.GetActiveScene().name;
        bool isAbruptScene = currentScene == "GameOver" || currentScene == "GameVictory";
        
        if (isAbruptScene)
        {
            musicSource.volume = maxVolume;
            musicSource.Play();
        }
        else
        {
            // Start playing with fade for other scenes
            musicSource.volume = 0f;
            musicSource.Play();
            fadeCoroutine = StartCoroutine(FadeIn());
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, maxVolume, elapsedTime / fadeInDuration);
            yield return null;
        }

        musicSource.volume = maxVolume;
        fadeCoroutine = null;
    }

    private void OnDestroy()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnValidate()
    {
        if (mainGameTrack == null)
        {
            Debug.LogWarning("AudioManager: Please assign the main game track!");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(m_MuteKey))
        {
            ToggleMute();
        }
    }

    private void ToggleMute()
    {
        m_IsMuted = !m_IsMuted;
        
        if (m_IsMuted)
        {
            // Store current volume before muting
            m_LastVolume = musicSource.volume;
            musicSource.volume = 0f;
        }
        else
        {
            // Restore previous volume
            musicSource.volume = m_LastVolume;
        }
        
        Debug.Log($"Audio {(m_IsMuted ? "muted" : "unmuted")}");
    }
} 