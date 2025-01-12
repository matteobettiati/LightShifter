using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Michsky.UI.Heat
{
    public class PanelManager : MonoBehaviour
    {
        public enum PanelMode
        {
            MainPanel,
            SubPanel,
            Custom
        }

        public enum UpdateMode
        {
            DeltaTime,
            UnscaledTime
        }

        // Content
        public List<PanelItem> panels = new();

        // Settings
        public int currentPanelIndex;
        public bool cullPanels = true;
        [SerializeField] private bool initializeButtons = true;
        [SerializeField] private bool useCooldownForHotkeys;
        [SerializeField] private bool bypassAnimationOnEnable;
        [SerializeField] private UpdateMode updateMode = UpdateMode.UnscaledTime;
        [SerializeField] private PanelMode panelMode = PanelMode.Custom;
        [Range(0.75f, 2)] public float animationSpeed = 1;
        public PanelChangeCallback onPanelChanged = new();
        public float cachedStateLength = 1;
        [HideInInspector] public int managerIndex;
        private readonly string animSpeedKey = "AnimSpeed";

        private PanelButton currentButton;
        private int currentButtonIndex;

        // Helpers
        private Animator currentPanel;

        private bool isInitialized;
        private int newPanelIndex;
        private PanelButton nextButton;
        private Animator nextPanel;

        private readonly string panelFadeIn = "Panel In";
        private readonly string panelFadeOut = "Panel Out";

        private void Awake()
        {
            if (panels.Count == 0)
                return;

            if (panelMode == PanelMode.MainPanel)
                cachedStateLength =
                    HeatUIInternalTools.GetAnimatorClipLength(panels[currentPanelIndex].panelObject, "MainPanel_In");
            else if (panelMode == PanelMode.SubPanel)
                cachedStateLength =
                    HeatUIInternalTools.GetAnimatorClipLength(panels[currentPanelIndex].panelObject, "SubPanel_In");
            else if (panelMode == PanelMode.Custom) cachedStateLength = 1f;

            if (ControllerManager.instance != null)
            {
                managerIndex = ControllerManager.instance.panels.Count;
                ControllerManager.instance.panels.Add(this);
            }
        }

        private void OnEnable()
        {
            if (!isInitialized) InitializePanels();
            if (ControllerManager.instance != null) ControllerManager.instance.currentManagerIndex = managerIndex;

            if (bypassAnimationOnEnable)
            {
                for (var i = 0; i < panels.Count; i++)
                {
                    if (panels[i].panelObject == null)
                        continue;

                    if (currentPanelIndex == i)
                    {
                        panels[i].panelObject.gameObject.SetActive(true);
                        panels[i].panelObject.enabled = true;
                        panels[i].panelObject.Play("Panel Instant In");
                    }

                    else
                    {
                        panels[i].panelObject.gameObject.SetActive(false);
                    }
                }
            }

            else if (isInitialized && !bypassAnimationOnEnable && nextPanel == null)
            {
                currentPanel.enabled = true;
                currentPanel.SetFloat(animSpeedKey, animationSpeed);
                currentPanel.Play(panelFadeIn);
                if (currentButton != null) currentButton.SetSelected(true);
            }

            else if (isInitialized && !bypassAnimationOnEnable && nextPanel != null)
            {
                nextPanel.enabled = true;
                nextPanel.SetFloat(animSpeedKey, animationSpeed);
                nextPanel.Play(panelFadeIn);
                if (nextButton != null) nextButton.SetSelected(true);
            }

            StopCoroutine("DisablePreviousPanel");
            StopCoroutine("DisableAnimators");
            StartCoroutine("DisableAnimators");
        }

        public void InitializePanels()
        {
            if (panels[currentPanelIndex].panelButton != null)
            {
                currentButton = panels[currentPanelIndex].panelButton;
                currentButton.SetSelected(true);
            }

            currentPanel = panels[currentPanelIndex].panelObject;
            currentPanel.enabled = true;
            currentPanel.gameObject.SetActive(true);

            currentPanel.SetFloat(animSpeedKey, animationSpeed);
            currentPanel.Play(panelFadeIn);

            onPanelChanged.Invoke(currentPanelIndex);

            for (var i = 0; i < panels.Count; i++)
            {
                if (panels[i].panelObject == null) continue;
                if (i != currentPanelIndex && cullPanels) panels[i].panelObject.gameObject.SetActive(false);
                if (initializeButtons)
                {
                    var tempName = panels[i].panelName;
                    if (panels[i].panelButton != null)
                        panels[i].panelButton.onClick.AddListener(() => OpenPanel(tempName));
                    if (panels[i].altPanelButton != null)
                        panels[i].altPanelButton.onClick.AddListener(() => OpenPanel(tempName));
                    if (panels[i].altBoxButton != null)
                        panels[i].altBoxButton.onClick.AddListener(() => OpenPanel(tempName));
                }

                if (panels[i].hotkeyParent != null)
                {
                    panels[i].hotkeys = panels[i].hotkeyParent.GetComponentsInChildren<HotkeyEvent>();
                    if (useCooldownForHotkeys)
                        foreach (var he in panels[i].hotkeys)
                            he.useCooldown = true;
                }
            }

            StopCoroutine("DisableAnimators");
            StartCoroutine("DisableAnimators");

            isInitialized = true;
        }

        public void OpenFirstPanel()
        {
            OpenPanelByIndex(0);
        }

        public void OpenPanel(string newPanel)
        {
            var catchedPanel = false;

            for (var i = 0; i < panels.Count; i++)
                if (panels[i].panelName == newPanel)
                {
                    newPanelIndex = i;
                    catchedPanel = true;
                    break;
                }

            if (catchedPanel == false)
            {
                Debug.LogWarning("There is no panel named '" + newPanel + "' in the panel list.", this);
                return;
            }

            if (newPanelIndex != currentPanelIndex)
            {
                if (cullPanels) StopCoroutine("DisablePreviousPanel");
                if (ControllerManager.instance != null) ControllerManager.instance.currentManagerIndex = managerIndex;

                currentPanel = panels[currentPanelIndex].panelObject;

                if (panels[currentPanelIndex].hotkeyParent != null)
                    foreach (var he in panels[currentPanelIndex].hotkeys)
                        he.enabled = false;
                if (panels[currentPanelIndex].panelButton != null)
                    currentButton = panels[currentPanelIndex].panelButton;
                if (ControllerManager.instance != null && EventSystem.current.currentSelectedGameObject != null)
                    panels[currentPanelIndex].latestSelected = EventSystem.current.currentSelectedGameObject;

                currentPanelIndex = newPanelIndex;
                nextPanel = panels[currentPanelIndex].panelObject;
                nextPanel.gameObject.SetActive(true);

                currentPanel.enabled = true;
                nextPanel.enabled = true;

                currentPanel.SetFloat(animSpeedKey, animationSpeed);
                nextPanel.SetFloat(animSpeedKey, animationSpeed);

                currentPanel.Play(panelFadeOut);
                nextPanel.Play(panelFadeIn);

                if (cullPanels) StartCoroutine("DisablePreviousPanel");
                if (panels[currentPanelIndex].hotkeyParent != null)
                    foreach (var he in panels[currentPanelIndex].hotkeys)
                        he.enabled = true;

                currentButtonIndex = newPanelIndex;

                if (ControllerManager.instance != null && panels[currentPanelIndex].latestSelected != null)
                    ControllerManager.instance.SelectUIObject(panels[currentPanelIndex].latestSelected);
                else if (ControllerManager.instance != null && panels[currentPanelIndex].latestSelected == null)
                    ControllerManager.instance.SelectUIObject(panels[currentPanelIndex].firstSelected);

                if (currentButton != null) currentButton.SetSelected(false);
                if (panels[currentButtonIndex].panelButton != null)
                {
                    nextButton = panels[currentButtonIndex].panelButton;
                    nextButton.SetSelected(true);
                }

                onPanelChanged.Invoke(currentPanelIndex);

                StopCoroutine("DisableAnimators");
                StartCoroutine("DisableAnimators");
            }
        }

        public void OpenPanelByIndex(int panelIndex)
        {
            if (panelIndex > panels.Count || panelIndex < 0)
            {
                Debug.LogWarning("Index '" + panelIndex + "' doesn't exist.", this);
                return;
            }

            for (var i = 0; i < panels.Count; i++)
                if (panels[i].panelName == panels[panelIndex].panelName)
                {
                    OpenPanel(panels[panelIndex].panelName);
                    break;
                }
        }

        public void NextPanel()
        {
            if (currentPanelIndex <= panels.Count - 2 && !panels[currentPanelIndex + 1].disableNavigation)
                OpenPanelByIndex(currentPanelIndex + 1);
        }

        public void PreviousPanel()
        {
            if (currentPanelIndex >= 1 && !panels[currentPanelIndex - 1].disableNavigation)
                OpenPanelByIndex(currentPanelIndex - 1);
        }

        public void ShowCurrentPanel()
        {
            if (nextPanel == null)
            {
                StopCoroutine("DisableAnimators");
                StartCoroutine("DisableAnimators");

                currentPanel.enabled = true;
                currentPanel.SetFloat(animSpeedKey, animationSpeed);
                currentPanel.Play(panelFadeIn);
            }

            else
            {
                StopCoroutine("DisableAnimators");
                StartCoroutine("DisableAnimators");

                nextPanel.enabled = true;
                nextPanel.SetFloat(animSpeedKey, animationSpeed);
                nextPanel.Play(panelFadeIn);
            }
        }

        public void HideCurrentPanel()
        {
            if (nextPanel == null)
            {
                StopCoroutine("DisableAnimators");
                StartCoroutine("DisableAnimators");

                currentPanel.enabled = true;
                currentPanel.SetFloat(animSpeedKey, animationSpeed);
                currentPanel.Play(panelFadeOut);
            }

            else
            {
                StopCoroutine("DisableAnimators");
                StartCoroutine("DisableAnimators");

                nextPanel.enabled = true;
                nextPanel.SetFloat(animSpeedKey, animationSpeed);
                nextPanel.Play(panelFadeOut);
            }
        }

        public void ShowCurrentButton()
        {
            if (nextButton == null)
                currentButton.SetSelected(true);
            else
                nextButton.SetSelected(true);
        }

        public void HideCurrentButton()
        {
            if (nextButton == null)
                currentButton.SetSelected(false);
            else
                nextButton.SetSelected(false);
        }

        public void AddNewItem()
        {
            var panel = new PanelItem();

            if (panels.Count != 0 && panels[panels.Count - 1].panelObject != null)
            {
                var tempIndex = panels.Count - 1;

                var tempPanel = panels[tempIndex].panelObject.transform.parent.GetChild(tempIndex).gameObject;
                var newPanel = Instantiate(tempPanel, new Vector3(0, 0, 0), Quaternion.identity);

                newPanel.transform.SetParent(panels[tempIndex].panelObject.transform.parent, false);
                newPanel.gameObject.name = "New Panel " + tempIndex;

                panel.panelName = "New Panel " + tempIndex;
                panel.panelObject = newPanel.GetComponent<Animator>();

                if (panels[tempIndex].panelButton != null)
                {
                    var tempButton = panels[tempIndex].panelButton.transform.parent.GetChild(tempIndex).gameObject;
                    var newButton = Instantiate(tempButton, new Vector3(0, 0, 0), Quaternion.identity);

                    newButton.transform.SetParent(panels[tempIndex].panelButton.transform.parent, false);
                    newButton.gameObject.name = "New Panel " + tempIndex;

                    panel.panelButton = newButton.GetComponent<PanelButton>();
                }

                else if (panels[tempIndex].altPanelButton != null)
                {
                    var tempButton = panels[tempIndex].altPanelButton.transform.parent.GetChild(tempIndex).gameObject;
                    var newButton = Instantiate(tempButton, new Vector3(0, 0, 0), Quaternion.identity);

                    newButton.transform.SetParent(panels[tempIndex].panelButton.transform.parent, false);
                    newButton.gameObject.name = "New Panel " + tempIndex;

                    panel.altPanelButton = newButton.GetComponent<ButtonManager>();
                }

                else if (panels[tempIndex].altBoxButton != null)
                {
                    var tempButton = panels[tempIndex].altBoxButton.transform.parent.GetChild(tempIndex).gameObject;
                    var newButton = Instantiate(tempButton, new Vector3(0, 0, 0), Quaternion.identity);

                    newButton.transform.SetParent(panels[tempIndex].panelButton.transform.parent, false);
                    newButton.gameObject.name = "New Panel " + tempIndex;

                    panel.altBoxButton = newButton.GetComponent<BoxButtonManager>();
                }
            }

            panels.Add(panel);
        }

        private IEnumerator DisablePreviousPanel()
        {
            if (updateMode == UpdateMode.UnscaledTime)
                yield return new WaitForSecondsRealtime(cachedStateLength * animationSpeed);
            else
                yield return new WaitForSeconds(cachedStateLength * animationSpeed);

            for (var i = 0; i < panels.Count; i++)
            {
                if (i == currentPanelIndex)
                    continue;

                panels[i].panelObject.gameObject.SetActive(false);
            }
        }

        private IEnumerator DisableAnimators()
        {
            if (updateMode == UpdateMode.UnscaledTime)
                yield return new WaitForSecondsRealtime(cachedStateLength * animationSpeed);
            else
                yield return new WaitForSeconds(cachedStateLength * animationSpeed);

            if (currentPanel != null) currentPanel.enabled = false;
            if (nextPanel != null) nextPanel.enabled = false;
        }

        // Events
        [Serializable]
        public class PanelChangeCallback : UnityEvent<int>
        {
        }

        [Serializable]
        public class PanelItem
        {
            [Tooltip("[Required] This is the variable that you use to call specific panels.")]
            public string panelName = "My Panel";

            [Tooltip("[Required] Main panel object.")]
            public Animator panelObject;

            [Tooltip(
                "[Optional] If you want the panel manager to have tabbing capability, you can assign a panel button here.")]
            public PanelButton panelButton;

            [Tooltip(
                "[Optional] Alternate panel button variable that supports standard buttons instead of panel buttons.")]
            public ButtonManager altPanelButton;

            [Tooltip("[Optional] Alternate panel button variable that supports box buttons instead of panel buttons.")]
            public BoxButtonManager altBoxButton;

            [Tooltip(
                "[Optional] This is the object that will be selected as the current UI object on panel activation. Useful for gamepad navigation.")]
            public GameObject firstSelected;

            [Tooltip(
                "[Optional] Enables or disables child hotkeys depending on the panel state to avoid conflict between hotkeys.")]
            public Transform hotkeyParent;

            [Tooltip("Enable or disable panel navigation when using the 'Previous' or 'Next' methods.")]
            public bool disableNavigation;

            [HideInInspector] public GameObject latestSelected;
            [HideInInspector] public HotkeyEvent[] hotkeys;
        }
    }
}