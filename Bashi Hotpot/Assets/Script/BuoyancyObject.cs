using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BuoyancyObject : MonoBehaviour
{
    private Rigidbody rb;

    [HideInInspector]
    public bool isActive = false; // 默认关闭，等下锅时由 FoodItem 打开

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!isActive) return; // 未激活时不施加浮力
        if (WaterManager.Instance == null) return;

        float waterHeight = WaterManager.Instance.GetWaterHeight(transform.position);
        float diff = waterHeight - transform.position.y;

        if (diff > 0f) // 低于水面，施加浮力
        {
            rb.AddForce(Vector3.up * diff * WaterManager.Instance.floatStrength,
                        ForceMode.Acceleration);

            rb.velocity *= (1f - WaterManager.Instance.damping * Time.fixedDeltaTime);
        }
    }
}
