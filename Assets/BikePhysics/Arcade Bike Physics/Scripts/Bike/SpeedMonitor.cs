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

    private float outOfZoneTimer = 0f; // Timer to track how long the scooter has been out of the zone or off-track
    public bool collisionWithWall = false;
    private bool gracePeriodActive = false;
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
    }

    void Update()
    {
        // Monitor the speed
        if (bikeController != null)
        {
            isRightDirection = bikeController.isRightDirection;
            isMoveRight = bikeController.isMoveRight;
            float currentSpeed = bikeController.bikeVelocity.magnitude * 3.6f; // Convert m/s to km/h

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

            //Zone, 속도 위반 시 TakeDamage.cs로 넘길 isEffectActive에 대한 조건문
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

            if (!isOnTrack || !isInZone)
            {
                outOfZoneTimer += Time.deltaTime;

                // If the scooter has been out of the zone or off-track for more than 10 seconds
                if (outOfZoneTimer >= 10f)
                {
                    ResetScooterPosition();
                }
            }
            else
            {
                // Reset the timer if the scooter is back on track or in the zone
                outOfZoneTimer = 0f;
            }


            if(collisionWithWall)
            {
                StartCoroutine(HandleCollisionAfterGracePeriod());
            }
            
        }
    }

    // Wall과의 충돌이 감지된 순간 해당 coroutine 수행
    IEnumerator HandleCollisionAfterGracePeriod()
    {
        gracePeriodActive = true;

        // Wait for 2 seconds (grace period)
        yield return new WaitForSeconds(2f);

        // If the collision is still detected after 2 seconds, reset the position
        if (collisionWithWall)
        {
            ResetScooterPosition();
        }

        // Reset flags after handling the collision
        collisionWithWall = false;
        gracePeriodActive = false;
    }

    void ResetScooterPosition()
    {
        Vector3 targetPosition = Vector3.zero;
        Quaternion targetRotation = Quaternion.identity;

        switch (zone_num)
        {
            case 0: 
                targetPosition = new Vector3(-69.75f, 0.590709f, 36.14f);
                targetRotation = Quaternion.Euler(0f, 180f, 0f);
                break;
            case 1: 
                targetPosition = new Vector3(-69.75f, 0.6f, -6.04f);
                targetRotation = Quaternion.Euler(0f, 180f, 0f);
                break;
            
            case 2: 
                targetPosition = new Vector3(-4.13f, 0.6f, -65.29f);
                targetRotation = Quaternion.Euler(0f, 90f, 0f);
                break;
            case 3: 
                targetPosition = new Vector3(21.13f, 0.6f, -77.66f);
                targetRotation = Quaternion.Euler(0f, 90f, 0f);
                break;
            case 4: 
                targetPosition = new Vector3(86.13f, 5.06f, -2.22f);
                targetRotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case 5: 
                targetPosition = new Vector3(86.13f, 0.6f, 30.9f);
                targetRotation = Quaternion.Euler(0f, 0f, 0f);
                break;
            case 6: 
                targetPosition = new Vector3(16.17f, 0.6f, 64.67f);
                targetRotation = Quaternion.Euler(0f, 270f, 0f);
                break;
            case 7: 
                targetPosition = new Vector3(-7.79f, 0.6f, 53.33f);
                targetRotation = Quaternion.Euler(0f, 270f, 0f);
                break;
            case 8: 
                targetPosition = new Vector3(-58.57f, 0.6f, 58.04f);
                targetRotation = Quaternion.Euler(0f, 225f, 0f);
                break;
            
        }

        bikeController.transform.position = targetPosition;
        bikeController.transform.rotation = targetRotation;
        bikeController.bikeBody.velocity = new Vector3(0.0f, 0.0f, 0.0f);
        outOfZoneTimer = 0.0f;
        collisionWithWall = false;
    }

    // 사람과의 충돌을 collisionWithPerson bool 변수에 담아 인식하는 함수
    // Taeyun: wall, building, object와의 충돌도 여기서 처리함.
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 물체의 root object의 tag를 읽어들이는 과정
        // Debug.Log("Entered Collision Mode");
        GameObject collisionObject = collision.gameObject;
        GameObject collisionObject_parent = collisionObject;
        if (collisionObject.transform.parent != null)
        {
            collisionObject_parent = collisionObject.transform.parent.gameObject;
        }
        // Debug.Log("Scooter hit: " + collisionObject_parent.tag);
        
        if (collisionObject_parent.CompareTag("Person"))
        {
            collisionWithPerson = true;
        }

        if (collisionObject.CompareTag("Wall") || collisionObject_parent.CompareTag("Wall") || collisionObject.CompareTag("Building") || collisionObject_parent.CompareTag("Building")
                || collisionObject.CompareTag("Object") || collisionObject_parent.CompareTag("Object"))
        {
            collisionWithWall = true;
            // Debug.Log("Scooter hit: " + collisionObject.tag);
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
                // zone_num = zoneNumber;
                // Debug.Log("Entered Zone: " + zone_num);
            }
            else
            {
                Debug.LogWarning("Could not parse zone number from tag: " + tag);
            }
        }else if (collisionObject_parent.tag == "Division1")
        {
            if(bikeController.enterZone0_Count == 2 && bikeController.enterZone1_Count != 2 && Vector3.Angle(transform.forward, collisionObject_parent.transform.up) < 90){
                // Debug.Log("AngleInto 1 : " + Vector3.Angle(transform.forward, collisionObject_parent.transform.up));
                zone_num = 1;
                bikeController.enterZone1 = true;
                bikeController.enterZone1_Count = 1;
                // Debug.Log("Count1: " + bikeController.enterZone1_Count);
            }
        }
        else if (collisionObject_parent.tag == "Division2")
        {
            if(bikeController.enterZone0_Count == 2 && bikeController.enterZone1_Count == 2 && bikeController.enterZone2_Count != 2 && Vector3.Angle(transform.forward, collisionObject_parent.transform.up) < 90){
                zone_num = 2;
                bikeController.enterZone2 = true;
                bikeController.enterZone2_Count = 1;
                // Debug.Log("Count2: " + bikeController.enterZone2_Count);
            }
        }
        else if (collisionObject_parent.tag == "Division3")
        {
            if(bikeController.enterZone0_Count == 2 && bikeController.enterZone1_Count == 2 && bikeController.enterZone2_Count == 2 && bikeController.enterZone3_Count != 2 && Vector3.Angle(transform.forward, collisionObject_parent.transform.up) < 90){
                zone_num = 3;
                bikeController.enterZone3 = true;
                bikeController.enterZone3_Count = 1;
                // Debug.Log("Count3: " + bikeController.enterZone3_Count);
            }
        }
        else if (collisionObject_parent.tag == "Division4")
        {
            if(bikeController.enterZone0_Count == 2 && bikeController.enterZone1_Count == 2 && bikeController.enterZone2_Count == 2 && bikeController.enterZone3_Count == 2 && bikeController.enterZone4_Count != 2 && Vector3.Angle(transform.forward, collisionObject_parent.transform.up) < 90){
                zone_num = 4;
                bikeController.enterZone4 = true;
                bikeController.enterZone4_Count = 1;
                // Debug.Log("Count4: " + bikeController.enterZone4_Count);
            }
        }
        else if (collisionObject_parent.tag == "Division5")
        {
            if(bikeController.enterZone0_Count == 2 && bikeController.enterZone1_Count == 2 && bikeController.enterZone2_Count == 2 && bikeController.enterZone3_Count == 2 && bikeController.enterZone4_Count == 2 && bikeController.enterZone5_Count != 2 && Vector3.Angle(transform.forward, collisionObject_parent.transform.up) < 90){
                zone_num = 5;
                bikeController.enterZone5 = true;
                bikeController.enterZone5_Count = 1;
                // Debug.Log("Count5: " + bikeController.enterZone5_Count);
            }
        }
        else if (collisionObject_parent.tag == "Division6")
        {
            if(bikeController.enterZone0_Count == 2 && bikeController.enterZone1_Count == 2 && bikeController.enterZone2_Count == 2 && bikeController.enterZone3_Count == 2 && bikeController.enterZone4_Count == 2 && bikeController.enterZone5_Count == 2 && bikeController.enterZone6_Count != 2 && Vector3.Angle(transform.forward, collisionObject_parent.transform.up) < 90){
                zone_num = 6;
                bikeController.enterZone6 = true;
                bikeController.enterZone6_Count = 1;
                // Debug.Log("Count6: " + bikeController.enterZone6_Count);
            }
        }
        else if (collisionObject_parent.tag == "Division7")
        {
            if(bikeController.enterZone0_Count == 2 && bikeController.enterZone1_Count == 2 && bikeController.enterZone2_Count == 2 && bikeController.enterZone3_Count == 2 && bikeController.enterZone4_Count == 2 && bikeController.enterZone5_Count == 2 && bikeController.enterZone6_Count == 2 && bikeController.enterZone7_Count != 2 && Vector3.Angle(transform.forward, collisionObject_parent.transform.up) < 90){
                zone_num = 7;
                bikeController.enterZone7 = true;
                bikeController.enterZone7_Count = 1;
                // Debug.Log("Count7: " + bikeController.enterZone7_Count);
            }
        }
        else if (collisionObject_parent.tag == "Division8")
        {
            if(bikeController.enterZone0_Count == 2 && bikeController.enterZone1_Count == 2 && bikeController.enterZone2_Count == 2 && bikeController.enterZone3_Count == 2 && bikeController.enterZone4_Count == 2 && bikeController.enterZone5_Count == 2 && bikeController.enterZone6_Count == 2 && bikeController.enterZone7_Count == 2 && bikeController.enterZone8_Count != 2 && Vector3.Angle(transform.forward, collisionObject_parent.transform.up) < 90){
                zone_num = 8;
                bikeController.enterZone8 = true;
                bikeController.enterZone8_Count = 1;
                // Debug.Log("Count8: " + bikeController.enterZone8_Count);
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
                // Debug.Log("You Are in Area of Zone: " + zone_num);
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
        // Debug.Log("Scooter hit: " + collisionObject_parent.tag);
        if (collisionObject_parent.tag.Contains("Zone"))
        {
            isInZone = false;
            // zone_num = -1;
            // Debug.Log("Exited Zone, no longer in a zone.");
        }
    }
}