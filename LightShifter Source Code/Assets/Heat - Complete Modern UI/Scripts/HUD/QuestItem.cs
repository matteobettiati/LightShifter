using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Michsky.UI.Heat
{
    [RequireComponent(typeof(Animator))]
    public class QuestItem : MonoBehaviour
    {
        public enum AfterMinimize
        {
            Disable,
            Destroy
        }

        public enum DefaultState
        {
            Minimized,
            Expanded
        }

        // Content
        [TextArea] public string questText = "Quest text here";
        public string localizationKey;

        // Resources
        [SerializeField] private Animator questAnimator;
        [SerializeField] private TextMeshProUGUI questTextObj;

        // Settings
        public bool useLocalization = true;
        [SerializeField] private bool updateOnAnimate = true;
        [Range(0, 10)] public float minimizeAfter = 3;
        public DefaultState defaultState = DefaultState.Minimized;
        public AfterMinimize afterMinimize = AfterMinimize.Disable;

        // Events
        public UnityEvent onDestroy;

        // Helpers
        private bool isOn;
        private LocalizedObject localizedObject;

        private void Start()
        {
            if (questAnimator == null) questAnimator = GetComponent<Animator>();
            if (useLocalization)
            {
                localizedObject = questTextObj.GetComponent<LocalizedObject>();

                if (localizedObject == null || localizedObject.CheckLocalizationStatus() == false)
                {
                    useLocalization = false;
                }
                else if (localizedObject != null && !string.IsNullOrEmpty(localizationKey))
                {
                    // Forcing component to take the localized output on awake
                    questText = localizedObject.GetKeyOutput(localizationKey);

                    // Change text on language change
                    localizedObject.onLanguageChanged.AddListener(delegate
                    {
                        questText = localizedObject.GetKeyOutput(localizationKey);
                        UpdateUI();
                    });
                }
            }

            if (defaultState == DefaultState.Minimized)
                gameObject.SetActive(false);
            else if (defaultState == DefaultState.Expanded) ExpandQuest();

            UpdateUI();
        }

        public void UpdateUI()
        {
            questTextObj.text = questText;
        }

        public void AnimateQuest()
        {
            ExpandQuest();
        }

        public void ExpandQuest()
        {
            if (isOn)
            {
                StopCoroutine("DisableAnimator");
                StartCoroutine("DisableAnimator");

                if (minimizeAfter != 0)
                {
                    StopCoroutine("MinimizeItem");
                    StartCoroutine("MinimizeItem");
                }

                return;
            }

            isOn = true;
            gameObject.SetActive(true);
            questAnimator.enabled = true;
            questAnimator.Play("In");

            if (updateOnAnimate) UpdateUI();
            if (minimizeAfter != 0)
            {
                StopCoroutine("MinimizeItem");
                StartCoroutine("MinimizeItem");
            }

            StopCoroutine("DisableAnimator");
            StartCoroutine("DisableAnimator");
        }

        public void MinimizeQuest()
        {
            if (isOn == false)
                return;

            StopCoroutine("DisableAnimator");

            questAnimator.enabled = true;
            questAnimator.Play("Out");

            StopCoroutine("DisableItem");
            StartCoroutine("DisableItem");
        }

        public void CompleteQuest()
        {
            afterMinimize = AfterMinimize.Destroy;
            MinimizeQuest();
        }

        public void DestroyQuest()
        {
            onDestroy.Invoke();
            Destroy(gameObject);
        }

        private IEnumerator DisableAnimator()
        {
            yield return new WaitForSeconds(HeatUIInternalTools.GetAnimatorClipLength(questAnimator, "QuestItem_In"));
            questAnimator.enabled = false;
        }

        private IEnumerator DisableItem()
        {
            yield return new WaitForSeconds(HeatUIInternalTools.GetAnimatorClipLength(questAnimator, "QuestItem_Out"));

            isOn = false;

            if (afterMinimize == AfterMinimize.Disable)
                gameObject.SetActive(false);
            else if (afterMinimize == AfterMinimize.Destroy) DestroyQuest();
        }

        private IEnumerator MinimizeItem()
        {
            yield return new WaitForSeconds(minimizeAfter);
            MinimizeQuest();
        }
    }
}