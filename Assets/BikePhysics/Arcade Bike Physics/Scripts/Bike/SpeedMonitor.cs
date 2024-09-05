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
    public float overspeedThreshold_Sidewalk = 25f; // 과속 기준 (Zone 1)
    public float overspeedThreshold_Road = 35f;
    public float underspeedThreshold_Road = 5f;
    public bool isOnTrack = true;  // 트랙 위에 있는지 여부를 나타내는 플래그
    public bool isSpeedViolationActive = false;  // 속도 위반 여부를 판단하는 플래그
    public bool isEffectActive = false;  // 이펙트 활성 여부를 판단하는 플래그
    public bool collisionWithPerson = false;
    public bool isInZone = false;
    public bool isRightDirection = false, isMoveRight = false;
    public int zone_num = -1; // 현재 Zone의 number을 담는 변수, Start를 -1, Finish를 8로 둠.

    private float zoneChangeTime = 0f;  // 존이 변경된 시간을 기록
    private float zoneChangeDelay = 0.2f;  // 존 변경 시 0.2초 딜레이
    private float initialDelay = 1.0f;  // 게임 시작 후 1초 동안 속도 위반 체크를 하지 않도록 하는 딜레이

    void Start()
    {
        if(speedText != null){
            speedText.gameObject.SetActive(true);
        }
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
        isSpeedViolationActive = false;

        zoneChangeTime = Time.time + initialDelay;  // 게임 시작 후 1초 동안 속도 위반 체크를 하지 않도록 설정
    }

    void Update()
    {
        // Monitor the speed
        if (bikeController != null)
        {
            isRightDirection = bikeController.isRightDirection;
            isMoveRight = bikeController.isMoveRight;
            float currentSpeed = bikeController.bikeVelocity.magnitude * 3.6f; // Convert m/s to km/h

            // 존 변경 후 딜레이 시간 확인 (게임 시작 후 1초 포함)
            if (Time.time >= zoneChangeTime + zoneChangeDelay)
            {
                // isOnTrack 변수값 설정
                if (zone_num == 0 || zone_num == 1 || zone_num == 7 || zone_num == 8)
                {
                    bikeController.trafficMode = ArcadeBP.ArcadeBikeController.TrafficMode.Track;
                    isOnTrack = bikeController.isOnBlock;
                }
                else if (zone_num == 3 || zone_num == 4 || zone_num == 5)
                {
                    bikeController.trafficMode = ArcadeBP.ArcadeBikeController.TrafficMode.Road;
                    isOnTrack = bikeController.isOnRoad; 
                }
                else if (zone_num == 21 || zone_num == 2 || zone_num ==6)
                {   
                    isOnTrack = true;
                }
                else
                {
                    isOnTrack = false;
                }

                // speedText 할당
                if (speedText != null)
                {
                    speedText.text = "Speed: " + currentSpeed.ToString("F2") + " km/h";
                }

                // 속도 위반 및 트랙 이탈 상태 업데이트
                if ((currentSpeed > overspeedThreshold_Sidewalk && (zone_num == 0 || zone_num == 1 || zone_num == 2 || zone_num == 21 || zone_num == 6 || zone_num == 7 || zone_num == 8)) ||
                    ((currentSpeed < underspeedThreshold_Road || currentSpeed > overspeedThreshold_Road) && (zone_num == 3 || zone_num == 5)))
                {
                    isSpeedViolationActive = true;
                }
                else
                {
                    isSpeedViolationActive = false;
                }
            }
            else
            {
                // 존 변경 후 0.2초 또는 게임 시작 후 1초 동안은 속도 위반 체크하지 않음
                isSpeedViolationActive = false;
            }

            // Zone, 속도 위반 시 TakeDamage.cs로 넘길 isEffectActive에 대한 조건문
            if (!isInZone || !isRightDirection || !isOnTrack || !isMoveRight)
            {
                isEffectActive = true; // 트랙을 벗어났을 때 효과 활성화
            }
            else if (isSpeedViolationActive)
            {
                isEffectActive = true; // 속도 위반 시 효과 활성화
            }
            else
            {
                isEffectActive = false; // 아무 위반도 없을 때 효과 비활성화
            }
        }
    }

    // 사람과의 충돌을 collisionWithPerson bool 변수에 담아 인식하는 함수
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 물체의 root object의 tag를 읽어들이는 과정
        Debug.Log("Entered Collision Mode");
        GameObject collisionObject = collision.gameObject;
        GameObject collisionObject_parent = collisionObject;
        if (collisionObject.transform.parent != null)
        {
            collisionObject_parent = collisionObject.transform.parent.gameObject;
        }
        Debug.Log("Scooter hit: " + collisionObject_parent.tag);
        if (collisionObject_parent.CompareTag("Person"))
        {
            collisionWithPerson = true;
        }
    }

    // Zone 진입 시 호출되는 함수 (Zone 구별하는 기능)
    void OnTriggerEnter(Collider other)
    {
        GameObject collisionObject = other.gameObject;
        GameObject collisionObject_parent = collisionObject;
        if (collisionObject.transform.parent != null)
        {
            collisionObject_parent = collisionObject.transform.parent.gameObject;
        }
        // Debug.Log("Scooter hit: " + collisionObject_parent.tag);
        if (collisionObject_parent.tag.Contains("Zone"))
        {
            if (int.TryParse(collisionObject_parent.tag.Replace("Zone", ""), out int zoneNumber))
            {
                isInZone = true;
                zoneChangeTime = Time.time;  // 존 변경 시간 기록
                if(zoneNumber == 21){
                    zone_num = 21;
                }else{
                    if(bikeController.enterZone8)
                        zone_num = 8;
                    else if(bikeController.enterZone7)
                        zone_num = 7;
                    else if(bikeController.enterZone6)
                        zone_num = 6;
                    else if(bikeController.enterZone5)
                        zone_num = 5;
                    else if(bikeController.enterZone4)
                        zone_num = 4;
                    else if(bikeController.enterZone3)
                        zone_num = 3;
                    else if(bikeController.enterZone2)
                        zone_num = 2;
                    else if(bikeController.enterZone1)
                        zone_num = 1;
                    else zone_num = 0;
                }
                Debug.Log("You Are in Area of Zone: " + zone_num);
            }
            else
            {
                Debug.LogWarning("Could not parse zone number from tag: " + tag);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        GameObject collisionObject = other.gameObject;
        GameObject collisionObject_parent = collisionObject;
        if (collisionObject.transform.parent != null)
        {
            collisionObject_parent = collisionObject.transform.parent.gameObject;
        }
        // Debug.Log("Scooter hit: " + collisionObject_parent.tag);
        if (collisionObject_parent.tag.Contains("Zone"))
        {
            if (int.TryParse(collisionObject_parent.tag.Replace("Zone", ""), out int zoneNumber))
            {
                isInZone = true;
                if(zoneNumber == 21){
                    zone_num = 21;
                }else{
                    if(bikeController.enterZone8)
                        zone_num = 8;
                    else if(bikeController.enterZone7)
                        zone_num = 7;
                    else if(bikeController.enterZone6)
                        zone_num = 6;
                    else if(bikeController.enterZone5)
                        zone_num = 5;
                    else if(bikeController.enterZone4)
                        zone_num = 4;
                    else if(bikeController.enterZone3)
                        zone_num = 3;
                    else if(bikeController.enterZone2)
                        zone_num = 2;
                    else if(bikeController.enterZone1)
                        zone_num = 1;
                    else zone_num = 0;
                }
                Debug.Log("You Are in Area of Zone: " + zone_num);
            }
            else
            {
                Debug.LogWarning("Could not parse zone number from tag: " + tag);
            }
        }
    }

    // Zone 퇴장 시 호출되는 함수 (Zone 퇴장 인식 기능)
    void OnTriggerExit(Collider other)
    {
        GameObject collisionObject = other.gameObject;
        GameObject collisionObject_parent = collisionObject;
        if (collisionObject.transform.parent != null)
        {
            collisionObject_parent = collisionObject.transform.parent.gameObject;
        }
        Debug.Log("Scooter hit: " + collisionObject_parent.tag);
        if (collisionObject_parent.tag.Contains("Zone"))
        {
            isInZone = false;
            // zone_num = -1;
            // Debug.Log("Exited Zone, no longer in a zone.");
        }
    }
}
