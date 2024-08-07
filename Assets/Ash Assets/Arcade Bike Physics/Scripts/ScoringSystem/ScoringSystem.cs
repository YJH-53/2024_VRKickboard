using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoringSystem : MonoBehaviour
{
    public int score = 100; // Initial score
    public TMP_Text scoreText; // Text component to display the score
    public ArcadeBP.ArcadeBikeController bikeController; // Reference to the bike controller script
    public SpeedMonitor speedMonitorScript;
    public float speedThreshold = 30f; // Speed threshold for penalty
    public int penaltyPoints = 5; // Points to deduct
    private bool deductPoint = false; // To track if the speed is over the threshold

    void Start()
    {
        // Display the initial score
        UpdateScoreText();

        // Optionally find the bike controller component if not assigned in the Inspector
        if (bikeController == null)
        {
            bikeController = GetComponent<ArcadeBP.ArcadeBikeController>();
        }

        // Start monitoring the speed
        StartCoroutine(SpeedCheckRoutine());
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
        while (true)
        {
            // Check if the speed exceeds the threshold
            if (bikeController != null)
            {
                if (speedMonitorScript.isEffectActive)
                {
                    if (!deductPoint)
                    {
                        // If this is the first time over the speed threshold, deduct initial points
                        deductPoint = true;
                        DeductPoints(penaltyPoints);
                    }
                    else
                    {
                        // Threshold 5초, 조정 가능
                        yield return new WaitForSeconds(5f);
                        DeductPoints(penaltyPoints);
                    }
                }
                else
                {
                    // Reset the overspeed flag when speed drops below the threshold
                    deductPoint = false;
                }
            }

            // Wait for a short duration before checking again
            yield return new WaitForSeconds(0.1f);
        }
    }

    void DeductPoints(int points)
    {
        score -= points;
        UpdateScoreText();
    }
}