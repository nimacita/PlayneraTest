using UnityEngine;

public class CameraMoveController : MonoBehaviour
{

    [Header("Sensetivity Settings")]
    [Tooltip("�������� �������")]
    [SerializeField] private float scrollSpeed = 1f;

    [Header("Item Drag Settings")]
    [Tooltip("��������� ������ � ������� ������ ���� ������� ��� ������ ��������"), Range(0f, 0.5f)]
    [SerializeField] private float edgeThreshold = 0.2f; // ���� ������ (�� 0 ��  0.5) ��������� ��������� ������ � ������� ������
    [Tooltip("�������� ������� � ��������� � ����")]
    [SerializeField] private float itemScrollSpeed = 1f;

    [Header("Borders")]
    [SerializeField] private float sceneLeftBorder;
    [SerializeField] private float sceneRightBorder;

    [Header("Components")]
    [Tooltip("������ ��� ��������")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private DragController dragController;
    [Tooltip("�������� ������ ������ � ������� �����������")]
    private float cameraHalfWidth;
    private Vector3 dragOrigin;

    private void Start()
    {
        // ������������ �������� ������ ������ �� ������
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
    }

    private void Update()
    {
        HandleTouchInput();
    }

    //��������� ������� �������
    private void HandleTouchInput()
    {
        //���� �������� ��� � ����
        if (!dragController.IsItemInHand)
        {
            //���������, ���� �� �������
            if (Input.touchCount > 0)
            {
                // �������� ������ �������
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        //������ �������
                        dragOrigin = mainCamera.ScreenToWorldPoint(touch.position);
                        dragOrigin.z = 0;
                        break;

                    case TouchPhase.Moved:
                        //����������� ������
                        MovedTouch(touch);
                        break;
                }
            }
        }
        //���� ������� � ����
        else
        {
            MovedWithItem();
        }
    }

    //���������� ������� ����� � ������� ������
    private void MovedTouch(Touch touch)
    {
        Vector3 currentTouchPosition = mainCamera.ScreenToWorldPoint(touch.position);
        currentTouchPosition.z = 0;
        Vector3 difference = dragOrigin - currentTouchPosition;

        //����������� ������ ������ �� ��� X
        Vector3 newPosition = mainCamera.transform.position + difference;
        //�� �������� �� Y
        newPosition.y = mainCamera.transform.position.y;

        // ������������ ��������� ������ ���, ����� � ���� �� �������� �� ������� �����
        newPosition.x = Mathf.Clamp(newPosition.x, sceneLeftBorder + cameraHalfWidth, sceneRightBorder - cameraHalfWidth);

        mainCamera.transform.position = newPosition;
    }

    //������� � ���������� � ����
    private void MovedWithItem()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // �������� �������� ���������� �������
            float touchX = touch.position.x;

            // �������� ������� ������ � ������� �����������
            float leftScreenEdge = mainCamera.ViewportToScreenPoint(new Vector3(0, 0, 0)).x;
            float rightScreenEdge = mainCamera.ViewportToScreenPoint(new Vector3(1, 0, 0)).x;

            // ���������, ��������� �� ����� ������ � ������ ����
            if (touchX <= leftScreenEdge + Screen.width * edgeThreshold)
            {
                MoveCameraHorizontally(-1); // �������� ������ �����
            }
            // ���������, ��������� �� ����� ������ � ������� ����
            else if (touchX >= rightScreenEdge - Screen.width * edgeThreshold)
            {
                MoveCameraHorizontally(1); // �������� ������ ������
            }
        }
    }

    //������� ������ �� ����������� � �������� �����������
    private void MoveCameraHorizontally(int direction)
    {
        // ������������ ����� ������� ������
        float moveStep = itemScrollSpeed * Time.deltaTime * direction;
        Vector3 newPosition = mainCamera.transform.position + new Vector3(moveStep, 0, 0);

        // ������������ ��������� ������, ����� � ���� �� �������� �� ������� �����
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
