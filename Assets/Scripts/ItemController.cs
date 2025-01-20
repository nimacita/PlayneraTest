using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.Rendering;

public class ItemController : MonoBehaviour
{

    [Header("Main Game Settings")]
    [SerializeField] private MainGameStats mainGameStats;

    [Header("Global Settings")]
    [Tooltip("���� ���������� ����������� �� ��� ��������, ��� ��� �������, ��� ������� ������ ��������")]
    private float gravityScale;

    [Header("Main Item Settings")]
    [Tooltip("����� ��������, �������� �� �������� ������� ��� ���������� ��������")]
    [SerializeField, Range(1f, 3f)] private float itemMass = 1;
    [Tooltip("���������� ID ��������")]
    private string itemId;

    [Header("Detection Settings")]
    [Tooltip("������ �������� ����")]
    [SerializeField] private float detectionRadius = 0.5f;
    [Tooltip("����������� ���������� ����")]
    [SerializeField] private float minDistance = 1f;
    [Tooltip("��� ��������� ���� ��� ������")]
    private float descentStep = 0.1f;
    [Tooltip("�����, ��� ��������� ����")]
    private Vector2 targetPosition;
    [Tooltip("�������� ������� ������������ ������ ����������")]
    [SerializeField] private Vector2 targetOffset;
    [Tooltip("������������ ���������� �������")]
    private float maxStepDistance = 100f;

    [Header("Animation Settings")]
    [Header("Bounce Animation")]
    [Tooltip("���� ������������")]
    [SerializeField] private float bounceHeight = 0.5f; 
    [Tooltip("�������� ��������")]
    [SerializeField] private float bounceSpeed = 1f;
    [Space(5), Header("Scale Animation")]
    [Tooltip("��������� ������ ��������")]
    [SerializeField] private float startItemScale = 1f;
    [Tooltip("������������ �������� ��������� �������")]
    [SerializeField] private float scaleDuration = 0.5f;
    private Coroutine scaleCoroutine;

    [Header("Trigger Settings")]
    [Tooltip("������� ���, ����������� �� ������� ����� �������")]
    private string currSurfaceTag;
    [Tooltip("���������� �������� ��������")]
    [SerializeField]
    private Collider2D currentTrigger;

    [Header("Mask Settigs")]
    [SerializeField] private LayerMask surfaceMask;
    [SerializeField] private LayerMask floorMask;

    [Header("Components")]
    [Tooltip("������ �������� ������� ��������, ��� ���������� ��������")]
    [SerializeField] private GameObject itemSpriteHolder;
    [SerializeField] private SortingGroup sortingGroup;

    [Header("Bools")]
    private bool isItemReleased;
    private bool isItemInHand;
    private bool isItemFalls;


    private void Awake()
    {
        // �������� ��� ������ ���������� ID �����
        itemId = IdItemData.GetOrCreateId(gameObject.name);
    }

    void Start()
    {
        LoadItemPosition();
        StartSettings();
        CheckStartSurface();
    }
    
    //��������� ���������
    private void StartSettings()
    {
        gravityScale = mainGameStats.gravityScale;

        currSurfaceTag = null;

        isItemReleased = false;
        isItemInHand = false;
        isItemFalls = false;

        targetPosition = Vector2.zero;
        currentTrigger = null;

    }

    //��������� ��������� ���������
    private void CheckStartSurface()
    {
        //��������� �� ����� �����������
        Vector3 checkPos = transform.position;
        Collider2D hit = Physics2D.OverlapCircle(checkPos, detectionRadius * 5f, surfaceMask);

        //���� ����� ������ Surface
        if (hit != null)
        {

            currentTrigger = hit;
            currSurfaceTag = hit.tag;

            //������������� ������ � ����������� �� �����������
            SurfaceStats currSurfaceStats = currentTrigger.gameObject.GetComponent<SurfaceStats>();
            Vector3 itemScale = new Vector3(currSurfaceStats.SurfaceItemSize, currSurfaceStats.SurfaceItemSize, currSurfaceStats.SurfaceItemSize);
            itemSpriteHolder.transform.localScale = itemScale;

            //������������� ����
            SetItemLayer(currSurfaceStats.SurfaceItemlLayer);
            StartScaleAnimation(currSurfaceStats.SurfaceItemSize);
        }
    }

    void FixedUpdate()
    {
        FindTarget();
        ItemFalls();
    }

    #region Find Target
    //���� ���� - ����� �� ������� ������
    private void FindTarget()
    {
        if (isItemInHand && !isItemFalls)
        {
            Vector2 origin = transform.position;

            // ������ ������ ����, �������� ��� ��������
            targetPosition = origin + Vector2.down * minDistance;
            DescendAndFindTrigger();
        }
    }

    private void DescendAndFindTrigger()
    {
        Vector2 origin = targetPosition - targetOffset;

        while (true)
        {
            //��������� �� Surface
            Collider2D hit = Physics2D.OverlapCircle(origin, detectionRadius, surfaceMask);

            //���� ����� ������ Surface
            if (hit != null)
            {
                currentTrigger = hit;
                currSurfaceTag = hit.tag;
                targetPosition = hit.ClosestPoint(origin) + targetOffset;
            }
            else
            {
                //���� �� ����� Surface - ���� floor
                hit = Physics2D.OverlapCircle(origin, detectionRadius, floorMask);

                if (hit != null)
                {
                    currentTrigger = hit;
                    currSurfaceTag = hit.tag;
                    targetPosition = hit.ClosestPoint(origin) + targetOffset;
                }
            }

            //���� ����� - �������
            if (hit != null) break;

            // ���������� �������� ����� ����
            origin += Vector2.down * descentStep;

            // ������������ ������� ������
            if (origin.y < transform.position.y - maxStepDistance)
                break;

        }
    }

    // ����� ��� ��������� Gizmos
    private void OnDrawGizmos()
    {
        if (isItemInHand)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPosition, transform.localScale.x / 2f);
        }
    }
    #endregion

    //�������� �������
    public void ItemGrabbed()
    {
        isItemInHand = true;
    }

    //��������� �������
    public void ItemReleased()
    {
        if (isItemInHand)
        {
            isItemReleased = true;
            isItemFalls = true;
            isItemInHand = false;
        }
    }

    //������� ������
    private void ItemFalls()
    {
        if (!isItemFalls || targetPosition == null) return;

        // ��������� ���������� �� ������� �����
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        // ���� ������� ������ ����, ������������� �������
        if (distanceToTarget <= 0.01f)
        {
            transform.position = targetPosition; // ������������� ������ �������
            isItemFalls = false;
            isItemReleased = false;

            //������������� ���� � ������
            SurfaceStats currSurfaceStats = currentTrigger.gameObject.GetComponent<SurfaceStats>();
            if (currSurfaceStats != null)
            {
                SetItemLayer(currSurfaceStats.SurfaceItemlLayer);
                StartScaleAnimation(currSurfaceStats.SurfaceItemSize);
            }

            //���������� ������� �������
            SavePosition();

            // ��������� �������� ������� ����� �������
            StartCoroutine(BounceAnimation());
            return;
        }

        // ��������� ������� (��� ������ gravityScale � itemMass, ��� ������� ������)
        float fallSpeed = gravityScale * itemMass * Time.fixedDeltaTime;

        // ������� ������ � ����������� ����
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, fallSpeed);
    }

    #region Save And Load Data

    //��������� ����������� ������� �������� - ���� ����
    private void LoadItemPosition()
    {
        var savedPosition = SaveData.LoadPosition(itemId);

        //���� ������� �� ������ - ������ ������ � �������
        if (savedPosition.HasValue)
        {
            transform.position = savedPosition.Value;
        }
    }

    //��������� ������� �������
    private void SavePosition()
    {
        //��������� ������� ������� 
        SaveData.SavePosition(itemId, transform.position);
    }

    #endregion

    #region Animations
    // ����� ��� �������� ������������
    private IEnumerator BounceAnimation()
    {
        float elapsedTime = 0f;
        Vector3 originalPosition = itemSpriteHolder.transform.position;

        // ������� �������� �����
        float firstStateSpeed = bounceSpeed / 2f;
        while (elapsedTime < firstStateSpeed)
        {
            float height = Mathf.Lerp(0, bounceHeight, elapsedTime / firstStateSpeed); // ������ �������
            itemSpriteHolder.transform.position = originalPosition + new Vector3(0, height, 0); // ����������� �����
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ������� ����������� ����
        elapsedTime = 0f;
        float secondStateSpeed = bounceSpeed / 2f;
        while (elapsedTime < secondStateSpeed)
        {
            float height = Mathf.Lerp(bounceHeight, 0, elapsedTime / secondStateSpeed); // ������������ �������
            itemSpriteHolder.transform.position = originalPosition + new Vector3(0, height, 0); // ����������� ����
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        itemSpriteHolder.transform.position = originalPosition; // ������������ � ��������� �������
    }

    // ����� ��� ������� �������� ��������� �������
    public void StartScaleAnimation(float targetScale)
    {
        // ���������, ���� ����� ������ ���������� �� ��������
        if (itemSpriteHolder.transform.localScale.x != targetScale)
        {
            // ���� �������� ��� ����, ������������� �������
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
            }

            // ��������� ����� ��������
            scaleCoroutine = StartCoroutine(ScaleAnimation(targetScale));
        }
    }

    // �������� � ��������� ��������� �������
    private IEnumerator ScaleAnimation(float targetScale)
    {
        //������ ������
        Vector3 targetScaleVector = new Vector3(targetScale, targetScale, targetScale);

        float elapsedTime = 0f;
        Vector3 initialScale = itemSpriteHolder.transform.localScale;

        // ������� ��������� ��������
        while (elapsedTime < scaleDuration)
        {
            itemSpriteHolder.transform.localScale = Vector3.Lerp(initialScale, targetScaleVector, elapsedTime / scaleDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ������������� ��������� �������
        itemSpriteHolder.transform.localScale = targetScaleVector;
    }

    #endregion

    #region Public Stats

    //������������� ������ ����
    public void SetItemLayer(int layerInd)
    {
        sortingGroup.sortingOrder = layerInd;
    }

    public bool IsItemFalls { get =>  isItemFalls; }

    #endregion
}
