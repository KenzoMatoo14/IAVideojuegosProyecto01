using UnityEngine;

// Thief state machine:
// - Evade cops/player by default.
// - Seek the nearest pedestrian once one enters range.
// - Flee (panic, no prediction) takes priority over both when a threat gets too close.
[RequireComponent(typeof(SteeringMovement))]
public class ThiefAI : MonoBehaviour
{
    [Header("Pedestrians (target to rob)")]
    public string pedestrianTag = "Civilian";
    public float seekRadius = 25f;

    [Header("Threats (police / player)")]
    public string[] threatTags = { "Police", "Player" };
    public float evadeRadius = 35f;
    public float panicRadius = 10f;

    [Header("Detection")]
    public float detectionInterval = 0.2f;

    [Header("Catching pedestrians")]
    [Tooltip("How close a pedestrian needs to be to get caught. NavMeshAgent movement never triggers CharacterController collisions, so this needs its own proximity-based catch (same as PoliceAI).")]
    public float catchRadius = 1.5f;
    public float explosionSpeed = 5f;
    public int particleCount = 30;

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

        // Keep fleeing/evading the same threat, or chasing the same pedestrian,
        // while it's still in range instead of re-picking the closest one every
        // tick (that flip-flopping between near-equal candidates looked jerky).
        GameObject nearestThreat = AgentPerception.StillValid(steering.target, transform.position, evadeRadius, threatTags)
            ? steering.target
            : AgentPerception.FindClosestWithTags(transform.position, threatTags, evadeRadius, gameObject, out _);
        float threatDistance = nearestThreat != null ? Vector3.Distance(transform.position, nearestThreat.transform.position) : Mathf.Infinity;

        if (nearestThreat != null && threatDistance <= panicRadius)
        {
            steering.target = nearestThreat;
            steering.state = SteeringState.Flee;
            return;
        }

        GameObject nearestPedestrian = AgentPerception.StillValid(steering.target, transform.position, seekRadius, pedestrianTag)
            ? steering.target
            : AgentPerception.FindClosestWithTag(transform.position, pedestrianTag, seekRadius, gameObject, out _);
        if (nearestPedestrian != null)
        {
            if (Vector3.Distance(transform.position, nearestPedestrian.transform.position) <= catchRadius)
            {
                CatchPedestrian(nearestPedestrian);
                return;
            }

            steering.target = nearestPedestrian;
            steering.state = SteeringState.Seek;
            return;
        }

        if (nearestThreat != null)
        {
            steering.target = nearestThreat;
            steering.state = SteeringState.Evade;
            return;
        }

        steering.state = SteeringState.Wander;
    }

    void CatchPedestrian(GameObject pedestrian)
    {
        ExplosionEffect.Spawn(pedestrian.transform.position, explosionSpeed, particleCount);
        Destroy(pedestrian);

        steering.target = null;
        steering.state = SteeringState.Evade;
    }

    // Select a thief in the Scene view to see its detection ranges and confirm
    // whether a nearby civilian/cop is actually inside them.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, seekRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, evadeRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, panicRadius);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.2f, GetComponent<SteeringMovement>().state.ToString());
#endif
    }
}
