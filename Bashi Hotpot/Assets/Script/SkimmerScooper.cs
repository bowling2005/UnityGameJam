using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkimmerScooper : MonoBehaviour
{
    [Header("必填引用")]
    public Transform approachTarget;     // 锅边的指定位置
    public Vector3 targetRotationRef;  // 指定的目标旋转(引用其 rotation)
    public Transform circleCenter;       // 绕圈的圆心
    public Transform scoopAnchor;        // 食物吸附点(漏勺下的一个子物体)
    public Transform offscreenTarget;    // 移出屏幕的目标点

    [Header("绕圈参数")]
    public float circleRadius = 0.4f;      // 半径(世界单位)
    public float circleDuration = 1.5f;    // 绕一圈用时
    public Vector3 circlePlaneNormal = Vector3.up; // 围绕的平面法线(默认水平绕圈)

    [Header("移动/旋转时长")]
    public float approachDuration = 0.6f;    // 飞到锅边时长(同时插值旋转)
    public float returnDuration = 0.6f;      // 回位时长
    public float exitDuration = 0.5f;        // 移出屏幕时长

    [Header("动画曲线")]
    public AnimationCurve moveEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve circleEase = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve collectEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("吸附参数")]
    public float collectDuration = 0.25f;     // 食物吸上来的时间
    public float collectScanInterval = 0.1f;  // 绕圈期间，每隔多久扫描一次可收集食物
    public float collectMaxDistance = 2.0f;   // 距离过远可不吸(可设大点)
    public float scoopOffsetJitter = 0.02f;   // 吸附到锚点时的微抖散布(防重叠)
    public float foodScaleOnCollect = 0.85f;  // 吸附时可略缩小一点，显得装进漏勺

    [Header("冷却设置")]
    public float cooldownTime = 25f;        // 冷却时长
    private bool onCooldown = false;
    private Material instMat;
    private Color originalColor;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private bool busy = false;
    private readonly List<FoodItem> collected = new List<FoodItem>();


    void Awake()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;

        instMat = GetComponent<Renderer>().material;
        originalColor = instMat.color;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !busy && !onCooldown) // 左键点击
        {
            Camera cam = Camera.main;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 忽略 HotpotLayer
            int layerMask = ~(LayerMask.GetMask("HotpotLayer") | LayerMask.GetMask("PotEdge"));
            if (Physics.Raycast(ray, out hit, 100f, layerMask))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    StartCoroutine(RunRoutine());
                }
            }
        }
    }


    //void OnMouseDown()
    //{
    //    if (!busy) StartCoroutine(RunRoutine());
    //}

    IEnumerator RunRoutine()
    {
        busy = true;
        collected.Clear();

        // 1) 平滑移动到锅边 + 旋转至目标四元数
        yield return TweenMoveAndRotate(transform, approachTarget.position,
                                        targetRotationRef != null ? Quaternion.Euler(targetRotationRef.x,targetRotationRef.y,targetRotationRef.z) : transform.rotation,
                                        approachDuration, moveEase);

        // 2) 保持该旋转，围绕 circleCenter 以 circleRadius 绕一圈
        yield return OrbitOnce();

        // 3) 移出屏幕，销毁已收集食物
        yield return TweenMoveAndRotate(transform, offscreenTarget.position, transform.rotation, exitDuration, moveEase);
        DestroyCollected();

        // 4) 回到初始位姿
        yield return TweenMoveAndRotate(transform, originalPos, originalRot, returnDuration, moveEase);

        busy = false;
        StartCoroutine(CooldownRoutine());
    }

    IEnumerator CooldownRoutine()
    {
        onCooldown = true;
        instMat.color = Color.black;

        yield return new WaitForSeconds(cooldownTime);

        // 冷却结束
        instMat.color = originalColor;

        // 提示动画：放大再缩回
        Vector3 startScale = transform.localScale;
        Vector3 bigScale = startScale * 1.2f;
        float dur = 0.2f;
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            transform.localScale = Vector3.Lerp(startScale, bigScale, k);
            yield return null;
        }

        t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = t / dur;
            transform.localScale = Vector3.Lerp(bigScale, startScale, k);
            yield return null;
        }
        transform.localScale = startScale;

        onCooldown = false;
    }

    IEnumerator OrbitOnce()
    {
        // 起始方向 = 当前点相对圆心的方向
        Vector3 startDir = (transform.position - circleCenter.position);
        if (startDir.sqrMagnitude < 1e-6f) startDir = Vector3.forward;
        startDir = Vector3.ProjectOnPlane(startDir, circlePlaneNormal).normalized;

        float radiusNow = (transform.position - circleCenter.position).magnitude;
        if (Mathf.Abs(radiusNow - circleRadius) > 0.001f)
        {
            // 如果当前位置不在圆周上，把半径修正成当前位置的半径
            circleRadius = radiusNow;
        }

        float t = 0f;
        float totalAngle = 360f;
        float scanTimer = 0f;

        // 记录进入 OrbitOnce 时的旋转
        Quaternion keepRot = transform.rotation;

        while (t < circleDuration)
        {
            float dt = Time.deltaTime;
            t += dt;
            float k = Mathf.Clamp01(t / circleDuration);
            float eased = circleEase.Evaluate(k);

            // 当前角度 = 起点 + eased * 总角度
            float angle = eased * totalAngle;

            Quaternion rotAround = Quaternion.AngleAxis(angle, circlePlaneNormal.normalized);
            Vector3 posOnCircle = circleCenter.position + rotAround * (startDir * circleRadius);

            transform.position = posOnCircle;
            transform.rotation = keepRot;  // 保持第一阶段的旋转，不跳

            // 周期性扫描
            scanTimer -= dt;
            if (scanTimer <= 0f)
            {
                ScanAndCollect();
                scanTimer = collectScanInterval;
            }

            yield return null;
        }

        ScanAndCollect();
        yield return new WaitForSeconds(collectDuration * 0.5f);
    }



    void ScanAndCollect()
    {
        // 找到所有符合条件的食物：在锅里 && 熟 或 老
        FoodItem[] all = GameObject.FindObjectsOfType<FoodItem>();
        foreach (var f in all)
        {
            if (collected.Contains(f)) continue;
            // 需要你已有的方法/字段：在锅里判定 + 熟/老
            bool inPot = (f.currentState == FoodItem.FoodState.InPot);
            var cs = f.GetCookState(); // 你前面已实现
            bool ready = (cs == FoodItem.CookState.Cooked || cs == FoodItem.CookState.Overcooked);

            if (!inPot || !ready) continue;

            float d = Vector3.Distance(f.transform.position, transform.position);
            if (d > collectMaxDistance) continue;

            StartCoroutine(MoveFoodToScoop(f));
            collected.Add(f);
        }
    }

    IEnumerator MoveFoodToScoop(FoodItem food)
    {
        if(food.GetCookState()==FoodItem.CookState.Cooked)
        {
            ScoreManager.Instance.ChangeScore(5);
        }
        // 关掉物理 & 浮力，避免拉扯
        var rb = food.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        var buoy = food.GetComponent<BuoyancyObject>();
        if (buoy) buoy.isActive = false;

        // 记住初始
        Transform tr = food.transform;
        Vector3 startPos = tr.position;
        Quaternion startRot = tr.rotation;
        Vector3 targetPos = scoopAnchor.position +
                            Random.insideUnitSphere * scoopOffsetJitter; // 微抖避免完全重叠
        Quaternion targetRot = transform.rotation; // 让食物朝向和漏勺一致
        Vector3 startScale = tr.localScale;
        Vector3 endScale = startScale * foodScaleOnCollect;

        float t = 0f;
        while (t < collectDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / collectDuration);
            float e = collectEase.Evaluate(k);

            tr.position = Vector3.Lerp(startPos, targetPos, e);
            tr.rotation = Quaternion.Slerp(startRot, targetRot, e);
            tr.localScale = Vector3.Lerp(startScale, endScale, e);
            yield return null;
        }

        // 吸附完成后，挂到漏勺下面
        tr.SetParent(scoopAnchor, true);
        tr.position = targetPos;
        tr.rotation = targetRot;
    }

    void DestroyCollected()
    {
        for (int i = 0; i < collected.Count; i++)
        {
            if (collected[i] != null)
            {
                Destroy(collected[i].gameObject);
            }
        }
        collected.Clear();
    }

    IEnumerator TweenMoveAndRotate(Transform target, Vector3 toPos, Quaternion toRot, float duration, AnimationCurve ease)
    {
        Vector3 fromPos = target.position;
        Quaternion fromRot = target.rotation;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float e = ease.Evaluate(k);
            target.position = Vector3.Lerp(fromPos, toPos, e);
            target.rotation = Quaternion.Slerp(fromRot, toRot, e);
            yield return null;
        }

        target.position = toPos;
        target.rotation = toRot;
    }
}
