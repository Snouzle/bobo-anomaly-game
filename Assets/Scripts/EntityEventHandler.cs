using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class EntityEventHandler : MonoBehaviour
    {
        public EventDataSO[] events;
        private Animator animator;
        private AudioSource audioSource;
        private Transform player;

        void Start()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        void Update()
        {
            if (player == null) return;

            foreach (var evt in events)
            {
                if (evt.trigger == "onDistance" && evt.condition != null)
                {
                    float dist = Vector3.Distance(transform.position, player.position);
                    if (dist <= evt.condition.distance)
                    {
                        ExecuteActions(evt.actions);
                        // Remove event after trigger? Or allow repeat
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                foreach (var evt in events)
                {
                    if (evt.trigger == "onPlayerEnter")
                    {
                        ExecuteActions(evt.actions);
                    }
                }
            }
        }

        private void ExecuteActions(ActionDataSO[] actions)
        {
            foreach (var action in actions)
            {
                switch (action.type)
                {
                    case "playAnimation":
                        if (animator != null && !string.IsNullOrEmpty(action.animationName))
                        {
                            animator.Play(action.animationName);
                        }
                        break;
                    case "playSound":
                        if (action.sound != null)
                        {
                            audioSource.PlayOneShot(action.sound, action.volume);
                        }
                        break;
                    case "showDialog":
                        if (!string.IsNullOrEmpty(action.text))
                        {
                            Debug.Log("Dialog: " + action.text);
                            // In a real game, show UI dialog for action.duration seconds
                        }
                        break;
                }
            }
        }
    }
}