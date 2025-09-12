using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Assets.Scripts;
using System;

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
}

[System.Serializable]
public class SectionData
{
    public string sectionId;
    public EntityData[] entities;
    public TriggerBoxData triggerBox;
    public string nextSectionFile;
}

public class MapManager : MonoBehaviour
{
    public string initialSectionFile = "Section1.json";
    private SectionData currentSection;
    private GameObject currentSectionObject;
    public Transform player;

    private Vector3 originPosition = new Vector3(0, 0, 0);

    private Quaternion originRotation = Quaternion.Euler(0, 0, 0);

    private int mapSize = 30; // TODO: Extract this out

    void Start()
    {
        LoadSection(initialSectionFile);
    }

    void Update()
    {
        // Trigger detection now handled by OnTriggerEnter
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


        if (currentSectionObject != null)
        {
            Destroy(currentSectionObject);
            originPosition = new Vector3(mapSize - originPosition.x, 0, -15 - originPosition.z);
            originRotation = originRotation * Quaternion.Euler(0, 180, 0);
        }

        currentSectionObject = new GameObject("CurrentSection");

        foreach (var entity in currentSection.entities)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.position = originPosition + originRotation * entity.position.ToVector3();
            obj.transform.rotation = originRotation;
            obj.transform.localScale = new Vector3(entity.size.width, entity.size.height, entity.size.depth);
            obj.transform.parent = currentSectionObject.transform;
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
                if (System.Array.Exists(entity.events, e => e.trigger == "onPlayerEnter"))
                {
                    BoxCollider triggerCollider = obj.AddComponent<BoxCollider>();
                    triggerCollider.isTrigger = true;
                    triggerCollider.size = new Vector3(entity.size.width, entity.size.height, entity.size.depth);
                }
            }
        }

        // Create trigger collider
        GameObject triggerObj = new GameObject("TriggerBox");
        triggerObj.transform.position = originPosition + originRotation * currentSection.triggerBox.position.ToVector3();
        BoxCollider collider = triggerObj.AddComponent<BoxCollider>();
        collider.size = new Vector3(currentSection.triggerBox.size.width, currentSection.triggerBox.size.height, currentSection.triggerBox.size.depth);
        collider.isTrigger = true;
        TriggerScript triggerScript = triggerObj.AddComponent<TriggerScript>();
        triggerScript.mapManager = this;
        triggerObj.transform.parent = currentSectionObject.transform;
    }

    public void LoadNextSection()
    {
        if (!string.IsNullOrEmpty(currentSection.nextSectionFile))
        {
            LoadSection(currentSection.nextSectionFile);
        }
    }
}