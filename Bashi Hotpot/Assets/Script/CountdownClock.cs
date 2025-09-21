using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CountdownClock : MonoBehaviour
{
    public static CountdownClock Instance;

    [Header("倒计时设置")]
    public float totalTime = 60f;   // 总时间（秒）
    private float timer;

    [Header("时钟指针")]
    public Transform clockHand;     // 指针对象
    public float offset = 53;

    [Header("UI 显示 (可选)")]
    public TextMeshProUGUI timerText;

    private bool isRunning = false;
    AudioSource audioSource;
    StartCountdownRawImages rawImages;

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        rawImages = GetComponent<StartCountdownRawImages>();
    }

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        audioSource.clip = SoundManager.Instance.countDownSound;
        audioSource.Play();
        StartCoroutine(rawImages.DoCountdown());
    }

    void Update()
    {
        if (!isRunning) return;

        timer -= Time.deltaTime;
        if (timer < 0f) timer = 0f;

        float progress = 1f - (timer / totalTime);

        // 指针旋转
        if (clockHand != null)
        {
            float angle = -progress * 360f + offset;
            Vector3 rot = clockHand.localEulerAngles;
            rot.y = angle;
            clockHand.localEulerAngles = rot;
        }

        // 文本更新
        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(timer).ToString();
        }

        // 倒计时结束
        if (timer <= 0f)
        {
            isRunning = false;
            Debug.Log("倒计时结束！");
            StartCoroutine(rawImages.GameOver());
            ResetManager.Instance.GameOver();
        }
    }

    public void StartCountdown()
    {
        timer = totalTime;
        isRunning = true;
    }

    // 新增：重置方法
    public void ResetClock()
    {
        StopAllCoroutines();   // 停止所有协程
        isRunning = false;

        // 停止音效
        if (audioSource != null)
            audioSource.Stop();

        // 重置时间
        timer = totalTime;

        // 指针回到初始角度
        if (clockHand != null)
        {
            Vector3 rot = clockHand.localEulerAngles;
            rot.y = offset;
            clockHand.localEulerAngles = rot;
        }

        // 文本重置
        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(totalTime).ToString();
        }
    }
}
