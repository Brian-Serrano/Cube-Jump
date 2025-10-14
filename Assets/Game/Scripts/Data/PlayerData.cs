using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    // login info
    public string playerAccessToken;
    public string playerRefreshToken;
    public int playerId;
    public string playerName;

    // primary data
    public int highscore;
    public int coins;
    public List<string> icons;
    public string colors;
    public int highestGameCoins;
    public int totalScore;
    public float totalTime;
    public int revivesDone;
    public int totalCoins;
    public int gamesPlayed;

    // daily reward data
    public int dailyRewardProgress;
    public string lastDailyLoadTime;

    // quests data
    public int playQuestTotal;
    public int playQuestProgress;

    public int coinsQuestTotal;
    public int coinsQuestProgress;

    public int scoreQuestTotal;
    public int scoreQuestProgress;

    public string lastQuestLoadTime;

    // audio settings
    public float musicVolume;
    public float sfxVolume;

    public PlayerData()
    {
        playerAccessToken = "";
        playerRefreshToken = "";
        playerId = 0;
        playerName = "";

        highscore = 0;
        coins = 50;
        icons = new List<string>
        {
            "20000000000000000000",
            "20000000000000000000",
            "20000000000000000000",
            "20000000000000000000",
            "20000000000000000000"
        };
        colors = "32000000000000000000";
        highestGameCoins = 0;
        totalScore = 0;
        totalTime = 0f;
        revivesDone = 0;
        totalCoins = 50;

        dailyRewardProgress = 0;
        lastDailyLoadTime = "";

        playQuestTotal = 0;
        playQuestProgress = -1;
        coinsQuestTotal = 0;
        coinsQuestProgress = -1;
        scoreQuestTotal = 0;
        scoreQuestProgress = -1;

        lastQuestLoadTime = "";

        musicVolume = 1f;
        sfxVolume = 1f;
    }

    public static string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, "player_data.cj");
    }

    public static PlayerData LoadData()
    {
        return PersistentDataController.LoadData<PlayerData>(GetPath());
    }

    public bool SaveData()
    {
        return PersistentDataController.SaveData(this, GetPath());
    }

    public static bool SaveData(byte[] data)
    {
        try
        {
            File.WriteAllBytes(GetPath(), data);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    public static byte[] ReadData()
    {
        string path = GetPath();

        if (File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }
        else
        {
            Debug.LogError("File not found: " + path);
            return null;
        }
    }

    public void SetPlayerDataFromServer(PlayerData playerData)
    {
        highscore = playerData.highscore;
        coins = playerData.coins;
        icons = playerData.icons;
        colors = playerData.colors;
        highestGameCoins = playerData.highestGameCoins;
        totalScore = playerData.totalScore;
        totalTime = playerData.totalTime;
        revivesDone = playerData.revivesDone;
        totalCoins = playerData.totalCoins;

        dailyRewardProgress = playerData.dailyRewardProgress;
        lastDailyLoadTime = playerData.lastDailyLoadTime;

        playQuestTotal = playerData.playQuestTotal;
        playQuestProgress = playerData.playQuestProgress;
        coinsQuestTotal = playerData.coinsQuestTotal;
        coinsQuestProgress = playerData.coinsQuestProgress;
        scoreQuestTotal = playerData.scoreQuestTotal;
        scoreQuestProgress = playerData.scoreQuestProgress;

        lastQuestLoadTime = playerData.lastQuestLoadTime;

        musicVolume = playerData.musicVolume;
        sfxVolume = playerData.sfxVolume;
    }
}
