using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "NewEntity", menuName = "Game/Entity")]
    public class EntitySO : ScriptableObject
    {
        public string id;
        public Vector3 position;
        public Vector3 size;
        public string type;
        public string[] connections;
        public Texture2D texture;
        public AnimationDataSO[] animations;
        public EventDataSO[] events;
    }
}