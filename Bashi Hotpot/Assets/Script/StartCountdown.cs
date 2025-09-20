using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartCountdownRawImages : MonoBehaviour
{
    [Header("UI")]
    public RawImage countdownImage;       
    public Texture[] numberTextures;      

    [Header("动画参数")]
    public float scaleStart = 4f; 
    public float scaleEnd = 1f; 
    public float duration = 0.5f;
    public float holdTime = 0.5f; 

    public System.Action onCountdownEnd;

    void Start()
    {
        StartCoroutine(DoCountdown());
    }

    IEnumerator DoCountdown()
    {
        countdownImage.gameObject.SetActive(true);

        for (int i = 0; i < numberTextures.Length; i++)
        {
            countdownImage.texture = numberTextures[i];

            // 重置缩放
            countdownImage.transform.localScale = Vector3.one * scaleStart;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float progress = Mathf.Clamp01(t / duration);

                float currentScale = Mathf.Lerp(scaleStart, scaleEnd, progress);
                countdownImage.transform.localScale = Vector3.one * currentScale;

                yield return null;
            }

            yield return new WaitForSeconds(holdTime);
        }

        // 倒计时结束
        countdownImage.gameObject.SetActive(false);
        onCountdownEnd?.Invoke();
    }
}
