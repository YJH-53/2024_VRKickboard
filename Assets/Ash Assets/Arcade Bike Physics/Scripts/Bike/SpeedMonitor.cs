using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedMonitor : MonoBehaviour
{
    public ArcadeBP.ArcadeBikeController bikeController;
    public TMP_Text speedText;
    public GameObject damageLayer;
    public TakeDamage takeDamageScript;
    public float overspeedThreshold = 30f; //과속 기준(Zone1)
    public float underspeedThreshold = 5f; //감속 감점 기준(Zone2)
    [HideInInspector]
    public bool isEffectActive = false;
    public bool collisionWithPerson = false;
    public bool isInZone = false;
    public int zone_num = 0; //현재 Zone의 number을 담는 변수


    void Start()
    {
        if (bikeController == null)
        {
            bikeController = GetComponent<ArcadeBP.ArcadeBikeController>();
        }

        if (takeDamageScript == null)
        {
            takeDamageScript = damageLayer.GetComponent<TakeDamage>();
        }

        if (bikeController == null || takeDamageScript == null)
        {
            Debug.LogError("Required components are not assigned or found!");
        }
    }

    void Update()
    {
        // Monitor the speed
        if (bikeController != null)
        {
            float currentSpeed = bikeController.bikeVelocity.magnitude * 3.6f; // Convert m/s to km/h
            bool isOnTrack = false;
            string groundType = bikeController.groundType;
            //isOnTrack 변수값 설정
            if(zone_num == 1 || zone_num == 2){
                isOnTrack = bikeController.isOnRoad;
                //Debug.Log("IsOnTrack : " + isOnTrack);
            }else if(zone_num == 0){
                isOnTrack = true;
            }else{
                isOnTrack = false;
            }
            //speedText 할당
            if (speedText != null)
            {
                speedText.text = "Speed: " + currentSpeed.ToString("F2") + " km/h";
            }

            if (((currentSpeed > overspeedThreshold) && zone_num == 1) || !isOnTrack)
            {
                isEffectActive = true;
            }else if(((currentSpeed < underspeedThreshold) && zone_num == 2) || !isOnTrack)
            {
                isEffectActive = true;
            }
            else
            {
                isEffectActive = false; // Reset the effect status when speed drops below the threshold
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //충돌한 물체의 root object의 tag를 읽어들이는 과정
        Debug.Log("Entered Collision Mode");
        GameObject collisionObject = collision.gameObject;
        GameObject collisionObject_parent = collisionObject;
        if(collisionObject.transform.parent!=null){
            collisionObject_parent = collisionObject.transform.parent.gameObject;
        }
        Debug.Log("Scooter hit: " + collisionObject_parent.tag);
        if (collisionObject_parent.CompareTag("Person"))
        {
            collisionWithPerson = true;
        }else if(collisionObject_parent.tag.Contains("Zone")){
            if(int.TryParse(collisionObject_parent.tag.Replace("Zone", ""), out int zoneNumber)){
                zone_num = zoneNumber;
                Debug.Log("Entered Zone: " + zone_num);
            }else{
                Debug.LogWarning("Could not parse zone number from tag: " + tag);
            }
        }
    }

    void OnTriggerEnter(Collider other){
        GameObject collisionObject = other.gameObject;
        GameObject collisionObject_parent = collisionObject;
        if(collisionObject.transform.parent!=null){
            collisionObject_parent = collisionObject.transform.parent.gameObject;
        }
        Debug.Log("Scooter hit: " + collisionObject_parent.tag);
        if(collisionObject_parent.tag.Contains("Zone")){
            if(int.TryParse(collisionObject_parent.tag.Replace("Zone", ""), out int zoneNumber)){
                isInZone = true;
                zone_num = zoneNumber;
                Debug.Log("Entered Zone: " + zone_num);
            }else{
                Debug.LogWarning("Could not parse zone number from tag: " + tag);
            }
        }
    }

    void OnTriggerExit(Collider other){
        GameObject collisionObject = other.gameObject;
        GameObject collisionObject_parent = collisionObject;
        if(collisionObject.transform.parent!=null){
            collisionObject_parent = collisionObject.transform.parent.gameObject;
        }
        Debug.Log("Scooter hit: " + collisionObject_parent.tag);
        if(collisionObject_parent.tag.Contains("Zone")){
            isInZone = false;
            zone_num = 0;
            Debug.Log("Exited Zone, no longer in a zone.");
        }
    }
}