using UnityEngine;

public class CameraMoveController : MonoBehaviour
{

    [Header("Sensetivity Settings")]
    [Tooltip("Скорость скролла")]
    [SerializeField] private float scrollSpeed = 1f;

    [Header("Item Drag Settings")]
    [Tooltip("Насколько близко к границе должно быть касание для начала движения"), Range(0f, 0.5f)]
    [SerializeField] private float edgeThreshold = 0.2f; // Доля экрана (от 0 до  0.5) насколько процентов близко к границе экрана
    [Tooltip("Скорость скролла с предметов в руре")]
    [SerializeField] private float itemScrollSpeed = 1f;

    [Header("Borders")]
    [SerializeField] private float sceneLeftBorder;
    [SerializeField] private float sceneRightBorder;

    [Header("Components")]
    [Tooltip("Камера для движения")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private DragController dragController;
    [Tooltip("Половина ширины камеры в мировых координатах")]
    private float cameraHalfWidth;
    private Vector3 dragOrigin;

    private void Start()
    {
        // Рассчитываем половину ширины камеры на старте
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
    }

    private void Update()
    {
        HandleTouchInput();
    }

    //считываем нажатие пальцев
    private void HandleTouchInput()
    {
        //если предмета нет в руке
        if (!dragController.IsItemInHand)
        {
            //проверяем, есть ли касания
            if (Input.touchCount > 0)
            {
                // Получаем первое касание
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        //начало касания
                        dragOrigin = mainCamera.ScreenToWorldPoint(touch.position);
                        dragOrigin.z = 0;
                        break;

                    case TouchPhase.Moved:
                        //перемещение пальца
                        MovedTouch(touch);
                        break;
                }
            }
        }
        //если предмет в руке
        else
        {
            MovedWithItem();
        }
    }

    //перемещаем зажатый палец и двигаем камеру
    private void MovedTouch(Touch touch)
    {
        Vector3 currentTouchPosition = mainCamera.ScreenToWorldPoint(touch.position);
        currentTouchPosition.z = 0;
        Vector3 difference = dragOrigin - currentTouchPosition;

        //Перемещение камеры только по оси X
        Vector3 newPosition = mainCamera.transform.position + difference;
        //не изменяем по Y
        newPosition.y = mainCamera.transform.position.y;

        // Ограничиваем положение камеры так, чтобы её края не выходили за границы сцены
        newPosition.x = Mathf.Clamp(newPosition.x, sceneLeftBorder + cameraHalfWidth, sceneRightBorder - cameraHalfWidth);

        mainCamera.transform.position = newPosition;
    }

    //двигаем с предеметов в руке
    private void MovedWithItem()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Получаем экранные координаты касания
            float touchX = touch.position.x;

            // Получаем границы камеры в мировых координатах
            float leftScreenEdge = mainCamera.ViewportToScreenPoint(new Vector3(0, 0, 0)).x;
            float rightScreenEdge = mainCamera.ViewportToScreenPoint(new Vector3(1, 0, 0)).x;

            // Проверяем, находится ли палец близко к левому краю
            if (touchX <= leftScreenEdge + Screen.width * edgeThreshold)
            {
                MoveCameraHorizontally(-1); // Движение камеры влево
            }
            // Проверяем, находится ли палец близко к правому краю
            else if (touchX >= rightScreenEdge - Screen.width * edgeThreshold)
            {
                MoveCameraHorizontally(1); // Движение камеры вправо
            }
        }
    }

    //двигаем камеру по горизонтали в выбраном направлении
    private void MoveCameraHorizontally(int direction)
    {
        // Рассчитываем новую позицию камеры
        float moveStep = itemScrollSpeed * Time.deltaTime * direction;
        Vector3 newPosition = mainCamera.transform.position + new Vector3(moveStep, 0, 0);

        // Ограничиваем положение камеры, чтобы её края не выходили за границы сцены
        newPosition.x = Mathf.Clamp(newPosition.x, sceneLeftBorder + cameraHalfWidth, sceneRightBorder - cameraHalfWidth);

        mainCamera.transform.position = newPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawLine(new Vector3(sceneLeftBorder, -10, 0), new Vector3(sceneLeftBorder, 10, 0));
        Gizmos.DrawLine(new Vector3(sceneRightBorder, -10, 0), new Vector3(sceneRightBorder, 10, 0));
    }
}
