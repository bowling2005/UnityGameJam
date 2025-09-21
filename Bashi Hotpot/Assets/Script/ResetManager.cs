using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetManager : MonoBehaviour
{
    public static ResetManager Instance;
    public GameObject buffle;
    public AudioSource potAudio;
    AudioSource bgmAudio;
    SkimmerScooper skimmerScooper;
    //public AudioSource noiseAudio;
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        bgmAudio = GetComponent<AudioSource>();

    }

    void Start()
    {
        skimmerScooper = FindObjectOfType<SkimmerScooper>();
        buffle.SetActive(true);
    }

    public void GameStart()
    {
        buffle.SetActive(false);
        CountdownClock.Instance.StartCountdown();
        potAudio.clip = SoundManager.Instance.boilingSound;
        bgmAudio.clip = SoundManager.Instance.bgm;
        //noiseAudio.clip = SoundManager.Instance.noise;
        potAudio.Play();
        bgmAudio.Play();
        //noiseAudio.Play();
    }

    public void GameOver()
    {
        buffle.SetActive(true);
        potAudio.Stop();
        bgmAudio.Stop();
        //noiseAudio.Stop();
        //Invoke("Reset", 3f);
    }

    public void Reset()
    {
        Debug.Log("reset");
        FoodSpawner.Instance.ResetGame();
        ComboManager.Instance.ResetCombo();
        CountdownClock.Instance.ResetClock();
        skimmerScooper.ResetSkimmer();

        CountdownClock.Instance.StartGame();
    }
}
