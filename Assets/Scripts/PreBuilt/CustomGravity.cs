using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    private Rigidbody2D rb;

    // Declare 'planet' as a Transform to point to the planet's Transform component
    public Transform planet;

    // Declare 'gravityForce' as a float to control the strength of the gravity
    public float gravityForce = 9.8f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component missing from this game object. Please add one.");
            this.enabled = false;  // Disable the script if no Rigidbody2D is attached.
        }

        // Optionally, you can check here if 'planet' has been assigned
        if (planet == null)
        {
            Debug.LogError("Planet Transform has not been assigned in the inspector.");
            this.enabled = false;  // Disable the script if no planet is assigned.
        }
    }

    void FixedUpdate()
    {
        if (planet != null)
        {
            Vector2 gravityDirection = (transform.position - planet.position).normalized;
            rb.AddForce(gravityDirection * -gravityForce, ForceMode2D.Force);
        }
    }
}
