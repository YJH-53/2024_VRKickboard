using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficPedLightController : MonoBehaviour
{
    [System.Serializable]
    public struct ProgressSignalSet
    {
        [SerializeField]
        public GameObject _MainLamp;
        [SerializeField]
        public List<GameObject> _ProgressLampSet;
    }

    [SerializeField]
    public ProgressSignalSet _GreenSignals = new ProgressSignalSet();
    [SerializeField]
    public ProgressSignalSet _RedSignals = new ProgressSignalSet();
    

    [SerializeField]
    private float _CycleStartTime = 5.0f;
    [SerializeField]
    private float _GreenTime = 10.0f;
    [SerializeField]
    private float _GreenFlashTime = 2.0f;
    [SerializeField]
    private float _RedTime = 10.0f;

    [SerializeField]
    private float _SigTime = 0.0f;

    private float _GreenSectorTime = 12.0f;
    private float _GreenProgressTime = 1.5f;
    private float _TotSectorTime = 12.0f;
    private float _RedProgressTime = 1.5f;
    public List<GameObject> _Signals = new List<GameObject>();

    [HideInInspector]
    public bool isGreenLight = false, isRedLight = false, scooterDetected = false;
    public float scooterDetectionRadius = 30.0f, timeThreshold = 0;
    private int scooterDetectCount = 0;
    private GameObject zone = null;

    // Start is called before the first frame update
    void Start()
    {
        _Signals.Add(_GreenSignals._MainLamp);
        _Signals.Add(_RedSignals._MainLamp);
        _GreenSectorTime = _GreenTime + _GreenFlashTime;
        _GreenProgressTime = _GreenSectorTime / (float)_GreenSignals._ProgressLampSet.Count;
        _TotSectorTime = _RedTime + _GreenSectorTime;
        _RedProgressTime = _RedTime / (float)_RedSignals._ProgressLampSet.Count;
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
        _SigTime = _SigTime - Mathf.Floor(_SigTime / _TotSectorTime) * _TotSectorTime;
        if(scooterDetected || scooterDetectCount != 0){
            if(scooterDetectCount == 0){
                scooterDetectCount = 1;
            }
            if(scooterDetectCount == 1){
                timeThreshold = Time.time;
                scooterDetectCount = 2;
            }
            if (_GreenSectorTime < _SigTime)
            {
                for (int i = 0; i < _GreenSignals._ProgressLampSet.Count; i++)
                {
                    _GreenSignals._ProgressLampSet[i].SetActive(false);
                }
                _GreenSignals._MainLamp.SetActive(false);
                isGreenLight = false; isRedLight = true;
            }
            else
            {
                int blueLampNum = Mathf.CeilToInt((_GreenSectorTime - _SigTime) / _GreenProgressTime);
                for (int i = 0; i < blueLampNum; i++)
                {
                    _GreenSignals._ProgressLampSet[i].SetActive(true);
                }
                for (int i = blueLampNum; i < _GreenSignals._ProgressLampSet.Count; i++)
                {
                    _GreenSignals._ProgressLampSet[i].SetActive(false);
                }
                if (_SigTime < _GreenTime)
                    _GreenSignals._MainLamp.SetActive(true);
                else
                    _GreenSignals._MainLamp.SetActive((_SigTime - (int)_SigTime) < 0.5f);

                for (int i = 0; i < _RedSignals._ProgressLampSet.Count; i++)
                {
                    _RedSignals._ProgressLampSet[i].SetActive(false);
                }
                isGreenLight = true; isRedLight = false;
            }

            float rSigTime = _SigTime - _GreenSectorTime;
            if (rSigTime < 0.0f)
            {
                _RedSignals._MainLamp.SetActive(false);
                isGreenLight = true; isRedLight = false;
            }
            else
            {

                int redLampNum = Mathf.CeilToInt((_RedTime - rSigTime) / _RedProgressTime);
                for (int i = 0; i < redLampNum; i++)
                {
                    _RedSignals._ProgressLampSet[i].SetActive(true);
                }
                for (int i = redLampNum; i < _RedSignals._ProgressLampSet.Count; i++)
                {
                    _RedSignals._ProgressLampSet[i].SetActive(false);
                }
                _RedSignals._MainLamp.SetActive(true);
                isGreenLight = false; isRedLight = true;
            }

        }else{
            if(_CycleStartTime == 0 && scooterDetectCount == 0)
            {
                _GreenSignals._MainLamp.SetActive(true);
                _RedSignals._MainLamp.SetActive(false);
                for (int i = 0; i < _GreenSignals._ProgressLampSet.Count; i++)
                {
                    _GreenSignals._ProgressLampSet[i].SetActive(true);
                }
                for (int i = 0; i < _RedSignals._ProgressLampSet.Count; i++)
                {
                    _RedSignals._ProgressLampSet[i].SetActive(false);
                }
                isGreenLight = true; isRedLight = false;
            }else if(_CycleStartTime == 7 && scooterDetectCount == 0){
                _GreenSignals._MainLamp.SetActive(false);
                for (int i = 0; i < _GreenSignals._ProgressLampSet.Count; i++)
                {
                    _GreenSignals._ProgressLampSet[i].SetActive(false);
                }
                for (int i = 0; i < _RedSignals._ProgressLampSet.Count; i++)
                {
                    _RedSignals._ProgressLampSet[i].SetActive(true);
                }
                _RedSignals._MainLamp.SetActive(true);
                isGreenLight = false; isRedLight = true;
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
