using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ArrowManager : MonoBehaviour
{
    public GameObject scooter; 
    public GameObject arrowObject;
    public int numberOfArrows = 4;
    private List<GameObject> arrows, sortedArrows;
    private List<bool> arrowsInFront = new List<bool>();

    void Start(){
        InitializeArrowList();
    }

    void Update()
    {
        arrows = GetAllChildGameObjects(arrowObject);
        UpdateClosestArrows();
    }

    void UpdateClosestArrows()
    {
        int idx0 = 0;
        if (scooter== null || arrows == null || arrows.Count == 0)
        {
            Debug.LogWarning("Kickboard or arrows are not properly assigned.");
            return;
        }

        Vector3 scooter_y = scooter.transform.position;
        sortedArrows = arrows.OrderBy(arrow => HorizontalDistance(scooter.transform.position, arrow.transform.position)).ToList();
        CheckArrowsInFront();

        for (int i = 0; i < sortedArrows.Count; i++)
        {
            if(arrowsInFront[i] && idx0 < numberOfArrows){
                idx0 += 1;
                sortedArrows[i].SetActive(true);
            }else{
                sortedArrows[i].SetActive(false);
            }
        }
    }

    float HorizontalDistance(Vector3 a, Vector3 b)
    {
        Vector3 flatA = new Vector3(a.x, 0, a.z);
        Vector3 flatB = new Vector3(b.x, 0, b.z);
        
        return Vector3.Distance(flatA, flatB);
    }

    List<GameObject> GetAllChildGameObjects(GameObject parent)
    {
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in parent.transform)
        {
            children.Add(child.gameObject);
        }

        return children;
    }

    void InitializeArrowList()
    {
        arrowsInFront.Clear();
        foreach (Transform arrow in arrowObject.transform)
        {
            arrowsInFront.Add(false); 
        }
    }

    void CheckArrowsInFront()
    {
        for (int i = 0; i < arrowObject.transform.childCount; i++)
        {
            Transform arrow = sortedArrows[i].transform;

            // Calculate the direction from the scooter to the arrow
            Vector3 directionToArrow = arrow.position - scooter.transform.position;

            // Check if the arrow is in front of the scooter
            bool isInFront = Vector3.Dot(scooter.transform.forward, directionToArrow.normalized) > 0;

            // Update the bool list with the current result
            arrowsInFront[i] = isInFront;
        }
    }
}