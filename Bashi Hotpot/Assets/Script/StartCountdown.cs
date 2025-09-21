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
    public float[] scaleEnd;
    public float duration = 0.5f;
    public float holdTime = 0.5f;
    public float extraHoldTime = 0.5f;
    public System.Action onCountdownEnd;
    AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public IEnumerator DoCountdown()
    {
        countdownImage.gameObject.SetActive(true);

        for (int i = 0; i < numberTextures.Length-1; i++)
        {
            countdownImage.texture = numberTextures[i];

            // 重置缩放
            countdownImage.transform.localScale = Vector3.one * scaleStart;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float progress = Mathf.Clamp01(t / duration);

                float currentScale = Mathf.Lerp(scaleStart, scaleEnd[i], progress);
                countdownImage.transform.localScale = Vector3.one * currentScale;

                yield return null;
            }

            yield return new WaitForSeconds(holdTime);
        }
        yield return new WaitForSeconds(extraHoldTime);
        // 倒计时结束
        countdownImage.gameObject.SetActive(false);
        ResetManager.Instance.GameStart();
        onCountdownEnd?.Invoke();
    }

    public IEnumerator GameOver()
    {
        countdownImage.gameObject.SetActive(true);

        countdownImage.texture = numberTextures[numberTextures.Length-1];

        // 重置缩放
        countdownImage.transform.localScale = Vector3.one * scaleStart;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / duration);

            float currentScale = Mathf.Lerp(scaleStart, scaleEnd[numberTextures.Length - 1], progress);
            countdownImage.transform.localScale = Vector3.one * currentScale;

            yield return null;
        }
        audioSource.clip = SoundManager.Instance.overSound;
        audioSource.Play();
        yield return new WaitForSeconds(extraHoldTime+holdTime*2);

        // 倒计时结束
        countdownImage.gameObject.SetActive(false);
    }
}
