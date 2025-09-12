using UnityEngine;

namespace Assets.Scripts
{
    public class TriggerScript : MonoBehaviour
    {
        public MapManager mapManager;

        private void OnTriggerEnter(Collider other)
        {
            print(other.tag);
            if (other.CompareTag("Player"))
            {
                mapManager.LoadNextSection();
            }
        }
    } 
}
