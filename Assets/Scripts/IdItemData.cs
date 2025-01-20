using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class IdItemData : MonoBehaviour
{
    private const string idFileName = "itemsIds.json";
    private static string IdFilePath => Path.Combine(Application.persistentDataPath, idFileName);

    private static Dictionary<string, string> idDictionary;

    private void Awake()
    {
        //��������� ��� ���� �� �����
        idDictionary = LoadIds();
    }

    //�������� ��� ������� ����
    public static string GetOrCreateId(string objectName)
    {
        // ��������, ��� ������� ���������������
        if (idDictionary == null)
        {
            idDictionary = LoadIds();
        }

        // ���������, ���� �� ��� ID ��� �������
        if (idDictionary.TryGetValue(objectName, out var existingId))
        {
            return existingId; // ���� ID ������, ���������� ���
        }

        // ���� ID �� ������, ������ �����
        var newId = Guid.NewGuid().ToString();
        idDictionary[objectName] = newId;
        SaveIds();
        return newId;
    }

    //��������� ����
    private static Dictionary<string, string> LoadIds()
    {
        if (File.Exists(IdFilePath))
        {
            var json = File.ReadAllText(IdFilePath);
            return JsonUtility.FromJson<IdWrapper>(json).ToDictionary();
        }

        // ���������� ������ �������, ���� ���� �� ����������
        return new Dictionary<string, string>();
    }

    //��������� ����
    private static void SaveIds()
    {
        if (idDictionary == null)
        {
            Debug.LogError("������� ������ �� ������� ��������� ID");
            return;
        }

        var json = JsonUtility.ToJson(new IdWrapper(idDictionary));
        File.WriteAllText(IdFilePath, json);
    }

    [Serializable] //����������� ���� � �������
    private class IdWrapper
    {
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();

        public IdWrapper(Dictionary<string, string> data)
        {
            foreach (var kvp in data)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public Dictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>();
            for (int i = 0; i < keys.Count; i++)
            {
                result[keys[i]] = values[i];
            }
            return result;
        }
    }
}
