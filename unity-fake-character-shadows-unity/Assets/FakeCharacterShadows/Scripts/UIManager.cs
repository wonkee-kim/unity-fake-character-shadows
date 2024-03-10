using UnityEngine;
using UnityEngine.UI;

namespace FakeShadows
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Toggle _toggleShadows;
        [SerializeField] private FakeShadowsManager _fakeShadowsManager;
        private void Awake()
        {
            _toggleShadows.onValueChanged.AddListener(OnShadowToggle);
        }
        private void OnDestroy()
        {
            _toggleShadows.onValueChanged.RemoveListener(OnShadowToggle);
        }
        private void OnShadowToggle(bool isOn)
        {
            _fakeShadowsManager.SetEnableShadows(isOn);
        }
    }
}
