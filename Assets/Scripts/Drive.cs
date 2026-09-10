using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Drive : MonoBehaviour
{
    public float speed = 10.0f;
    public float rotationSpeed = 100.0f;
    public float currentSpeed = 0;
    public float gravity = -20f;

    CharacterController controller;
    float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Get the horizontal and vertical axis.
        // By default they are mapped to the arrow keys.
        // The value is in the range -1 to 1
        float translation = Input.GetAxis("Vertical") * speed;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;

        currentSpeed = translation;

        // Rotate around our y-axis
        transform.Rotate(0, rotation * Time.deltaTime, 0);

        // Keep the controller grounded on slopes/ramps, otherwise apply gravity
        if (controller.isGrounded)
            verticalVelocity = -0.5f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = transform.forward * translation + Vector3.up * verticalVelocity;
        controller.Move(move * Time.deltaTime);
    }
}
