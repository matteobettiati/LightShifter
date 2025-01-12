using System.Collections.Generic;
using UnityEngine;

namespace Michsky.UI.Heat
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Heat UI/Animation/Box Container")]
    public class BoxContainer : MonoBehaviour
    {
        public enum UpdateMode
        {
            DeltaTime,
            UnscaledTime
        }

        [Header("Animation")]
        public AnimationCurve animationCurve = new(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));

        [Range(0.5f, 10)] public float curveSpeed = 1;
        [Range(0, 5)] public float animationDelay;

        [Header("Fading")] [Range(0, 0.99f)] public float fadeAfterScale = 0.75f;

        [Range(0.1f, 10)] public float fadeSpeed = 5f;

        [Header("Settings")] public UpdateMode updateMode = UpdateMode.DeltaTime;

        [Range(0, 1)] public float itemCooldown = 0.1f;
        public bool playOnce;

        // Helpers
        [HideInInspector] public bool isPlayedOnce;
        private readonly List<BoxContainerItem> cachedItems = new();

        private void Awake()
        {
            foreach (Transform child in transform)
            {
                var temp = child.gameObject.AddComponent<BoxContainerItem>();
                temp.container = this;
                cachedItems.Add(temp);
            }
        }

        private void OnEnable()
        {
            if (animationDelay > 0)
                Invoke(nameof(Animate), animationDelay);
            else
                Animate();
        }

        public void Animate()
        {
            if (playOnce && isPlayedOnce)
                return;

            float tempTime = 0;

            if (cachedItems.Count > 0)
                foreach (var item in cachedItems)
                {
                    item.Process(tempTime);
                    tempTime += itemCooldown;
                }

            isPlayedOnce = true;
        }
    }
}