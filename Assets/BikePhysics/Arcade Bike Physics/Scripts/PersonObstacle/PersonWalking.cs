using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonWalking : MonoBehaviour
{
    public Transform waypointsParent;

    public float moveSpeed = 2f;
    public float stopDistance = 1.1f;
    public float time_threshold = 1.0f;
    public GameObject kyle; 
    public Rigidbody sphereRigidbody; // Reference to the Rigidbody on the invisible sphere
    public SpeedMonitor speedMonitor;
    public PauseMenu pauseMenu;

    [HideInInspector]
    public List<Transform> waypoints = new List<Transform>();
    private bool set_position = false;
    private Animator kyleAnimator;
    private Collider kyleCollider;
    private int currentWayPointIndex = 0;
    private Vector3 kyleOffset; // Offset to ensure Kyle's position relative to the sphere is correct

    void Start()
    {
        if (kyle == null || sphereRigidbody == null)
        {
            Debug.LogError("Kyle or Sphere Rigidbody is not assigned.");
            return;
        }

        foreach (Transform child in waypointsParent)
        {
            waypoints.Add(child);
        }

        // Calculate the offset between Kyle and the sphere
        

        if(kyle.name == "RobotKyleWalk_1"){
            Debug.Log("Kyle Transform: " + kyle.transform.position);
            Debug.Log("SphereRB Transform: "+ sphereRigidbody.position);
        }

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
        if(speedMonitor.zone_num != 6){
            StopWalking();
        }else{
            kyle.gameObject.SetActive(true);
            sphereRigidbody.gameObject.SetActive(true);
            // Debug.Log("Time Difference: " + (Time.time - pauseMenu.showPerson_time));
            if(Time.timeScale != 0f && Time.time - pauseMenu.showPerson_time >= time_threshold){
                // Debug.Log("Waypoints num : " + waypoints.Count);
                kyleAnimator.SetBool("isWalking", true);
                if(currentWayPointIndex == 0 && set_position == false){
                    kyleOffset = kyle.transform.position - sphereRigidbody.position;
                    // Set the sphere's position to the spawn point
                    sphereRigidbody.position = waypoints[0].position;
                    // Set Kyle's initial position and rotation to match the sphere
                    kyle.transform.position = sphereRigidbody.position + kyleOffset;
                    set_position = true;
                }
                if (currentWayPointIndex < waypoints.Count)
                {
                    if(kyle.name == "RobotKyleWalk_1"){
                        Debug.Log("Kyle Transform: " + kyle.transform.position);
                        Debug.Log("SphereRB Transform: "+ sphereRigidbody.position);
                    }
                    // Move the sphere towards the disappear point
                    float step = moveSpeed * Time.deltaTime;
                    sphereRigidbody.MovePosition(Vector3.MoveTowards(sphereRigidbody.position, waypoints[currentWayPointIndex].position, step));

                    // Update Kyle's position to follow the sphere, applying the offset
                    kyle.transform.position = sphereRigidbody.position + kyleOffset;

                    // Dynamically change Kyle's walking direction towards the disappear point
                    Vector3 directionToFace = (waypoints[currentWayPointIndex].position - kyle.transform.position).normalized;
                    directionToFace.y = 0; // Keep the rotation on the horizontal plane
                    Quaternion targetRotation = Quaternion.LookRotation(directionToFace);
                    kyle.transform.rotation = Quaternion.Slerp(kyle.transform.rotation, targetRotation, Time.deltaTime * moveSpeed);

                    // Check if reached the waypoint
                    if (Vector3.Distance(sphereRigidbody.position, waypoints[currentWayPointIndex].position) < 0.1f)
                    {
                        currentWayPointIndex++;
                    }
                }
                else
                {
                    StopWalking();
                }
            }else{
                kyleAnimator.SetBool("isWalking", false);
            }
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

        // Debug.Log("Kyle reached the disappear point and has been disabled.");
    }
}
