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
    public int zone_num = -1; // 현재 Zone의 number을 담는 변수, Start를 -1, Finish를 8로 둠.

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

            // isOnTrack 변수값 설정
            if (zone_num == 0 || zone_num == 1 || zone_num == 7 || zone_num == 8)
            {
                bikeController.trafficMode = ArcadeBP.ArcadeBikeController.TrafficMode.Track;
                isOnTrack = bikeController.isOnBlock;
            }
            else if (zone_num == -1 || zone_num == 21 || zone_num == 2)
            {
                isOnTrack = true;
            }
            else
            {
                bikeController.trafficMode = ArcadeBP.ArcadeBikeController.TrafficMode.Road;
                isOnTrack = bikeController.isOnRoad;
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

            if (!isOnTrack)
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
        else if (collisionObject_parent.tag.Contains("Zone"))
        {
            if (int.TryParse(collisionObject_parent.tag.Replace("Zone", ""), out int zoneNumber))
            {
                zone_num = zoneNumber;
                Debug.Log("Entered Zone: " + zone_num);
            }
            else
            {
                Debug.LogWarning("Could not parse zone number from tag: " + tag);
            }
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
        Debug.Log("Scooter hit: " + collisionObject_parent.tag);
        if (collisionObject_parent.tag.Contains("Zone"))
        {
            if (int.TryParse(collisionObject_parent.tag.Replace("Zone", ""), out int zoneNumber))
            {
                isInZone = true;
                zone_num = zoneNumber;
                Debug.Log("Entered Zone: " + zone_num);
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
            zone_num = 0;
            Debug.Log("Exited Zone, no longer in a zone.");
        }
    }
}
