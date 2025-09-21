using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameResetManager : MonoBehaviour
{
    #region Singleton Pattern
    private static GameResetManager _instance;
    public static GameResetManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameResetManager>();

                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameResetManager");
                    _instance = obj.AddComponent<GameResetManager>();
                }
            }
            return _instance;
        }
    }
    #endregion

    [System.Serializable]
    public class HighScoreData
    {
        public List<int> highScores = new List<int>();
    }

    private string highScoreFilePath;
    private HighScoreData highScoreData = new HighScoreData();
    private int currentScore = 0;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        
        highScoreFilePath = Path.Combine(Application.persistentDataPath, "highscores.json");

        LoadHighScores();
    }

    // 更新当前分数并检查是否需要更新最高分
    public void UpdateScore(int newScore)
    {
        currentScore = newScore;
        CheckAndUpdateHighScores();
    }

    private void CheckAndUpdateHighScores()
    {
        
        if (highScoreData.highScores.Count < 10 || currentScore > highScoreData.highScores[highScoreData.highScores.Count - 1])
        {
            highScoreData.highScores.Add(currentScore);
            highScoreData.highScores.Sort((a, b) => b.CompareTo(a)); 

            
            if (highScoreData.highScores.Count > 10)
            {
                highScoreData.highScores = highScoreData.highScores.GetRange(0, 10);
            }

            SaveHighScores();
        }
    }

    
    private void LoadHighScores()
    {
        if (File.Exists(highScoreFilePath))
        {
            string json = File.ReadAllText(highScoreFilePath);
            highScoreData = JsonUtility.FromJson<HighScoreData>(json);
        }
        else
        {
            
            highScoreData = new HighScoreData();
        }
    }

   
    private void SaveHighScores()
    {
        string json = JsonUtility.ToJson(highScoreData, true);
        File.WriteAllText(highScoreFilePath, json);
    }

    
    public List<int> GetHighScores()
    {
        return new List<int>(highScoreData.highScores);
    }

   
    public void ResetGame()
    {
        ResetTimeScale();
        StartCoroutine(ResetGameCoroutine());
    }

    private IEnumerator ResetGameCoroutine()
    {
       
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.buildIndex != currentSceneIndex && scene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }

       
        ResetAllResettableObjects();

        
        yield return SceneManager.LoadSceneAsync(currentSceneIndex, LoadSceneMode.Single);

        
        currentScore = 0;

        Debug.Log("游戏已重置！");
    }

    // 查找并重置所有实现了IResettable接口的对象
    private void ResetAllResettableObjects()
    {
        // 查找所有实现了IResettable接口的对象
        var resettables = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var obj in resettables)
        {
            if (obj is IResettable resettable)
            {
                resettable.ResetToInitialState();
            }
        }

        // 清理资源
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
    public void ResetTimeScale()
    {
        Time.timeScale = 1f;
    }

}

// 可重置接口
public interface IResettable
{
    void ResetToInitialState();
}