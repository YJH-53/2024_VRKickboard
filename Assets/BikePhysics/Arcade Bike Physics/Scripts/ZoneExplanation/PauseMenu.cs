using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public float pauseTime = 5.0f;
    public bool isStart = true, isEnd = false; //게임의 시작, 끝 화면에서 띄우는 변수
    [SerializeField]
    public List<GameObject> _Canvas_Objects = new List<GameObject>();
    public GameObject otherCanvasObject;
    public ArcadeBP.ArcadeBikeController bikeController;
    public SpeedMonitor speedMonitor;
    public ScoringSystem scoringSystem;
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
            isActive[7] = (speedMonitor.zone_num == 7 && scoringSystem.isPass);
            isActive[8] = (speedMonitor.zone_num == 7 && !scoringSystem.isPass);
            //9번 인덱스를 시작 화면으로 작성하셈. 
        }
        // Debug.Log("isActive: " + isActive);
        for(i = 0; i < _Canvas_Objects.Count; i++){
            if(i >= 0 && i <= 6){
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
                        else if(i == 6){
                            bikeController.enterZone6_Count = 2;
                            showPerson_time = Time.time;
                        }else if(i == 7)
                            bikeController.enterZone7_Count = 2;
                        else if(i == 8)
                            bikeController.enterZone7_Count = 2;
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
            }else if(i == 7 || i == 8){
                //화면 end하는 코드
                if(isActive[i]){
                    isEnd = true;
                    effectStartTime[i] = Time.realtimeSinceStartup;
                    _Canvas_Objects[i].SetActive(true);
                    StartCoroutine(PauseCoroutine());
                }
            }else if(i==9){
                isStart = true;
                //시작 화면 논리는 조웅찬이 작성하셈. 
            }
        }
    }

    IEnumerator PauseCoroutine(){
        isPauseState = true;
        //Pause 상태에서 bike 속도 0으로 만들기
        bikeController.bikeBody.velocity = Vector3.zero;
        bikeController.rb.velocity = Vector3.zero;
        bikeController.rb.angularVelocity = Vector3.zero;
        bikeController.verticalInput = 0f;
        foreach(Transform child in otherCanvasObject.transform){
            child.gameObject.SetActive(false);
        }
        Debug.Log("Game paused at Zone 0!");
        Time.timeScale = 0f;
        yield return new WaitForSeconds(0.1f);
    }
}
