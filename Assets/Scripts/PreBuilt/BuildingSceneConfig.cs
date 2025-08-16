using UnityEngine;

public class BuildingSceneConfig : MonoBehaviour
{
    [Header("Building Scene Configuration")]
    [SerializeField] private string sceneName = "home";
    [SerializeField] private bool enableDirectLoading = true;
    [SerializeField] private bool requirePlayerCollision = true;
    
    public string SceneName => sceneName;
    public bool EnableDirectLoading => enableDirectLoading;
    public bool RequirePlayerCollision => requirePlayerCollision;
} 