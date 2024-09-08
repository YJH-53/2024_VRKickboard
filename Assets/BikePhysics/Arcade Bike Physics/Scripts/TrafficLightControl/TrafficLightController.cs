using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    [SerializeField]
    private float _SigTime = 0.0f;

    [SerializeField]
    private float _CycleStartTime = 5.0f;

    [SerializeField]
    public List<GameObject> _Signals = new List<GameObject>();
    //_Signals라는 list에 각각의 GameObject가 SetActive로 조절되는 중, 순서대로 green, yellow, red

    [SerializeField]
    private List<float> _SectorTimes = new List<float>();
    [HideInInspector]
    public bool isGreenLight = false, isYellowLight = false, isRedLight = false, scooterDetected = false;
    private GameObject zone = null;
    public float scooterDetectionRadius = 30.0f, timeThreshold = 0;
    private int scooterDetectCount = 0; //scooter 첫 접근을 확인하는 용도. 첫 detect 이후에는 신호등은 계속 작동하도록 함. 

    private float _SectorTotTime = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        if (_Signals.Count == 0)
            throw new System.Exception();
        else if (_SectorTimes.Count == 0)
            throw new System.Exception();
        else if (_Signals.Count != _SectorTimes.Count)
            throw new System.Exception();

        foreach (var tim in _SectorTimes)
            _SectorTotTime += tim;
        zone = transform.Find("TrafficZone").gameObject;
        if(zone == null){
            Debug.LogError("Traffic Zone is Not Assigned");
        }
    }

    // Update is called once per frame
    void Update()
    {
        scooterDetected = DetectScooter();
        _SigTime = Time.time + _CycleStartTime - timeThreshold;
        _SigTime = _SigTime - Mathf.Floor(_SigTime / _SectorTotTime) * _SectorTotTime;

        if (!scooterDetected && scooterDetectCount == 0)
        {
            // Before scooter is detected, keep only the green light on
            _Signals[0].SetActive(true); // Green light on
            _Signals[1].SetActive(false); // Yellow light off
            _Signals[2].SetActive(false); // Red light off

            isGreenLight = true;
            isYellowLight = false;
            isRedLight = false;
        }
        else
        {
            if (scooterDetectCount == 0)
            {
                scooterDetectCount = 1;
            }

            if (scooterDetectCount == 1)
            {
                timeThreshold = Time.time;
                scooterDetectCount = 2;
            }

            // Cycle through lights based on _SigTime and _SectorTimes
            int idx0 = 0;
            float prevtim = 0.0f;
            float tottim = 0.0f;
            foreach (var tim in _SectorTimes)
            {
                tottim += tim;
                if ((prevtim < _SigTime) && (_SigTime <= tottim))
                {
                    _Signals[idx0].SetActive(true);
                    if (idx0 == 0)
                    {
                        isGreenLight = true; isYellowLight = false; isRedLight = false;
                    }
                    else if (idx0 == 1)
                    {
                        isGreenLight = false; isYellowLight = true; isRedLight = false;
                    }
                    else if (idx0 == 2)
                    {
                        isGreenLight = false; isYellowLight = false; isRedLight = true;
                    }
                }
                else
                {
                    _Signals[idx0].SetActive(false);
                }
                prevtim = tottim;
                idx0++;
            }
        }
    }



    bool DetectScooter()
    {
        // Find scooter within detection radius
        ArcadeBP.ArcadeBikeController scooterScript = FindObjectOfType<ArcadeBP.ArcadeBikeController>();

        if(scooterScript != null){
            GameObject scooterObject = scooterScript.gameObject;
            float distanceToScooter = Vector3.Distance(zone.transform.position, scooterObject.transform.position);
            if(distanceToScooter <= scooterDetectionRadius){
                return true;
            }
        }
        return false;
    }
}