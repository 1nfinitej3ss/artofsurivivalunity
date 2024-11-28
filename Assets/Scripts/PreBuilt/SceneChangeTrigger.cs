using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTrigger : MonoBehaviour {
    public string newSceneName;

    void OnTriggerEnter2D(Collider2D other) {
        // Check if the other object is the player
        // This assumes the player has a tag of "Player"
        if (other.CompareTag("Player")) {
            SceneManager.LoadScene(newSceneName);
        }
    }
}
