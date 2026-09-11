using UnityEngine;
using UnityEngine.AI;

public enum SteeringState
{
    Seek,
    Flee,
    Pursue,
    Evade,
    Wander,
    Hide
}

public class SteeringMovement : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;
    public SteeringState state = SteeringState.Wander;

    [Header("Wander settings")]
    public float wanderRadius = 10f;
    public float wanderDistance = 20f;
    public float wanderJitter = 5f;
    [Tooltip("How long to commit to an escape heading after Wander/Flee gets blocked by a wall, before re-checking the normal path. Too short and it flip-flops (jerky); too long and it ignores nearby obstacles longer than needed.")]
    public float wanderRecoveryDuration = 1.5f;

    [Header("Flee settings")]
    [Tooltip("How far away from the threat Flee/Evade tries to move in one go.")]
    public float fleeDistance = 10f;

    [Header("Movement smoothing")]
    [Tooltip("Only re-issue a destination when it moved at least this far from the current one, to avoid constant NavMesh re-planning (jerky movement).")]
    public float destinationThreshold = 1f;

    Vector3 wanderTarget;
    Vector3 recoveryDestination;
    float recoveryTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderTarget = this.transform.forward * wanderRadius;
    }
    void Update()
    {
        // The AI scripts only re-validate their target every detectionInterval
        // (to save perf), but that target can get Destroyed by someone else
        // (caught by another agent) on any frame in between. Fall back to
        // wandering rather than crashing on a dead reference until the AI script's
        // next tick picks a new target.
        bool needsTarget = state == SteeringState.Seek || state == SteeringState.Flee
            || state == SteeringState.Pursue || state == SteeringState.Evade;

        if (needsTarget && target == null)
        {
            Wander();
            return;
        }

        switch (state)
        {
            case SteeringState.Seek:
                Seek(target.transform.position);
                break;
            case SteeringState.Flee:
                Flee(target.transform.position);
                break;
            case SteeringState.Pursue:
                Pursue();
                break;
            case SteeringState.Evade:
                Evade();
                break;
            case SteeringState.Wander:
                Wander();
                break;
            case SteeringState.Hide:
                Hide();
                break;
        }
    }

    void Seek(Vector3 location)
    {
        SetDestinationThrottled(location);
    }
    void Flee(Vector3 threatPosition)
    {
        // Same commitment mechanism as Wander: if we're mid-escape from being
        // cornered, stick with that heading instead of re-rolling a new random
        // direction every frame.
        if (recoveryTimer > 0f)
        {
            recoveryTimer -= Time.deltaTime;
            Seek(recoveryDestination);
            return;
        }

        Vector3 awayFromThreat = (this.transform.position - threatPosition).normalized;

        // Straight away from the threat is the natural choice, but near a map
        // edge/wall that point can be off the NavMesh - the agent would clamp to
        // the edge and just sit there while the threat keeps closing in.
        if (TryFleeDirection(awayFromThreat, out Vector3 destination))
        {
            SetDestinationThrottled(destination);
            return;
        }

        // Blocked going straight back - try sliding sideways along whatever is
        // behind us instead of pushing uselessly into it.
        Vector3 sideways = Vector3.Cross(Vector3.up, awayFromThreat);
        if (TryFleeDirection(sideways, out destination) || TryFleeDirection(-sideways, out destination))
        {
            SetDestinationThrottled(destination);
            return;
        }

        // Cornered on every side - just go anywhere reachable instead of freezing.
        PickNewWanderHeading();
    }

    bool TryFleeDirection(Vector3 direction, out Vector3 destination)
    {
        Vector3 candidate = this.transform.position + direction * fleeDistance;
        return TryResolveDestination(candidate, fleeDistance, out destination);
    }

    // A point can pass NavMesh.SamplePosition (it lies on *some* NavMesh polygon
    // within range) without the agent actually being able to walk there - e.g. it's
    // on the far side of a wall corner with no connected path. Confirming a full
    // path here is what fixes agents getting stuck aiming at "valid" but
    // unreachable points near walls/corners.
    bool TryResolveDestination(Vector3 candidatePoint, float searchRadius, out Vector3 destination)
    {
        if (NavMesh.SamplePosition(candidatePoint, out NavMeshHit hit, searchRadius, NavMesh.AllAreas)
            && (hit.position - this.transform.position).sqrMagnitude > destinationThreshold * destinationThreshold
            && HasCompletePath(hit.position))
        {
            destination = hit.position;
            return true;
        }

        destination = Vector3.zero;
        return false;
    }

    bool HasCompletePath(Vector3 destination)
    {
        if (!agent.isOnNavMesh) return false;

        NavMeshPath path = new NavMeshPath();
        return agent.CalculatePath(destination, path) && path.status == NavMeshPathStatus.PathComplete;
    }

    // Skip re-planning the path for tiny changes in destination; this is what
    // caused agents to jerk around instead of moving smoothly. Both Seek and Flee
    // go through here (Flee used to bypass this and jerk every frame).
    void SetDestinationThrottled(Vector3 location)
    {
        if ((location - agent.destination).sqrMagnitude < destinationThreshold * destinationThreshold)
            return;

        agent.SetDestination(location);
    }
    void Pursue()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;
        float targetSpeed = GetTargetSpeed(target);
        float lookAhead = targetDir.magnitude / (agent.speed + targetSpeed);

        float toTarget = Vector3.Angle(this.transform.forward, this.transform.TransformVector(targetDir));

        if (toTarget > 90 || targetSpeed < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }
        Seek(target.transform.position + target.transform.forward * lookAhead * 10);

    }
    void Evade()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;
        float targetSpeed = GetTargetSpeed(target);
        float lookAhead = targetDir.magnitude / (agent.speed + targetSpeed);
        Flee(target.transform.position + target.transform.forward * lookAhead * 10);
    }

    // Pursue/Evade need the target's current speed to predict where it's heading.
    // The player (Drive) and AI agents (NavMeshAgent) expose speed differently.
    float GetTargetSpeed(GameObject targetObject)
    {
        Drive drive = targetObject.GetComponent<Drive>();
        if (drive != null)
            return drive.currentSpeed;

        NavMeshAgent otherAgent = targetObject.GetComponent<NavMeshAgent>();
        if (otherAgent != null)
            return otherAgent.velocity.magnitude;

        return 0f;
    }

    void Wander()
    {
        // While recovering from a blocked wander point (see PickNewWanderHeading),
        // commit to that escape heading for a while instead of re-checking the
        // normal forward-facing point every frame - the agent needs time to actually
        // turn away from the wall first, otherwise this re-triggers every frame with
        // a brand new random direction each time, which is its own kind of jerk.
        if (recoveryTimer > 0f)
        {
            recoveryTimer -= Time.deltaTime;
            Seek(recoveryDestination);
            return;
        }

        // Nudge the existing wander point instead of picking a brand new random
        // point every frame - that full randomization was the other cause of the
        // jerky movement (destination flipping direction every frame).
        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter, 0, Random.Range(-1.0f, 1.0f) * wanderJitter) * Time.deltaTime;
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0,0, wanderDistance);
        Vector3 targetWorld = this.transform.TransformPoint(targetLocal);

        // The wander point is always projected in front of the agent. Near a wall or
        // corner that point can land off the NavMesh (or on a NavMesh polygon with no
        // real path back to the agent), so give up on the forward-facing point and
        // pick a fresh heading instead when it isn't actually reachable.
        if (TryResolveDestination(targetWorld, wanderRadius, out Vector3 wanderDestination))
        {
            Seek(wanderDestination);
        }
        else
        {
            PickNewWanderHeading();
        }
    }

    void PickNewWanderHeading()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float searchRadius = wanderRadius + wanderDistance;
        Vector3 randomPoint = this.transform.position + new Vector3(randomDirection.x, 0, randomDirection.y) * searchRadius;

        // Only commit (and start the recovery timer) if this random direction is an
        // actual, reachable step away from the current position. Otherwise just try
        // again next frame with a fresh direction instead of freezing on a useless one.
        if (TryResolveDestination(randomPoint, searchRadius, out Vector3 destination))
        {
            recoveryDestination = destination;
            recoveryTimer = wanderRecoveryDuration;
            Seek(recoveryDestination);
            wanderTarget = this.transform.InverseTransformPoint(destination) - new Vector3(0, 0, wanderDistance);
        }
    }
    void Hide()
    {
        float closestDistance = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        GameObject[] hidingSpots = getHidingSpots("hide");

        for (int i = 0; i < hidingSpots.Length; i++)
        {
            Vector3 hideDirection = hidingSpots[i].transform.position - target.transform.position;
            Vector3 hidePosition = hidingSpots[i].transform.position + hideDirection.normalized * 5;

            if (Vector3.Distance(this.transform.position, hidePosition) < closestDistance)
            {
                closestDistance = Vector3.Distance(this.transform.position, hidePosition);
                chosenSpot = hidePosition;
            }
        }

        Seek(chosenSpot);
    }

    private static GameObject[] getHidingSpots(string tag)
    {
        GameObject[] hidingSpots = GameObject.FindGameObjectsWithTag(tag);
        return hidingSpots;
    }
}
