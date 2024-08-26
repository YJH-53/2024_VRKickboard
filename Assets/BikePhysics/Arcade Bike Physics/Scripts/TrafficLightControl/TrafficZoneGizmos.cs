using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficZoneGizmos : MonoBehaviour
{
    public TrafficLightController trafficLightController;
    [HideInInspector]
    private float radius;

    void Start(){
        if(trafficLightController != null){
            radius = trafficLightController.scooterDetectionRadius;
        } 
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}