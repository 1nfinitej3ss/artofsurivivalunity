using UnityEngine;
using System.Collections;

public class CardWiggle : MonoBehaviour
{
    [Header("Wiggle Settings")]
    [SerializeField] private float wiggleDuration = 0.3f;
    [SerializeField] private float wiggleAmount = 15f;
    [SerializeField] private float wiggleSpeed = 40f;

    private Quaternion originalRotation;
    private bool hasBeenClicked = false;

    private void Awake()
    {
        originalRotation = transform.rotation;
    }

    public void OnClick()
    {
        if (!hasBeenClicked)
        {
            hasBeenClicked = true;
            StartCoroutine(WiggleRoutine());
        }
    }

    private IEnumerator WiggleRoutine()
    {
        float elapsed = 0f;
        
        while (elapsed < wiggleDuration)
        {
            float rotationAmount = Mathf.Sin(elapsed * wiggleSpeed) * wiggleAmount;
            transform.rotation = originalRotation * Quaternion.Euler(0f, 0f, rotationAmount);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = originalRotation;
    }
} 