using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Michsky.UI.Heat
{
    [RequireComponent(typeof(TMP_InputField))]
    public class SliderInput : MonoBehaviour
    {
        [Header("Resources")] [SerializeField] private SliderManager sliderManager;

        [SerializeField] private TMP_InputField inputField;

        [Header("Settings")] [SerializeField] private bool multiplyValue;

        [Range(1, 10)] public int maxChar = 5;
        [Range(0, 4)] public int decimals = 1;

        [Header("Events")] public UnityEvent onSubmit;

        private void Awake()
        {
            if (sliderManager == null)
            {
                Debug.LogWarning("'Slider Manager' is missing!");
                return;
            }

            if (inputField == null) inputField = GetComponent<TMP_InputField>();

            if (sliderManager.mainSlider.wholeNumbers)
            {
                inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            }
            else if (sliderManager.mainSlider.wholeNumbers)
            {
                inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
                decimals = 0;
            }

            inputField.characterLimit = maxChar;
            inputField.selectionColor = new Color(inputField.textComponent.color.r, inputField.textComponent.color.g,
                inputField.textComponent.color.b, inputField.selectionColor.a);
            inputField.onDeselect.AddListener(delegate { SetText(sliderManager.mainSlider.value); });
            onSubmit.AddListener(SetValue);

            sliderManager.mainSlider.onValueChanged.AddListener(SetText);
            sliderManager.mainSlider.onValueChanged.Invoke(sliderManager.mainSlider.value);
        }

        private void Update()
        {
            if (string.IsNullOrEmpty(inputField.text) ||
                EventSystem.current.currentSelectedGameObject != inputField.gameObject) return;
            if (Keyboard.current.enterKey.wasPressedThisFrame) onSubmit.Invoke();
        }

        private void SetText(float value)
        {
            if (multiplyValue) value = value * 100;

            if (decimals == 0)
                inputField.text = value.ToString("F0");
            else if (decimals == 1)
                inputField.text = value.ToString("F1");
            else if (decimals == 2)
                inputField.text = value.ToString("F2");
            else if (decimals == 3)
                inputField.text = value.ToString("F3");
            else if (decimals == 4) inputField.text = value.ToString("F4");
        }

        private void SetValue()
        {
            if (sliderManager.mainSlider.wholeNumbers)
                sliderManager.mainSlider.value = int.Parse(inputField.text);
            else if (multiplyValue)
                sliderManager.mainSlider.value = float.Parse(inputField.text) / 100;
            else
                sliderManager.mainSlider.value = float.Parse(inputField.text);

            if (multiplyValue == false && float.Parse(inputField.text) > sliderManager.mainSlider.maxValue)
                sliderManager.mainSlider.value = sliderManager.mainSlider.maxValue;
            else if (multiplyValue && float.Parse(inputField.text) / 100 > sliderManager.mainSlider.maxValue)
                sliderManager.mainSlider.value = sliderManager.mainSlider.maxValue;
        }
    }
}