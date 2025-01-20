using UnityEngine;

public class SurfaceStats : MonoBehaviour
{

    [Header("Surface To Item Stats")]
    [Tooltip("Размер предметов на данной поверхности")]
    [SerializeField] private float surfaceItemSize = 1;
    [Tooltip("Слой предмета на данной поверхности")]
    [SerializeField] private int surfaceItemlLayer = 0;

    void Start()
    {
        
    }

    public float SurfaceItemSize { get => surfaceItemSize;}
    public int SurfaceItemlLayer { get => surfaceItemlLayer;}
}
