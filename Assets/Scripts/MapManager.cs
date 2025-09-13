using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Assets.Scripts;
using TMPro;

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
public class AnimationData
{
    public string name;
    public string clip;
    public bool loop;
    public float speed;
}

[System.Serializable]
public class ConditionData
{
    public float distance;
}

[System.Serializable]
public class ActionData
{
    public string type;
    public string animationName;
    public string soundPath;
    public float volume;
    public string text;
    public float duration;
}

[System.Serializable]
public class EventData
{
    public string trigger;
    public ConditionData condition;
    public ActionData[] actions;
}

[System.Serializable]
public class EntityData
{
    public string id;
    public Vector3Data position;
    public SizeData size;
    public string type;
    public string[] connections;
    public string texture;
    public AnimationData[] animations;
    public EventData[] events;
}

[System.Serializable]
public class TriggerBoxData
{
    public Vector3Data position;
    public SizeData size;
    public bool valid;
    public Vector3Data newOrigin;
    public Vector3Data newRotation;
}

[System.Serializable]
public class SectionData
{
    public string sectionId;
    public EntityData[] entities;
    public TriggerBoxData[] triggerBoxes;
}

public class MapManager : MonoBehaviour
{
    private SectionData currentSection;
    private GameObject currentSectionObject;
    public Transform player;

    public int anomalyPercentChance = 30;

    private Section sectionInfo = new Section
    {
        origin = new Vector3(0, 0, 0),
        rotation = Quaternion.Euler(0, 0, 0)
    };

    private int passCounter = 0; // TODO: Handle this in another class

    private string anomalyDirectory = Path.Combine(Application.dataPath, "MapData", "anomaly");

    private string defaultMapPath = Path.Combine(Application.dataPath, "MapData", "default.json");

    private string[] anomalyMaps = new string[] { };

    void Start()
    {
        anomalyMaps = Directory.GetFiles(anomalyDirectory, "*.json");
        LoadSection(sectionInfo, defaultMapPath);
    }

    void Update()
    {
    }

    void LoadSection(Section nextSection, string sectionFilePath)
    {
        print(passCounter); // DEBUG

        if (File.Exists(sectionFilePath))
        {
            string json = File.ReadAllText(sectionFilePath);
            currentSection = JsonUtility.FromJson<SectionData>(json);
        }

        if (currentSectionObject != null)
        {
            Destroy(currentSectionObject);
            sectionInfo.origin += sectionInfo.rotation * nextSection.origin;
            sectionInfo.rotation *= nextSection.rotation;
        }

        currentSectionObject = new GameObject("CurrentSection");

        SpawnEntities(currentSection.entities, sectionInfo, currentSectionObject.transform);

        CreateTriggerColliders(currentSection.triggerBoxes, sectionInfo, currentSectionObject.transform);
    }

    public void LoadValidSection(Section section)
    {
        passCounter++;

        var isAnomaly = UnityEngine.Random.Range(1, 101) > anomalyPercentChance;

        if (isAnomaly)
        {
            var anomalyIndex = UnityEngine.Random.Range(0, anomalyMaps.Length);
            LoadSection(section, Path.Combine(anomalyDirectory, anomalyMaps[anomalyIndex]));
        }
        else
        {
            LoadSection(section, defaultMapPath);
        }
    }

    public void LoadInvalidSection(Section section)
    {
        passCounter = 0;
        LoadSection(section, defaultMapPath);
    }

    private void SpawnEntities(EntityData[] entities, Section section, Transform parent)
    {
        foreach (var entity in entities)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.position = section.origin + section.rotation * entity.position.ToVector3();
            obj.transform.rotation = section.rotation;
            obj.transform.localScale = new Vector3(entity.size.width, entity.size.height, entity.size.depth);
            obj.transform.parent = parent;
            obj.name = entity.id;

            // Apply texture
            if (!string.IsNullOrEmpty(entity.texture))
            {
                Texture2D tex = Resources.Load<Texture2D>(entity.texture);
                if (tex != null)
                {
                    obj.GetComponent<Renderer>().material.mainTexture = tex;
                }
            }

            // Apply animations
            if (entity.animations != null && entity.animations.Length > 0)
            {
                Animation animationComponent = obj.AddComponent<Animation>();
                foreach (var anim in entity.animations)
                {
                    AnimationClip clip = Resources.Load<AnimationClip>(anim.clip);
                    if (clip != null)
                    {
                        animationComponent.AddClip(clip, anim.name);
                        if (anim.loop)
                        {
                            animationComponent[anim.name].wrapMode = WrapMode.Loop;
                        }
                        animationComponent[anim.name].speed = anim.speed;
                    }
                }
                // Play first animation as default
                if (entity.animations.Length > 0)
                {
                    animationComponent.Play(entity.animations[0].name);
                }
            }

            // Add event handler
            if (entity.events != null && entity.events.Length > 0)
            {
                EntityEventHandler handler = obj.AddComponent<EntityEventHandler>();
                handler.events = entity.events;
                // Add collider for trigger if needed
                if (Array.Exists(entity.events, e => e.trigger == "onPlayerEnter"))
                {
                    BoxCollider triggerCollider = obj.AddComponent<BoxCollider>();
                    triggerCollider.isTrigger = true;
                    triggerCollider.size = new Vector3(entity.size.width, entity.size.height, entity.size.depth);
                }
            }
        }
    }

    private void CreateTriggerColliders(TriggerBoxData[] boxes, Section section, Transform parent)
    {
        if (boxes != null)
        {
            for (int i = 0; i < boxes.Length; i++)
            {
                var triggerData = boxes[i];
                GameObject triggerObj = new GameObject($"TriggerBox_{i}");
                triggerObj.transform.position = section.origin + section.rotation * triggerData.position.ToVector3();
                BoxCollider collider = triggerObj.AddComponent<BoxCollider>();
                collider.size = new Vector3(triggerData.size.width, triggerData.size.height, triggerData.size.depth);
                collider.isTrigger = true;
                TriggerScript triggerScript = triggerObj.AddComponent<TriggerScript>();
                triggerScript.mapManager = this;

                triggerScript.targetSection = new Section
                {
                    valid = triggerData.valid,
                    origin = triggerData.newOrigin.ToVector3(),
                    rotation = Quaternion.Euler(triggerData.newRotation.x, triggerData.newRotation.y, triggerData.newRotation.z)
                };
                triggerObj.transform.parent = parent;
            }
        }
    }
}