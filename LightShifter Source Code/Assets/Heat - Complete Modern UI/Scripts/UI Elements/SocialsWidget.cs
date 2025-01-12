using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Michsky.UI.Heat
{
    public class SocialsWidget : MonoBehaviour
    {
        public enum UpdateMode
        {
            DeltaTime,
            UnscaledTime
        }

        // Content
        public List<Item> socials = new();

        // Resources
        [SerializeField] private GameObject itemPreset;
        [SerializeField] private Transform itemParent;
        [SerializeField] private GameObject buttonPreset;
        [SerializeField] private Transform buttonParent;
        [SerializeField] private Image background;

        // Settings
        public bool allowTransition = true;
        public bool useLocalization = true;
        [Range(1, 30)] public float timer = 4;
        [Range(0.1f, 3)] public float tintSpeed = 0.5f;
        [SerializeField] private AnimationCurve tintCurve = new(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));
        [SerializeField] private UpdateMode updateMode = UpdateMode.DeltaTime;
        private readonly List<ButtonManager> buttons = new();
        private Animator currentItemObject;

        // Helpers
        private int currentSliderIndex;
        private bool isInitialized;
        private bool isTransitionInProgress;
        private LocalizedObject localizedObject;
        private Image raycastImg;
        private float timerCount;
        private bool updateTimer;

        private void Update()
        {
            if (!isInitialized || !updateTimer || !allowTransition)
                return;

            if (updateMode == UpdateMode.UnscaledTime)
                timerCount += Time.unscaledDeltaTime;
            else
                timerCount += Time.deltaTime;

            if (timerCount > timer)
            {
                SetSocialByTimer();
                timerCount = 0;
            }
        }

        private void OnEnable()
        {
            if (!isInitialized)
                InitializeItems();
            else
                StartCoroutine(InitCurrentItem());
        }

        public void InitializeItems()
        {
            if (useLocalization)
            {
                localizedObject = gameObject.GetComponent<LocalizedObject>();
                if (localizedObject == null || !localizedObject.CheckLocalizationStatus()) useLocalization = false;
            }

            foreach (Transform child in itemParent) Destroy(child.gameObject);
            foreach (Transform child in buttonParent) Destroy(child.gameObject);
            for (var i = 0; i < socials.Count; ++i)
            {
                var tempIndex = i;

                var itemGO = Instantiate(itemPreset, new Vector3(0, 0, 0), Quaternion.identity);
                itemGO.transform.SetParent(itemParent, false);
                itemGO.gameObject.name = socials[i].socialID;
                itemGO.SetActive(false);

                var itemDescription = itemGO.transform.GetComponentInChildren<TextMeshProUGUI>();
                if (!useLocalization || string.IsNullOrEmpty(socials[i].descriptionKey))
                {
                    itemDescription.text = socials[i].description;
                }
                else
                {
                    var tempLoc = itemDescription.GetComponent<LocalizedObject>();
                    if (tempLoc != null)
                    {
                        tempLoc.tableIndex = localizedObject.tableIndex;
                        tempLoc.localizationKey = socials[i].descriptionKey;
                        tempLoc.onLanguageChanged.AddListener(delegate
                        {
                            itemDescription.text = tempLoc.GetKeyOutput(tempLoc.localizationKey);
                        });
                        tempLoc.InitializeItem();
                        tempLoc.UpdateItem();
                    }
                }

                var btnGO = Instantiate(buttonPreset, new Vector3(0, 0, 0), Quaternion.identity);
                btnGO.transform.SetParent(buttonParent, false);
                btnGO.gameObject.name = socials[i].socialID;

                var btn = btnGO.GetComponent<ButtonManager>();
                buttons.Add(btn);
                btn.SetIcon(socials[i].icon);
                btn.onClick.AddListener(delegate
                {
                    socials[tempIndex].onClick.Invoke();
                    if (!string.IsNullOrEmpty(socials[tempIndex].link)) Application.OpenURL(socials[tempIndex].link);
                });
                btn.onHover.AddListener(delegate
                {
                    foreach (var tempBtn in buttons)
                    {
                        if (tempBtn == btn)
                            continue;

                        tempBtn.Interactable(false);
                        tempBtn.UpdateState();

                        StopCoroutine("SetSocialByHover");
                        StartCoroutine("SetSocialByHover", tempIndex);
                    }
                });
                btn.onLeave.AddListener(delegate
                {
                    updateTimer = true;
                    foreach (var tempBtn in buttons)
                    {
                        if (tempBtn == btn)
                        {
                            tempBtn.StartCoroutine("SetHighlight");
                            continue;
                        }

                        tempBtn.Interactable(true);
                        tempBtn.UpdateState();
                    }
                });
                btn.onSelect.AddListener(delegate
                {
                    foreach (var tempBtn in buttons)
                    {
                        if (tempBtn == btn)
                            continue;

                        tempBtn.overrideInteractable = true;
                        tempBtn.Interactable(false);
                        tempBtn.UpdateState();

                        StopCoroutine("SetSocialByHover");
                        StartCoroutine("SetSocialByHover", tempIndex);
                    }
                });
                btn.onDeselect.AddListener(delegate
                {
                    updateTimer = true;
                    foreach (var tempBtn in buttons)
                    {
                        if (tempBtn == btn)
                        {
                            tempBtn.StartCoroutine("SetHighlight");
                            continue;
                        }

                        tempBtn.overrideInteractable = false;
                        tempBtn.Interactable(true);
                        tempBtn.UpdateState();
                    }
                });
            }

            StartCoroutine(InitCurrentItem());
            isInitialized = true;

            // Register raycast with image
            if (raycastImg == null)
            {
                raycastImg = gameObject.AddComponent<Image>();
                raycastImg.color = new Color(0, 0, 0, 0);
            }
        }

        public void SetSocialByTimer()
        {
            if (socials.Count > 1 && currentItemObject != null)
            {
                currentItemObject.enabled = true;
                currentItemObject.Play("Out");
                buttons[currentSliderIndex].UpdateState();

                if (currentSliderIndex == socials.Count - 1)
                    currentSliderIndex = 0;
                else
                    currentSliderIndex++;

                currentItemObject = itemParent.GetChild(currentSliderIndex).GetComponent<Animator>();
                currentItemObject.gameObject.SetActive(true);
                currentItemObject.enabled = true;
                currentItemObject.Play("In");
                buttons[currentSliderIndex].StartCoroutine("SetHighlight");

                StopCoroutine("DisableItemAnimators");
                StartCoroutine("DisableItemAnimators");

                if (background != null)
                {
                    StopCoroutine("DoBackgroundColorLerp");
                    StartCoroutine("DoBackgroundColorLerp");
                }
            }
        }

        public void AllowTransition(bool canSwitch)
        {
            allowTransition = canSwitch;
        }

        private IEnumerator InitCurrentItem()
        {
            if (updateMode == UpdateMode.UnscaledTime)
                yield return new WaitForSecondsRealtime(0.02f);
            else
                yield return new WaitForSeconds(0.02f);

            currentItemObject = itemParent.GetChild(currentSliderIndex).GetComponent<Animator>();
            currentItemObject.gameObject.SetActive(true);
            currentItemObject.enabled = true;
            currentItemObject.Play("In");
            buttons[currentSliderIndex].StartCoroutine("SetHighlight");

            StopCoroutine("DisableItemAnimators");
            StartCoroutine("DisableItemAnimators");

            if (background != null)
            {
                StopCoroutine("DoBackgroundColorLerp");
                StartCoroutine("DoBackgroundColorLerp");
            }

            timerCount = 0;
            updateTimer = true;
        }

        private IEnumerator SetSocialByHover(int index)
        {
            updateTimer = false;
            timerCount = 0;

            if (currentSliderIndex == index) yield break;
            if (isTransitionInProgress)
            {
                if (updateMode == UpdateMode.UnscaledTime)
                    yield return new WaitForSecondsRealtime(0.15f);
                else
                    yield return new WaitForSeconds(0.15f);

                isTransitionInProgress = false;
            }

            isTransitionInProgress = true;

            currentItemObject.enabled = true;
            currentItemObject.Play("Out");

            currentSliderIndex = index;

            currentItemObject = itemParent.GetChild(currentSliderIndex).GetComponent<Animator>();
            currentItemObject.gameObject.SetActive(true);
            currentItemObject.enabled = true;
            currentItemObject.Play("In");

            StopCoroutine("DisableItemAnimators");
            StartCoroutine("DisableItemAnimators");

            if (background != null)
            {
                StopCoroutine("DoBackgroundColorLerp");
                StartCoroutine("DoBackgroundColorLerp");
            }
        }

        private IEnumerator DoBackgroundColorLerp()
        {
            float elapsedTime = 0;

            while (background.color != socials[currentSliderIndex].backgroundTint)
            {
                if (updateMode == UpdateMode.UnscaledTime)
                    elapsedTime += Time.unscaledDeltaTime;
                else
                    elapsedTime += Time.deltaTime;

                background.color = Color.Lerp(background.color, socials[currentSliderIndex].backgroundTint,
                    tintCurve.Evaluate(elapsedTime * tintSpeed));
                yield return null;
            }

            background.color = socials[currentSliderIndex].backgroundTint;
        }

        private IEnumerator DisableItemAnimators()
        {
            if (updateMode == UpdateMode.UnscaledTime)
                yield return new WaitForSecondsRealtime(0.6f);
            else
                yield return new WaitForSeconds(0.6f);

            for (var i = 0; i < socials.Count; i++)
            {
                if (i != currentSliderIndex) itemParent.GetChild(i).gameObject.SetActive(false);
                itemParent.GetChild(i).GetComponent<Animator>().enabled = false;
            }
        }

        [Serializable]
        public class Item
        {
            public string socialID = "News title";
            public Sprite icon;
            public Color backgroundTint = new(255, 255, 255, 255);
            [TextArea] public string description = "News description";
            public string link;
            public UnityEvent onClick = new();

            [Header("Localization")] public string descriptionKey = "DescriptionKey";
        }
    }
}