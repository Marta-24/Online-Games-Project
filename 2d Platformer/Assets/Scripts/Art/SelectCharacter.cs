using UnityEngine;
using TMPro;  // Import for TextMeshPro support

public class SelectCharacter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;  // Holds the SpriteRenderer component
    private Color originalColor;            // Stores the original sprite color

    public Color hiddenColor = Color.black;  // Color when not hovered (default black)
    public GameObject abilitiesText;         // Reference to the UI text GameObject
    public string abilityDescription = "This character can perform a double jump and wall climb.";  // Custom description

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();  // Get the SpriteRenderer component
        originalColor = spriteRenderer.color;             // Save the original color
        spriteRenderer.color = hiddenColor;               // Set sprite color to black by default

        // Hide the text at the start
        if (abilitiesText != null)
            abilitiesText.SetActive(false);
    }

    void OnMouseEnter()
    {
        spriteRenderer.color = originalColor;  // Show the sprite’s original color
        if (abilitiesText != null)
        {
            abilitiesText.SetActive(true);     // Show the abilities text
            abilitiesText.GetComponent<TextMeshProUGUI>().text = abilityDescription;  // Update the text dynamically
        }
    }

    void OnMouseExit()
    {
        spriteRenderer.color = hiddenColor;  // Hide the sprite (back to black)
        if (abilitiesText != null)
            abilitiesText.SetActive(false);  // Hide the abilities text
    }
}
