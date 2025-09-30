using UnityEngine;
using Steamworks;
using System.Collections.Generic;

public static class SteamStatsAndAchievements
{
    private static Dictionary<string, int> intStatsCache = new Dictionary<string, int>();
    private static Dictionary<string, float> floatStatsCache = new Dictionary<string, float>();

    public static void Initialize()
    {
        SteamUserStats.OnAchievementProgress += AchievementChanged;

        Debug.Log("SteamStatsAndAchievements Initialized. Stats loading is automatic.");
    }

    private static void AchievementChanged(Steamworks.Data.Achievement ach, int currentProgress, int progress)
    {
        if (ach.State)
        {
            Debug.Log($"Achievement UNLOCKED: {ach.Name}!");
        }
    }

    public static void SaveStatsToSteam()
    {
        Debug.Log("Storing stats to Steam backend...");
        SteamUserStats.StoreStats();
    }
}