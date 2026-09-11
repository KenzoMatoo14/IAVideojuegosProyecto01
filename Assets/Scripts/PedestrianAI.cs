using UnityEngine;

// Pedestrian state machine: Wander by default, Flee when a thief gets close.
[RequireComponent(typeof(SteeringMovement))]
public class PedestrianAI : MonoBehaviour
{
    [Header("Detection")]
    public string thiefTag = "Thief";
    public float fleeRadius = 20f;
    public float detectionInterval = 0.2f;

    SteeringMovement steering;
    float timer;

    void Awake()
    {
        steering = GetComponent<SteeringMovement>();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = detectionInterval;

        // Keep fleeing the same thief while it's still in range instead of
        // re-picking the closest one every tick (that flip-flopping looked jerky).
        GameObject nearestThief = AgentPerception.StillValid(steering.target, transform.position, fleeRadius, thiefTag)
            ? steering.target
            : AgentPerception.FindClosestWithTag(transform.position, thiefTag, fleeRadius, gameObject, out _);

        if (nearestThief != null)
        {
            steering.target = nearestThief;
            steering.state = SteeringState.Flee;
        }
        else
        {
            steering.state = SteeringState.Wander;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, fleeRadius);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.2f, GetComponent<SteeringMovement>().state.ToString());
#endif
    }
}
