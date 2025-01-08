using UnityEngine;
using TMPro;

public class SelectCharacter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public Color hiddenColor = Color.black;
    public GameObject abilitiesText;
    public string abilityDescription = "";

    private static SelectCharacter selectedCharacter;

    private bool isSelected = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        spriteRenderer.color = hiddenColor;

        if (abilitiesText != null)
            abilitiesText.SetActive(false);
    }

    void OnMouseEnter()
    {
        if (!isSelected)
            spriteRenderer.color = originalColor;

        if (abilitiesText != null)
        {
            abilitiesText.SetActive(true);
            abilitiesText.GetComponent<TextMeshProUGUI>().text = abilityDescription;
        }
    }

    void OnMouseExit()
    {
        if (!isSelected)
            spriteRenderer.color = hiddenColor;

        if (abilitiesText != null)
            abilitiesText.SetActive(false);
    }

    void OnMouseDown()
    {
        if (selectedCharacter == this)
            return;

        if (selectedCharacter != null)
        {
            selectedCharacter.Deselect();
        }

        Select();
    }

    private void Select()
    {
        isSelected = true;
        spriteRenderer.color = originalColor;
        selectedCharacter = this;
    }

    private void Deselect()
    {
        isSelected = false;
        spriteRenderer.color = hiddenColor;
    }
}