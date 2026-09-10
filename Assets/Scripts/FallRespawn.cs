using UnityEngine;
using UnityEngine.AI;

public class FallRespawn : MonoBehaviour
{
    public float fallLimitY = -10f;

    Vector3 startPosition;
    Quaternion startRotation;
    CharacterController controller;
    NavMeshAgent agent;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        controller = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (transform.position.y < fallLimitY)
            Respawn();
    }

    void Respawn()
    {
        if (controller != null)
            controller.enabled = false;

        transform.position = startPosition;
        transform.rotation = startRotation;

        if (agent != null)
            agent.Warp(startPosition);

        if (controller != null)
            controller.enabled = true;
    }
}
