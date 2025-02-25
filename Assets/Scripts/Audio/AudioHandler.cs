using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    public enum Sound
    {
        //Errors
        CheckFastener, CheckKeyway, CheckSidelap, TryDifferentShake,
        //Introduction
        Welcome, IntroInThisTraining, IntroThisIsHandsOn, LetsGetStarted,
        //After Intro Starting game
        TheStarterUsed, BeginByChalking,
        // After Chalking first line
        PlaceAndFasten,
        //After Placing first Starter
        Two1and34, EnsureFasteners,
        //After fastening first Starter
        ContinueinstallingStarters,
        //After fastening second Starter
        A316gaporkeyway,



        

        //Cut the shake
        CutShakesLeaving,
        // After installing last starter
        MaximumExposureis,ChalkLineto,SelectYourDesired,

        //After Selecting exposure
        InstallFirstCourseOf, MaintainAMinimumKeyway, Minimum112, FieldShakesAre, SelectDifferentShake,

        // After planing first shake
        Avoidcombinations,

        //After planing last shake of course
        CutTheShake,

        //After cut shake is installed
        SolidShakesAre,BeginTheNextCourse


    }


    public bool introductionDone = false;
    public bool showDimensions = false;
    public bool showStarterDimensions;


    public static AudioHandler Instance { get; private set; }
    public AudioSource audioSource; // Single AudioSource for simplicity, add more if needed
    public List<AudioClip> audioClips; // List to store all your audio clips

    private Dictionary<Sound, AudioClip> soundClipDictionary;

    public Action DoBeforeSound;
    public Action DoAfterSound;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSoundDictionary()
    {
        soundClipDictionary = new Dictionary<Sound, AudioClip>();
        foreach (var clip in audioClips)
        {
            Sound sound;
            if (Enum.TryParse<Sound>(clip.name, true, out sound))  // Correctly parsing the enum from clip name
            {
                if (!soundClipDictionary.ContainsKey(sound))  // Ensure no duplicates
                {
                    soundClipDictionary[sound] = clip;
                }
                else
                {
                    Debug.LogWarning("Duplicate sound entry: " + sound);
                }
            }
            else
            {
                Debug.LogError("AudioClip name does not match any Sound enum: " + clip.name);
            }
        }
    }

    public void PlaySound(Sound sound)
    {
        DoBeforeSound?.Invoke();

        if (soundClipDictionary.TryGetValue(sound, out AudioClip clip))
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Sound not found in dictionary: " + sound);
        }
        StartCoroutine(DoAfter(clip.length + 0.5f));
    }

    public void PlayListOfSound(List<Sound> sounds)
    {
        StartCoroutine(PlaySoundsCoroutine(sounds));
    }

    IEnumerator PlaySoundsCoroutine(List<Sound> sounds)
    {
        foreach (Sound s in sounds)
        {
            PlaySound(s);
            yield return new WaitForSeconds(GetClip(s).length);
        }
    }
    IEnumerator DoAfter(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        DoAfterSound?.Invoke();
    }


    public void StopSound()
    {
        audioSource.Stop();
    }


    public void StartIntroductionSound()
    {
        StartCoroutine(playIntroClips());
    }

   public AudioClip GetClip(Sound sound)
    {
        soundClipDictionary.TryGetValue(sound, out AudioClip clip);
        return clip;
    }

    IEnumerator playIntroClips()
    {
        PlaySound(Sound.Welcome);
        yield return new WaitForSeconds(GetClip(Sound.Welcome).length + 0.5f);
        PlaySound(Sound.IntroInThisTraining);
        yield return new WaitForSeconds(GetClip(Sound.IntroInThisTraining).length + 0.5f);
        PlaySound(Sound.IntroThisIsHandsOn);
        yield return new WaitForSeconds(GetClip(Sound.IntroThisIsHandsOn).length + 0.5f);
        PlaySound(Sound.LetsGetStarted);
        yield return new WaitForSeconds(GetClip(Sound.LetsGetStarted).length + 2);

        StartCoroutine(playAfterIntroClips());
    }

    IEnumerator playAfterIntroClips()
    {
        PlaySound(Sound.TheStarterUsed);
        showDimensions = true;
        yield return new WaitForSeconds(GetClip(Sound.TheStarterUsed).length + 0.5f);
        showDimensions = false;
        showStarterDimensions = true;
        PlaySound(Sound.BeginByChalking);
        yield return new WaitForSeconds(GetClip(Sound.BeginByChalking).length + 0.5f);
        showStarterDimensions = false;
        introductionDone = true;
    }
}
