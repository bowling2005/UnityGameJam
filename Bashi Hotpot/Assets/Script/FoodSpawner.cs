using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FoodSpawnData
{
    public GameObject prefab;   // 食物预制体
    public int count = 1;       // 生成数量
}

public class FoodSpawner : MonoBehaviour
{
    [Header("生成配置")]
    public List<FoodSpawnData> foodList = new List<FoodSpawnData>();

    public Transform startPosition;
    //public Vector3 startPosition.position = new Vector3(-5f, 0.5f, 0f); // 起始位置
    public Vector3 spacing = new Vector3(2f, 0f, 0f);          // 间隔（默认 X 方向排开）

    void Start()
    {
        SpawnAllFood();
    }

    private void SpawnAllFood()
    {
        Vector3 currentPos = startPosition.position;

        foreach (var data in foodList)
        {
            for (int i = 0; i < data.count; i++)
            {
                Instantiate(data.prefab, currentPos, Quaternion.identity);
                currentPos += spacing; // 下一个食物往右/前排开
            }
        }
    }
}
