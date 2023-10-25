using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OneWayBoxCollider : MonoBehaviour
{
    private BoxCollider _boxCollider = null;
    private BoxCollider _collisionCheckTrigger = null;

    [SerializeField] private Vector3 _entryDirection = Vector3.up;
    [SerializeField] private bool _isLocalDirection = false;

    [SerializeField] private Vector3 _triggerScale = Vector3.one * 1.25f;
    [SerializeField] private float _penetrationDepthThreshold = 0.2f;

    public Vector3 PassthroughDirection => _isLocalDirection ? transform.TransformDirection(_entryDirection.normalized) : _entryDirection.normalized;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();

        InitializeCollisionCheckTrigger();
    }

    private void InitializeCollisionCheckTrigger()
    {
        _collisionCheckTrigger = gameObject.AddComponent<BoxCollider>();
        _collisionCheckTrigger.isTrigger = true;

        _collisionCheckTrigger.size = new Vector3(
            _boxCollider.size.x * _triggerScale.x,
            _boxCollider.size.y * _triggerScale.y,
            _boxCollider.size.z * _triggerScale.z
        );
        _collisionCheckTrigger.center = _boxCollider.center;
    }

    private void OnValidate()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _boxCollider.isTrigger = false;
    }

    private void OnTriggerStay(Collider other)
    {
        TryIgnoreCollision(other);
    }

    public void TryIgnoreCollision(Collider other)
    {
        if (Physics.ComputePenetration(
            _collisionCheckTrigger, _collisionCheckTrigger.bounds.center, transform.rotation,
            other, other.transform.position, other.transform.rotation,
            out Vector3 collisionDirection, out float penetrationDepth))
        {
            bool isSimilarDirection = Vector3.Dot(PassthroughDirection, collisionDirection) >= 0;
            if (isSimilarDirection)
            {
                Physics.IgnoreCollision(_boxCollider, other, true);
                return;
            }

            if (penetrationDepth < _penetrationDepthThreshold)
            {
                Physics.IgnoreCollision(_boxCollider, other, false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.TransformPoint(_boxCollider.center), PassthroughDirection * 2);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.TransformPoint(_boxCollider.center), -PassthroughDirection * 2);
    }
}
