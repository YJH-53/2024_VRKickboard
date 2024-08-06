using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Overspeed : MonoBehaviour
{
    public int score = 100; // Initial score
    public TMP_Text scoreText; // Text component to display the score
    public ArcadeBP.ArcadeBikeController bikeController; // Reference to the bike controller script
    public float speedThreshold = 30f; // Speed threshold for penalty
    public int penaltyPoints = 5; // Points to deduct
    private bool isOverSpeed = false; // To track if the speed is over the threshold

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
                float currentSpeed = bikeController.bikeVelocity.magnitude * 3.6f; // Convert m/s to km/h

                if (currentSpeed > speedThreshold)
                {
                    if (!isOverSpeed)
                    {
                        // If this is the first time over the speed threshold, deduct initial points
                        isOverSpeed = true;
                        DeductPoints(penaltyPoints);
                    }
                    else
                    {
                        // If the speed has been over the threshold for 5 seconds, deduct additional points
                        yield return new WaitForSeconds(5f);
                        DeductPoints(penaltyPoints);
                    }
                }
                else
                {
                    // Reset the overspeed flag when speed drops below the threshold
                    isOverSpeed = false;
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