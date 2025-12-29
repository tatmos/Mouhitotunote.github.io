using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// ゲームのオーディオ（BGM, SFX, 環境音）を管理するクラス
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Clips")]
        private AudioClip[] wordGetSounds;
        private AudioClip creditsBGM;
        private AudioClip selectionBGM;
        private AudioClip typewriterSound;
        private AudioClip lostLetterSound;
        private AudioClip sparkleSound;
        private AudioClip buttonHoverSound;
        private AudioClip thunderSound;
        private AudioClip[] ambientSounds;

        private AudioSource bgmAudioSource;
        private AudioSource sfxAudioSource;
        private AudioSource ambientAudioSource;
        private AudioLowPassFilter bgmLowPassFilter;

        private Coroutine fadeOutCoroutine;
        private Coroutine fadeInCoroutine;
        private Coroutine sfxFadeOutCoroutine;
        private Coroutine ambientFadeOutCoroutine;
        private Coroutine ambientFadeInCoroutine;
        private Coroutine lowPassFadeCoroutine;
        private Coroutine pitchFadeCoroutine;
        private Coroutine selectionBGMVolumeCoroutine;

        private float selectionBGMPausedTime = 0f;
        private bool isSelectionBGMPlaying = false;
        private int currentAmbientScenarioId = -1;

        private const float LowPassNormalCutoff = 22000f;
        private const float LowPassMuffledCutoff = 1000f;
        private const float NormalPitch = 1.0f;
        private const float LoweredPitch = 0.5f;
        private const float SelectionBGMNormalVolume = 1.0f;
        private const float SelectionBGMLoweredVolume = 0.5f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeAudioSources();
        }

        private void InitializeAudioSources()
        {
            // BGM専用のGameObject
            GameObject bgmObject = new GameObject("BGMPlayer");
            bgmObject.transform.SetParent(this.transform);
            bgmAudioSource = bgmObject.AddComponent<AudioSource>();
            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.volume = 1f;
            
            bgmLowPassFilter = bgmObject.AddComponent<AudioLowPassFilter>();
            bgmLowPassFilter.cutoffFrequency = LowPassNormalCutoff;
            bgmLowPassFilter.enabled = true;
            
            // 効果音専用のGameObject
            GameObject sfxObject = new GameObject("SFXPlayer");
            sfxObject.transform.SetParent(this.transform);
            sfxAudioSource = sfxObject.AddComponent<AudioSource>();
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.volume = 1f;
            
            // 環境音専用のGameObject
            GameObject ambientObject = new GameObject("AmbientPlayer");
            ambientObject.transform.SetParent(this.transform);
            ambientAudioSource = ambientObject.AddComponent<AudioSource>();
            ambientAudioSource.playOnAwake = false;
            ambientAudioSource.volume = 0.5f;
            ambientAudioSource.loop = true;
        }

        public void SetAudioClips(
            AudioClip[] wordGetSounds, 
            AudioClip creditsBGM, 
            AudioClip selectionBGM, 
            AudioClip typewriterSound, 
            AudioClip lostLetterSound, 
            AudioClip sparkleSound, 
            AudioClip buttonHoverSound, 
            AudioClip thunderSound, 
            AudioClip[] ambientSounds)
        {
            this.wordGetSounds = wordGetSounds;
            this.creditsBGM = creditsBGM;
            this.selectionBGM = selectionBGM;
            this.typewriterSound = typewriterSound;
            this.lostLetterSound = lostLetterSound;
            this.sparkleSound = sparkleSound;
            this.buttonHoverSound = buttonHoverSound;
            this.thunderSound = thunderSound;
            this.ambientSounds = ambientSounds;
        }

        public void PlayWordGetSound()
        {
            if (wordGetSounds != null && wordGetSounds.Length > 0 && sfxAudioSource != null)
            {
                int randomIndex = Random.Range(0, wordGetSounds.Length);
                AudioClip selectedSound = wordGetSounds[randomIndex];
                if (selectedSound != null)
                {
                    sfxAudioSource.PlayOneShot(selectedSound);
                    FadeOutAmbientSound();
                }
            }
        }

        public void PlaySparkleSound()
        {
            if (sparkleSound != null && sfxAudioSource != null)
            {
                sfxAudioSource.PlayOneShot(sparkleSound);
            }
        }

        public void PlayHoverSound()
        {
            if (buttonHoverSound != null && sfxAudioSource != null)
            {
                sfxAudioSource.PlayOneShot(buttonHoverSound);
            }
        }

        public void PlayThunderSound()
        {
            if (thunderSound != null && sfxAudioSource != null)
            {
                sfxAudioSource.PlayOneShot(thunderSound);
            }
        }

        public void PlayCreditsBGM()
        {
            if (creditsBGM != null && bgmAudioSource != null)
            {
                StopFadeOutBGM();
                FadeOutAmbientSound();
                bgmAudioSource.clip = creditsBGM;
                bgmAudioSource.loop = true;
                bgmAudioSource.volume = 1f;
                bgmAudioSource.Play();
            }
        }

        public void FadeOutBGM(float duration)
        {
            if (bgmAudioSource == null) return;
            StopFadeOutBGM();
            fadeOutCoroutine = StartCoroutine(FadeOutAudioCoroutine(duration));
        }

        private void StopFadeOutBGM()
        {
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = null;
            }
        }

        private IEnumerator FadeOutAudioCoroutine(float duration)
        {
            float startVolume = bgmAudioSource.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            bgmAudioSource.Stop();
            bgmAudioSource.volume = startVolume;
            fadeOutCoroutine = null;
            StartCoroutine(CheckAndFadeInAmbientAfterBGM());
        }

        private IEnumerator CheckAndFadeInAmbientAfterBGM()
        {
            yield return new WaitForSeconds(0.1f);
            if (!IsAnyAudioPlaying() && ambientAudioSource != null && ambientAudioSource.isPlaying)
            {
                ambientFadeInCoroutine = StartCoroutine(FadeInAmbientSoundCoroutine(1f));
            }
        }

        public void FadeOutAudioOnSceneChange()
        {
            if (sfxAudioSource != null && sfxAudioSource.isPlaying)
            {
                if (sfxFadeOutCoroutine != null) StopCoroutine(sfxFadeOutCoroutine);
                sfxFadeOutCoroutine = StartCoroutine(FadeOutSfxAudioCoroutine(0.5f));
            }
            
            if (bgmLowPassFilter != null)
            {
                if (lowPassFadeCoroutine != null) { StopCoroutine(lowPassFadeCoroutine); lowPassFadeCoroutine = null; }
                bgmLowPassFilter.cutoffFrequency = LowPassNormalCutoff;
            }

            StartCoroutine(CheckAndFadeInAmbientAfterSfx());
            
            if (bgmAudioSource != null)
            {
                if (pitchFadeCoroutine != null) { StopCoroutine(pitchFadeCoroutine); pitchFadeCoroutine = null; }
                bgmAudioSource.pitch = NormalPitch;
            }
        }

        private IEnumerator FadeOutSfxAudioCoroutine(float duration)
        {
            float startVolume = sfxAudioSource.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                sfxAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            sfxAudioSource.Stop();
            sfxAudioSource.volume = startVolume;
            sfxFadeOutCoroutine = null;
        }

        private IEnumerator CheckAndFadeInAmbientAfterSfx()
        {
            yield return new WaitForSeconds(0.1f);
            if (!IsAnyAudioPlaying() && ambientAudioSource != null && ambientAudioSource.isPlaying)
            {
                ambientFadeInCoroutine = StartCoroutine(FadeInAmbientSoundCoroutine(1f));
            }
        }

        public void StartSelectionBGM()
        {
            if (selectionBGM == null || bgmAudioSource == null) return;
            if (bgmAudioSource.clip == selectionBGM && bgmAudioSource.isPlaying) return;

            bgmAudioSource.clip = selectionBGM;
            bgmAudioSource.loop = true;
            bgmAudioSource.time = selectionBGMPausedTime;
            bgmAudioSource.volume = 0f;
            bgmAudioSource.Play();
            isSelectionBGMPlaying = true;
            
            if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);
            fadeInCoroutine = StartCoroutine(FadeInAudioCoroutine(2f));
            FadeOutAmbientSound();
        }

        public void PauseSelectionBGM()
        {
            if (bgmAudioSource != null && bgmAudioSource.clip == selectionBGM && bgmAudioSource.isPlaying)
            {
                StartCoroutine(FadeOutAndPauseSelectionBGM(1f));
            }
        }

        private IEnumerator FadeOutAndPauseSelectionBGM(float duration)
        {
            float startVolume = bgmAudioSource.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            selectionBGMPausedTime = bgmAudioSource.time;
            bgmAudioSource.Pause();
            isSelectionBGMPlaying = false;
        }

        private IEnumerator FadeInAudioCoroutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bgmAudioSource.volume = Mathf.Lerp(0f, SelectionBGMNormalVolume, elapsed / duration);
                yield return null;
            }
            bgmAudioSource.volume = SelectionBGMNormalVolume;
            fadeInCoroutine = null;
        }

        public void LowerSelectionBGMVolume()
        {
            if (bgmAudioSource != null && bgmAudioSource.clip == selectionBGM && bgmAudioSource.isPlaying)
            {
                if (selectionBGMVolumeCoroutine != null) StopCoroutine(selectionBGMVolumeCoroutine);
                selectionBGMVolumeCoroutine = StartCoroutine(FadeSelectionBGMVolume(bgmAudioSource.volume, SelectionBGMLoweredVolume, 1f));

                bool isDarkMode = GameManager.Instance != null && GameManager.Instance.IsDarkMode();
                if (isDarkMode)
                {
                    StartCoroutine(FadePitch(LoweredPitch, 1f));
                }
                else
                {
                    StartCoroutine(FadeLowPassFilter(LowPassMuffledCutoff, 1f));
                }
            }
        }

        private IEnumerator FadeSelectionBGMVolume(float fromVolume, float toVolume, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bgmAudioSource.volume = Mathf.Lerp(fromVolume, toVolume, elapsed / duration);
                yield return null;
            }
            bgmAudioSource.volume = toVolume;
        }

        private IEnumerator FadeLowPassFilter(float targetCutoff, float duration)
        {
            if (bgmLowPassFilter == null) yield break;
            float startCutoff = bgmLowPassFilter.cutoffFrequency;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bgmLowPassFilter.cutoffFrequency = Mathf.Lerp(startCutoff, targetCutoff, elapsed / duration);
                yield return null;
            }
            bgmLowPassFilter.cutoffFrequency = targetCutoff;
        }

        private IEnumerator FadePitch(float targetPitch, float duration)
        {
            if (bgmAudioSource == null) yield break;
            float startPitch = bgmAudioSource.pitch;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bgmAudioSource.pitch = Mathf.Lerp(startPitch, targetPitch, elapsed / duration);
                yield return null;
            }
            bgmAudioSource.pitch = targetPitch;
        }

        public void StartAmbientSound(int scenarioId)
        {
            int index = scenarioId - 1;
            if (ambientSounds != null && index >= 0 && index < ambientSounds.Length && ambientSounds[index] != null)
            {
                if (currentAmbientScenarioId == scenarioId && ambientAudioSource.isPlaying) return;
                
                currentAmbientScenarioId = scenarioId;
                ambientAudioSource.clip = ambientSounds[index];
                ambientAudioSource.volume = 0f;
                ambientAudioSource.Play();
                
                if (ambientFadeInCoroutine != null) StopCoroutine(ambientFadeInCoroutine);
                ambientFadeInCoroutine = StartCoroutine(FadeInAmbientSoundCoroutine(2f));
            }
        }

        public void StopAmbientSound()
        {
            if (ambientAudioSource != null && ambientAudioSource.isPlaying)
            {
                ambientAudioSource.Stop();
                currentAmbientScenarioId = -1;
            }
        }

        public void FadeOutAmbientSound()
        {
            if (ambientAudioSource != null && ambientAudioSource.isPlaying)
            {
                if (ambientFadeOutCoroutine != null) StopCoroutine(ambientFadeOutCoroutine);
                ambientFadeOutCoroutine = StartCoroutine(FadeOutAmbientSoundCoroutine(1f));
            }
        }

        public void FadeOutAmbientSoundForResult()
        {
            if (ambientAudioSource != null && ambientAudioSource.isPlaying)
            {
                if (ambientFadeOutCoroutine != null) StopCoroutine(ambientFadeOutCoroutine);
                ambientFadeOutCoroutine = StartCoroutine(FadeOutAmbientSoundCoroutine(5f));
            }
        }

        private IEnumerator FadeInAmbientSoundCoroutine(float duration)
        {
            float targetVolume = 0.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                ambientAudioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }
            ambientAudioSource.volume = targetVolume;
            ambientFadeInCoroutine = null;
        }

        private IEnumerator FadeOutAmbientSoundCoroutine(float duration)
        {
            float startVolume = ambientAudioSource.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                ambientAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            ambientAudioSource.Stop();
            ambientAudioSource.volume = startVolume;
            ambientFadeOutCoroutine = null;
        }

        public bool IsAnyAudioPlaying()
        {
            bool bgmPlaying = bgmAudioSource != null && bgmAudioSource.isPlaying && bgmAudioSource.volume > 0.1f;
            bool sfxPlaying = sfxAudioSource != null && sfxAudioSource.isPlaying && sfxAudioSource.volume > 0.1f;
            return bgmPlaying || sfxPlaying;
        }

        public AudioSource GetBgmAudioSource() => bgmAudioSource;
        public AudioClip GetCreditsBGM() => creditsBGM;
        public AudioClip GetSelectionBGM() => selectionBGM;
        public AudioClip GetTypewriterSound() => typewriterSound;
        public AudioClip GetLostLetterSound() => lostLetterSound;
    }
}
