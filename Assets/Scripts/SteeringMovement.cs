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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {
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
        agent.SetDestination(location);
    }
    void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - this.transform.position;
        agent.SetDestination(this.transform.position - fleeVector);
    }
    void Pursue()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;
        float lookAhead = targetDir.magnitude / (agent.speed + target.GetComponent<Drive>().currentSpeed);

        float toTarget = Vector3.Angle(this.transform.forward, this.transform.TransformVector(targetDir));

        if (toTarget > 90 || target.GetComponent<Drive>().currentSpeed < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }
        Seek(target.transform.position + target.transform.forward * lookAhead * 10);

    }
    void Evade()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;
        float lookAhead = targetDir.magnitude / (agent.speed + target.GetComponent<Drive>().currentSpeed);
        Flee(target.transform.position + target.transform.forward * lookAhead * 10);
    }
    void Wander()
    {
        Vector3 wanderTarget = Vector3.zero;
        float wanderRadius = 10;
        float wanderDistance = 20;
        float wanderJitter = 5;

        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter , 0, Random.Range(-1.0f, 1.0f) * wanderJitter);
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0,0, wanderDistance);
        Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);
        Seek(targetWorld);

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
