using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class Vector3Data
{
    public float x, y, z;
    public Vector3 ToVector3() => new Vector3(x, y, z);
}

[System.Serializable]
public class SizeData
{
    public float width, height, depth;
}

[System.Serializable]
public class RoomData
{
    public string id;
    public Vector3Data position;
    public SizeData size;
    public string type;
    public string[] connections;
}

[System.Serializable]
public class SectionData
{
    public string sectionId;
    public RoomData[] rooms;
    public Vector3Data triggerPosition;
    public string nextSectionFile;
}

public class MapManager : MonoBehaviour
{
    public string initialSectionFile = "Section1.json";
    private SectionData currentSection;
    private GameObject currentSectionObject;
    public Transform player;

    void Start()
    {
        LoadSection(initialSectionFile);
    }

    void Update()
    {
        if (currentSection != null)
        {
            Vector3 triggerPos = currentSection.triggerPosition.ToVector3();
            float distance = Vector2.Distance(new Vector2(player.position.x, player.position.z), new Vector2(triggerPos.x, triggerPos.z));
            if (distance < 1f)
            {
                LoadNextSection();
            }
        }
    }

    void LoadSection(string fileName)
    {
        string path = Path.Combine(Application.dataPath, "MapData", fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            currentSection = JsonUtility.FromJson<SectionData>(json);
            GenerateSection();
        }
    }

    void GenerateSection()
    {
        if (currentSectionObject != null) Destroy(currentSectionObject);
        currentSectionObject = new GameObject("CurrentSection");

        foreach (var room in currentSection.rooms)
        {
            GameObject roomObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roomObj.transform.position = room.position.ToVector3();
            roomObj.transform.localScale = new Vector3(room.size.width, room.size.height, room.size.depth);
            roomObj.transform.parent = currentSectionObject.transform;
            roomObj.name = room.id;
        }
    }

    void LoadNextSection()
    {
        if (!string.IsNullOrEmpty(currentSection.nextSectionFile))
        {
            LoadSection(currentSection.nextSectionFile);
        }
    }
}