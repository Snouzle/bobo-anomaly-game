using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "NewEventData", menuName = "Game/EventData")]
    public class EventDataSO : ScriptableObject
    {
        public string trigger;
        public ConditionDataSO condition;
        public ActionDataSO[] actions;
    }
}