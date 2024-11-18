using UnityEngine;

namespace LevelController
{
    namespace LevelController
    {
        public class FinishPoint : MonoBehaviour
        {
            private void OnTriggerEnter2D(Collider2D collision)
            {
                if (collision.CompareTag("Player"))
                    SceneController.Instance.NextLevel();
            }
        }
    }

}