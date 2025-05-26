using System.IO;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]             //게임엔진입문 1강 8p
public class StageResult
{
   
        public string playerName;
        public int stage;
        public int score;
}

[System.Serializable]
public class stageResultList
{
    public List<StageResult> results = new List<StageResult>();
}

public static class StageResultSaver
{
    private const string FILE = "stage_result.json";
    private const string PLAYER_NAME = "playerName";
    private static string filePath = Path.Combine(Application.persistentDataPath, FILE);
    public static void SaveStage(int stage, int score)
    {
        StageResultList list = LoadInternal();                 //게임엔진입문 10강 HighScore 부분 StageResult 연동
        string playerName = PlayerPrefs.GetString(PLAYER_NAME, "");
        StageResult entry = new StageResult
        {
            playerName = playerName,
            stage = stage,
            score = score
        };
        list.results.Add(entry);
        string json = JsonUtility.ToJson(list, true);
        File.WriteAllText(filePath, json);
    }

    private static stageResultList LoadInternal()
    {
        if (!File.Exists(filePath))
            return new stageResultList();
        string json = File.ReadAllText(filePath);
        StageResultList list = JsonUtility.FromJson<StageResultList>(json);   // 위와 같음
        if (list == null)
            return new stageResultList();
        else
            return list;
    }
}
