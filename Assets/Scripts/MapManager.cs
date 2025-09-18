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
    private SectionSO currentSection;
    private GameObject currentSectionObject;
    public Transform player;

    public int anomalyPercentChance = 30;

    private Section sectionInfo = new Section
    {
        origin = new Vector3(0, 0, 0),
        rotation = Quaternion.Euler(0, 0, 0)
    };

    private int passCounter = 0; // TODO: Handle this in another class

    public string defaultSectionPath = "Sections/default"; // Path in Resources

    private int anomaliesCount = 1; // How to set this, 

    void Start()
    {
        LoadSection(sectionInfo, defaultSectionPath);
    }

    void Update()
    {
    }

    void LoadSection(Section nextSection, string sectionPath)
    {
        print(passCounter); // DEBUG

        SectionSO sectionSO = Resources.Load<SectionSO>(sectionPath);
        if (sectionSO == null)
        {
            Debug.LogError("Failed to load SectionSO from path: " + sectionPath);
            return;
        }
        currentSection = sectionSO;

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
            var anomalyIndex = UnityEngine.Random.Range(1, anomaliesCount+1);
            LoadSection(section, GetAnomalySectionFileName(anomalyIndex));
        }
        else
        {
            LoadSection(section, defaultSectionPath);
        }
    }

    public void LoadInvalidSection(Section section)
    {
        passCounter = 0;
        LoadSection(section, defaultSectionPath);
    }

    private void SpawnEntities(EntitySO[] entities, Section section, Transform parent)
    {
        foreach (var entity in entities)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube); // TODO: Replace with asset
            obj.transform.SetPositionAndRotation(section.origin + section.rotation * entity.position, section.rotation);
            obj.transform.localScale = entity.size;
            obj.transform.parent = parent;
            obj.name = entity.id;

            // Apply texture
            if (entity.texture != null)
            {
                obj.GetComponent<Renderer>().material.mainTexture = entity.texture;
            }

            // Apply animations
            if (entity.animations != null && entity.animations.Length > 0)
            {
                Animation animationComponent = obj.AddComponent<Animation>();
                foreach (var anim in entity.animations)
                {
                    if (anim.clip != null)
                    {
                        animationComponent.AddClip(anim.clip, anim.name);
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
                    triggerCollider.size = entity.size;
                }
            }
        }
    }

    private void CreateTriggerColliders(TriggerBoxDataSO[] boxes, Section section, Transform parent)
    {
        if (boxes != null)
        {
            for (int i = 0; i < boxes.Length; i++)
            {
                var triggerData = boxes[i];
                GameObject triggerObj = new GameObject($"TriggerBox_{i}");
                triggerObj.transform.position = section.origin + section.rotation * triggerData.position;
                BoxCollider collider = triggerObj.AddComponent<BoxCollider>();
                collider.size = triggerData.size;
                collider.isTrigger = true;
                TriggerScript triggerScript = triggerObj.AddComponent<TriggerScript>();
                triggerScript.mapManager = this;

                triggerScript.targetSection = new Section
                {
                    valid = triggerData.valid,
                    origin = triggerData.newOrigin,
                    rotation = Quaternion.Euler(triggerData.newRotation.x, triggerData.newRotation.y, triggerData.newRotation.z)
                };
                triggerObj.transform.parent = parent;
            }
        }
    }

    private string GetAnomalySectionFileName(int anomaly_idx)
    {
        return $"Sections/Anomaly{anomaly_idx}";
    }
}