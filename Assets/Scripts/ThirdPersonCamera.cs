using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3.5f, -6f);
    public float followSmoothTime = 0.12f;
    public float rotationSmoothSpeed = 10f;
    public float collisionRadius = 0.3f;
    public LayerMask collisionMask = ~0;

    Vector3 currentVelocity;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // Avoid clipping through walls/ramps: pull the camera closer if something blocks the view
        Vector3 castOrigin = target.position + Vector3.up * offset.y;
        Vector3 castDirection = desiredPosition - castOrigin;
        float castDistance = castDirection.magnitude;

        if (castDistance > 0.01f && Physics.SphereCast(castOrigin, collisionRadius, castDirection.normalized, out RaycastHit hit, castDistance, collisionMask, QueryTriggerInteraction.Ignore))
        {
            desiredPosition = castOrigin + castDirection.normalized * hit.distance;
        }

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, followSmoothTime);

        Quaternion desiredRotation = Quaternion.LookRotation((target.position + Vector3.up * offset.y * 0.5f) - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}
