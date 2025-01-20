using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    private static string saveFilePath => Path.Combine(Application.persistentDataPath, "itemPositions.json");
    private static Dictionary<string, Vector3> itemPositions = new Dictionary<string, Vector3>();

    //��������� ������� ����������� ��������
    public static void SavePosition(string id, Vector3 position)
    {
        //��������� ��� ��������� ������� ����������� ��������
        itemPositions[id] = position;

        //���������
        File.WriteAllText(saveFilePath, JsonUtility.ToJson(new SaveDataWrapper(itemPositions)));
    }

    //�������� ������� ���� ������
    public static Vector3? LoadPosition(string id)
    {
        // ���� ������ ��� �� ���������, �������� �� �� �����
        if (itemPositions.Count == 0 && File.Exists(saveFilePath))
        {
            var json = File.ReadAllText(saveFilePath);
            itemPositions = JsonUtility.FromJson<SaveDataWrapper>(json).ToDictionary();
        }

        //���������� �������, ���� ����, ���� ��� ���������� ������ ������
        return itemPositions.ContainsKey(id) ? itemPositions[id] : (Vector3?)null;
    }

    //����������� ������ �� ������ � dictionary
    [System.Serializable]
    private class SaveDataWrapper
    {
        public List<string> keys = new List<string>();
        public List<Vector3> values = new List<Vector3>();

        //�����������
        public SaveDataWrapper(Dictionary<string, Vector3> data)
        {
            foreach (var kvp in data)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        //����������� � Dictionary
        public Dictionary<string, Vector3> ToDictionary()
        {
            var result = new Dictionary<string, Vector3>();
            for (int i = 0; i < keys.Count; i++)
            {
                result[keys[i]] = values[i];
            }
            return result;
        }
    }
}
