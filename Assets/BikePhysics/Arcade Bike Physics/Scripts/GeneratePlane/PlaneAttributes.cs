using UnityEngine;

public class PlaneAttributes : MonoBehaviour
{
    public bool isLeft;
    public bool isRight;
    public bool isForward;
    public bool isBack;
    public bool isLeftForward;
    public bool isLeftBack;
    public bool isRightForward;
    public bool isRightBack;

    private float planeWidth = 235.9738f;
    private float planeHeight = 220.0f;
    private float plane_diagonal;
    public Vector3 boxSize = new Vector3(10, 10, 10);

    void Start()
    {
        // Check in four directions: left, right, up, down
    }

    void Update(){
        plane_diagonal = Mathf.Sqrt(Mathf.Pow(planeWidth, 2) + Mathf.Pow(planeHeight, 2));
        Vector3 left_forward, left_back, right_forward, right_back;
        left_forward = new Vector3(planeWidth * (-1), 0, planeHeight);
        left_back = new Vector3(planeWidth * (-1), 0, planeHeight * (-1));
        right_forward = new Vector3(planeWidth, 0, planeHeight);
        right_back = new Vector3(planeWidth, 0, planeHeight * (-1));
        CheckForNeighbor(Vector3.left, "left");
        CheckForNeighbor(Vector3.right, "right");
        CheckForNeighbor(Vector3.forward, "forward"); // Upward relative to the plane
        CheckForNeighbor(Vector3.back, "backward");   // Downward relative to the plane
        CheckForNeighbor(left_forward, "leftforward");
        CheckForNeighbor(left_back, "leftback");
        CheckForNeighbor(right_forward, "rightforward");
        CheckForNeighbor(right_back, "rightback");
    }

    void CheckForNeighbor(Vector3 direction, string directionName)
    {
        Vector3 checkPosition;
        if(directionName == "left" || directionName == "right"){
            checkPosition = transform.position + direction * planeWidth;
        }else if(directionName == "forward" || directionName == "backward"){
            checkPosition = transform.position + direction * planeHeight;
        }else{
            checkPosition = transform.position + direction;
        }

        // Check if there are any objects with colliders in a box at the check position
        Collider[] hitColliders = Physics.OverlapBox(checkPosition, boxSize / 2);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Plane"))
            {
                if(directionName == "left")
                    isLeft = true;
                else if(directionName == "right")
                    isRight = true;
                else if(directionName == "forward")
                    isForward = true;
                else if(directionName == "backward")
                    isBack = true;
                else if(directionName == "leftforward")
                    isLeftForward = true;
                else if(directionName == "leftback")
                    isLeftBack = true;
                else if(directionName == "rightforward")
                    isRightForward = true;
                else if(directionName == "rightback")
                    isRightBack = true;
            }
        }
    }
}