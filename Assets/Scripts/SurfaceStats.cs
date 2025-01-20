using UnityEngine;

public class SurfaceStats : MonoBehaviour
{

    [Header("Surface To Item Stats")]
    [Tooltip("������ ��������� �� ������ �����������")]
    [SerializeField] private float surfaceItemSize = 1;
    [Tooltip("���� �������� �� ������ �����������")]
    [SerializeField] private int surfaceItemlLayer = 0;

    void Start()
    {
        
    }

    public float SurfaceItemSize { get => surfaceItemSize;}
    public int SurfaceItemlLayer { get => surfaceItemlLayer;}
}
