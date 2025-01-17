using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public float followSpeed = 10f;
    public Vector3 offset = new Vector3(0f, 2f, -5f);

    [Header("Collision Settings")]
    public float collisionRadius = 0.2f;
    public LayerMask collisionLayer;

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position based on target's rotation
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // Handle collision
        Vector3 direction = (desiredPosition - target.position).normalized;
        float targetDistance = Vector3.Distance(target.position, desiredPosition);
        if (Physics.SphereCast(target.position, collisionRadius, direction, 
            out RaycastHit hit, targetDistance, collisionLayer))
        {
            desiredPosition = target.position + direction * (hit.distance - collisionRadius);
        }

        // Smooth camera movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, 
                                        followSpeed * Time.deltaTime);
        
        // Make camera look at target
        transform.LookAt(target.position + Vector3.up);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
} 