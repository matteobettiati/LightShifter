using System;
using System.Collections;
using LevelController;
using UnityEngine;

namespace Platforms
{
    public class RespawnPlatform : MonoBehaviour
    {
        public static RespawnPlatform Instance;
        [SerializeField] private Transform respawnPlatform;
        public GameObject lastCheckpoint;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
        
        private void Start()
        {
            lastCheckpoint = null;
        }

        public void UpdateCheckpoint(GameObject checkpoint)
        {
            lastCheckpoint = checkpoint;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            StartCoroutine(WaitAndRespawn());
        }

        private IEnumerator WaitAndRespawn()
        {
            yield return new WaitForSeconds(0.5f);
            SceneController.Instance.Respawn(lastCheckpoint);
        }
    }
}