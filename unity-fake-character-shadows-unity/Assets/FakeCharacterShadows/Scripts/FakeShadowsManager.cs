using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;

namespace FakeShadows
{
    public class FakeShadowsManager : MonoBehaviour
    {
        private const int LAYER_LIGHTS = 6; // "LocalPlayerOnly"

        private static readonly Vector3 PLAYER_POSITION_OFFSET = new Vector3(0f, 0.5f, 0f);
        private static readonly Vector4 DEFAULT_LIGHT_VALUE = new Vector4(0f, 0f, 0f, 1f);
        private static readonly int PROP_LIGHTS_ARRAY = Shader.PropertyToID("_FakeShadowsLights");
        private static readonly int PROP_PLAYER_SHADOW_POSITION = Shader.PropertyToID("_PlayerPosition");
        private static readonly int PROP_FAKE_SHADOWS_ARGS = Shader.PropertyToID("_FakeShadowsArgs");

        [Header("Settings")]
        [SerializeField] private GameObject[] _lightsRoots;
        [SerializeField] private GameObject _fakeShadowsLightsRoot;
        [SerializeField] private float _colliderRadiusMultiplier = 0.75f;

        [Header("Runtime Data")]
        [SerializeField] private float _playerRadius = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _shadowStrength = 0.7f;
        private Vector4[] _lightsData = new Vector4[4];
        private Collider[] _overlappedLights = new Collider[0];

        [Header("Debug")]
        [SerializeField] private Transform _testTransform;
        private bool _isShadowsEnabled = true;

        private void Awake()
        {
            for (int i = 0; i < 4; i++)
            {
                _lightsData[i] = DEFAULT_LIGHT_VALUE;
            }
#if !UNITY_EDITOR
            if (_testTransform != null)
                _testTransform.gameObject.SetActive(false);
#endif
        }

        public void SetEnableShadows(bool enable)
        {
            _isShadowsEnabled = enable;
        }

        private void FixedUpdate()
        {
            Vector3 playerPosition = Vector3.zero;
            if (SpatialBridge.actorService != null && SpatialBridge.actorService.localActor != null)
                playerPosition = SpatialBridge.actorService.localActor.avatar.position;
#if UNITY_EDITOR
            if (_testTransform != null)
                playerPosition = _testTransform.position;
#endif
            Collider[] overlappedLights = Physics.OverlapSphere(playerPosition, 0.5f, 1 << LAYER_LIGHTS, QueryTriggerInteraction.Collide);
            // Assume adding new light and removing old one at the same time happens rarely.
            if (_overlappedLights.Length != overlappedLights.Length)
            {
                _overlappedLights = overlappedLights;
                if (overlappedLights.Length > 4) // if more than 4, sort by distance
                {
                    System.Array.Sort(overlappedLights, (x, y) => Vector3.Distance(x.transform.position, playerPosition).CompareTo(Vector3.Distance(y.transform.position, playerPosition)));
                }
                for (int i = 0; i < 4; i++)
                {
                    if (i < overlappedLights.Length)
                    {
                        Vector3 position = overlappedLights[i].transform.position;
                        float radius = overlappedLights[i].transform.rotation.eulerAngles.x;
                        _lightsData[i] = new Vector4(position.x, position.y, position.z, radius);
                    }
                    else
                    {
                        _lightsData[i] = DEFAULT_LIGHT_VALUE;
                    }
                }
            }

            Shader.SetGlobalVectorArray(PROP_LIGHTS_ARRAY, _lightsData);
            Shader.SetGlobalVector(PROP_PLAYER_SHADOW_POSITION, playerPosition + PLAYER_POSITION_OFFSET);
            Shader.SetGlobalVector(PROP_FAKE_SHADOWS_ARGS, new Vector4(_playerRadius, _shadowStrength * (_isShadowsEnabled ? 1 : 0), 0, 0));
        }

#if UNITY_EDITOR
        [ContextMenu(nameof(GenerateFakeShadowsLights))]
        public void GenerateFakeShadowsLights()
        {
            gameObject.layer = LAYER_LIGHTS;

            List<Light> pointLights = new List<Light>();
            foreach (var root in _lightsRoots)
            {
                foreach (var light in root.GetComponentsInChildren<Light>())
                {
                    if (light.type == LightType.Point && light.gameObject.activeSelf)
                    {
                        pointLights.Add(light);
                    }
                }
            }

            // Remove children gameobjects
            foreach (Transform child in _fakeShadowsLightsRoot.transform)
            {
                DestroyImmediate(child.gameObject);
            }

            for (int i = 0; i < pointLights.Count; i++)
            {
                Vector3 lightPos = pointLights[i].transform.position;
                float lightRadius = pointLights[i].range;

                // Generate collider Objects (To send light indices to shader)
                GameObject fakeShadowsLight = new GameObject("FakeShadowsLight_" + pointLights[i].name);
                fakeShadowsLight.transform.SetParent(_fakeShadowsLightsRoot.transform);
                // store radius in rotation.x
                fakeShadowsLight.transform.SetPositionAndRotation(lightPos, Quaternion.Euler(lightRadius, 0, 0));
                fakeShadowsLight.layer = LAYER_LIGHTS;

                SphereCollider collider = fakeShadowsLight.AddComponent<SphereCollider>();
                collider.radius = lightRadius * _colliderRadiusMultiplier;
                collider.isTrigger = true;
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(FakeShadowsManager))]
    public class FakeShadowsManagerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            UnityEditor.EditorGUILayout.Space(10f);

            FakeShadowsManager manager = (FakeShadowsManager)target;

            if (GUILayout.Button(nameof(manager.GenerateFakeShadowsLights)))
            {
                manager.GenerateFakeShadowsLights();
            }
        }
    }
#endif
}
