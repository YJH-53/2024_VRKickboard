using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu6 : MonoBehaviour
{
    public float pauseTime = 5.0f;
    public GameObject canvasObject;
    public GameObject otherCanvasObject;
    public ArcadeBP.ArcadeBikeController bikeController;
    [HideInInspector]
    public bool isActive = false, isEffectTriggered = false;
    private float effectStartTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        canvasObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(bikeController != null){
            isActive = (bikeController.enterZone6_Count == 1);
        }
        // Debug.Log("isActive: " + isActive);
        if(isActive && !isEffectTriggered){
            effectStartTime = Time.realtimeSinceStartup;
            StartCoroutine(PauseCoroutine());
            isEffectTriggered = true;
        }else if(isActive){
            if(Time.realtimeSinceStartup - effectStartTime >= pauseTime){
                Time.timeScale = 1.0f;
                isEffectTriggered = false;
                bikeController.enterZone6_Count = 2;
                foreach(Transform child in otherCanvasObject.transform){
                    if(child.gameObject.name.Contains("Score") || child.gameObject.name.Contains("Speed")){
                        child.gameObject.SetActive(true);
                    }
                }
            }else{
                isEffectTriggered = true;
            }
        }else{
            isEffectTriggered = false;
            canvasObject.SetActive(false);
        }
    }

    IEnumerator PauseCoroutine(){
        canvasObject.SetActive(true);
        foreach(Transform child in otherCanvasObject.transform){
            child.gameObject.SetActive(false);
        }
        Debug.Log("Game paused at Zone 6!");
        Time.timeScale = 0f;
        yield return new WaitForSeconds(0.1f);
    }
}
