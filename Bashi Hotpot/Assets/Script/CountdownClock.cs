using UnityEngine;
using TMPro;

public class CountdownClock : MonoBehaviour
{
    [Header("倒计时设置")]
    public float totalTime = 60f;   // 总时间（秒）
    private float timer;

    [Header("时钟指针")]
    public Transform clockHand;     // 指针对象
    public float offset= 53;

    [Header("UI 显示 (可选)")]
    public TextMeshProUGUI timerText;

    private bool isRunning = false;

    void Start()
    {
        StartCountdown();
    }

    void Update()
    {
        if (!isRunning) return;

        timer -= Time.deltaTime;
        if (timer < 0f) timer = 0f;

        // 计算倒计时进度 (0~1)
        float progress = 1f - (timer / totalTime);

        // 旋转指针（局部 Y 轴）
        if (clockHand != null)
        {
            float angle = -progress * 360f + offset;
            Vector3 rot = clockHand.localEulerAngles;
            rot.y = angle;
            clockHand.localEulerAngles = rot;
        }

        // 更新文本
        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(timer).ToString();
        }

        // 时间结束
        if (timer <= 0f)
        {
            isRunning = false;
            Debug.Log("倒计时结束！");
        }
    }

    public void StartCountdown()
    {
        timer = totalTime;
        isRunning = true;
    }
}
