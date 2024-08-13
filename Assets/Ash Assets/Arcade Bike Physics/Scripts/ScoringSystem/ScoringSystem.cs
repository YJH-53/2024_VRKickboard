using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoringSystem : MonoBehaviour
{
    public int score = 100; // Initial score
    public float OffTrackTimeThreshold = 5.0f; //경로 이탈 시 재감점 시간 간격
    public float CollisionTimeThreshold = 3.0f; //충돌 이후 재충돌 까지 감점 간격(물체랑 닿아 있는동안 계속 감점되는 거 방지)
    public TMP_Text scoreText; // Text component to display the score
    public TMP_Text collisionText; // Text component to display collision alert
    public ArcadeBP.ArcadeBikeController bikeController; // Reference to the bike controller script
    public SpeedMonitor speedMonitorScript;
    [HideInInspector]
    private int penaltyPoints_zone = 5; // 구간 규칙(경로 이탈, 과속 등) 위반 시 감점
    private int penaltyPoints_collision = 10; //장애물 충돌 시 감점 
    private bool deductPoint_zone = false;
    private bool deductPoint_collision = false;
    private string collisionMessage = "Collision Detected!";
    private float collisionDuration = 1.2f;
    private float lastOffTrackTime, lastCollisionTime;

    void Start()
    {
        // Display the initial score
        UpdateScoreText();
        // Hide collision message initially
        if (collisionText != null)
        {
            collisionText.gameObject.SetActive(false);
        }

        // Optionally find the bike controller component if not assigned in the Inspector
        if (bikeController == null)
        {
            bikeController = GetComponent<ArcadeBP.ArcadeBikeController>();
        }

        if(speedMonitorScript != null){
            speedMonitorScript = GetComponent<SpeedMonitor>();
        }
        lastCollisionTime = CollisionTimeThreshold * (-1); //첫 충돌은 반드시 일어나도록 하기 위해서
    }

    void Update(){
        UpdateScoreText();
        if(speedMonitorScript.isEffectActive && !deductPoint_zone){
            lastOffTrackTime = Time.time;
            deductPoint_zone = true;
            DeductPoints(penaltyPoints_zone);
            StartCoroutine(SpeedCheckRoutine());
        }else if(speedMonitorScript.isEffectActive){
            deductPoint_zone = true;
        }else{
            deductPoint_zone = false;
            StopCoroutine(SpeedCheckRoutine());
        }

        if(speedMonitorScript.collisionWithPerson && (Time.time - lastCollisionTime >= CollisionTimeThreshold)){
            deductPoint_collision = true;
            StartCoroutine(CollisionCheckRoutine());
        }else if(speedMonitorScript.collisionWithPerson){
            deductPoint_collision = false;
            StartCoroutine(CollisionCheckRoutine());
        }else{
            deductPoint_collision = false;
            StopCoroutine(CollisionCheckRoutine());
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
        while(deductPoint_zone){
            // Threshold 5초, 조정 가능
            if(Time.time - lastOffTrackTime >= OffTrackTimeThreshold){
                lastOffTrackTime = Time.time;
                DeductPoints(penaltyPoints_zone);
            }
            // Wait for a short duration before checking again
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator CollisionCheckRoutine(){
        // Threshold 조정 가능
        speedMonitorScript.collisionWithPerson = false;
        if(deductPoint_collision){  
            Debug.Log("lastCollisionTime: " + lastCollisionTime);
            lastCollisionTime = Time.time;
            DeductPoints(penaltyPoints_collision);
            deductPoint_collision = false;
            if (collisionText != null){
                collisionText.text = collisionMessage;
                collisionText.gameObject.SetActive(true);
                StartCoroutine(HideCollisionTextAfterDelay(collisionDuration));
            }
        }
        yield return new WaitForSeconds(0.1f);
    
    }

    //collisionText 일정 시간동안 띄우는 coroutine
    private IEnumerator HideCollisionTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (collisionText != null)
        {
            collisionText.gameObject.SetActive(false);  // Hide the text after the delay
        }
    }

    void DeductPoints(int points)
    {
        score -= points;
        UpdateScoreText();
    }
}