using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.Heat
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu("Heat UI/Misc/Text Time Counter")]
    public class TextTimeCounter : MonoBehaviour
    {
        private int minutes;
        private float seconds;
        private RectTransform tmpRect;
        private TextMeshProUGUI tmpText;

        private void OnEnable()
        {
            if (tmpText == null) tmpText = GetComponent<TextMeshProUGUI>();
            if (tmpRect == null) tmpRect = tmpText.GetComponent<RectTransform>();

            ResetTimer();
        }

        public void ResetTimer()
        {
            minutes = 0;
            seconds = 0;
            tmpText.text = "0:00";

            StopCoroutine("Count");
            StartCoroutine("Count");
        }

        private IEnumerator Count()
        {
            while (true)
            {
                if (seconds == 60)
                {
                    seconds = 0;
                    minutes++;
                }

                if (seconds < 10)
                    tmpText.text = minutes + ":0" + seconds.ToString("F0");
                else
                    tmpText.text = minutes + ":" + seconds.ToString("F0");

                LayoutRebuilder.ForceRebuildLayoutImmediate(tmpRect);
                seconds++;

                yield return new WaitForSeconds(1);
            }
        }
    }
}