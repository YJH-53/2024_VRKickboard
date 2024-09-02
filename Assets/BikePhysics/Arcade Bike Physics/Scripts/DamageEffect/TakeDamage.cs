using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class TakeDamage : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject scooterPreset;
    public ArcadeBP.ArcadeBikeController bikeController;
    private SpeedMonitor speedMonitorScript;
    PostProcessVolume _volume;
    Vignette _vignette;
    [HideInInspector]
    public float intensity = 0;
    public bool damageEffectTriggered = false;
    public Coroutine damageEffectCoroutine = null;

    void Start()
    {
        _volume = GetComponent<PostProcessVolume>();
        _volume.profile.TryGetSettings<Vignette>(out _vignette);
        if(!_vignette){
            print("error, vignette empty");
        }else{
            _vignette.enabled.Override(false);
        }
        if(speedMonitorScript == null)
        {
            speedMonitorScript = scooterPreset.GetComponent<SpeedMonitor>();
        }
        if(bikeController != null){
            bikeController = scooterPreset.GetComponent<ArcadeBP.ArcadeBikeController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Damage Layer이 뜨는 상황을 설정하는 부분, 구간에 따른 이탈 상황 + 사람과 충돌
        if((speedMonitorScript.isEffectActive) && !damageEffectTriggered){
            damageEffectCoroutine = StartCoroutine(TakeDamageEffect());
        }
        if(speedMonitorScript.collisionWithPerson || bikeController.isRedTrafficViolation || bikeController.isGreenTrafficViolation){
            TriggerDamageEffect();
        }
    }

    public IEnumerator TakeDamageEffect()
    {
        damageEffectTriggered = true;
        intensity = 0.4f;
        _vignette.enabled.Override(true);
        _vignette.intensity.Override(0.4f);

        yield return new WaitForSeconds(0.4f);
        
        while(intensity > 0){
            intensity -= 0.04f;
            if(intensity < 0) intensity = 0;
            _vignette.intensity.Override(intensity);
            yield return new WaitForSeconds(0.1f);
        }

        //_vignette.enabled.Override(false);
        damageEffectTriggered = false;
        damageEffectCoroutine = null;
        yield break;
    }

   void TriggerDamageEffect(){
        if(damageEffectCoroutine != null){
            StopCoroutine(damageEffectCoroutine);
        }
        damageEffectCoroutine = StartCoroutine(TakeDamageEffect());
    }
}
