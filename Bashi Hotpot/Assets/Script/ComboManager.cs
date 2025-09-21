using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    [Header("UI")]
    public RawImage[] comboSlots;  // UI 上三个槽位
    public Texture2D rollIcon, cabIcon, ballIcon, doufuIcon, mushroomIcon, stomachIcon, brainIcon,shrimpIcon,lettuceIcon,clamIcon;  // 食材图标（普通图片）

    private List<FoodType> currentCombo = new List<FoodType>();
    private int currentIndex = 0;

    void Awake()
    {
        Instance = this;
    }


    public void GenerateNewCombo()
    {
        currentCombo.Clear();
        currentIndex = 0;

        List<FoodType> available = FoodSpawner.Instance.GetAvailableTypes();
        if (available.Count < 3)
        {
            Debug.Log("食材不足无法生成");
            return; // 食材不足无法生成
        }

            // 随机选3种不同食材
            for (int i = 0; i < 3; i++)
        {
            int rand = Random.Range(0, available.Count);
            currentCombo.Add(available[rand]);
            available.RemoveAt(rand);
        }

        // 更新 UI
        for (int i = 0; i < comboSlots.Length; i++)
        {
            comboSlots[i].texture = GetIcon(currentCombo[i]);
            comboSlots[i].color = Color.white; // 重置颜色
        }
    }

    Texture2D GetIcon(FoodType type)
    {
        switch (type)
        {
            case FoodType.MeatRoll: return rollIcon;
            case FoodType.Cabbage: return cabIcon;
            case FoodType.MeatBall: return ballIcon;
            case FoodType.Doufu: return doufuIcon;
            case FoodType.Mushroom: return mushroomIcon;
            case FoodType.Stomach: return stomachIcon;
            case FoodType.Brain: return brainIcon;
            case FoodType.Lettuce: return lettuceIcon;
            case FoodType.Shrimp: return shrimpIcon;
            case FoodType.Clam: return clamIcon;
            default: return null;
        }
    }

    public void OnFoodTaken(FoodItem food)
    {
        // 必须是熟的
        if (food.GetCookState() != FoodItem.CookState.Cooked)
        {
            BreakCombo();
            return;
        }

        // 判断是否正确顺序
        if (food.foodType == currentCombo[currentIndex])
        {
            comboSlots[currentIndex].color = Color.green; // UI 上打勾效果
            currentIndex++;

            if (currentIndex >= currentCombo.Count)
            {
                // 成功完成连击！
                ScoreManager.Instance.ChangeScore(50); // 额外奖励
                GenerateNewCombo();
            }
        }
        else
        {
            for (int i = 0; i < currentCombo.Count; i++)
            {
                if (food.foodType == currentCombo[i] && FoodSpawner.Instance.GetRemaining(food.foodType) <= 0)
                {
                    GenerateNewCombo();
                    return;
                }
            }
            BreakCombo();
        }
    }

    void BreakCombo()
    {
        StopAllCoroutines();  // 避免上一次还在闪
        StartCoroutine(FlashRed());
        currentIndex = 0;
    }

    IEnumerator FlashRed()
    {
        int flashes = 3;           // 闪烁次数
        float interval = 0.2f;     // 每次闪烁间隔

        for (int f = 0; f < flashes; f++)
        {
            // 变红
            for (int i = 0; i < comboSlots.Length; i++)
                comboSlots[i].color = Color.red;

            yield return new WaitForSeconds(interval);

            // 还原白色
            for (int i = 0; i < comboSlots.Length; i++)
                comboSlots[i].color = Color.white;

            yield return new WaitForSeconds(interval);
        }

        // 最后保持白色
        for (int i = 0; i < comboSlots.Length; i++)
            comboSlots[i].color = Color.white;
    }

    public void ResetCombo()
    {
        StopAllCoroutines();   // 停止所有闪烁协程
        currentCombo.Clear();  // 清空当前组合
        currentIndex = 0;

        // UI 重置为白色，图标清空
        for (int i = 0; i < comboSlots.Length; i++)
        {
            comboSlots[i].texture = null;
            comboSlots[i].color = Color.white;
        }

        // 重新生成一套组合
        GenerateNewCombo();
    }
}
