using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "NewTriggerBoxData", menuName = "Game/TriggerBoxData")]
    public class TriggerBoxDataSO : ScriptableObject
    {
        public Vector3 position;
        public Vector3 size;
        public bool valid;
        public Vector3 newOrigin;
        public Vector3 newRotation;
    }
}