using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedMonitor : MonoBehaviour
{
    public ArcadeBP.ArcadeBikeController bikeController;
    public TMP_Text speedText;
    public TakeDamage takeDamageScript;
    public float speedThreshold = 30f;
    private bool damageEffectTriggered = false;
    private bool isEffectActive = false;


    void Start()
    {
        // Optionally find the components if not assigned in the Inspector
        if (bikeController == null)
        {
            bikeController = GetComponent<ArcadeBP.ArcadeBikeController>();
        }

        if (takeDamageScript == null)
        {
            takeDamageScript = GetComponent<TakeDamage>();
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

            if (speedText != null)
            {
                speedText.text = "Speed: " + currentSpeed.ToString("F2") + " km/h";
            }

            if (currentSpeed > speedThreshold && !damageEffectTriggered)
            {
                if (!isEffectActive)
                {
                    StartCoroutine(ContinuousDamageEffect());
                }
            }
            else
            {
                isEffectActive = false; // Reset the effect status when speed drops below the threshold
            }
        }
    }

    private IEnumerator ContinuousDamageEffect()
    {
        isEffectActive = true;
        while (isEffectActive)
        {
            yield return StartCoroutine(takeDamageScript.TakeDamageEffect());
        }
    }
}