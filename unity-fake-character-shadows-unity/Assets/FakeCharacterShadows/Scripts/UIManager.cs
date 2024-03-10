using UnityEngine;
using UnityEngine.UI;

namespace FakeShadows
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Toggle _toggleShadows;
        [SerializeField] private Slider _sliderRadius;
        [SerializeField] private Text _sliderRadiusText;
        [SerializeField] private Slider _sliderStrength;
        [SerializeField] private Text _sliderStrengthText;
        [SerializeField] private FakeShadowsManager _fakeShadowsManager;
        private void Awake()
        {
            _toggleShadows.onValueChanged.AddListener(OnShadowToggle);
            _sliderRadius.onValueChanged.AddListener(OnSliderRadiusChanged);
            _sliderStrength.onValueChanged.AddListener(OnSliderStrengthChanged);
            _sliderRadius.value = _fakeShadowsManager.playerRadius;
            _sliderStrength.value = _fakeShadowsManager.shadowStrength;
        }
        private void OnDestroy()
        {
            _toggleShadows.onValueChanged.RemoveListener(OnShadowToggle);
            _sliderRadius.onValueChanged.RemoveListener(OnSliderRadiusChanged);
            _sliderStrength.onValueChanged.RemoveListener(OnSliderStrengthChanged);
        }

        private void OnShadowToggle(bool isOn)
        {
            _fakeShadowsManager.SetEnableShadows(isOn);
        }

        private void OnSliderRadiusChanged(float value)
        {
            _fakeShadowsManager.playerRadius = value;
            _sliderRadiusText.text = "Shadow Radius: " + value.ToString("F2");
        }

        private void OnSliderStrengthChanged(float value)
        {
            _fakeShadowsManager.shadowStrength = value;
            _sliderStrengthText.text = "Shadow Strength: " + value.ToString("F2");
        }
    }
}
