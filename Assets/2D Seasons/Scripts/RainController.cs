using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainController : MonoBehaviour {
    //my day night cycle
    public DayNightCycle2D dayNightCycle;
    //Particals
    public ParticleSystem rainEmitter;
    public ParticleSystem fogEmitter;
    //dull slate for rain shadow effect (it's kind of hard to explain)
    public Renderer fogSlate;
    Color slateColor;
    //Min Max Delays in cycles
    public float rainSeasonDelayMin = 5.0f;
    public float rainSeasonDelayMax = 7.0f;
    public float rainDurationMin = 0.5f;
    public float rainDurationMax = 0.8f;
    public float nextStormMin = 3;
    public float nextStormMax = 5;
    public float lightningDelayMin = 0.05f;
    public float lightningDelayMax = 0.07f;
    //Lightning presets and spawns
    public GameObject[] lightiningSpawns;
    public GameObject[] lightningPresets;
    public float lightningVisualsDuration = 0.05f;
    public SpriteRenderer lightningGradient;
    float nextStrikeIn = 0.0f;
    GameObject currentStrike;
    float totalTimePassed = 0.0f;
    bool isRaining = false;
    bool isStorming = false;
    float nextRainIn = 0.0f;
    float stopRainIn = 0.0f;
    float rainsHappened = 0.0f;
    float nextStormIn = 0.0f;
    //SFX controls 
    public AudioClip[] rainSFX;
    public AudioClip[] thunderSFX;
    AudioSource audioPlayer;
    public AudioSource ambienceSource;
    public AudioClip birdChirp;
    public AudioClip cricketsChirp;
    bool singleCallOuterOnWork = false;
    // Use this for initialization
    void Start () {

        //a smart man starts with setting defaults
        if (fogSlate)
            slateColor = fogSlate.material.color;
        if (transform.GetComponent<AudioSource>())
        {
            audioPlayer = transform.GetComponent<AudioSource>();
        }
        else {
            Debug.Log("Please assign a audio source component to " + gameObject.name.ToString());
        }

        nextRainIn = Random.Range(rainSeasonDelayMin, rainSeasonDelayMax);
        nextStormIn = Random.Range(nextStormMin, nextStormMax);
    }
	
	// Update is called once per frame
	void Update () {
        if (!dayNightCycle || !rainEmitter || !fogEmitter)
        {
            Debug.LogWarning("Some variables not assigned");
            return;
        }

        totalTimePassed = dayNightCycle.TotalTimePassed();
        ControlRain();
        AmbienceAudio();
	}
    //controls rain
    void ControlRain()
    {

        if (!singleCallOuterOnWork && !isRaining && totalTimePassed >= nextRainIn)
        {
            isRaining = true;
            rainsHappened++;
            if (rainsHappened >= nextStormIn) {
                isStorming = true;
                nextStormIn = rainsHappened + Random.Range(nextStormMin, nextStormMax);
            }
            stopRainIn = Random.Range(rainDurationMin, rainDurationMax);
            stopRainIn += totalTimePassed;
            nextRainIn = stopRainIn + Random.Range(rainSeasonDelayMin, rainSeasonDelayMax);
            if (audioPlayer)
            {
                RainAudio();
            }
        }

            Rain();

        
    }
    //Enable and disable Rain
    void Rain() {
        ParticleSystem.EmissionModule rainEm = rainEmitter.emission;

        if (isRaining)
            rainEm.enabled = true;
        else
            rainEm.enabled = false;

        ParticleSystem.EmissionModule stormEm = fogEmitter.emission;



        if (isStorming)
        {
            stormEm.enabled = true;
            Lightning();
        }
        else
        {
            stormEm.enabled = false;
        }

        if (!singleCallOuterOnWork)
        {
            if (totalTimePassed >= stopRainIn)
            {
                rainEm.enabled = false;
                isRaining = false;
                if (isStorming)
                    isStorming = false;
                stormEm.enabled = false;
                if (audioPlayer)
                {
                    RainAudio();
                }
            }
        }
        
        
    }
    //Controls lightning
    void Lightning() {
        int randForPoint = Random.Range(0, lightiningSpawns.Length);
        int randForPreset = Random.Range(0, lightningPresets.Length);

        if (!lightiningSpawns[randForPoint] || !lightningPresets[randForPreset]) {
            return;
        }

        if (nextStrikeIn < totalTimePassed && !currentStrike)
        {
            lightningPresets[randForPreset].transform.position = lightiningSpawns[randForPoint].transform.position;
            lightningPresets[randForPreset].SetActive(true);
            currentStrike = lightningPresets[randForPreset];
            if (lightningGradient)
                lightningGradient.enabled = true;
            Invoke("DisableCurrentStrike", lightningVisualsDuration);
            nextStrikeIn = totalTimePassed + Random.Range(lightningDelayMin, lightningDelayMax);
            int randThunder = Random.Range(0, thunderSFX.Length);

            if (thunderSFX[randThunder] && audioPlayer)
                audioPlayer.PlayOneShot(thunderSFX[randThunder], 1.0f);

        }

        if (currentStrike) {
            if (currentStrike.GetComponent<SpriteRenderer>()) {
                currentStrike.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, Random.Range(.5f, 1f));
            }

            if (lightningGradient) {
                lightningGradient.color = new Color(255, 255, 255, Random.Range(.5f, 1f));
            }
                
        }

    }
    //Disables the strike
    void DisableCurrentStrike()
    {
        if (currentStrike)
        {
            currentStrike.SetActive(false);
            currentStrike = null;
        }
        if (lightningGradient)
            lightningGradient.enabled = false;
    }
    //Control Rain Audio
    void RainAudio() {
        if (isRaining)
        {
            int randRain = Random.Range(0, rainSFX.Length);
            if (rainSFX[randRain])
            {
                audioPlayer.clip = rainSFX[randRain];
                audioPlayer.Play();
                StartCoroutine(AudioIncrement());
            }
        }
        else {
            StartCoroutine(AudioDecrement());
        }
    }
    //Sound fade routine
    IEnumerator AudioIncrement() {
        float lastTime = totalTimePassed;
        if (!audioPlayer)
            yield return null;

        while (audioPlayer.volume < .49f)
        {
            audioPlayer.volume += (totalTimePassed - lastTime) * 8.0f;
            if (fogSlate)
            {
                fogSlate.material.color = Color.Lerp(fogSlate.material.color, new Color(slateColor.r, slateColor.g, slateColor.b, 0.4f), audioPlayer.volume);
            }
            
            lastTime = totalTimePassed;
            yield return null;
        }
        if (fogSlate)
            fogSlate.material.color = new Color(slateColor.r, slateColor.g, slateColor.b, 0.4f);

        audioPlayer.volume = 0.5f;

        yield return null;
    }

    //Sound fade routine
    IEnumerator AudioDecrement()
    {
        float lastTime = totalTimePassed;
        if (!audioPlayer)
            yield return null;

        while (audioPlayer.volume > .01f)
        {
            audioPlayer.volume -= (totalTimePassed - lastTime) * 8.0f;
            if (fogSlate)
            {
                fogSlate.material.color = Color.Lerp(Color.clear, new Color(slateColor.r, slateColor.g, slateColor.b, 0.4f), audioPlayer.volume);
            }

            lastTime = totalTimePassed;
            yield return null;
        }

        if (fogSlate)
            fogSlate.material.color = Color.clear;

        audioPlayer.volume = 0.0f;

        yield return null;
    }
    //Controls ambience audio , bird chirping and crickets
    void AmbienceAudio() {
        if (!ambienceSource || !birdChirp || !cricketsChirp) {
            Debug.LogWarning("Some variables missing");
            return;
        }

        float timeOfTheDay = totalTimePassed- Mathf.FloorToInt(totalTimePassed);

        if (isRaining) {

        }

        if (timeOfTheDay < 0.5f) {//Day
            if (ambienceSource.clip != birdChirp) {

                StartCoroutine(AmbientSoundFade(birdChirp));
              
            }
            
        }
        else {
            if (ambienceSource.clip != cricketsChirp)
            {
                StartCoroutine(AmbientSoundFade(cricketsChirp));
                
            }
        }

        
        
    }
    //Sound fade routine
    IEnumerator AmbientSoundFade( AudioClip to) {
        float lastTime = totalTimePassed;
        while (ambienceSource.volume > 0.1f) {
            ambienceSource.volume -= (totalTimePassed - lastTime) * 8.0f;
            lastTime = totalTimePassed;
            yield return null;
        }
        ambienceSource.volume = 0.0f;
        ambienceSource.clip = to;
        ambienceSource.Play();

        while (ambienceSource.volume < 0.9f)
        {
            ambienceSource.volume += (totalTimePassed - lastTime) * 8.0f;
            lastTime = totalTimePassed;
            yield return null;
        }

        ambienceSource.volume = 1.0f;

        yield return null;
    }

    //SingleCallOuters


    public void KeepOnRaining() {
        singleCallOuterOnWork = true;
        isRaining = true;
        rainsHappened++;
        isStorming = false;
        Rain();
        if (audioPlayer)
        {
            RainAudio();
        }
    }

    public void ResetToAutoMode()
    {
        singleCallOuterOnWork = false;
     
    }

    public void KeepOnStorming (){
        singleCallOuterOnWork = true;
        isRaining = true;
        rainsHappened++;
        isStorming = true;
        Rain();
        if (audioPlayer)
        {
            RainAudio();
        }
    }

    public void NoRainNoStorm(){
        singleCallOuterOnWork = true;
        isRaining = false;
        isStorming = false;
        Rain();
        if (audioPlayer)
        {
            RainAudio();
        }
    }
}
