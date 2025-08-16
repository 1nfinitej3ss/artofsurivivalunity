using UnityEngine;

public class Home2ContentManager : MonoBehaviour
{
    public static Home2ContentManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // We'll add content loading methods here
} 