using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "NewActionData", menuName = "Game/ActionData")]
    public class ActionDataSO : ScriptableObject
    {
        public string type;
        public string animationName;
        public AudioClip sound; // Changed to AudioClip for direct reference
        public float volume;
        public string text;
        public float duration;
    }
}