using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace LeastSquares
{
    /// <summary>
    /// Class that represents a Steam achievements & stats.
    /// </summary>
    public class SteamAchievementsAndStats : MonoBehaviour
    {
        public Achievement[] Achievements { get; private set; } = Array.Empty<Achievement>();
        public string[] AchievementsIdentifiers;
        private bool _statsLoaded;

        /// <summary>
        /// Method called when the GO is initialized
        /// </summary>
        private void Start()
        {
            Load();
        }

        /// <summary>
        /// Load all stats and achievements from Steamworks
        /// </summary>
        private void Load()
        {
            if (!SteamClient.IsValid) return;
            _statsLoaded = SteamUserStats.RequestCurrentStats();
            Achievements = SteamUserStats.Achievements.ToArray();
            AchievementsIdentifiers = Achievements.Select(A => A.Identifier).ToArray();
        }

        /// <summary>
        /// Sets a stat to a given value.
        /// </summary>
        /// <param name="statName">The stat identifier</param>
        /// <param name="value">The given value</param>
        public void SetStat(string statName, int value)
        {
            if (!LoadedGuard()) return;
            SteamUserStats.SetStat(statName, value);
        }
        
        /// <summary>
        /// Adds a value to an existing stat. e.g. if S_STAT = 5 and AddStat(S_STAT, 4) results in S_STAT  = 9
        /// </summary>
        /// <param name="statName">The stat identifier</param>
        /// <returns>The stat identifier</returns>
        public void AddStat(string statName, int value)
        {
            if (!LoadedGuard()) return;
            SteamUserStats.AddStat(statName, value);
        }
        
        /// <summary>
        /// Returns a float stat from steamworks
        /// </summary>
        /// <param name="statName"></param>
        /// <returns>The stat identifier</returns>
        public float GetStatFloat(string statName)
        {
            if (!LoadedGuard()) return 0;
            return SteamUserStats.GetStatFloat(statName);
        }
        
        /// <summary>
        /// Returns a int stat from steamworks
        /// </summary>
        /// <param name="statName"></param>
        /// <returns>The stat identifier</returns>
        public int GetStatInt(string statName)
        {
            if (!LoadedGuard()) return 0;
            return SteamUserStats.GetStatInt(statName);
        }

        /// <summary>
        /// Triggers an achievement by identifier (gives the achievement to the user)
        /// </summary>
        /// <param name="identifier">The achievement identifier</param>
        public void TriggerAchievement(string identifier)
        {
            if (!LoadedGuard()) return;
            var ach = GetAchievement(identifier);
            ach?.Trigger();
        }

        /// <summary>
        /// Finds the achievement with the given identifier and returns its object.
        /// </summary>
        /// <param name="identifier">An achievement identifier, this can be seen from the Steamworks page</param>
        /// <returns>The achievement object</returns>
        public Achievement? GetAchievement(string identifier)
        {
            if (!LoadedGuard()) return null;
            for (var i = 0; i < Achievements.Length; ++i)
            {
                if (Achievements[i].Identifier == identifier)
                    return Achievements[i];
            }

            return null;
        }

        /// <summary>
        /// Returns an array of all the loaded achievements, null if Steam has not loaded yet.
        /// </summary>
        /// <returns></returns>
        public Achievement[] GetAchievements()
        {
            if (!LoadedGuard()) return null;
            return Achievements;
        }

        /// <summary>
        /// Check if the stats have been loaded correctly, otherwise emit a warning
        /// </summary>
        /// <returns></returns>
        public bool LoadedGuard()
        {
            if(!_statsLoaded)
                Debug.LogWarning("Stats have not been loaded, ignoring action.");
            return _statsLoaded;
        }
    }
}
