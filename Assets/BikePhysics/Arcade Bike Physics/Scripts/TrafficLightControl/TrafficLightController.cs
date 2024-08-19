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
    public bool isGreenLight = false, isYellowLight = false, isRedLight = false;

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
    }

    // Update is called once per frame
    void Update()
    {
        _SigTime = Time.realtimeSinceStartup + _CycleStartTime;
        _SigTime = _SigTime - Mathf.Floor(_SigTime / _SectorTotTime) * _SectorTotTime;
        int idx0 = 0;
        float prevtim = 0.0f;
        float tottim = 0.0f;
        foreach (var tim in _SectorTimes)
        {
            tottim += tim;
            if ((prevtim < _SigTime) && (_SigTime <= tottim))
            {
                _Signals[idx0].SetActive(true);
                if(idx0 == 0){
                    isGreenLight = true; isYellowLight = false; isRedLight = false;
                }else if(idx0 == 1){
                    isGreenLight = false; isYellowLight = true; isRedLight = false;
                }else if(idx0 == 2){
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