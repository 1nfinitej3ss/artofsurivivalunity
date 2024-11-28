using UnityEngine;

public class SceneTrigger : MonoBehaviour {
    // String to store the name of the scene to load
    public string sceneToLoad;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            GameObject panel = GameObject.FindGameObjectWithTag("Gallery");
            if (panel != null) {
                SceneSwitcher sceneSwitcher = panel.GetComponent<SceneSwitcher>();
                if (sceneSwitcher != null) {
                    sceneSwitcher.PromptSceneSwitch(sceneToLoad);
                }
            }
        }
    }
}
