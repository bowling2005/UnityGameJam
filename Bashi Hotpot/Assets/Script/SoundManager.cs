using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    AudioSource backGorund;

    public AudioClip pickSound;
    public AudioClip dropSound;
    public AudioClip ripeSound;
    public AudioClip climpSound;
    public AudioClip countDownSound;
    public AudioClip overSound;
    public AudioClip boilingSound;
    public AudioClip bgm;
    public AudioClip noise;
    public AudioClip addScore;
    public AudioClip subScore;
    public AudioClip skimmer;
    public AudioClip combo;
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        backGorund = GetComponent<AudioSource>();
    }
    void Start()
    {
        
    }
}
