using UnityEngine;

public interface IInteractionFeedback
{
    void ShowHighlight();
    void HideHighlight();
    void PlayInteractionSound();
    void ShowInteractionUI(string prompt);
    void HideInteractionUI();
}
