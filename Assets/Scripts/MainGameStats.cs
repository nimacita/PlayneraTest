using UnityEngine;

[CreateAssetMenu(fileName = "MainGameStats")]
public class MainGameStats : ScriptableObject
{

    [Header("Gravity Settings")]
    [Tooltip("���� ���������� ����������� �� ��� ��������, ��� ��� �������, ��� ������� ������ ��������")]
    public float gravityScale = 10f;

}