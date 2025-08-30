using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScooterCameraController : MonoBehaviour
{
    public Transform scooterPreset; // Reference to the scooter's transform
    public Vector3 offset = new Vector3(5.0f, -5.0f, 5.0f); // Offset from the scooter's position
    public float smoothSpeed = 0.125f; // Speed of the camera smoothing

    private void LateUpdate()
    {
        // Calculate the desired position directly in local space
        Vector3 desiredPosition = scooterPreset.TransformPoint(offset);

        // Smooth the camera movement
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Make the camera follow the scooter's rotation
        transform.rotation = scooterPreset.rotation;
    }
}

