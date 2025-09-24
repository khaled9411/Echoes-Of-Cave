using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PuzzleController : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [SerializeField] private StoneSlotInteractable[] allStoneSlots;
    [SerializeField] private UnityEvent onAllStonesPlacedCorrectly;

    private void Start()
    {
        if (allStoneSlots == null || allStoneSlots.Length == 0)
        {
            Debug.LogError("No Stone Slots assigned to the Puzzle Controller!");
        }

        foreach (var slot in allStoneSlots)
        {
            slot.onStonePlacedCorrectly.AddListener(CheckPuzzleCompletion);
        }
    }

    public void CheckPuzzleCompletion()
    {
        bool allCorrect = true;

        foreach (var slot in allStoneSlots)
        {
            if (slot.PlacedStone == null || slot.PlacedStone.StoneType != slot.RequiredStoneType)
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            Debug.Log("Puzzle solved! All stones are in their correct slots.");
            StartCoroutine(Win());
        }
    }

    private IEnumerator Win()
    {
        yield return new WaitForSeconds(1.5f);
        onAllStonesPlacedCorrectly?.Invoke();
    }
    

}