using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    [Header("UI")]
    public RawImage[] comboSlots;  // UI 上三个槽位
    public Texture2D rollIcon, cabIcon, ballIcon, doufuIcon, mushroomIcon, stomachIcon, brainIcon, shrimpIcon, lettuceIcon, clamIcon;  // 食材图标（普通图片）

    private List<FoodType> currentCombo = new List<FoodType>();
    public AudioSource audioSource;
    private List<bool> collectedFoods = new List<bool>(); // 跟踪已收集的食材

    void Awake()
    {
        Instance = this;
    }

    public void GenerateNewCombo()
    {
        currentCombo.Clear();
        collectedFoods.Clear();

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
            collectedFoods.Add(false); // 初始化为未收集
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

        // 检查是否在当前combo中
        int index = currentCombo.IndexOf(food.foodType);

        if (index >= 0)
        {
            // 如果已经收集过这个食材，忽略
            if (collectedFoods[index])
                return;

            // 标记为已收集
            collectedFoods[index] = true;
            comboSlots[index].color = Color.green; // UI 上打勾效果

            // 检查是否所有食材都已收集
            if (CheckComboComplete())
            {
                // 成功完成连击！
                ScoreManager.Instance.ChangeScore(50); // 额外奖励
                audioSource.clip = SoundManager.Instance.combo;
                audioSource.Play();
                GenerateNewCombo();
            }
        }
        else
        {
            // 如果食材不在combo中，检查是否已经不存在了
            if (FoodSpawner.Instance.GetRemaining(food.foodType) <= 0)
            {
                GenerateNewCombo();
            }
            else
            {
                BreakCombo();
            }
        }
    }

    bool CheckComboComplete()
    {
        foreach (bool collected in collectedFoods)
        {
            if (!collected)
                return false;
        }
        return true;
    }

    void BreakCombo()
    {
        StopAllCoroutines();  // 避免上一次还在闪
        StartCoroutine(FlashRed());
        ResetComboProgress();
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

    void ResetComboProgress()
    {
        // 重置收集状态但不生成新combo
        for (int i = 0; i < collectedFoods.Count; i++)
        {
            collectedFoods[i] = false;
            comboSlots[i].color = Color.white;
        }
    }

    public void ResetCombo()
    {
        StopAllCoroutines();   // 停止所有闪烁协程
        currentCombo.Clear();  // 清空当前组合
        collectedFoods.Clear();

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