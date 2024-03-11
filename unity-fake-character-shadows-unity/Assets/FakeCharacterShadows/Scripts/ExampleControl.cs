using UnityEngine;

[ExecuteInEditMode]
public class ExampleControl : MonoBehaviour
{
    private static readonly int PROP_PLAYER_POSITION = Shader.PropertyToID("_PlayerPosition");
    private static readonly int PROP_PLAYER_RADIUS = Shader.PropertyToID("_PlayerRadius");
    private static readonly int PROP_LIGHT_POSITION = Shader.PropertyToID("_LightPosition");
    private static readonly int PROP_LIGHT_RADIUS = Shader.PropertyToID("_LightRadius");
    [SerializeField] private Material _material;

    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _lightTransform;
    [SerializeField] private float _playerRadius = 0.5f;
    [SerializeField] private float _lightRadius = 10f;

    private void Update()
    {
        if (_material != null && _playerTransform != null && _lightTransform != null)
        {
            _material.SetVector(PROP_PLAYER_POSITION, _playerTransform.position);
            _material.SetFloat(PROP_PLAYER_RADIUS, _playerRadius);
            _material.SetVector(PROP_LIGHT_POSITION, _lightTransform.position);
            _material.SetFloat(PROP_LIGHT_RADIUS, _lightRadius);
        }
    }
}