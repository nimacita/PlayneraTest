using UnityEngine;

public class DragController : MonoBehaviour
{

    [Header("Position Settings")]
    [Tooltip("Смещение между пальцем и центром текущего объекта")]
    private Vector3 offset;

    [Header("Item Settings")]
    [Tooltip("Размер предмета в руке")]
    [SerializeField] private float itemInHandScale = 1.15f;
    [Tooltip("Номер слоя для предмета в руке")]
    [SerializeField] private int itemInHandLayer = 50;

    [Header("Mask")]
    [SerializeField] private LayerMask itemMask;

    [Header("Components")]
    [SerializeField] private Camera mainCamera;
    private GameObject selectedObject;  // Текущий выбранный объект
    private ItemController currItemController; // Скрипт текущего выбранного объекта

    [Header("Bools")]
    private bool isItemInHand;

    void Start()
    {
        StartSettings();
    }

    //начальные настройки
    private void StartSettings()
    {
        isItemInHand = false;
    }

    void Update()
    {
        TouchsHandler();
    }

    //обрабатываем касания если есть
    private void TouchsHandler()
    {
        //проверяем наличие касаний
        if (Input.touchCount > 0) 
        {
            //получаем касание
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = mainCamera.ScreenToWorldPoint(touch.position);
            touchPosition.z = 0f;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    //начало касания
                    HandleTouchBegin(touchPosition);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    //перемещение пальца
                    if (selectedObject != null && isItemInHand)
                    {
                        //двигаем предмет
                        MoveItem(touchPosition);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    //завершение касания
                    if (selectedObject != null)
                    {
                        HandleTouchEnd();
                    }
                    break;
            }
        }
    }

    //двигаем предмет
    private void MoveItem(Vector3 touchPosition)
    {
        selectedObject.transform.position = new Vector3(touchPosition.x - offset.x, 
            touchPosition.y - offset.y, selectedObject.transform.position.z);
    }

    // Обработка начала касания и подбора предмета
    private void HandleTouchBegin(Vector3 touchPosition)
    {
        //если предмет не в руке
        if (!isItemInHand)
        {
            // Проверяем на попадаение
            Collider2D collider = Physics2D.OverlapPoint(touchPosition, itemMask);

            // Проверяем, попали ли в объект с тэгом "Item"
            if (collider != null && collider.CompareTag("Item"))
            {
                //если объект не падает
                if (!collider.gameObject.GetComponent<ItemController>().IsItemFalls)
                {
                    //выбираем объект
                    selectedObject = collider.gameObject;
                    currItemController = selectedObject.GetComponent<ItemController>();

                    //Отмечаем, собранный предмет
                    currItemController.ItemGrabbed();

                    //меняем его размер и слой
                    currItemController.SetItemLayer(itemInHandLayer);
                    currItemController.StartScaleAnimation(itemInHandScale);

                    // Вычисляем смещение между пальцем и объектом
                    offset = mainCamera.ScreenToWorldPoint(Input.GetTouch(0).position) - selectedObject.transform.position;

                    //отмечаем собранный объект
                    isItemInHand = true;
                }
            }
        }
    }

    // Обработка завершения касания
    private void HandleTouchEnd()
    {
        if (isItemInHand && selectedObject != null)
        {
            // Вызываем пользовательскую логику при отпускании объекта
            OnDrop(currItemController);

            // Сбрасываем выбранный объект
            selectedObject = null;
            currItemController = null;

            //отмечаем
            isItemInHand = false;
        }
    }

    // Логика, которая выполняется при отпускании объекта
    private void OnDrop(ItemController itemController)
    {
        //отпускаем предмет
        itemController.ItemReleased();
    }

    //возвращаем есть ли предмет в руке
    public bool IsItemInHand { get => isItemInHand; }
}
