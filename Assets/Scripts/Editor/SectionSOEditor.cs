using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Assets.Scripts;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(SectionSO))]
public class SectionSOEditor : Editor
{
    private SectionSO sectionSO;

    void OnEnable()
    {
        sectionSO = (SectionSO)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Load in Scene for Editing"))
        {
            LoadSectionInScene();
        }

        if (GUILayout.Button("Save Scene Changes to SO"))
        {
            SaveSceneToSO();
        }
    }

    void OnSceneGUI()
    {
        // Draw gizmos for entities and triggers
        if (sectionSO.entities != null)
        {
            foreach (var entity in sectionSO.entities)
            {
                Handles.DrawWireCube(entity.position, entity.size);
                // Add position handles for editing
                entity.position = Handles.PositionHandle(entity.position, Quaternion.identity);
            }
        }
    }

    void LoadSectionInScene()
    {
        // Find or create Environment GameObject
        GameObject environment = GameObject.Find("Environment");
        if (environment == null)
        {
            environment = new GameObject("Environment");
        }

        // Clear existing children under Environment
        foreach (Transform child in environment.transform)
        {
            DestroyImmediate(child.gameObject);
        }

        // Instantiate entities from SO
        foreach (var entity in sectionSO.entities)
        {
            GameObject obj;
            if (entity.mesh != null)
            {
                obj = new GameObject();
                MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = entity.mesh;
                MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
                if (entity.materials != null && entity.materials.Length > 0)
                {
                    if (entity.mesh.subMeshCount > 1 && entity.materials.Length == entity.mesh.subMeshCount)
                    {
                        meshRenderer.materials = entity.materials;
                    }
                    else
                    {
                        meshRenderer.material = entity.materials[0];
                    }
                }
                // Add collider
                BoxCollider collider = obj.AddComponent<BoxCollider>();
                collider.size = entity.size;
            }
            else
            {
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
            obj.transform.position = entity.position;
            obj.transform.localScale = entity.size;
            obj.name = entity.id;
            obj.transform.parent = environment.transform;
        }

        // Instantiate triggerBoxes from SO
        if (sectionSO.triggerBoxes != null)
        {
            foreach (var trigger in sectionSO.triggerBoxes)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.position = trigger.position;
                obj.transform.localScale = trigger.size;
                obj.name = "TriggerBox";
                obj.GetComponent<Collider>().isTrigger = true;
                obj.transform.parent = environment.transform;
            }
        }
    }

    void SaveSceneToSO()
    {
        GameObject environment = GameObject.Find("Environment");
        if (environment == null)
        {
            Debug.LogError("Environment GameObject not found!");
            return;
        }

        string entitiesFolder = "Assets/Resources/Entities/";
        Directory.CreateDirectory(entitiesFolder);

        List<EntitySO> entities = new List<EntitySO>();
        List<TriggerBoxDataSO> triggers = new List<TriggerBoxDataSO>();

        foreach (Transform child in environment.transform)
        {
            if (child.GetComponent<Collider>()?.isTrigger == true)
            {
                // It's a trigger
                TriggerBoxDataSO triggerSO = CreateInstance<TriggerBoxDataSO>();
                triggerSO.position = child.position;
                triggerSO.size = child.localScale;
                // Populate other fields as needed (valid, newOrigin, newRotation)
                triggers.Add(triggerSO);
                AssetDatabase.AddObjectToAsset(triggerSO, sectionSO);
            }
            else
            {
                // It's an entity
                string entityPath = Path.Combine(entitiesFolder, $"{child.name}.asset");
                EntitySO entitySO = AssetDatabase.LoadAssetAtPath<EntitySO>(entityPath);
                if (entitySO == null)
                {
                    entitySO = CreateInstance<EntitySO>();
                    AssetDatabase.CreateAsset(entitySO, entityPath);
                }

                entitySO.id = child.name;
                entitySO.position = child.position;
                entitySO.size = child.localScale;

                // Get mesh from MeshFilter
                MeshFilter meshFilter = child.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    // Check if it's a primitive mesh
                    string assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
                    if (string.IsNullOrEmpty(assetPath) || assetPath.Contains("unity default resources"))
                    {
                        // It's a primitive - create a temporary reference to identify type
                        if (meshFilter.sharedMesh.name.Contains("Cube"))
                        {
                            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            entitySO.mesh = temp.GetComponent<MeshFilter>().sharedMesh;
                            DestroyImmediate(temp);
                        }
                        else if (meshFilter.sharedMesh.name.Contains("Sphere"))
                        {
                            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            entitySO.mesh = temp.GetComponent<MeshFilter>().sharedMesh;
                            DestroyImmediate(temp);
                        }
                        else if (meshFilter.sharedMesh.name.Contains("Cylinder"))
                        {
                            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            entitySO.mesh = temp.GetComponent<MeshFilter>().sharedMesh;
                            DestroyImmediate(temp);
                        }
                        else if (meshFilter.sharedMesh.name.Contains("Capsule"))
                        {
                            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                            entitySO.mesh = temp.GetComponent<MeshFilter>().sharedMesh;
                            DestroyImmediate(temp);
                        }
                        else if (meshFilter.sharedMesh.name.Contains("Plane"))
                        {
                            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Plane);
                            entitySO.mesh = temp.GetComponent<MeshFilter>().sharedMesh;
                            DestroyImmediate(temp);
                        }
                        else if (meshFilter.sharedMesh.name.Contains("Quad"))
                        {
                            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                            entitySO.mesh = temp.GetComponent<MeshFilter>().sharedMesh;
                            DestroyImmediate(temp);
                        }
                        else
                        {
                            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            entitySO.mesh = temp.GetComponent<MeshFilter>().sharedMesh;
                            DestroyImmediate(temp);
                        }
                    }
                    else
                    {
                        // Custom mesh
                        entitySO.mesh = meshFilter.sharedMesh;
                    }
                }

                // Get materials from MeshRenderer
                MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    entitySO.materials = renderer.materials;
                }

                // Save the entitySO
                EditorUtility.SetDirty(entitySO);
                AssetDatabase.SaveAssets();

                // Populate other fields as needed
                entities.Add(entitySO);
            }
        }

        sectionSO.entities = entities.ToArray();
        sectionSO.triggerBoxes = triggers.ToArray();
        EditorUtility.SetDirty(sectionSO);
        AssetDatabase.SaveAssets();
    }
}
