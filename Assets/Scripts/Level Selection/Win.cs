using UnityEngine;

public class Win : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if this is the player
        if (other.CompareTag("Player"))
        {
            int currentLevel = LevelManager.Instance.GetSelectedLevel();

            // Call OnLevelWin first - it handles last level check internally
            LevelManager.Instance.OnLevelWin();

            // Only continue to next level if it's not the last level
            if (!LevelManager.Instance.IsLastLevel(currentLevel))
            {
                LevelManager.Instance.LoadSelectedLevel();
                if (MainManu.Instance != null)
                {
                    MainManu.Instance.StartGame(1);
                }
            }
            // If it's the last level, OnLevelWin() will handle showing credits
        }
    }
}