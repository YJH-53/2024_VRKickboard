using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonSpawn : MonoBehaviour
{
    public Transform spawnPoint;
    public Transform disappearPoint;
    public float moveSpeed = 2f;
    public float stopDistance = 1.1f;
    public Transform kyle; // Reference to Kyle's transform
    public Rigidbody sphereRigidbody; // Reference to the Rigidbody on the invisible sphere

    private Animator kyleAnimator;
    private Collider kyleCollider;
    private Vector3 kyleOffset; // Offset to ensure Kyle's position relative to the sphere is correct

    void Start()
    {
        if (kyle == null || sphereRigidbody == null)
        {
            Debug.LogError("Kyle or Sphere Rigidbody is not assigned.");
            return;
        }

        // Calculate the offset between Kyle and the sphere
        kyleOffset = kyle.position - sphereRigidbody.position;

        // Set the sphere's position to the spawn point
        sphereRigidbody.position = spawnPoint.position;

        // Set Kyle's initial position and rotation to match the sphere
        kyle.position = sphereRigidbody.position + kyleOffset;

        // Get the Animator component from Kyle
        kyleAnimator = kyle.GetComponent<Animator>();
        kyleCollider = kyle.GetComponent<CapsuleCollider>();
        if(kyleCollider != null){
            kyleCollider.enabled = true;
        }

        // Start the walking animation
        if (kyleAnimator != null)
        {
            kyleAnimator.SetBool("isWalking", true);
        }
    }

    void Update()
    {
        // Move the sphere towards the disappear point
        float step = moveSpeed * Time.deltaTime;
        sphereRigidbody.MovePosition(Vector3.MoveTowards(sphereRigidbody.position, disappearPoint.position, step));

        // Update Kyle's position to follow the sphere, applying the offset
        kyle.position = sphereRigidbody.position + kyleOffset;

        // Dynamically change Kyle's walking direction towards the disappear point
        Vector3 directionToFace = (disappearPoint.position - kyle.position).normalized;
        directionToFace.y = 0; // Keep the rotation on the horizontal plane
        Quaternion targetRotation = Quaternion.LookRotation(directionToFace);
        kyle.rotation = Quaternion.Slerp(kyle.rotation, targetRotation, Time.deltaTime * moveSpeed);

        // Check if Kyle has reached close enough to the disappear point
        if (Vector3.Distance(sphereRigidbody.position, disappearPoint.position) < stopDistance)
        {
            StopWalking();
        }
    }

    void StopWalking()
    {
        // Stop walking animation
        if (kyleAnimator != null)
        {
            kyleAnimator.SetBool("isWalking", false);
        }

        // Optionally, disable Kyle and the sphere instead of destroying them
        kyle.gameObject.SetActive(false);
        sphereRigidbody.gameObject.SetActive(false);

        Debug.Log("Kyle reached the disappear point and has been disabled.");
    }
}
