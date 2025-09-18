using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "NewAnimationData", menuName = "Game/AnimationData")]
    public class AnimationDataSO : ScriptableObject
    {
        public string name;
        public AnimationClip clip;
        public bool loop;
        public float speed;
    }
}