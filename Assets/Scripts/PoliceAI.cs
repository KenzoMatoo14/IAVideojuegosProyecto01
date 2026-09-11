using UnityEngine;

// Police state machine: Wander by default, Seek when a thief gets close.
[RequireComponent(typeof(SteeringMovement))]
public class PoliceAI : MonoBehaviour
{
    [Header("Detection")]
    public string thiefTag = "Thief";
    public float seekRadius = 12f;
    public float detectionInterval = 0.2f;

    [Header("Catching")]
    [Tooltip("How close a thief needs to be to get caught. NavMeshAgent movement never triggers CharacterController collisions (that's how ThiefCatcher works for the player), so AI cops need their own proximity-based catch.")]
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

        // Keep chasing the same thief while it's still in range instead of
        // re-picking the closest one every tick (that flip-flopping looked jerky).
        GameObject nearestThief = AgentPerception.StillValid(steering.target, transform.position, seekRadius, thiefTag)
            ? steering.target
            : AgentPerception.FindClosestWithTag(transform.position, thiefTag, seekRadius, gameObject, out _);

        if (nearestThief != null)
        {
            if (Vector3.Distance(transform.position, nearestThief.transform.position) <= catchRadius)
            {
                CatchThief(nearestThief);
                return;
            }

            steering.target = nearestThief;
            steering.state = SteeringState.Seek;
        }
        else
        {
            steering.state = SteeringState.Wander;
        }
    }

    void CatchThief(GameObject thief)
    {
        ExplosionEffect.Spawn(thief.transform.position, explosionSpeed, particleCount);
        Destroy(thief);

        if (ThiefManager.Instance != null)
            ThiefManager.Instance.ThiefCaught();

        steering.target = null;
        steering.state = SteeringState.Wander;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, seekRadius);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.2f, GetComponent<SteeringMovement>().state.ToString());
#endif
    }
}
