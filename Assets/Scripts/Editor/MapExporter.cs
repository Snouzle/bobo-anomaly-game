using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts;

public class MapExporter : EditorWindow
{
    private string sectionId = "MySection";
    private string outputPath = "Assets/MapData/MySection.json";

    [MenuItem("Tools/Export Map to JSON")]
    static void ShowWindow()
    {
        GetWindow<MapExporter>("Map Exporter");
    }

    void OnGUI()
    {
        sectionId = EditorGUILayout.TextField("Section ID", sectionId);
        outputPath = EditorGUILayout.TextField("Output JSON Path", outputPath);

        if (GUILayout.Button("Export Environment Objects"))
        {
            ExportEnvironmentToJSON();
        }
    }

    void ExportEnvironmentToJSON()
    {
        GameObject environment = GameObject.Find("Environment");
        if (environment == null)
        {
            Debug.LogError("Environment GameObject not found! Create an 'Environment' GameObject with your map objects as children.");
            return;
        }

        SectionData sectionData = new SectionData { sectionId = sectionId };
        List<EntityData> entities = new List<EntityData>();
        List<TriggerBoxData> triggers = new List<TriggerBoxData>();

        foreach (Transform child in environment.transform)
        {
            GameObject obj = child.gameObject;
            if (obj.GetComponent<Collider>() != null && obj.GetComponent<Collider>().isTrigger)
            {
                // It's a trigger box
                var triggerScript = obj.GetComponent<TriggerScript>();
                if (triggerScript != null)
                {
                    triggers.Add(new TriggerBoxData
                    {
                        position = new Vector3Data { x = obj.transform.position.x, y = obj.transform.position.y, z = obj.transform.position.z },
                        size = new SizeData { width = obj.transform.localScale.x, height = obj.transform.localScale.y, depth = obj.transform.localScale.z },
                        valid = triggerScript.targetSection.valid,
                        newOrigin = new Vector3Data { x = triggerScript.targetSection.origin.x, y = triggerScript.targetSection.origin.y, z = triggerScript.targetSection.origin.z },
                        newRotation = new Vector3Data { x = triggerScript.targetSection.rotation.eulerAngles.x, y = triggerScript.targetSection.rotation.eulerAngles.y, z = triggerScript.targetSection.rotation.eulerAngles.z }
                    });
                }
                else
                {
                    // Fallback: assume it's a trigger without script
                    triggers.Add(new TriggerBoxData
                    {
                        position = new Vector3Data { x = obj.transform.position.x, y = obj.transform.position.y, z = obj.transform.position.z },
                        size = new SizeData { width = obj.transform.localScale.x, height = obj.transform.localScale.y, depth = obj.transform.localScale.z },
                        valid = true, // Default
                        newOrigin = new Vector3Data { x = 0, y = 0, z = 0 }, // Default
                        newRotation = new Vector3Data { x = 0, y = 0, z = 0 } // Default
                    });
                }
            }
            else
            {
                // Entity
                string meshPath = "";
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    // Check if it's a primitive mesh
                    string assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
                    if (string.IsNullOrEmpty(assetPath) || assetPath.Contains("unity default resources"))
                    {
                        // It's a primitive - identify by name
                        if (meshFilter.sharedMesh.name.Contains("Cube"))
                            meshPath = "Primitive:Cube";
                        else if (meshFilter.sharedMesh.name.Contains("Sphere"))
                            meshPath = "Primitive:Sphere";
                        else if (meshFilter.sharedMesh.name.Contains("Cylinder"))
                            meshPath = "Primitive:Cylinder";
                        else if (meshFilter.sharedMesh.name.Contains("Capsule"))
                            meshPath = "Primitive:Capsule";
                        else if (meshFilter.sharedMesh.name.Contains("Plane"))
                            meshPath = "Primitive:Plane";
                        else if (meshFilter.sharedMesh.name.Contains("Quad"))
                            meshPath = "Primitive:Quad";
                        else
                            meshPath = "Primitive:Cube"; // Default fallback
                    }
                    else
                    {
                        // It's a custom mesh asset
                        meshPath = assetPath;
                    }
                }

                entities.Add(new EntityData
                {
                    id = obj.name,
                    position = new Vector3Data { x = obj.transform.position.x, y = obj.transform.position.y, z = obj.transform.position.z },
                    size = new SizeData { width = obj.transform.localScale.x, height = obj.transform.localScale.y, depth = obj.transform.localScale.z },
                    type = "corridor", // Customize based on your logic
                    connections = new string[] { }, // Add logic to determine connections
                    texture = "", // Path to texture asset
                    mesh = meshPath,
                    animations = new AnimationData[] { }, // Add animation data if applicable
                    events = new EventData[] { } // Add event data if applicable
                });
            }
        }

        sectionData.entities = entities.ToArray();
        sectionData.triggerBoxes = triggers.ToArray();

        string json = JsonUtility.ToJson(sectionData, true);
        File.WriteAllText(outputPath, json);
        AssetDatabase.Refresh();
        Debug.Log("Map exported to " + outputPath);
    }
}