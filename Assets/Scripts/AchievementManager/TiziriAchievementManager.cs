using UnityEngine;

namespace LeastSquares
{
    public class TiziriAchievementManager : MonoBehaviour
    {
        private SteamAchievementsAndStats steam;

        private int totalLevels = 15;
        private int completedLevels = 0;

        private void Awake()
        {
            if (steam == null)
                steam = FindFirstObjectByType<SteamAchievementsAndStats>();
        }

        private void Start()
        {
            OnTiziriStart();
            LevelManager.Instance.levelWinAction += WinLevels;
        }

        private void OnDestroy()
        {
            LevelManager.Instance.levelWinAction -= WinLevels;
        }

        private void WinLevels()
        {
            OnTiziriSolve(LevelManager.Instance.GetCurrentLevelInfo().levelNumber);
        }

        public void OnTiziriStart()
        {
            steam.TriggerAchievement("ACH_TIZIRI_START");
            CheckAllTiziriAchievements();
        }

        public void OnTiziriSolve(int gateNumber)
        {
            if (gateNumber == 1)
                steam.TriggerAchievement("ACH_TIZIRI_SOLVE_1");
            else if (gateNumber == 5)
                steam.TriggerAchievement("ACH_TIZIRI_SOLVE_5");

            completedLevels++;
            if (completedLevels >= totalLevels)
                steam.TriggerAchievement("ACH_TIZIRI_COMPLETE");

            CheckAllTiziriAchievements();
        }


        private void CheckAllTiziriAchievements()
        {
            var all = steam.GetAchievements();
            if (all == null) return;

            bool allTiziriUnlocked = true;

            foreach (var ach in all)
            {
                if (ach.Identifier.StartsWith("ACH_TIZIRI") && !ach.State)
                {
                    allTiziriUnlocked = false;
                    break;
                }
            }

            if (allTiziriUnlocked)
                steam.TriggerAchievement("ACH_TIZIRI_FULL");
        }
    }
}
