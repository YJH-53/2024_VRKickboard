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
    public float overspeedThreshold = 30f;
    public float underspeedThreshold = 5f;
    public bool isEffectActive = false;


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
            string zone = bikeController.zone, groundType = bikeController.groundType;
            //isOnTrack 변수값 설정
            if(zone == "Zone1" || zone == "Zone2"){
                isOnTrack = bikeController.isOnRoad;
                //Debug.Log("IsOnTrack : " + isOnTrack);
            }else{
                isOnTrack = false;
            }
            //speedText 할당
            if (speedText != null)
            {
                speedText.text = "Speed: " + currentSpeed.ToString("F2") + " km/h";
            }

            if (((currentSpeed > overspeedThreshold) && zone == "Zone1") || !isOnTrack)
            {
                isEffectActive = true;
            }else if(((currentSpeed < underspeedThreshold) && zone == "Zone2") || !isOnTrack)
            {
                isEffectActive = true;
            }
            else
            {
                isEffectActive = false; // Reset the effect status when speed drops below the threshold
            }
        }
    }
}