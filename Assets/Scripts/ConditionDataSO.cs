using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "NewConditionData", menuName = "Game/ConditionData")]
    public class ConditionDataSO : ScriptableObject
    {
        public float distance;
    }
}