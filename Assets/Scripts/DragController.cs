using UnityEngine;

public class DragController : MonoBehaviour
{

    [Header("Position Settings")]
    [Tooltip("�������� ����� ������� � ������� �������� �������")]
    private Vector3 offset;

    [Header("Item Settings")]
    [Tooltip("������ �������� � ����")]
    [SerializeField] private float itemInHandScale = 1.15f;
    [Tooltip("����� ���� ��� �������� � ����")]
    [SerializeField] private int itemInHandLayer = 50;

    [Header("Mask")]
    [SerializeField] private LayerMask itemMask;

    [Header("Components")]
    [SerializeField] private Camera mainCamera;
    private GameObject selectedObject;  // ������� ��������� ������
    private ItemController currItemController; // ������ �������� ���������� �������

    [Header("Bools")]
    private bool isItemInHand;

    void Start()
    {
        StartSettings();
    }

    //��������� ���������
    private void StartSettings()
    {
        isItemInHand = false;
    }

    void Update()
    {
        TouchsHandler();
    }

    //������������ ������� ���� ����
    private void TouchsHandler()
    {
        //��������� ������� �������
        if (Input.touchCount > 0) 
        {
            //�������� �������
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = mainCamera.ScreenToWorldPoint(touch.position);
            touchPosition.z = 0f;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    //������ �������
                    HandleTouchBegin(touchPosition);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    //����������� ������
                    if (selectedObject != null && isItemInHand)
                    {
                        //������� �������
                        MoveItem(touchPosition);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    //���������� �������
                    if (selectedObject != null)
                    {
                        HandleTouchEnd();
                    }
                    break;
            }
        }
    }

    //������� �������
    private void MoveItem(Vector3 touchPosition)
    {
        selectedObject.transform.position = new Vector3(touchPosition.x - offset.x, 
            touchPosition.y - offset.y, selectedObject.transform.position.z);
    }

    // ��������� ������ ������� � ������� ��������
    private void HandleTouchBegin(Vector3 touchPosition)
    {
        //���� ������� �� � ����
        if (!isItemInHand)
        {
            // ��������� �� ����������
            Collider2D collider = Physics2D.OverlapPoint(touchPosition, itemMask);

            // ���������, ������ �� � ������ � ����� "Item"
            if (collider != null && collider.CompareTag("Item"))
            {
                //���� ������ �� ������
                if (!collider.gameObject.GetComponent<ItemController>().IsItemFalls)
                {
                    //�������� ������
                    selectedObject = collider.gameObject;
                    currItemController = selectedObject.GetComponent<ItemController>();

                    //��������, ��������� �������
                    currItemController.ItemGrabbed();

                    //������ ��� ������ � ����
                    currItemController.SetItemLayer(itemInHandLayer);
                    currItemController.StartScaleAnimation(itemInHandScale);

                    // ��������� �������� ����� ������� � ��������
                    offset = mainCamera.ScreenToWorldPoint(Input.GetTouch(0).position) - selectedObject.transform.position;

                    //�������� ��������� ������
                    isItemInHand = true;
                }
            }
        }
    }

    // ��������� ���������� �������
    private void HandleTouchEnd()
    {
        if (isItemInHand && selectedObject != null)
        {
            // �������� ���������������� ������ ��� ���������� �������
            OnDrop(currItemController);

            // ���������� ��������� ������
            selectedObject = null;
            currItemController = null;

            //��������
            isItemInHand = false;
        }
    }

    // ������, ������� ����������� ��� ���������� �������
    private void OnDrop(ItemController itemController)
    {
        //��������� �������
        itemController.ItemReleased();
    }

    //���������� ���� �� ������� � ����
    public bool IsItemInHand { get => isItemInHand; }
}
