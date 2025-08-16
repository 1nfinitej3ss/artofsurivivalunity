using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

[RequireComponent(typeof(VideoPlayer))]
public class FeatherEdges : MonoBehaviour
{
    #region Private Fields
    [SerializeField, Range(0f, 0.5f)] private float m_FeatherAmount = 0.2f;
    [SerializeField, Range(0.1f, 2f)] private float m_FeatherSoftness = 1f;
    
    private Material m_FeatherMaterial;
    private VideoPlayer m_VideoPlayer;
    private RawImage m_RawImage;
    private static readonly string c_ShaderPath = "Hidden/VideoFeather";
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        SetupVideo();
    }

    private void OnDestroy()
    {
        if (m_FeatherMaterial != null)
        {
            Destroy(m_FeatherMaterial);
        }
    }
    #endregion

    #region Private Methods
    private void InitializeComponents()
    {
        m_VideoPlayer = GetComponent<VideoPlayer>();
        m_RawImage = GetComponent<RawImage>();
        
        // Create material from shader
        Shader featherShader = Shader.Find(c_ShaderPath);
        if (featherShader != null)
        {
            m_FeatherMaterial = new Material(featherShader);
            if (m_RawImage != null)
            {
                m_RawImage.material = m_FeatherMaterial;
            }
        }
        else
        {
            Debug.LogError("Feather shader not found! Make sure to create the shader at: " + c_ShaderPath);
        }
    }

    private void SetupVideo()
    {
        if (m_VideoPlayer != null && m_RawImage != null)
        {
            // Create a RenderTexture for the video
            RenderTexture videoTexture = new RenderTexture(1920, 1080, 24);
            m_VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            m_VideoPlayer.targetTexture = videoTexture;
            
            // Assign the texture to the RawImage
            m_RawImage.texture = videoTexture;
        }
    }

    private void Update()
    {
        if (m_FeatherMaterial != null)
        {
            m_FeatherMaterial.SetFloat("_FeatherAmount", m_FeatherAmount);
            m_FeatherMaterial.SetFloat("_Softness", m_FeatherSoftness);
        }
    }
    #endregion
}
