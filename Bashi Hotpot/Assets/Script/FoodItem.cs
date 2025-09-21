using System.Collections;
using UnityEngine;

public class FoodItem : MonoBehaviour
{
    public enum FoodState { Wait, InPot, Exiting }
    public FoodState currentState = FoodState.Wait;

    private Rigidbody rb;
    private BuoyancyObject buoyancy;

    private Camera cam;
    private bool isDragging = false;
    private Vector3 offset;

    [Header("夹起动画参数")]
    public float exitSpeed = 5f;
    public Vector3 exitDirection = new Vector3(0, 1, -1);
    [Header("煮熟参数")]
    public float cookTime = 5f;         // 煮熟所需时间
    public float overcookTime = 10f;    // 变老所需时间
    private float cookTimer = 0f;
    public Color raw;
    public Color ripe;
    public Color burnt;
    public enum CookState { Raw, Cooked, Overcooked }
    private CookState cookState = CookState.Raw;

    private Material instanceMaterial;
    public FoodType foodType;
    private Vector3 initialPos;
    private Quaternion initialRot;

    private Collider col;

    AudioSource audioSource;

    private int foodLayer;
    private int potEdgeLayer;
    GameObject smokePrefab;
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        buoyancy = GetComponent<BuoyancyObject>();
        cam = Camera.main;
        col = GetComponent<Collider>();
        smokePrefab = FoodSpawner.Instance.smoke;

        rb.useGravity = false;
        rb.isKinematic = true;
        if (buoyancy != null) buoyancy.isActive = false;

        if (col != null) col.isTrigger = true;   // 初始禁用碰撞体

        instanceMaterial = GetComponent<Renderer>().material;
        instanceMaterial.color = raw;

        initialPos = transform.position;
        initialRot = transform.rotation;

        foodLayer = LayerMask.NameToLayer("Food");
        potEdgeLayer = LayerMask.NameToLayer("PotEdge");

        // 初始禁用 Food 与 PotEdge 的碰撞
        if (foodLayer >= 0 && potEdgeLayer >= 0)
        {
            Physics.IgnoreLayerCollision(foodLayer, potEdgeLayer, true);
        }
    }

    void OnMouseDown()
    {
        Debug.Log("OnMouseDown");
        if (currentState != FoodState.Wait) return;
        isDragging = true;

        Vector3 worldPos = cam.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            cam.WorldToScreenPoint(transform.position).z));
        offset = transform.position - worldPos;

        //audioSource.clip = SoundManager.Instance.pickSound;
        //audioSource.Play();
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 worldPos = cam.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            cam.WorldToScreenPoint(transform.position).z));
        transform.position = worldPos + offset;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        // 检测是否丢进锅里
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Hotpot"))
        {
            EnterPot();
            audioSource.clip = SoundManager.Instance.dropSound;
            audioSource.Play();
        }
        else
        {
            // 回到原位
            StartCoroutine(ReturnToOrigin());
        }
    }

    IEnumerator ReturnToOrigin()
    {
        float duration = 0.3f;  // 回弹时长
        float t = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;
            // 可加个缓动
            k = Mathf.Sin(k * Mathf.PI * 0.5f);

            transform.position = Vector3.Lerp(startPos, initialPos, k);
            transform.rotation = Quaternion.Slerp(startRot, initialRot, k);

            yield return null;
        }

        transform.position = initialPos;
        transform.rotation = initialRot;
    }


    public CookState GetCookState()
    {
        return cookState;
    }

    private void EnterPot()
    {
        currentState = FoodState.InPot;

        rb.isKinematic = false;
        rb.useGravity = true;
        if (buoyancy != null) buoyancy.isActive = true;

        if (col != null) col.isTrigger = false;   // 下锅时启用碰撞体
        if (foodLayer >= 0 && potEdgeLayer >= 0)
        {
            Physics.IgnoreLayerCollision(foodLayer, potEdgeLayer, false);
        }

        Debug.Log(name + " 下锅了！");
    }

    private void StartExit()
    {
        currentState = FoodState.Exiting;

        rb.isKinematic = true;
        rb.useGravity = false;

        if (buoyancy != null) buoyancy.isActive = false;

        FoodSpawner.Instance.OnFoodTaken(foodType);
        ComboManager.Instance.OnFoodTaken(this);

        Debug.Log(name + " 被夹起来了！");
    }

    void Update()
    {
        // 煮熟计时逻辑
        if (currentState == FoodState.InPot)
        {
            cookTimer += Time.deltaTime;

            if (cookState == CookState.Raw && cookTimer >= cookTime)
            {
                cookState = CookState.Cooked;
                instanceMaterial.color = ripe; // 煮熟颜色
                Debug.Log(name + " 煮熟了");
                audioSource.clip = SoundManager.Instance.ripeSound;
                audioSource.Play();

                if (smokePrefab != null)
                {
                    Instantiate(smokePrefab, transform.position,smokePrefab.transform.rotation);
                }
            }
            else if (cookState == CookState.Cooked && cookTimer >= overcookTime+cookTime)
            {
                cookState = CookState.Overcooked;
                instanceMaterial.color = burnt; // 过老颜色
                Debug.Log(name + " 煮老了");
                ScoreManager.Instance.ChangeScore(-3);
            }

            // 点击检测（夹起）
            if (Input.GetMouseButtonDown(0))
            {
                int foodLayerMask = 1 << LayerMask.NameToLayer("Food");
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100f, foodLayerMask))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        if (cookState == CookState.Cooked)
                        {
                            ScoreManager.Instance.ChangeScore(5);
                        }
                        else if(cookState == CookState.Raw)
                        {
                            ScoreManager.Instance.ChangeScore(-3);
                        }

                        StartExit();

                        audioSource.clip = SoundManager.Instance.climpSound;
                        audioSource.Play();
                    }
                }
            }
        }

        if (currentState == FoodState.Exiting)
        {
            transform.position += exitDirection.normalized * exitSpeed * Time.deltaTime;

            Vector3 screenPos = cam.WorldToScreenPoint(transform.position);
            if (screenPos.y > Screen.height + 200)
            {
                Destroy(gameObject);
            }
        }
    }

}
