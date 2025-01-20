using UnityEngine;

[CreateAssetMenu(fileName = "MainGameStats")]
public class MainGameStats : ScriptableObject
{

    [Header("Gravity Settings")]
    [Tooltip("Сила гравитация действующая на все предметы, чем она сильнее, тем быстрее падают предметы")]
    public float gravityScale = 10f;

}