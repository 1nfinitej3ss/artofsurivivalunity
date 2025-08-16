using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathLoop : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    [SerializeField]
    private string deathAnimationTrigger = "Death"; // The name of your death animation trigger parameter
    
    // Start is called before the first frame update
    void Start()
    {
        // Get the required components
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (animator == null)
        {
            Debug.LogError("No Animator component found on the object!");
            return;
        }
        
        // Trigger the death animation immediately
        PlayDeathAnimation();
    }

    void PlayDeathAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(deathAnimationTrigger);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
