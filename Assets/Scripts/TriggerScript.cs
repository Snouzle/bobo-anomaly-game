using UnityEngine;

namespace Assets.Scripts
{
    public class Section {
        public bool valid;

        public Vector3 origin;

        public Quaternion rotation;
    }

    public class TriggerScript : MonoBehaviour
    {
        public MapManager mapManager;
        public Section targetSection;


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (targetSection.valid)
                {
                    mapManager.LoadValidSection(targetSection);
                }
                else
                {
                    mapManager.LoadInvalidSection(targetSection);
                }
            }
        }
    }
}
