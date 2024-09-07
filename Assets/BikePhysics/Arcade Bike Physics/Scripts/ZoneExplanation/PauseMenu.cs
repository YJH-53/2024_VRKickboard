using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public float pauseTime = 5.0f;
    [SerializeField]
    public List<GameObject> _Canvas_Objects = new List<GameObject>();
    public GameObject otherCanvasObject;
    public ArcadeBP.ArcadeBikeController bikeController;
    [HideInInspector]
    private int i = 0;
    public float showPerson_time = 0f; //Zone7에서 사람을 보여주기 시작하는 용도
    public bool isPauseState = false;
    public List<bool> isActive, isEffectTriggered;
    public List<int> countList;
    private List<float> effectStartTime;

    // Start is called before the first frame update
    void Start()
    {
        isActive = new List<bool>(new bool[_Canvas_Objects.Count]);
        isEffectTriggered = new List<bool>(new bool[_Canvas_Objects.Count]);
        effectStartTime = new List<float>(new float[_Canvas_Objects.Count]);
        for(i = 0; i < _Canvas_Objects.Count; i++){
            isActive[i] = false; isEffectTriggered[i] = false; effectStartTime[i] = 0f;
        }
        foreach(GameObject canvasObject in _Canvas_Objects){
            canvasObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        bikeController.isPause = isPauseState;
        if(bikeController != null){
            isActive[0] = (bikeController.enterZone0_Count == 1);
            isActive[1] = (bikeController.enterZone1_Count == 1);
            isActive[2] = (bikeController.enterZone2_Count == 1);
            isActive[3] = (bikeController.enterZone3_Count == 1);
            isActive[4] = (bikeController.enterZone4_Count == 1);
            isActive[5] = (bikeController.enterZone5_Count == 1);
            isActive[6] = (bikeController.enterZone6_Count == 1);
            isActive[7] = (bikeController.enterZone7_Count == 1);
            isActive[8] = (bikeController.enterZone8_Count == 1);
        }
        // Debug.Log("isActive: " + isActive);
        for(i = 0; i < _Canvas_Objects.Count; i++){
            if(isActive[i] && !isEffectTriggered[i]){
                effectStartTime[i] = Time.realtimeSinceStartup;
                _Canvas_Objects[i].SetActive(true);
                StartCoroutine(PauseCoroutine());
                isEffectTriggered[i] = true;
            }else if(isActive[i]){
                if(Time.realtimeSinceStartup - effectStartTime[i] >= pauseTime){
                    isPauseState = false;
                    Time.timeScale = 1.0f;
                    isEffectTriggered[i] = false;
                    if(i == 0)
                        bikeController.enterZone0_Count = 2;
                    else if(i == 1)
                        bikeController.enterZone1_Count = 2;
                    else if(i == 2)
                        bikeController.enterZone2_Count = 2;
                    else if(i == 3)
                        bikeController.enterZone3_Count = 2;
                    else if(i == 4)
                        bikeController.enterZone4_Count = 2;
                    else if(i == 5)
                        bikeController.enterZone5_Count = 2;
                    else if(i == 6)
                        bikeController.enterZone6_Count = 2;
                    else if(i == 7){
                        bikeController.enterZone7_Count = 2;
                        showPerson_time = Time.time;
                    }
                    else if(i == 8)
                        bikeController.enterZone8_Count = 2;
                    foreach(Transform child in otherCanvasObject.transform){
                        if(child.gameObject.name.Contains("Score") || child.gameObject.name.Contains("Speed")){
                            child.gameObject.SetActive(true);
                        }
                    }
                }else{
                    isEffectTriggered[i] = true;
                }
            }else{
                isEffectTriggered[i] = false;
                _Canvas_Objects[i].SetActive(false);
            }
        }
    }

    IEnumerator PauseCoroutine(){
        isPauseState = true;
        foreach(Transform child in otherCanvasObject.transform){
            child.gameObject.SetActive(false);
        }
        Debug.Log("Game paused at Zone 0!");
        Time.timeScale = 0f;
        yield return new WaitForSeconds(0.1f);
    }
}
