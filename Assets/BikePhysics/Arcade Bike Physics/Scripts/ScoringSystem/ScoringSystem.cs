using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoringSystem : MonoBehaviour
{
    public int score = 100; // Initial score
    public float OffTrackTimeThreshold = 5.0f; //경로 이탈 시 재감점 시간 간격
    public float speedViolationTimeThreshold = 3.0f; // 속도 위반 동안의 감점 간격
    public float CollisionTimeThreshold = 2.01f; //충돌 이후 재충돌 까지 감점 간격(물체랑 닿아 있는동안 계속 감점되는 거 방지)
    public float RedTrafficViolationTimeThreshold = 1.0f; //빨강 신호 위반 동안의 감점 간격
    public float GreenTrafficViolationTimeThreshold = 2.0f; //초록 신호 위반 동안의 감점 간격
    public TMP_Text scoreText; // Text component to display the score
    //Track 주행 관련 text
    public TMP_Text offZoneText;
    public TMP_Text wrongDirectionText;
    public TMP_Text offTrackText;
    public TMP_Text moveRightText;
    public TMP_Text collisionText; // Text component to display collision alert
    public TMP_Text trafficViolationText;
    public TMP_Text speedViolationText; // 속도 위반 경고를 표시할 텍스트 컴포넌트
    public ArcadeBP.ArcadeBikeController bikeController; // Reference to the bike controller script
    public SpeedMonitor speedMonitorScript;
    public PauseMenu pauseMenuScript;

    [HideInInspector]
    public bool isZonePenalty = false;
    private int penaltyPoints_zone = 5; // 구간 규칙(경로 이탈, 과속 등) 위반 시 감점
    private int penaltyPoints_collision = 10; //장애물 충돌 시 감점 
    private int penaltyPoints_trafficViolation = 10; //신호 위반 시 감점
    private int penaltyPoints_speedViolation = 5; // 속도 위반 시 감점

    private bool deductPoint_firstzone = false, deductPoint_zone = false, deductPoint_speedViolation = false;
    private bool deductPoint_collision = false, deductPoint_redTrafficViolation = false, deductPoint_greenTrafficViolation = false;
    private string offZoneMessage = "Off Zone!", offTrackMessage = "Off Track!", wrongDirectionMessage = "Wrong Direction!", moveRightMessage = "Move Right!", speedMessage = "Off Speed Limit!", collisionMessage = "Collision Detected!", redMessage = "Red Traffic Violation!", greenMessage = "Green Traffic Violation!";
    private float collisionDuration = 1.2f, trafficDuration = 1.0f;
    private float lastOffTrackTime, lastCollisionTime, lastRedTrafficViolationTime, lastGreenTrafficViolationTime, lastSpeedViolationTime;
    

    void Start()
    {
        // Display the initial score
        if(scoreText != null){
            scoreText.gameObject.SetActive(true);
        }
        UpdateScoreText();

        // Hide message initially
        if(offZoneText != null)
        {
            offZoneText.text = offZoneMessage;
            offZoneText.color = Color.red;
            offZoneText.gameObject.SetActive(false);
        }
        if(offTrackText != null)
        {
            offTrackText.text = offTrackMessage;
            offTrackText.color = Color.red;
            offTrackText.gameObject.SetActive(false);
        }
        if(wrongDirectionText != null)
        {
            wrongDirectionText.text = wrongDirectionMessage;
            wrongDirectionText.color = Color.red;
            wrongDirectionText.gameObject.SetActive(false);
        }
        if(moveRightText != null)
        {
            moveRightText.text = moveRightMessage;
            moveRightText.color = Color.red;
            moveRightText.gameObject.SetActive(false);
        }
        if (collisionText != null)
        {
            collisionText.gameObject.SetActive(false);
        }
        if (trafficViolationText != null)
        {
            trafficViolationText.gameObject.SetActive(false);
        }
        if (speedViolationText != null)
        {
            speedViolationText.text = speedMessage;
            speedViolationText.color = Color.red;
            speedViolationText.gameObject.SetActive(false);
        }

        // Optionally find the bike controller component if not assigned in the Inspector
        if (bikeController == null)
        {
            bikeController = GetComponent<ArcadeBP.ArcadeBikeController>();
        }

        if(speedMonitorScript != null)
        {
            speedMonitorScript = GetComponent<SpeedMonitor>();
        }
        if(pauseMenuScript != null){
            pauseMenuScript = GetComponent<PauseMenu>();
        }

        lastCollisionTime = CollisionTimeThreshold * (-1); // 첫 충돌은 반드시 일어나도록 하기 위해서
    }

    void Update()
    {
        UpdateScoreText();

        // 트랙 관련 텍스트 활성화/비활성화
        if(!speedMonitorScript.isInZone && !deductPoint_firstzone)
        {
            lastOffTrackTime = Time.time;
            deductPoint_firstzone = true;
            deductPoint_zone = false;
            //isInZone은 Zone 경계에서 민감하게 반응하므로 0.5초의 시간간격을 주고 감점한다. 
            // DeductPoints(penaltyPoints_zone);
            // offZoneText.gameObject.SetActive(true);
            // offTrackText.gameObject.SetActive(false);
            // wrongDirectionText.gameObject.SetActive(false);
            // moveRightText.gameObject.SetActive(false);
            StartCoroutine(OffTrackCheckRoutine());
        }
        else if(!speedMonitorScript.isInZone && deductPoint_firstzone){
            deductPoint_firstzone = true;
            if(Time.time - lastOffTrackTime >= 1.0f){
                lastOffTrackTime = Time.time + 1.0f;
                deductPoint_zone = true; 
                DeductPoints(penaltyPoints_zone);
                offZoneText.gameObject.SetActive(true);
                offTrackText.gameObject.SetActive(false);
                wrongDirectionText.gameObject.SetActive(false);
                moveRightText.gameObject.SetActive(false);
            }
        }
        else if(!speedMonitorScript.isInZone && deductPoint_zone)
        {
            deductPoint_zone = true;
            offZoneText.gameObject.SetActive(true);
            offTrackText.gameObject.SetActive(false);
            wrongDirectionText.gameObject.SetActive(false);
            moveRightText.gameObject.SetActive(false);
        }
        else if(!speedMonitorScript.isRightDirection && !deductPoint_zone)
        {
            deductPoint_firstzone = false;
            lastOffTrackTime = Time.time;
            deductPoint_zone = true;
            DeductPoints(penaltyPoints_zone);
            offZoneText.gameObject.SetActive(false);
            offTrackText.gameObject.SetActive(false);
            wrongDirectionText.gameObject.SetActive(true);
            moveRightText.gameObject.SetActive(false);
            StartCoroutine(OffTrackCheckRoutine());
        }
        else if(!speedMonitorScript.isRightDirection)
        {
            deductPoint_firstzone = false;
            deductPoint_zone = true;
            offZoneText.gameObject.SetActive(false);
            offTrackText.gameObject.SetActive(false);
            wrongDirectionText.gameObject.SetActive(true);
            moveRightText.gameObject.SetActive(false);
        }
        else if(!speedMonitorScript.isOnTrack && !deductPoint_zone)
        {
            deductPoint_firstzone = false;
            lastOffTrackTime = Time.time;
            deductPoint_zone = true;
            DeductPoints(penaltyPoints_zone);
            offZoneText.gameObject.SetActive(false);
            offTrackText.gameObject.SetActive(true);
            wrongDirectionText.gameObject.SetActive(false);
            moveRightText.gameObject.SetActive(false);
            StartCoroutine(OffTrackCheckRoutine());
        }
        else if(!speedMonitorScript.isOnTrack)
        {
            deductPoint_firstzone = false;
            deductPoint_zone = true;
            offZoneText.gameObject.SetActive(false);
            offTrackText.gameObject.SetActive(true);
            wrongDirectionText.gameObject.SetActive(false);
            moveRightText.gameObject.SetActive(false);
        }
        else if(!speedMonitorScript.isMoveRight && !deductPoint_zone)
        {
            deductPoint_firstzone = false;
            lastOffTrackTime = Time.time;
            deductPoint_zone = true;
            DeductPoints(penaltyPoints_zone);
            offZoneText.gameObject.SetActive(false);
            offTrackText.gameObject.SetActive(false);
            wrongDirectionText.gameObject.SetActive(false);
            moveRightText.gameObject.SetActive(true);
            StartCoroutine(OffTrackCheckRoutine());
        }
        else if(!speedMonitorScript.isMoveRight)
        {
            deductPoint_firstzone = false;
            deductPoint_zone = true;
            offZoneText.gameObject.SetActive(false);
            offTrackText.gameObject.SetActive(false);
            wrongDirectionText.gameObject.SetActive(false);
            moveRightText.gameObject.SetActive(true);
        }
        else
        {
            deductPoint_firstzone = false;
            deductPoint_zone = false;
            offZoneText.gameObject.SetActive(false);
            offTrackText.gameObject.SetActive(false);
            wrongDirectionText.gameObject.SetActive(false);
            moveRightText.gameObject.SetActive(false);
            StopCoroutine(OffTrackCheckRoutine());
        }

        // 속도 위반 처리
        if(speedMonitorScript.isSpeedViolationActive && !deductPoint_speedViolation)
        {
            lastSpeedViolationTime = Time.time;
            deductPoint_speedViolation = true;
            DeductPoints(penaltyPoints_speedViolation);
            speedViolationText.gameObject.SetActive(true);
            StartCoroutine(SpeedCheckRoutine());
        }
        else if(speedMonitorScript.isSpeedViolationActive)
        {
            deductPoint_speedViolation = true;
        }
        else
        {
            deductPoint_speedViolation = false;
            speedViolationText.gameObject.SetActive(false);
            StopCoroutine(SpeedCheckRoutine());
        }

        // 빨간 신호 위반 처리
        if(bikeController.isRedTrafficViolation && !deductPoint_redTrafficViolation)
        {
            lastRedTrafficViolationTime = Time.time;
            deductPoint_redTrafficViolation = true;
            DeductPoints(penaltyPoints_trafficViolation);
            if(trafficViolationText != null)
            {
                trafficViolationText.text = redMessage;
                trafficViolationText.color = Color.red;
                trafficViolationText.gameObject.SetActive(true);
                StartCoroutine(HideTrafficViolationTextAfterDelay(trafficDuration));
            }
            StartCoroutine(RedTrafficViolationCheckRoutine());
        }
        else if(bikeController.isRedTrafficViolation)
        {
            deductPoint_redTrafficViolation = true;
        }
        else
        {
            deductPoint_redTrafficViolation = false;
            StopCoroutine(RedTrafficViolationCheckRoutine());
        }

        // 초록 신호 위반 처리
        if(bikeController.isGreenTrafficViolation && !deductPoint_greenTrafficViolation)
        {
            lastGreenTrafficViolationTime = Time.time;
            deductPoint_greenTrafficViolation = true;
            DeductPoints(penaltyPoints_trafficViolation);
            if(trafficViolationText != null)
            {
                trafficViolationText.text = greenMessage;
                trafficViolationText.color = Color.green;
                trafficViolationText.gameObject.SetActive(true);
                StartCoroutine(HideTrafficViolationTextAfterDelay(trafficDuration));
            }
            StartCoroutine(GreenTrafficViolationCheckRoutine());
        }
        else if(bikeController.isGreenTrafficViolation)
        {
            deductPoint_greenTrafficViolation = true;
        }
        else
        {
            deductPoint_greenTrafficViolation = false;
            StopCoroutine(GreenTrafficViolationCheckRoutine());
        }

        // 사람과의 충돌 처리
        if(speedMonitorScript.collisionWithPerson &&(Time.time - lastCollisionTime >= CollisionTimeThreshold))
        {
            deductPoint_collision = true;
            StartCoroutine(CollisionCheckRoutine());
        }
        else if(speedMonitorScript.collisionWithPerson)
        {
            deductPoint_collision = false;
            StartCoroutine(CollisionCheckRoutine());
        }
        else
        {
            deductPoint_collision = false;
            StopCoroutine(CollisionCheckRoutine());
        }

        // 물체 혹은 건물과의 충돌 처리
        if(speedMonitorScript.collisionWithWall &&(Time.time - lastCollisionTime >= CollisionTimeThreshold))
        {
            deductPoint_collision = true;
            StartCoroutine(CollisionCheckRoutine());
        }
        else if(speedMonitorScript.collisionWithPerson)
        {
            deductPoint_collision = false;
            StartCoroutine(CollisionCheckRoutine());
        }
        else
        {
            deductPoint_collision = false;
            StopCoroutine(CollisionCheckRoutine());
        }

        //Zone 설명 창을 위해 pause 한 경우 모든 경고 글귀 제거
        if(pauseMenuScript.isPauseState){
            offZoneText.gameObject.SetActive(false);
            wrongDirectionText.gameObject.SetActive(false);
            offTrackText.gameObject.SetActive(false);
            moveRightText.gameObject.SetActive(false);
            collisionText.gameObject.SetActive(false);
            trafficViolationText.gameObject.SetActive(false);
            speedViolationText.gameObject.SetActive(false);
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    IEnumerator SpeedCheckRoutine()
    {
        while(deductPoint_speedViolation)
        {
            // Threshold 3초, 조정 가능
            if(Time.time - lastSpeedViolationTime >= speedViolationTimeThreshold)
            {
                lastSpeedViolationTime = Time.time;
                DeductPoints(penaltyPoints_speedViolation);
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator OffTrackCheckRoutine()
    {
        while(deductPoint_zone)
        {
            // Threshold 5초, 조정 가능
            if(Time.time - lastOffTrackTime >= OffTrackTimeThreshold)
            {
                lastOffTrackTime = Time.time;
                DeductPoints(penaltyPoints_zone);
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator RedTrafficViolationCheckRoutine()
    {
        while(deductPoint_redTrafficViolation)
        {
            if(Time.time - lastRedTrafficViolationTime >= RedTrafficViolationTimeThreshold)
            {
                lastRedTrafficViolationTime = Time.time;
                DeductPoints(penaltyPoints_trafficViolation);
                if(trafficViolationText != null)
                {
                    trafficViolationText.text = redMessage;
                    trafficViolationText.color = Color.red;
                    trafficViolationText.gameObject.SetActive(true);
                    StartCoroutine(HideTrafficViolationTextAfterDelay(trafficDuration));
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator GreenTrafficViolationCheckRoutine()
    {
        while(deductPoint_greenTrafficViolation)
        {
            if(Time.time - lastGreenTrafficViolationTime >= GreenTrafficViolationTimeThreshold)
            {
                lastGreenTrafficViolationTime = Time.time;
                DeductPoints(penaltyPoints_trafficViolation);
                if(trafficViolationText != null)
                {
                    trafficViolationText.text = greenMessage;
                    trafficViolationText.color = Color.green;
                    trafficViolationText.gameObject.SetActive(true);
                    StartCoroutine(HideTrafficViolationTextAfterDelay(trafficDuration));
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator CollisionCheckRoutine()
    {
        // Threshold 조정 가능
        speedMonitorScript.collisionWithPerson = false;
        if(deductPoint_collision)
        {
            Debug.Log("lastCollisionTime: " + lastCollisionTime);
            lastCollisionTime = Time.time;
            DeductPoints(penaltyPoints_collision);
            deductPoint_collision = false;
            if (collisionText != null)
            {
                collisionText.text = collisionMessage;
                collisionText.gameObject.SetActive(true);
                StartCoroutine(HideCollisionTextAfterDelay(collisionDuration));
            }
        }
        yield return new WaitForSeconds(0.1f);
    }
    
    // collisionText 일정 시간 동안 띄우는 코루틴
    private IEnumerator HideCollisionTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (collisionText != null)
        {
            collisionText.gameObject.SetActive(false);  // 일정 시간 후 텍스트 비활성화
        }
    }

    private IEnumerator HideTrafficViolationTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (trafficViolationText != null)
        {
            trafficViolationText.gameObject.SetActive(false);  // 일정 시간 후 텍스트 비활성화
        }
    }

    void DeductPoints(int points)
    {
        score -= points;
        UpdateScoreText();
    }
}

