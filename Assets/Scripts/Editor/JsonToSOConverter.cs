using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts;

public class JsonToSOConverter : EditorWindow
{
    private string jsonPath = "Assets/MapData/anomaly/Section_1.json"; // Example path
    private string outputFolder = "Assets/Resources/Sections/"; // Folder to save SOs (for Resources.Load)
    private string entitiesFolder = "Assets/Resources/Entities/"; // Folder to save EntitySOs

    [MenuItem("Tools/Convert JSON to SOs")]
    static void ShowWindow()
    {
        GetWindow<JsonToSOConverter>("JSON to SO Converter");
    }

    void OnGUI()
    {
        jsonPath = EditorGUILayout.TextField("JSON File Path", jsonPath);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        if (GUILayout.Button("Convert"))
        {
            ConvertJsonToSOs();
        }
    }

    void ConvertJsonToSOs()
    {
        if (!File.Exists(jsonPath))
        {
            Debug.LogError("JSON file not found!");
            return;
        }

        string json = File.ReadAllText(jsonPath);
        SectionData sectionData = JsonUtility.FromJson<SectionData>(json);

        // Define asset path
        string assetPath = Path.Combine(outputFolder, $"{sectionData.sectionId}.asset");

        // Create SectionSO
        SectionSO sectionSO = CreateInstance<SectionSO>();
        sectionSO.sectionId = sectionData.sectionId;

        // Save the main SO first
        AssetDatabase.CreateAsset(sectionSO, assetPath);

        // Ensure entities folder exists
        Directory.CreateDirectory(entitiesFolder);

        // Convert entities
        List<EntitySO> entitySOs = new List<EntitySO>();
        foreach (var entity in sectionData.entities)
        {
            string entityPath = Path.Combine(entitiesFolder, $"{entity.id}.asset");
            EntitySO entitySO = AssetDatabase.LoadAssetAtPath<EntitySO>(entityPath);

            bool isNew = entitySO == null;
            if (isNew)
            {
                entitySO = CreateInstance<EntitySO>();
                AssetDatabase.CreateAsset(entitySO, entityPath);
            }

            // Populate/update data
            entitySO.id = entity.id;
            entitySO.position = entity.position.ToVector3();
            entitySO.size = new Vector3(entity.size.width, entity.size.height, entity.size.depth);
            entitySO.type = entity.type;
            entitySO.connections = entity.connections;
            Debug.Log($"Populated EntitySO {entitySO.id} with position {entitySO.position}, size {entitySO.size}");

            // Load textures and create materials
            if (entity.textures != null && entity.textures.Length > 0)
            {
                Material[] mats = new Material[entity.textures.Length];
                for (int i = 0; i < entity.textures.Length; i++)
                {
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(entity.textures[i]);
                    Material mat = new Material(GetDefaultShader());
                    mat.name = entity.id + "_Material_" + i;
                    if (tex != null)
                    {
                        mat.mainTexture = tex;
                    }
                    mats[i] = mat;
                    AssetDatabase.AddObjectToAsset(mat, entitySO);
                }
                entitySO.materials = mats;
            }
            else
            {
                // No textures, create one default material
                Material mat = new Material(GetDefaultShader());
                mat.name = entity.id + "_Material";
                entitySO.materials = new Material[] { mat };
                AssetDatabase.AddObjectToAsset(mat, entitySO);
            }

            // Load mesh if path exists
            if (!string.IsNullOrEmpty(entity.mesh))
            {
                if (entity.mesh.StartsWith("Primitive:"))
                {
                    // Handle primitive meshes
                    string primitiveType = entity.mesh.Replace("Primitive:", "");
                    PrimitiveType type = PrimitiveType.Cube; // Default

                    switch (primitiveType)
                    {
                        case "Cube":
                            type = PrimitiveType.Cube;
                            break;
                        case "Sphere":
                            type = PrimitiveType.Sphere;
                            break;
                        case "Cylinder":
                            type = PrimitiveType.Cylinder;
                            break;
                        case "Capsule":
                            type = PrimitiveType.Capsule;
                            break;
                        case "Plane":
                            type = PrimitiveType.Plane;
                            break;
                        case "Quad":
                            type = PrimitiveType.Quad;
                            break;
                    }

                    // Create temporary primitive to get its mesh
                    GameObject tempObj = GameObject.CreatePrimitive(type);
                    entitySO.mesh = tempObj.GetComponent<MeshFilter>().sharedMesh;
                    DestroyImmediate(tempObj);
                }
                else
                {
                    // Load custom mesh asset
                    entitySO.mesh = AssetDatabase.LoadAssetAtPath<Mesh>(entity.mesh);
                }
            }

            // Convert animations
            List<AnimationDataSO> animSOs = new List<AnimationDataSO>();
            foreach (var anim in entity.animations)
            {
                AnimationDataSO animSO = CreateInstance<AnimationDataSO>();
                animSO.name = anim.name;
                animSO.clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(anim.clip);
                animSO.loop = anim.loop;
                animSO.speed = anim.speed;
                AssetDatabase.AddObjectToAsset(animSO, entitySO);
                animSOs.Add(animSO);
            }
            entitySO.animations = animSOs.ToArray();

            // Convert events
            List<EventDataSO> eventSOs = new List<EventDataSO>();
            foreach (var evt in entity.events)
            {
                EventDataSO eventSO = CreateInstance<EventDataSO>();
                eventSO.trigger = evt.trigger;

                if (evt.condition != null)
                {
                    ConditionDataSO condSO = CreateInstance<ConditionDataSO>();
                    condSO.distance = evt.condition.distance;
                    AssetDatabase.AddObjectToAsset(condSO, entitySO);
                    eventSO.condition = condSO;
                }

                List<ActionDataSO> actionSOs = new List<ActionDataSO>();
                foreach (var action in evt.actions)
                {
                    ActionDataSO actionSO = CreateInstance<ActionDataSO>();
                    actionSO.type = action.type;
                    actionSO.animationName = action.animationName;
                    actionSO.sound = AssetDatabase.LoadAssetAtPath<AudioClip>(action.soundPath);
                    actionSO.volume = action.volume;
                    actionSO.text = action.text;
                    actionSO.duration = action.duration;
                    AssetDatabase.AddObjectToAsset(actionSO, entitySO);
                    actionSOs.Add(actionSO);
                }
                eventSO.actions = actionSOs.ToArray();
                AssetDatabase.AddObjectToAsset(eventSO, entitySO);
                eventSOs.Add(eventSO);
            }
            entitySO.events = eventSOs.ToArray();

            // Mark dirty and save the entitySO
            EditorUtility.SetDirty(entitySO);
            AssetDatabase.SaveAssets();
            Debug.Log($"Saved EntitySO {entitySO.id} with position {entitySO.position}, size {entitySO.size}");

            entitySOs.Add(entitySO);
        }
        sectionSO.entities = entitySOs.ToArray();

        // Convert triggerBoxes
        List<TriggerBoxDataSO> triggerSOs = new List<TriggerBoxDataSO>();
        foreach (var trigger in sectionData.triggerBoxes)
        {
            TriggerBoxDataSO triggerSO = CreateInstance<TriggerBoxDataSO>();
            triggerSO.position = trigger.position.ToVector3();
            triggerSO.size = new Vector3(trigger.size.width, trigger.size.height, trigger.size.depth);
            triggerSO.valid = trigger.valid;
            triggerSO.newOrigin = trigger.newOrigin.ToVector3();
            triggerSO.newRotation = trigger.newRotation.ToVector3();
            AssetDatabase.AddObjectToAsset(triggerSO, sectionSO);
            triggerSOs.Add(triggerSO);
        }
        sectionSO.triggerBoxes = triggerSOs.ToArray();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Log the Resources path for loading
        string resourcesPath = assetPath.Replace("Assets/Resources/", "").Replace(".asset", "");
        Debug.Log($"Conversion complete! Load with Resources.Load<SectionSO>(\"{resourcesPath}\")");
    }

    private Shader GetDefaultShader()
    {
        // Try URP first (Universal Render Pipeline)
        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpShader != null)
            return urpShader;

        // Try HDRP (High Definition Render Pipeline)
        Shader hdrpShader = Shader.Find("HDRP/Lit");
        if (hdrpShader != null)
            return hdrpShader;

        // Fallback to Standard shader (built-in pipeline)
        Shader standardShader = Shader.Find("Standard");
        if (standardShader != null)
            return standardShader;

        // Last resort - any available shader
        return Shader.Find("Diffuse") ?? Shader.Find("Specular") ?? Shader.Find("VertexLit");
    }
}