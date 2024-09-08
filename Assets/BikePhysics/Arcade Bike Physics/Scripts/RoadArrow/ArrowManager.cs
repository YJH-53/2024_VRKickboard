using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ArrowManager : MonoBehaviour
{
    public GameObject scooter; 
    public List<GameObject> arrows;

    void Update()
    {
        UpdateClosestArrows();
    }

    void UpdateClosestArrows()
    {
        if (scooter== null || arrows == null || arrows.Count == 0)
        {
            Debug.LogWarning("Kickboard or arrows are not properly assigned.");
            return;
        }

        Vector3 scooter_y = scooter.transform.position;
        var sortedArrows = arrows.OrderBy(arrow => HorizontalDistance(scooter.transform.position, arrow.transform.position)).ToList();

        for (int i = 0; i < sortedArrows.Count; i++)
        {
            if (i < 2) 
            {
                sortedArrows[i].SetActive(true);
            }
            else
            {
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
}