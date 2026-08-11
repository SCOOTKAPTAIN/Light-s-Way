using UnityEngine;

public class IdleJuice : MonoBehaviour
{
    public float bounceSpeed = 4f;
    public float bounceAmount = 0.05f;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Calculate a smooth sine wave cycle over time
        float change = Mathf.Sin(Time.time * bounceSpeed) * bounceAmount;
        
        // Squash down slightly while stretching out sideways
        transform.localScale = new Vector3(
            originalScale.x + change, 
            originalScale.y - change, 
            originalScale.z
        );
    }
}
