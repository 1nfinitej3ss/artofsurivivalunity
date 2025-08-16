using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateTitle : MonoBehaviour
{
    [Header("Arc Settings")]
    public float radius = 5f;
    public float letterSpacing = 6f;
    public float startAngle = 170f;
    public GameObject letterPrefab;
    public float letterScale = 0.12f;
    public Vector3 centerOffset = new Vector3(-1.5f, 2.2f, 0);
    public bool showDebugMarker = true;
    
    [Header("Animation Settings")]
    public float animationDuration = 0.45f;
    public float letterDelay = 0.05f;
    public float loopDelay = 2f;
    public bool loopAnimation = true;
    
    [Header("Wiggle Settings")]
    public float wiggleDuration = 0.4f;
    public float wiggleSpeed = 12f;
    public float wiggleAmount = 0.8f;
    public float spawnOffset = 0.15f;
    public float bounceHeight = 0.08f;

    private readonly string titleText = "ART OF SURVIVAL";
    private List<GameObject> letters = new List<GameObject>();
    private Coroutine animationCoroutine;
    private GameObject centerMarker;
    private GameObject m_TitleObject;
    private bool m_WasHidden = false;
    private GameObject m_StartScene;

    void Start()
    {
        // Cache the title object reference
        m_TitleObject = GameObject.FindGameObjectWithTag("title");
        if (m_TitleObject == null)
        {
            Debug.LogWarning("No GameObject with 'title' tag found!");
        }

        if (showDebugMarker)
        {
            CreateCenterMarker();
        }
        StartAnimation();

        m_StartScene = GameObject.Find("StartScene");
        if (m_StartScene == null)
        {
            Debug.LogWarning("StartScene GameObject not found!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            RestartAnimation();
        }

        // Check for Onboarding objects and update visibility
        bool hasOnboardingObjects = GameObject.FindGameObjectsWithTag("Onboarding").Length > 0;
        
        // Handle title visibility
        if (m_TitleObject != null)
        {
            if (hasOnboardingObjects && !m_WasHidden)
            {
                m_TitleObject.SetActive(false);
                m_WasHidden = true;
            }
            else if (!hasOnboardingObjects && m_WasHidden)
            {
                m_TitleObject.SetActive(true);
                m_WasHidden = false;
            }
        }

        // Handle StartScene visibility
        if (m_StartScene != null)
        {
            m_StartScene.SetActive(!hasOnboardingObjects);
        }
    }

    void StartAnimation()
    {
        Debug.Log("Starting title animation");
        if (letterPrefab == null)
        {
            Debug.LogError("Letter prefab is not assigned!");
            return;
        }

        // Debug prefab setup
        SpriteRenderer prefabRenderer = letterPrefab.GetComponent<SpriteRenderer>();
        if (prefabRenderer == null)
        {
            Debug.LogError("Letter prefab must have a SpriteRenderer component!");
            return;
        }
        Debug.Log("Prefab checks passed successfully");
        
        animationCoroutine = StartCoroutine(AnimateLetters());
    }

    void RestartAnimation()
    {
        // Update marker position when restarting
        if (centerMarker != null)
        {
            centerMarker.transform.localPosition = centerOffset;
        }

        // Stop existing animation if running
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        // Clear existing letters
        foreach (GameObject letter in letters)
        {
            if (letter != null)
            {
                Destroy(letter);
            }
        }
        letters.Clear();

        // Start new animation
        StartAnimation();
    }

    void CreateCenterMarker()
    {
        // Create a sphere to mark the center
        centerMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        centerMarker.name = "Arc Center Marker";
        centerMarker.transform.parent = transform;
        centerMarker.transform.localPosition = centerOffset;
        centerMarker.transform.localScale = Vector3.one * 0.1f; // Small sphere

        // Make it bright red
        Renderer renderer = centerMarker.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }

        // Disable collision
        Collider collider = centerMarker.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }

    void OnDestroy()
    {
        // Clean up
        if (centerMarker != null)
        {
            Destroy(centerMarker);
        }
    }

    IEnumerator AnimateLetters()
    {
        Debug.Log($"Looking for sprites in: {Application.dataPath}/Resources/");
        
        // Test loading of first letter
        Sprite testSprite = Resources.Load<Sprite>("capA");
        if (testSprite == null)
        {
            Debug.LogError("Could not load test sprite 'capA'. Check Resources folder setup!");
            yield break;
        }
        Debug.Log("Successfully loaded test sprite 'capA'");

        while (true)
        {
            // Calculate total arc and starting angle
            int letterCount = titleText.Replace(" ", "").Length;
            float totalArc = (letterCount - 1) * letterSpacing;
            float halfArc = totalArc / 2f;
            float currentAngle = startAngle - halfArc;

            // Reset all letters
            foreach (GameObject letter in letters)
            {
                if (letter != null)
                {
                    Destroy(letter);
                }
            }
            letters.Clear();

            for (int i = 0; i < titleText.Length; i++)
            {
                char letter = titleText[i];
                if (letter != ' ')
                {
                    GameObject letterObj = Instantiate(letterPrefab, transform);
                    letters.Add(letterObj);

                    SpriteRenderer spriteRenderer = letterObj.GetComponent<SpriteRenderer>();
                    if (spriteRenderer == null)
                    {
                        Debug.LogError("SpriteRenderer not found on letter prefab!");
                        continue;
                    }

                    spriteRenderer.sortingLayerName = "UI";
                    spriteRenderer.sortingOrder = 10;

                    // Special case for first 'A'
                    string spritePath;
                    if (i == 0 && letter == 'A')
                    {
                        spritePath = "capA";
                    }
                    else
                    {
                        spritePath = $"{letter.ToString().ToLower()}";
                    }

                    Sprite letterSprite = Resources.Load<Sprite>(spritePath);
                    
                    if (letterSprite == null)
                    {
                        Debug.LogError($"Failed to load sprite for letter '{letter}' at path: {spritePath}");
                        continue;
                    }
                    
                    spriteRenderer.sprite = letterSprite;

                    // Calculate final position
                    float angleRad = -currentAngle * Mathf.Deg2Rad;
                    float x = Mathf.Cos(angleRad) * radius;
                    float y = Mathf.Sin(angleRad) * radius;
                    Vector3 targetPos = new Vector3(x, y, 0) + centerOffset;

                    // Calculate spawn position (slightly offset from target)
                    Vector3 spawnPos = Vector3.Lerp(targetPos, centerOffset, spawnOffset);

                    // Set initial scale and rotation
                    letterObj.transform.localScale = new Vector3(letterScale, letterScale, letterScale);
                    letterObj.transform.rotation = Quaternion.Euler(0, 0, -currentAngle - 90);

                    // Animate from offset position
                    letterObj.transform.localPosition = spawnPos;
                    letterObj.SetActive(true);
                    spriteRenderer.enabled = true;

                    float elapsedTime = 0f;
                    while (elapsedTime < animationDuration)
                    {
                        elapsedTime += Time.deltaTime;
                        float progress = elapsedTime / animationDuration;
                        float easedProgress = EaseOutBack(progress);
                        
                        letterObj.transform.localPosition = Vector3.Lerp(
                            spawnPos,
                            targetPos,
                            easedProgress
                        );

                        yield return null;
                    }

                    letterObj.transform.localPosition = targetPos;

                    // Start wiggle immediately after letter appears
                    StartCoroutine(WiggleLetter(letterObj));

                    yield return new WaitForSeconds(0.1f);
                    currentAngle += letterSpacing;
                }
                else
                {
                    currentAngle += letterSpacing / 2;
                }
            }

            // After all letters are animated
            if (loopAnimation)
            {
                yield return new WaitForSeconds(loopDelay);
            }
            else
            {
                break; // Exit if looping is disabled
            }
        }
    }

    IEnumerator WiggleLetter(GameObject letter)
    {
        if (letter == null) yield break;

        Vector3 originalPosition = letter.transform.localPosition;
        Vector3 originalRotation = letter.transform.rotation.eulerAngles;

        while (loopAnimation) // Continue wiggling if animation is looping
        {
            float elapsed = 0f;
            while (elapsed < wiggleDuration)
            {
                elapsed += Time.deltaTime;
                float percentComplete = elapsed / wiggleDuration;
                
                // Reduce wiggle intensity for continuous animation
                float wiggleIntensity = 0.3f; // Reduced constant intensity for smoother continuous motion
                
                float xOffset = Mathf.Sin(Time.time * wiggleSpeed) * wiggleAmount * wiggleIntensity;
                float yOffset = Mathf.Cos(Time.time * wiggleSpeed * 1.1f) * wiggleAmount * wiggleIntensity;
                float rotationOffset = Mathf.Sin(Time.time * wiggleSpeed * 0.9f) * wiggleAmount * 2f * wiggleIntensity;

                letter.transform.localPosition = originalPosition + new Vector3(
                    xOffset * 0.03f, 
                    yOffset * 0.03f, 
                    0
                );
                
                letter.transform.rotation = Quaternion.Euler(
                    originalRotation.x,
                    originalRotation.y,
                    originalRotation.z + rotationOffset
                );

                yield return null;
            }

            // No need to return to original position since we're continuing the animation
            yield return null;
        }

        // Return to original position when animation stops looping
        letter.transform.localPosition = originalPosition;
        letter.transform.rotation = Quaternion.Euler(originalRotation);
    }

    private float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    private float EaseOutQuad(float x)
    {
        return 1 - (1 - x) * (1 - x);
    }

    #region Public Methods
    
    /// <summary>
    /// Shows the title and restarts the animation
    /// </summary>
    public void ShowTitle()
    {
        // Get the title object if we don't have it
        if (m_TitleObject == null)
        {
            m_TitleObject = GameObject.FindGameObjectWithTag("title");
        }

        if (m_TitleObject != null)
        {
            // Enable the title object and its children
            m_TitleObject.SetActive(true);
            gameObject.SetActive(true);  // Enable this component's GameObject
            m_WasHidden = false;
            
            // Force restart the animation
            RestartAnimation();
        }
        else
        {
            Debug.LogWarning("Title object not found! Make sure there is a GameObject with the 'title' tag.");
        }
    }
    
    #endregion
}
