using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Michsky.UI.Heat
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Heat UI/Animation/UI Popup")]
    public class UIPopup : MonoBehaviour
    {
        public enum AnimationMode
        {
            Scale,
            Horizontal,
            Vertical
        }

        public enum StartBehaviour
        {
            Default,
            Disabled,
            Static
        }

        public enum UpdateMode
        {
            DeltaTime,
            UnscaledTime
        }

        [Header("Settings")] [SerializeField] private bool playOnEnable = true;

        [SerializeField] private bool closeOnDisable;

        [Tooltip("Skip out animation.")] [SerializeField]
        private bool instantOut;

        [Tooltip("Enables content size fitter mode.")] [SerializeField]
        private bool fitterMode;

        [SerializeField] private StartBehaviour startBehaviour;
        [SerializeField] private UpdateMode updateMode = UpdateMode.UnscaledTime;

        [Header("Animation")] [SerializeField] private AnimationMode animationMode;

        [SerializeField]
        private AnimationCurve animationCurve = new(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));

        [Range(0.5f, 10)] public float curveSpeed = 4f;

        [Header("Events")] public UnityEvent onEnable = new();

        public UnityEvent onVisible = new();
        public UnityEvent onDisable = new();
        [HideInInspector] public bool isOn;
        private CanvasGroup cg;
        private bool isFitterInitialized;
        private bool isInitialized;

        // Helpers
        private RectTransform rect;
        private Vector2 rectHelper;

        private void Start()
        {
            if (startBehaviour == StartBehaviour.Disabled) gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (!isInitialized) Initialize();
            if (playOnEnable)
            {
                isOn = false;
                PlayIn();
            }
        }

        private void OnDisable()
        {
            if (closeOnDisable)
            {
                gameObject.SetActive(false);
                isOn = false;
            }
        }

        private void Initialize()
        {
            if (rect == null) rect = GetComponent<RectTransform>();
            if (cg == null) cg = GetComponent<CanvasGroup>();
            if (startBehaviour == StartBehaviour.Disabled || startBehaviour == StartBehaviour.Static)
                rectHelper = rect.sizeDelta;
            else if (startBehaviour == StartBehaviour.Default && gameObject.activeInHierarchy && !fitterMode)
                isOn = true;

            isInitialized = true;
        }

        public void ResetFitterData()
        {
            isFitterInitialized = false;
        }

        public void Animate()
        {
            if (isOn)
                PlayOut();
            else
                PlayIn();
        }

        public void PlayIn()
        {
            if (isOn)
                return;

            gameObject.SetActive(true);

            if (fitterMode && !isFitterInitialized)
            {
                cg.alpha = 0;
                StartCoroutine("InitFitter");
                return;
            }

            if (animationMode == AnimationMode.Scale && cg != null)
            {
                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine("ScaleIn");
                }
                else
                {
                    cg.alpha = 1;
                    rect.localScale = new Vector3(1, 1, 1);
                }
            }

            else if (animationMode == AnimationMode.Horizontal && cg != null)
            {
                if (gameObject.activeInHierarchy)
                    StartCoroutine("HorizontalIn");
                else
                    cg.alpha = 1;
            }

            else if (animationMode == AnimationMode.Vertical && cg != null)
            {
                if (gameObject.activeInHierarchy)
                    StartCoroutine("VerticalIn");
                else
                    cg.alpha = 1;
            }

            isOn = true;
            onEnable.Invoke();
        }

        public void PlayOut()
        {
            if (!isOn)
                return;

            if (instantOut)
            {
                gameObject.SetActive(false);
                isOn = false;
                return;
            }

            if (animationMode == AnimationMode.Scale && cg != null)
            {
                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine("ScaleOut");
                }
                else
                {
                    cg.alpha = 0;
                    rect.localScale = new Vector3(0, 0, 0);
                    gameObject.SetActive(false);
                }
            }

            else if (animationMode == AnimationMode.Horizontal && cg != null)
            {
                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine("HorizontalOut");
                }
                else
                {
                    cg.alpha = 0;
                    gameObject.SetActive(false);
                }
            }

            else if (animationMode == AnimationMode.Vertical && cg != null)
            {
                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine("VerticalOut");
                }
                else
                {
                    cg.alpha = 0;
                    gameObject.SetActive(false);
                }
            }

            isOn = false;
            onDisable.Invoke();
        }

        private IEnumerator InitFitter()
        {
            if (updateMode == UpdateMode.UnscaledTime)
                yield return new WaitForSecondsRealtime(0.04f);
            else
                yield return new WaitForSeconds(0.04f);

            var csf = GetComponent<ContentSizeFitter>();
            csf.enabled = false;

            rectHelper = rect.sizeDelta;
            isFitterInitialized = true;

            PlayIn();
        }

        private IEnumerator ScaleIn()
        {
            StopCoroutine("ScaleOut");

            float elapsedTime = 0;
            float startingPoint = 0;
            float smoothValue = 0;

            rect.localScale = new Vector3(0, 0, 0);
            cg.alpha = 0;

            while (rect.localScale.x < 0.99)
            {
                if (updateMode == UpdateMode.UnscaledTime)
                    elapsedTime += Time.unscaledDeltaTime;
                else
                    elapsedTime += Time.deltaTime;

                smoothValue = Mathf.Lerp(startingPoint, 1, animationCurve.Evaluate(elapsedTime * curveSpeed));
                rect.localScale = new Vector3(smoothValue, smoothValue, smoothValue);
                cg.alpha = smoothValue;

                yield return null;
            }

            cg.alpha = 1;
            rect.localScale = new Vector3(1, 1, 1);
            onVisible.Invoke();
        }

        private IEnumerator ScaleOut()
        {
            StopCoroutine("ScaleIn");

            float elapsedTime = 0;
            float startingPoint = 1;
            float smoothValue = 0;

            rect.localScale = new Vector3(1, 1, 1);
            cg.alpha = 1;

            while (rect.localScale.x > 0.01)
            {
                if (updateMode == UpdateMode.UnscaledTime)
                    elapsedTime += Time.unscaledDeltaTime;
                else
                    elapsedTime += Time.deltaTime;

                smoothValue = Mathf.Lerp(startingPoint, 0, animationCurve.Evaluate(elapsedTime * curveSpeed));
                rect.localScale = new Vector3(smoothValue, smoothValue, smoothValue);
                cg.alpha = smoothValue;

                yield return null;
            }

            cg.alpha = 0;
            rect.localScale = new Vector3(0, 0, 0);
            gameObject.SetActive(false);
        }

        private IEnumerator HorizontalIn()
        {
            StopCoroutine("HorizontalOut");

            float elapsedTime = 0;

            var startPos = new Vector2(0, rect.sizeDelta.y);
            var endPos = rectHelper;

            if (!fitterMode && startBehaviour == StartBehaviour.Default)
                endPos = rect.sizeDelta;
            else if (fitterMode && startBehaviour == StartBehaviour.Default) endPos = rectHelper;

            rect.sizeDelta = startPos;

            while (rect.sizeDelta.x <= endPos.x - 0.1f)
            {
                if (updateMode == UpdateMode.UnscaledTime)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    cg.alpha += Time.unscaledDeltaTime * (curveSpeed * 2);
                }

                else
                {
                    elapsedTime += Time.deltaTime;
                    cg.alpha += Time.deltaTime * (curveSpeed * 2);
                }

                rect.sizeDelta = Vector2.Lerp(startPos, endPos, animationCurve.Evaluate(elapsedTime * curveSpeed));
                yield return null;
            }

            cg.alpha = 1;
            rect.sizeDelta = endPos;
            onVisible.Invoke();
        }

        private IEnumerator HorizontalOut()
        {
            StopCoroutine("HorizontalIn");

            float elapsedTime = 0;

            var startPos = rect.sizeDelta;
            var endPos = new Vector2(0, rect.sizeDelta.y);

            while (rect.sizeDelta.x >= 0.1f)
            {
                if (updateMode == UpdateMode.UnscaledTime)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    cg.alpha -= Time.unscaledDeltaTime * (curveSpeed * 2);
                }

                else
                {
                    elapsedTime += Time.deltaTime;
                    cg.alpha -= Time.deltaTime * (curveSpeed * 2);
                }

                rect.sizeDelta = Vector2.Lerp(startPos, endPos, animationCurve.Evaluate(elapsedTime * curveSpeed));
                yield return null;
            }

            cg.alpha = 0;
            rect.sizeDelta = endPos;
            rect.gameObject.SetActive(false);
        }

        private IEnumerator VerticalIn()
        {
            StopCoroutine("VerticalOut");

            float elapsedTime = 0;

            var startPos = new Vector2(rect.sizeDelta.x, 0);
            var endPos = rectHelper;

            if (!fitterMode && startBehaviour == StartBehaviour.Default)
                endPos = rect.sizeDelta;
            else if (fitterMode && startBehaviour == StartBehaviour.Default) endPos = rectHelper;

            rect.sizeDelta = startPos;

            while (rect.sizeDelta.y <= endPos.y - 0.1f)
            {
                if (updateMode == UpdateMode.UnscaledTime)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    cg.alpha += Time.unscaledDeltaTime * (curveSpeed * 2);
                }

                else
                {
                    elapsedTime += Time.deltaTime;
                    cg.alpha += Time.deltaTime * (curveSpeed * 2);
                }

                rect.sizeDelta = Vector2.Lerp(startPos, endPos, animationCurve.Evaluate(elapsedTime * curveSpeed));
                yield return null;
            }

            cg.alpha = 1;
            rect.sizeDelta = endPos;
            onVisible.Invoke();
        }

        private IEnumerator VerticalOut()
        {
            StopCoroutine("VerticalIn");

            float elapsedTime = 0;

            var startPos = rect.sizeDelta;
            var endPos = new Vector2(rect.sizeDelta.x, 0);

            while (rect.sizeDelta.y >= 0.1f)
            {
                if (updateMode == UpdateMode.UnscaledTime)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    cg.alpha -= Time.unscaledDeltaTime * (curveSpeed * 2);
                }

                else
                {
                    elapsedTime += Time.deltaTime;
                    cg.alpha -= Time.deltaTime * (curveSpeed * 2);
                }

                rect.sizeDelta = Vector2.Lerp(startPos, endPos, animationCurve.Evaluate(elapsedTime * curveSpeed));
                yield return null;
            }

            cg.alpha = 0;
            rect.sizeDelta = endPos;
            rect.gameObject.SetActive(false);
        }
    }
}