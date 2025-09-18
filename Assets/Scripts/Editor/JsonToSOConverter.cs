using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts;

public class JsonToSOConverter : EditorWindow
{
    private string jsonPath = "Assets/MapData/anomaly/Section_1.json"; // Example path
    private string outputFolder = "Assets/Resources/Sections/"; // Folder to save SOs (for Resources.Load)

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

        // Convert entities
        List<EntitySO> entitySOs = new List<EntitySO>();
        foreach (var entity in sectionData.entities)
        {
            EntitySO entitySO = CreateInstance<EntitySO>();
            entitySO.id = entity.id;
            entitySO.position = entity.position.ToVector3();
            entitySO.size = new Vector3(entity.size.width, entity.size.height, entity.size.depth);
            entitySO.type = entity.type;
            entitySO.connections = entity.connections;

            // Load texture if path exists
            if (!string.IsNullOrEmpty(entity.texture))
            {
                entitySO.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(entity.texture);
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
                AssetDatabase.AddObjectToAsset(animSO, sectionSO);
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
                    AssetDatabase.AddObjectToAsset(condSO, sectionSO);
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
                    AssetDatabase.AddObjectToAsset(actionSO, sectionSO);
                    actionSOs.Add(actionSO);
                }
                eventSO.actions = actionSOs.ToArray();
                AssetDatabase.AddObjectToAsset(eventSO, sectionSO);
                eventSOs.Add(eventSO);
            }
            entitySO.events = eventSOs.ToArray();

            AssetDatabase.AddObjectToAsset(entitySO, sectionSO);
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
}