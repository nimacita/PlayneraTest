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
        //загружаем все айди из файла
        idDictionary = LoadIds();
    }

    //получаем или создаем айди
    public static string GetOrCreateId(string objectName)
    {
        // Убедимся, что словарь инициализирован
        if (idDictionary == null)
        {
            idDictionary = LoadIds();
        }

        // Проверяем, есть ли уже ID для объекта
        if (idDictionary.TryGetValue(objectName, out var existingId))
        {
            return existingId; // Если ID найден, возвращаем его
        }

        // Если ID не найден, создаём новый
        var newId = Guid.NewGuid().ToString();
        idDictionary[objectName] = newId;
        SaveIds();
        return newId;
    }

    //Загружаем айди
    private static Dictionary<string, string> LoadIds()
    {
        if (File.Exists(IdFilePath))
        {
            var json = File.ReadAllText(IdFilePath);
            return JsonUtility.FromJson<IdWrapper>(json).ToDictionary();
        }

        // Возвращаем пустой словарь, если файл не существует
        return new Dictionary<string, string>();
    }

    //сохраняем айди
    private static void SaveIds()
    {
        if (idDictionary == null)
        {
            Debug.LogError("Словарь пустой не удается сохранить ID");
            return;
        }

        var json = JsonUtility.ToJson(new IdWrapper(idDictionary));
        File.WriteAllText(IdFilePath, json);
    }

    [Serializable] //преобразуем айди в словарь
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
