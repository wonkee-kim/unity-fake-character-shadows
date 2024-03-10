using UnityEngine;
public class DemoSelfRotation : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 2f;
    [SerializeField] private float _rotationRadius = 3f;
    private void Update()
    {
        float radiusOffset = Mathf.Cos(Time.time * _rotationSpeed * 0.5f) * 0.3f + 0.7f;
        float x = Mathf.Cos(Time.time * _rotationSpeed) * _rotationRadius * radiusOffset;
        float z = Mathf.Sin(Time.time * _rotationSpeed) * _rotationRadius * radiusOffset;
        transform.position = new Vector3(x, transform.position.y, z);
    }
}