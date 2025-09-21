using UnityEngine;
using System.Collections.Generic;

public enum FoodType
{
    MeatRoll,
    Cabbage,
    MeatBall,
    Doufu,
    Mushroom,
    Stomach,
    Brain,
    Shrimp,
    Lettuce,
    Clam
}


[System.Serializable]
public class FoodSpawnData
{
    public GameObject prefab;   // 食物预制体
    public int count = 1;       // 生成数量
    public FoodType type;
}

[System.Serializable]
public class FoodRow
{
    public List<FoodSpawnData> foodList = new List<FoodSpawnData>();  // 一行的食物
    public Vector3 spacing = new Vector3(2f, 0f, 0f);                 // 行内食物的间距
}

public class FoodSpawner : MonoBehaviour
{
    public static FoodSpawner Instance;


    [Header("生成配置")]
    public List<FoodRow> rows = new List<FoodRow>();  // 多行配置
    public List<FoodRow> rows_1 = new List<FoodRow>();  // 多行配置
    public Transform startPosition;     // 起始位置
    public Vector3 rowSpacing = new Vector3(0f, 0f, 2f);  // 行与行之间的间距
    [Header("层级配置")]
    //public int secondLayerStartRow = 3;     // 从第几行开始放到第二层
    public Vector3 secondLayerOffset = new Vector3(0f, 1f, 0f); // 第二层整体偏移（比如向上1）

    public GameObject smoke;

    private Dictionary<FoodType, int> remaining = new Dictionary<FoodType, int>();
    private readonly List<GameObject> spawned = new List<GameObject>();
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SpawnAllFood();
    }

    private void SpawnAllFood()
    {
        Vector3 basePos = startPosition.position;

        // —— 第一层 ——
        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            FoodRow row = rows[rowIndex];
            Vector3 currentPos = basePos + rowSpacing * rowIndex;

            foreach (var data in row.foodList)
            {
                data.type = data.prefab.GetComponent<FoodItem>().foodType;
                if (remaining.ContainsKey(data.type)) remaining[data.type] += data.count;
                else remaining[data.type] = data.count;

                for (int i = 0; i < data.count; i++)
                {
                    var obj = Instantiate(data.prefab, currentPos, data.prefab.transform.rotation);
                    obj.GetComponent<FoodItem>().foodType = data.type;
                    currentPos += row.spacing; // 行内间距
                    spawned.Add(obj);
                }
            }
        }

        // —— 第二层 ——
        for (int rowIndex = 0; rowIndex < rows_1.Count; rowIndex++)
        {
            FoodRow row = rows_1[rowIndex];
            // 第二层整体+偏移
            Vector3 currentPos = basePos + secondLayerOffset + rowSpacing * rowIndex;

            foreach (var data in row.foodList)
            {
                data.type = data.prefab.GetComponent<FoodItem>().foodType;
                if (remaining.ContainsKey(data.type)) remaining[data.type] += data.count;
                else remaining[data.type] = data.count;

                for (int i = 0; i < data.count; i++)
                {
                    var obj = Instantiate(data.prefab, currentPos, data.prefab.transform.rotation);
                    obj.GetComponent<FoodItem>().foodType = data.type;
                    currentPos += row.spacing; // 行内间距
                    spawned.Add(obj);
                }
            }
        }

        ComboManager.Instance.GenerateNewCombo();
    }


    public void OnFoodTaken(FoodType type)
    {
        if (remaining.ContainsKey(type))
            remaining[type]--;
    }

    public int GetRemaining(FoodType type)
    {
        return remaining.ContainsKey(type) ? remaining[type] : 0;
    }

    public List<FoodType> GetAvailableTypes()
    {
        List<FoodType> available = new List<FoodType>();
        foreach (var kv in remaining)
        {
            if (kv.Value > 0) available.Add(kv.Key);
        }
        return available;
    }
    public void ResetGame()
    {
        // 安全销毁上一次生成的食物
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] != null) Destroy(spawned[i]);
        }
        spawned.Clear();

        remaining.Clear();

        // 重新生成
        SpawnAllFood();

        // 如需同步重置分数/连击，可按需启用
        // ScoreManager.Instance?.ResetScore();
        ComboManager.Instance?.GenerateNewCombo();
    }

}
