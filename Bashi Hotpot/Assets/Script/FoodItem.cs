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


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        buoyancy = GetComponent<BuoyancyObject>();
        cam = Camera.main;

        // 初始状态不受物理影响
        rb.useGravity = false;
        rb.isKinematic = true;
        if (buoyancy != null) buoyancy.isActive = false;
        // 克隆一个独立材质实例，避免改到别的食物
        instanceMaterial = GetComponent<Renderer>().material;
        instanceMaterial.color = raw; // 初始未熟颜色
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
        }
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
