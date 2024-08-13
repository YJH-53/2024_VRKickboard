using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uniuni
{
    public class TrafficSignalProgressCtrl : MonoBehaviour
    {
        [System.Serializable]
        private struct ProgressSignalSet
        {
            [SerializeField]
            public GameObject _MainLamp;
            [SerializeField]
            public List<GameObject> _ProgressLampSet;
        }

        [SerializeField]
        private ProgressSignalSet _GreenSignals = new ProgressSignalSet();
        [SerializeField]
        private ProgressSignalSet _RedSignals = new ProgressSignalSet();

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

        // Start is called before the first frame update
        void Start()
        {
            _GreenSectorTime = _GreenTime + _GreenFlashTime;
            _GreenProgressTime = _GreenSectorTime / (float)_GreenSignals._ProgressLampSet.Count;
            _TotSectorTime = _RedTime + _GreenSectorTime;
            _RedProgressTime = _RedTime / (float)_RedSignals._ProgressLampSet.Count;
        }

        // Update is called once per frame
        void Update()
        {
            _SigTime = Time.realtimeSinceStartup + _CycleStartTime;
            _SigTime = _SigTime - Mathf.Floor(_SigTime / _TotSectorTime) * _TotSectorTime;

            if (_GreenSectorTime < _SigTime)
            {
                for (int i = 0; i < _GreenSignals._ProgressLampSet.Count; i++)
                {
                    _GreenSignals._ProgressLampSet[i].SetActive(false);
                }
                _GreenSignals._MainLamp.SetActive(false);
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
            }

            float rSigTime = _SigTime - _GreenSectorTime;
            if (rSigTime < 0.0f)
            {
                _RedSignals._MainLamp.SetActive(false);
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
            }

        }
    }

}