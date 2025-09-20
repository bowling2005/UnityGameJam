using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class StartGameButton2D : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("按钮设置")]
    public UnityEvent onButtonClick;
    public float fadeDuration = 0.1f; // 变暗动画持续时间

    [Header("视觉效果")]
    public Color normalColor = Color.white;
    public Color pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f); // 变暗的颜色

    [Header("场景过渡")]
    public PlayableDirector timelineDirector; // Timeline导演组件
    public string nextSceneName; // 下一个场景名称
    public float sceneLoadDelay = 2f; // 延迟加载场景的时间

    private Image buttonImage;
    private bool isPressed = false;

    void Start()
    {
        // 获取按钮的Image组件
        buttonImage = GetComponent<Image>();

        // 如果没有指定事件，创建一个空事件
        if (onButtonClick == null)
            onButtonClick = new UnityEvent();
    }

    // 当手指按下按钮时
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPressed) return;

        isPressed = true;

        // 按钮变暗效果
        if (buttonImage != null)
        {
            buttonImage.CrossFadeColor(pressedColor, fadeDuration, true, true);
        }
    }

    // 当手指释放时
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed) return;

        isPressed = false;

        // 恢复按钮颜色
        if (buttonImage != null)
        {
            buttonImage.CrossFadeColor(normalColor, fadeDuration, true, true);
        }
    }

    // 当完成点击时
    public void OnPointerClick(PointerEventData eventData)
    {
        // 触发点击事件
        onButtonClick.Invoke();

        // 执行游戏开始流程
        StartGameSequence();
    }

    // 游戏开始序列
    private void StartGameSequence()
    {
        // 禁用按钮防止重复点击
        GetComponent<Button>().interactable = false;

        // 如果有Timeline，播放Timeline
        if (timelineDirector != null)
        {
            timelineDirector.Play();

            // 在Timeline播放结束后加载新场景
            Invoke("LoadNextScene", (float)timelineDirector.duration);
        }
        else
        {
            // 如果没有Timeline，直接延迟加载场景
            Invoke("LoadNextScene", sceneLoadDelay);
        }
    }

    // 加载下一个场景
    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}