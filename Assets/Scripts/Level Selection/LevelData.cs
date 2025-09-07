using UnityEngine;

[System.Serializable]
public class LevelInfo
{
    public int levelNumber;
    [TextArea(3, 5)]
    public string description;
    public string prefabName;
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Information")]
    public LevelInfo[] levels;

    public LevelInfo GetLevelInfo(int levelNumber)
    {
        foreach (var level in levels)
        {
            if (level.levelNumber == levelNumber)
                return level;
        }
        return null;
    }

    public int GetTotalLevels()
    {
        return levels.Length;
    }
}