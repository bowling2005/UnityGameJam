using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class GameUIHandler : MonoBehaviour
{
    [Header("UI References")]
    public Button resetButton;
    public Button highScoreButton;
    public GameObject highScorePanel;
    public Text highScoreText;
    public Button closeHighScoreButton;
    public GameObject Panel;

    private bool isHighScorePanelOpen = false;

    void Start()
    {
        // 绑定按钮事件
        resetButton.onClick.AddListener(OnResetButtonClicked);
        highScoreButton.onClick.AddListener(OnHighScoreButtonClicked);
        closeHighScoreButton.onClick.AddListener(OnCloseHighScoreButtonClicked);

        // 初始隐藏高分面板
        highScorePanel.SetActive(false);
    }

    void Update()
    {
        // 如果高分面板打开，按返回键也可以关闭
        if (isHighScorePanelOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseHighScorePanel();
        }
        if (Panel.activeSelf)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }

    // 重置按钮点击事件
    private void OnResetButtonClicked()
    {
        GameResetManager.Instance.ResetGame();
    }

    // 高分按钮点击事件
    private void OnHighScoreButtonClicked()
    {
        ShowHighScorePanel();
    }

    // 关闭高分面板按钮事件
    private void OnCloseHighScoreButtonClicked()
    {
        CloseHighScorePanel();
    }

    // 显示高分面板
    private void ShowHighScorePanel()
    {
        // 暂停游戏时间
        Time.timeScale = 0f;

        // 显示面板
        highScorePanel.SetActive(true);
        isHighScorePanelOpen = true;

        // 获取并显示最高分
        List<int> highScores = GameResetManager.Instance.GetHighScores();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("最高分榜:");

        if (highScores.Count == 0)
        {
            sb.AppendLine("暂无记录");
        }
        else
        {
            for (int i = 0; i < highScores.Count; i++)
            {
                sb.AppendLine($"{i + 1}. {highScores[i]}");
            }
        }

        highScoreText.text = sb.ToString();
    }

    // 关闭高分面板
    private void CloseHighScorePanel()
    {
        // 恢复游戏时间
        Time.timeScale = 1f;

        // 隐藏面板
        highScorePanel.SetActive(false);
        isHighScorePanelOpen = false;
    }
}