using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//해당 스크립트는 ScooterPreset에 달 것
public class GeneratePlane : MonoBehaviour
{
    public GameObject planePrefab;
    public ArcadeBP.ArcadeBikeController bikeController;
    public Transform townPosition;
    public float detectionDistance = 1000f;
    public float diagonalDistance = 450f;
    private float boundaryDistance = 10f;
    // Plane Width: 235.9738, Plane Height: 220
    public float planeWidth = 230f; // The width of the plane
    public float planeHeight = 215.0f; // The height of the plane
    private float townDistanceThreshold;
    
    private Transform lastGeneratedPlane, currentPlane;
    private Vector3 currentPlanePosition;

    void Start()
    {
        townDistanceThreshold = Mathf.Sqrt(Mathf.Pow(planeWidth/ 2 + boundaryDistance, 2) + Mathf.Pow(planeHeight/2 + boundaryDistance, 2));
        lastGeneratedPlane = GameObject.FindWithTag("Plane").transform;

    }

    void Update()
    {
        if(bikeController != null && bikeController.hitObject != null){
            if(bikeController.hitObject.tag == "Plane"){
                // Debug.Log("Is Plane: "+ bikeController.hitObject.name);
                currentPlane = bikeController.hitObject.transform;
                currentPlanePosition = bikeController.hitObject.transform.position;
            }else{
                currentPlane = null;
                currentPlanePosition = townPosition.position;
            }
        }
        // Debug.Log("Current Plane: " + currentPlanePosition);
        CheckAndGeneratePlane();
    }

    void CheckAndGeneratePlane()
    {
        bool isRight = true, isLeft =true, isForward = true, isBack = true, isLeftForward = true, isRightForward = true, isLeftBack = true, isRightBack = true;
        PlaneAttributes planeAttributes = null;
        if(currentPlane != null){
            planeAttributes = currentPlane.GetComponent<PlaneAttributes>();
            isLeft = planeAttributes.isLeft;
            isRight = planeAttributes.isRight;
            isForward = planeAttributes.isForward;
            isBack = planeAttributes.isBack;
            isLeftForward = planeAttributes.isLeftForward;
            isLeftBack = planeAttributes.isLeftBack;
            isRightForward = planeAttributes.isRightForward;
            isRightBack = planeAttributes.isRightBack;
        }
        Vector3 scooterPosition = transform.position;
        Vector3 scooterPosition_y = scooterPosition;
        scooterPosition_y.y = 0;

        // Calculate the distance from the scooter to the edges of the last generated plane
        float distanceToLeft = Mathf.Abs(scooterPosition.x - (currentPlanePosition.x - planeWidth / 2));
        float distanceToRight = Mathf.Abs(scooterPosition.x - (currentPlanePosition.x + planeWidth / 2));
        float distanceToTop = Mathf.Abs(scooterPosition.z - (currentPlanePosition.z + planeHeight / 2));
        float distanceToBack = Mathf.Abs(scooterPosition.z - (currentPlanePosition.z - planeHeight / 2));
        float distanceToLeftTop = Mathf.Sqrt(Mathf.Pow(distanceToLeft, 2) + Mathf.Pow(distanceToTop, 2));
        float distanceToRightTop = Mathf.Sqrt(Mathf.Pow(distanceToRight, 2) + Mathf.Pow(distanceToTop, 2));
        float distanceToLeftBack = Mathf.Sqrt(Mathf.Pow(distanceToLeft, 2) + Mathf.Pow(distanceToBack ,2));
        float distanceToRightBack = Mathf.Sqrt(Mathf.Pow(distanceToRight, 2) + Mathf.Pow(distanceToBack ,2));
        bool farFromTown = Vector3.Distance(scooterPosition_y, townPosition.position) >= townDistanceThreshold;

        // Determine if a new plane is needed based on the distance to the edges
        if (farFromTown  && !isLeft)
        {
            GenerateNewPlane(new Vector3(currentPlanePosition.x - planeWidth, currentPlanePosition.y, currentPlanePosition.z));
            if(planeAttributes != null){
                planeAttributes.isLeft = true;
            }
        }
        if ( farFromTown && !isRight)
        {
            GenerateNewPlane(new Vector3(currentPlanePosition.x + planeWidth, currentPlanePosition.y, currentPlanePosition.z));
            if(planeAttributes != null){
                planeAttributes.isRight = true;
            }
        }
        if (farFromTown && !isForward)
        {
            GenerateNewPlane(new Vector3(currentPlanePosition.x, currentPlanePosition.y, currentPlanePosition.z + planeHeight));
            if(planeAttributes != null){
                planeAttributes.isForward = true;
            }
        }
        if (farFromTown && !isBack)
        {
            GenerateNewPlane(new Vector3(currentPlanePosition.x, currentPlanePosition.y, currentPlanePosition.z - planeHeight));
            if(planeAttributes != null){
                planeAttributes.isBack = true;
            }
        }
        if (farFromTown  && !isLeftForward)
        {
            GenerateNewPlane(new Vector3(currentPlanePosition.x - planeWidth, currentPlanePosition.y, currentPlanePosition.z + planeHeight));
            if(planeAttributes != null){
                planeAttributes.isLeftForward = true;
            }
        }
        if (farFromTown && !isRightForward)
        {
            GenerateNewPlane(new Vector3(currentPlanePosition.x + planeWidth, currentPlanePosition.y, currentPlanePosition.z + planeHeight));
            if(planeAttributes != null){
                planeAttributes.isRightForward = true;
            }
        }
        if (farFromTown && !isLeftBack)
        {
            GenerateNewPlane(new Vector3(currentPlanePosition.x - planeWidth, currentPlanePosition.y, currentPlanePosition.z - planeHeight));
            if(planeAttributes != null){
                planeAttributes.isLeftBack = true;
            }
        }
        if (farFromTown && !isRightBack)
        {
            GenerateNewPlane(new Vector3(currentPlanePosition.x + planeWidth, currentPlanePosition.y, currentPlanePosition.z - planeHeight));
            if(planeAttributes != null){
                planeAttributes.isRightBack = true;
            }
        }
        // Debug.Log("Left: " + distanceToLeft +  ", Right: " + distanceToRight);
        // Debug.Log("Top: " + distanceToTop + ", Bottom: " + distanceToBack);
        // Debug.Log("Far Enough: " + farFromTown);
    }

    void GenerateNewPlane(Vector3 position)
    {
        // Instantiate a new plane at the specified position
        lastGeneratedPlane = Instantiate(planePrefab, position, Quaternion.identity).transform;
        // Debug.Log("New Plane On " + lastGeneratedPlane.position);

        // Tag the new plane as "Plane"
        lastGeneratedPlane.tag = "Plane";
    }
}

