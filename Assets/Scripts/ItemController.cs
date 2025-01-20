using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.Rendering;

public class ItemController : MonoBehaviour
{

    [Header("Main Game Settings")]
    [SerializeField] private MainGameStats mainGameStats;

    [Header("Global Settings")]
    [Tooltip("Сила гравитация действующая на все предметы, чем она сильнее, тем быстрее падают предметы")]
    private float gravityScale;

    [Header("Main Item Settings")]
    [Tooltip("Масса предмета, влияющая на скорость падения при отпускании предмета")]
    [SerializeField, Range(1f, 3f)] private float itemMass = 1;
    [Tooltip("Уникальный ID предмета")]
    private string itemId;

    [Header("Detection Settings")]
    [Tooltip("Радиус проверки цели")]
    [SerializeField] private float detectionRadius = 0.5f;
    [Tooltip("Минимальное расстояние вниз")]
    [SerializeField] private float minDistance = 1f;
    [Tooltip("Шаг опускания вниз при поиске")]
    private float descentStep = 0.1f;
    [Tooltip("Точка, где находится цель")]
    private Vector2 targetPosition;
    [Tooltip("Смещение объекта относительно границ коллайдера")]
    [SerializeField] private Vector2 targetOffset;
    [Tooltip("Максимальное расстояние падения")]
    private float maxStepDistance = 100f;

    [Header("Animation Settings")]
    [Header("Bounce Animation")]
    [Tooltip("Сила отскакивания")]
    [SerializeField] private float bounceHeight = 0.5f; 
    [Tooltip("Скорость анимации")]
    [SerializeField] private float bounceSpeed = 1f;
    [Space(5), Header("Scale Animation")]
    [Tooltip("Начальный размер предмета")]
    [SerializeField] private float startItemScale = 1f;
    [Tooltip("Длительность анимации изменения размера")]
    [SerializeField] private float scaleDuration = 0.5f;
    private Coroutine scaleCoroutine;

    [Header("Trigger Settings")]
    [Tooltip("Текущий тэг, поверхности на которой стоит предмет")]
    private string currSurfaceTag;
    [Tooltip("Сохранение текущего триггера")]
    [SerializeField]
    private Collider2D currentTrigger;

    [Header("Mask Settigs")]
    [SerializeField] private LayerMask surfaceMask;
    [SerializeField] private LayerMask floorMask;

    [Header("Components")]
    [Tooltip("Объект родитель спрайта предмета, для дальнейших анимаций")]
    [SerializeField] private GameObject itemSpriteHolder;
    [SerializeField] private SortingGroup sortingGroup;

    [Header("Bools")]
    private bool isItemReleased;
    private bool isItemInHand;
    private bool isItemFalls;


    private void Awake()
    {
        // Получаем или создаём уникальный ID через
        itemId = IdItemData.GetOrCreateId(gameObject.name);
    }

    void Start()
    {
        LoadItemPosition();
        StartSettings();
        CheckStartSurface();
    }
    
    //начальные настройки
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

    //проверяем стартовое положение
    private void CheckStartSurface()
    {
        //проверяем на любую поверхность
        Vector3 checkPos = transform.position;
        Collider2D hit = Physics2D.OverlapCircle(checkPos, detectionRadius * 5f, surfaceMask);

        //если нашли объект Surface
        if (hit != null)
        {

            currentTrigger = hit;
            currSurfaceTag = hit.tag;

            //устанавливаем размер в зависимости от поверхности
            SurfaceStats currSurfaceStats = currentTrigger.gameObject.GetComponent<SurfaceStats>();
            Vector3 itemScale = new Vector3(currSurfaceStats.SurfaceItemSize, currSurfaceStats.SurfaceItemSize, currSurfaceStats.SurfaceItemSize);
            itemSpriteHolder.transform.localScale = itemScale;

            //устанавливаем слой
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
    //ищем цель - место на которое падаем
    private void FindTarget()
    {
        if (isItemInHand && !isItemFalls)
        {
            Vector2 origin = transform.position;

            // Всегда шагаем вниз, проверяя все триггеры
            targetPosition = origin + Vector2.down * minDistance;
            DescendAndFindTrigger();
        }
    }

    private void DescendAndFindTrigger()
    {
        Vector2 origin = targetPosition - targetOffset;

        while (true)
        {
            //проверяем на Surface
            Collider2D hit = Physics2D.OverlapCircle(origin, detectionRadius, surfaceMask);

            //если нашли объект Surface
            if (hit != null)
            {
                currentTrigger = hit;
                currSurfaceTag = hit.tag;
                targetPosition = hit.ClosestPoint(origin) + targetOffset;
            }
            else
            {
                //если не нашли Surface - ищем floor
                hit = Physics2D.OverlapCircle(origin, detectionRadius, floorMask);

                if (hit != null)
                {
                    currentTrigger = hit;
                    currSurfaceTag = hit.tag;
                    targetPosition = hit.ClosestPoint(origin) + targetOffset;
                }
            }

            //если нашли - выходим
            if (hit != null) break;

            // Продолжаем опускать точку вниз
            origin += Vector2.down * descentStep;

            // Ограничиваем глубину поиска
            if (origin.y < transform.position.y - maxStepDistance)
                break;

        }
    }

    // Метод для отрисовки Gizmos
    private void OnDrawGizmos()
    {
        if (isItemInHand)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPosition, transform.localScale.x / 2f);
        }
    }
    #endregion

    //схватили предмет
    public void ItemGrabbed()
    {
        isItemInHand = true;
    }

    //Отпустили предмет
    public void ItemReleased()
    {
        if (isItemInHand)
        {
            isItemReleased = true;
            isItemFalls = true;
            isItemInHand = false;
        }
    }

    //предмет падает
    private void ItemFalls()
    {
        if (!isItemFalls || targetPosition == null) return;

        // Вычисляем расстояние до целевой точки
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

        // Если предмет достиг цели, останавливаем падение
        if (distanceToTarget <= 0.01f)
        {
            transform.position = targetPosition; // Устанавливаем точную позицию
            isItemFalls = false;
            isItemReleased = false;

            //устанавливаем слой и размер
            SurfaceStats currSurfaceStats = currentTrigger.gameObject.GetComponent<SurfaceStats>();
            if (currSurfaceStats != null)
            {
                SetItemLayer(currSurfaceStats.SurfaceItemlLayer);
                StartScaleAnimation(currSurfaceStats.SurfaceItemSize);
            }

            //сохраняяем позицию объекта
            SavePosition();

            // Запускаем анимацию отскока после падения
            StartCoroutine(BounceAnimation());
            return;
        }

        // Ускорение падения (чем больше gravityScale и itemMass, тем быстрее падает)
        float fallSpeed = gravityScale * itemMass * Time.fixedDeltaTime;

        // Двигаем объект в направлении цели
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, fallSpeed);
    }

    #region Save And Load Data

    //загружаем сохраненную позицию предмета - если есть
    private void LoadItemPosition()
    {
        var savedPosition = SaveData.LoadPosition(itemId);

        //если позиция не пустая - ставим объект в позицию
        if (savedPosition.HasValue)
        {
            transform.position = savedPosition.Value;
        }
    }

    //сохраняем позицию объекта
    private void SavePosition()
    {
        //сохраняем текущую позицию 
        SaveData.SavePosition(itemId, transform.position);
    }

    #endregion

    #region Animations
    // Метод для анимации отскакивания
    private IEnumerator BounceAnimation()
    {
        float elapsedTime = 0f;
        Vector3 originalPosition = itemSpriteHolder.transform.position;

        // Плавное движение вверх
        float firstStateSpeed = bounceSpeed / 2f;
        while (elapsedTime < firstStateSpeed)
        {
            float height = Mathf.Lerp(0, bounceHeight, elapsedTime / firstStateSpeed); // Высота отскока
            itemSpriteHolder.transform.position = originalPosition + new Vector3(0, height, 0); // Перемещение вверх
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Плавное возвращение вниз
        elapsedTime = 0f;
        float secondStateSpeed = bounceSpeed / 2f;
        while (elapsedTime < secondStateSpeed)
        {
            float height = Mathf.Lerp(bounceHeight, 0, elapsedTime / secondStateSpeed); // Возвращаемся обратно
            itemSpriteHolder.transform.position = originalPosition + new Vector3(0, height, 0); // Перемещение вниз
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        itemSpriteHolder.transform.position = originalPosition; // Возвращаемся в начальную позицию
    }

    // Метод для запуска анимации изменения размера
    public void StartScaleAnimation(float targetScale)
    {
        // Проверяем, если новый размер отличается от текущего
        if (itemSpriteHolder.transform.localScale.x != targetScale)
        {
            // Если анимация уже идет, останавливаем текущую
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
            }

            // Запускаем новую анимацию
            scaleCoroutine = StartCoroutine(ScaleAnimation(targetScale));
        }
    }

    // Карутина с анимацией изменения размера
    private IEnumerator ScaleAnimation(float targetScale)
    {
        //задаем вектор
        Vector3 targetScaleVector = new Vector3(targetScale, targetScale, targetScale);

        float elapsedTime = 0f;
        Vector3 initialScale = itemSpriteHolder.transform.localScale;

        // Плавное изменение масштаба
        while (elapsedTime < scaleDuration)
        {
            itemSpriteHolder.transform.localScale = Vector3.Lerp(initialScale, targetScaleVector, elapsedTime / scaleDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Устанавливаем финальный масштаб
        itemSpriteHolder.transform.localScale = targetScaleVector;
    }

    #endregion

    #region Public Stats

    //устанавливаем индекс слоя
    public void SetItemLayer(int layerInd)
    {
        sortingGroup.sortingOrder = layerInd;
    }

    public bool IsItemFalls { get =>  isItemFalls; }

    #endregion
}
