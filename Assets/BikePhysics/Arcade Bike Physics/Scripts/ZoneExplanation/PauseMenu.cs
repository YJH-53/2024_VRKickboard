using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public float pauseTime = 5.0f;
    public bool isStart = false, isEnd = false; //게임의 시작, 끝 화면에서 띄우는 변수
    [SerializeField]
    public List<GameObject> _Canvas_Objects = new List<GameObject>();
    public GameObject otherCanvasObject;
    public ArcadeBP.ArcadeBikeController bikeController;
    public SpeedMonitor speedMonitor;
    public ScoringSystem scoringSystem;
    public MessageListener messageListener;
    // public Initializer initializer;

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
        Debug.Log("isStart " + isStart);
        isActive = new List<bool>(new bool[_Canvas_Objects.Count]);
        isEffectTriggered = new List<bool>(new bool[_Canvas_Objects.Count]);
        effectStartTime = new List<float>(new float[_Canvas_Objects.Count]);
        for (i = 0; i < _Canvas_Objects.Count - 1; i++)
        {
            isActive[i] = false; isEffectTriggered[i] = false; effectStartTime[i] = 0f;
        }

        isActive[9] = true; // 9번째 객체를 시작 시 활성화
        isEffectTriggered[9] = true;
        effectStartTime[9] = 0f;

        foreach (GameObject canvasObject in _Canvas_Objects)
        {
            canvasObject.SetActive(false);
        }

        _Canvas_Objects[9].SetActive(true); // 시작 화면 표시
    }

    // Update is called once per frame
    void Update()
    {
        bikeController.isPause = isPauseState;

        // 게임이 시작되었는지 여부에 따라 로직 실행
        if (bikeController != null && isStart)
        {
            isActive[0] = (bikeController.enterZone0_Count == 1);
            isActive[1] = (bikeController.enterZone1_Count == 1);
            isActive[2] = (bikeController.enterZone2_Count == 1);
            isActive[3] = (bikeController.enterZone3_Count == 1);
            isActive[4] = (bikeController.enterZone4_Count == 1);
            isActive[5] = (bikeController.enterZone5_Count == 1);
            isActive[6] = (bikeController.enterZone6_Count == 1);
            isActive[7] = (speedMonitor.zone_num == 7 && scoringSystem.isPass);
            isActive[8] = (speedMonitor.zone_num == 7 && !scoringSystem.isPass);
        }

        for (i = 0; i < 10; i++)
        {
            if (isStart)
            {
                if (i >= 0 && i <= 6)
                {
                    if (isActive[i] && !isEffectTriggered[i])
                    {
                        effectStartTime[i] = Time.realtimeSinceStartup;
                        _Canvas_Objects[i].SetActive(true);
                        StartCoroutine(PauseCoroutine());
                        isEffectTriggered[i] = true;
                    }
                    else if (isActive[i])
                    {
                        if (Time.realtimeSinceStartup - effectStartTime[i] >= pauseTime)
                        {
                            isPauseState = false;
                            Time.timeScale = 1.0f;
                            isEffectTriggered[i] = false;
                            if (i == 0)
                                bikeController.enterZone0_Count = 2;
                            else if (i == 1)
                                bikeController.enterZone1_Count = 2;
                            else if (i == 2)
                                bikeController.enterZone2_Count = 2;
                            else if (i == 3)
                                bikeController.enterZone3_Count = 2;
                            else if (i == 4)
                                bikeController.enterZone4_Count = 2;
                            else if (i == 5)
                                bikeController.enterZone5_Count = 2;
                            else if (i == 6)
                            {
                                bikeController.enterZone6_Count = 2;
                                showPerson_time = Time.time;
                            }
                            else if (i == 7)
                                bikeController.enterZone7_Count = 2;
                            else if (i == 8)
                                bikeController.enterZone7_Count = 2;
                            foreach (Transform child in otherCanvasObject.transform)
                            {
                                if (child.gameObject.name.Contains("Score") || child.gameObject.name.Contains("Speed"))
                                {
                                    child.gameObject.SetActive(true);
                                }
                            }
                        }
                        else
                        {
                            isEffectTriggered[i] = true;
                        }
                    }
                    else
                    {
                        isEffectTriggered[i] = false;
                        _Canvas_Objects[i].SetActive(false);
                    }
                }
                else if (i == 7 || i == 8)
                {
                    // 화면 end하는 코드
                    if (isActive[i])
                    {
                        isEnd = true;
                        effectStartTime[i] = Time.realtimeSinceStartup;
                        _Canvas_Objects[i].SetActive(true);
                        StartCoroutine(Pause_End());
                    }
                }
            }
            if (i == 9)
            {
                Debug.Log("Start Menu");
                if (isActive[i])
                {
                    _Canvas_Objects[i].SetActive(true);
                    StartCoroutine(Pause_Start());
                }
            }

        }
    }

    IEnumerator PauseCoroutine()
    {
        isPauseState = true;
        //Pause 상태에서 bike 속도 0으로 만들기
        bikeController.bikeBody.velocity = Vector3.zero;
        bikeController.rb.velocity = Vector3.zero;
        bikeController.rb.angularVelocity = Vector3.zero;
        bikeController.verticalInput = 0f;
        foreach (Transform child in otherCanvasObject.transform)
        {
            child.gameObject.SetActive(false);
        }
        Debug.Log("Game paused at Zone 0!");
        Time.timeScale = 0f;
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator Pause_End()
    {
        isPauseState = true;
        bikeController.bikeBody.velocity = Vector3.zero;
        bikeController.rb.velocity = Vector3.zero;
        bikeController.rb.angularVelocity = Vector3.zero;
        bikeController.verticalInput = 0f;

        foreach (Transform child in otherCanvasObject.transform)
        {
            child.gameObject.SetActive(false);
        }
        Time.timeScale = 0f;

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Input Received! Returning to Start Menu.");
                _Canvas_Objects[7].SetActive(false);
                _Canvas_Objects[8].SetActive(false);

                isActive[7] = false;
                isActive[8] = false;
                isActive[9] = true;
                
                isStart = false; // 게임 재시작을 위해 isStart를 false로 설정
                _Canvas_Objects[9].SetActive(true); // 시작 화면으로 회귀

                /***** Initialize를 여기서 해줘야 함 ******/
                break;
            }
            yield return null;
        }
    }

    IEnumerator Pause_Start()
    {
        isPauseState = true;
        bikeController.bikeBody.velocity = Vector3.zero;
        bikeController.rb.velocity = Vector3.zero;
        bikeController.rb.angularVelocity = Vector3.zero;
        bikeController.verticalInput = 0f;
        foreach (Transform child in otherCanvasObject.transform)
        {
            child.gameObject.SetActive(false);
        }

        // 스페이스 바 입력 대기
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Input Received! Starting game.");
                _Canvas_Objects[9].SetActive(false); // 9번째 객체 비활성화
                isActive[9] = false;  // 9번째 객체 비활성화 플래그 설정
                isStart = true; // 게임 시작 플래그 설정
                break;
            }
            yield return null;
        }

        // 게임이 시작되었으므로 시간 흐름 재개
        Time.timeScale = 1.0f;
        yield return null;
    }
}
