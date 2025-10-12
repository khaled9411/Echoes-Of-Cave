using Unity.VisualScripting;
using UnityEngine;

namespace LeastSquares
{
    public class YubaAchievementManager : MonoBehaviour
    {
        [Header("References")]
        public InteractionManager interactionManager;
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
            LevelManager.Instance.levelWinAction += WinLevels;
            LevelManager.Instance.levelWinAction += OnSymbolCollected;
        }

        private void OnDestroy()
        {
            LevelManager.Instance.levelWinAction -= WinLevels;
            LevelManager.Instance.levelWinAction -= OnSymbolCollected;
        }

        void Update()
        {
            winTorchTrofee();
        }

        private void winTorchTrofee()
        {
            if(interactionManager!= null && interactionManager.heldObject is TorchInteractable)
            {
                OnTorchLit();
            }
        }

        private void WinLevels()
        {
            OnGateSolved(LevelManager.Instance.GetCurrentLevelInfo().levelNumber);
        }

        //---------------------------------------------------------------
        public void OnTorchLit()
        {
            steam.TriggerAchievement("ACH_FIRST_LIGHT");
            CheckAllAchievements();
        }

        public void OnGateSolved(int gateNumber)
        {
            if (gateNumber == 1)
                steam.TriggerAchievement("ACH_SOLVE_GATE_1");
            else if (gateNumber == 5)
                steam.TriggerAchievement("ACH_SOLVE_GATE_5");
            else if (gateNumber == 10)
                steam.TriggerAchievement("ACH_SOLVE_GATE_10");

            completedLevels++;
            if (completedLevels >= totalLevels)
                steam.TriggerAchievement("ACH_SOLVE_ALL_LEVELS");

            CheckAllAchievements();
        }

        public void OnSymbolCollected()
        {
            if (LevelManager.Instance.GetCurrentLevelInfo().levelNumber == 1)
                steam.TriggerAchievement("ACH_FIND_SYMBOL_1");

            if (LevelManager.Instance.GetCurrentLevelInfo().levelNumber >= totalLevels)
                steam.TriggerAchievement("ACH_FIND_SYMBOL_ALL");

            CheckAllAchievements();
        }

        private void CheckAllAchievements()
        {
            var all = steam.GetAchievements();
            if (all == null) return;

            bool allUnlocked = true;
            foreach (var ach in all)
            {
                if (!ach.State)
                {
                    allUnlocked = false;
                    break;
                }
            }

            if (allUnlocked)
                steam.TriggerAchievement("ACH_ALL_ACHIEVEMENTS");
        }
    }
}
