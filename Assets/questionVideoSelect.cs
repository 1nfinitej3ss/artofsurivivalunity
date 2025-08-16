using UnityEngine;
using UnityEngine.Video;
using System.Linq;

public class questionVideoSelect : MonoBehaviour
{
    [SerializeField] private VideoClip[] videoClips; // Array to store video clips
    [SerializeField] private VideoPlayer videoPlayer; // Reference to the Video Player component

    private static int lastSelectedIndex = -1; // Static to persist between scenes
    public static int LastSelectedIndex => lastSelectedIndex; // Public getter for the index

    private void Awake()
    {
        // Get the VideoPlayer component if not assigned
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
    }

    private void Start()
    {
        SelectRandomVideo();
    }

    public void SelectRandomVideo()
    {
        if (videoClips == null || videoClips.Length == 0)
        {
            Debug.LogWarning("No video clips assigned to questionVideoSelect script!");
            return;
        }

        // Select a random video from the array
        lastSelectedIndex = Random.Range(0, videoClips.Length);
        VideoClip selectedClip = videoClips[lastSelectedIndex];

        // Assign the selected clip to the video player
        videoPlayer.clip = selectedClip;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
