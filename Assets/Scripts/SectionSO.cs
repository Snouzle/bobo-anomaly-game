using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "NewSection", menuName = "Game/Section")]
    public class SectionSO : ScriptableObject
    {
        public string sectionId;
        public EntitySO[] entities;
        public TriggerBoxDataSO[] triggerBoxes;
    }
}