using UnityEngine;

public class WaterManager : MonoBehaviour
{
    public static WaterManager Instance { get; private set; }

    [Header("基础参数")]
    public float baseWaterHeight = 0f;
    public float floatStrength = 10f;
    public float damping = 0.5f;

    [Header("波浪参数")]
    public bool enableWaves = false;
    public float waveAmplitude = 0.5f;
    public float waveFrequency = 0.5f;
    public float waveSpeed = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    /// 获取某个位置的水面高度
    public float GetWaterHeight(Vector3 pos)
    {
        if (!enableWaves) return baseWaterHeight;

        // 简单波浪公式 (sin+cos)
        float wave = Mathf.Sin(pos.x * waveFrequency + Time.time * waveSpeed) * waveAmplitude
                   + Mathf.Cos(pos.z * waveFrequency + Time.time * waveSpeed) * waveAmplitude * 0.5f;
        return baseWaterHeight + wave;
    }
}
