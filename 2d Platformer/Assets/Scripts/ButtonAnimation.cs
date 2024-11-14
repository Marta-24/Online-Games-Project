using UnityEngine;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour
{
    private Animator animator;
    private Button button;

    void Start()
    {
        animator = GetComponent<Animator>();
        button = GetComponent<Button>();

        // Subscribe to button click events
        button.onClick.AddListener(OnButtonPressed);
    }

    void OnButtonPressed()
    {
        animator.SetBool("IsPressed", true);
        Invoke("ResetButtonState", 0.1f); // Small delay to return to normal
    }

    void ResetButtonState()
    {
        animator.SetBool("IsPressed", false);
    }
}
