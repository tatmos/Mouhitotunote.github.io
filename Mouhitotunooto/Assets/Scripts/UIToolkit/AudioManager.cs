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
        private AudioClip wordGetIncreaseSound; // ワードゲット数が増える時の効果音
        private AudioClip wordGetDecreaseSound; // ワードゲット数が減る時の効果音
        private AudioClip creditsBGM;
        private AudioClip selectionBGM;
        private AudioClip selectionBGMMuffled; // ローパスフィルター済みのselectionBGM（Webビルド用）
        private AudioClip typewriterSound;
        private AudioClip lostLetterSound;
        private AudioClip sparkleSound;
        private AudioClip buttonHoverSound;
        private AudioClip thunderSound;
        private AudioClip truthDoorUnlockSound;
        private AudioClip[] ambientSounds;

        [Header("Audio Mixer")]
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup bgmMixerGroup;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup sfxMixerGroup;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup ambientMixerGroup;

        private AudioSource bgmAudioSource;
        private AudioSource creditBgmAudioSource;
        private AudioSource sfxAudioSource;
        private AudioSource ambientAudioSource;
        private AudioLowPassFilter bgmLowPassFilter;
        
        // Webビルドかどうかを判定（ローパスフィルターが動作しないため）
        private bool isWebBuild => Application.platform == RuntimePlatform.WebGLPlayer;

        private Coroutine fadeOutCoroutine;
        private Coroutine fadeInCoroutine;
        private Coroutine sfxFadeOutCoroutine;
        private Coroutine ambientFadeOutCoroutine;
        private Coroutine ambientFadeInCoroutine;
        private Coroutine lowPassFadeCoroutine;
        private Coroutine pitchFadeCoroutine;
        private Coroutine selectionBGMVolumeCoroutine;

        private float selectionBGMPausedTime = 0f;
        private int currentAmbientScenarioId = -1;

        private const float LowPassNormalCutoff = 22000f;
        private const float LowPassMuffledCutoff = 1000f;
        private const float NormalPitch = 1.0f;
        private const float LoweredPitch = 0.5f;
        private const float SelectionBGMNormalVolume = 1.0f;
        private const float SelectionBGMLoweredVolume = 0.75f;
        
        // 歪み効果のピッチ変動用パラメータ（歪みシェーダーと同期）
        private const float DistortionSpeed = 1.0f; // 歪みシェーダーの_DistortionSpeedと同じ値
        private const float DistortionFrequency = 10.0f; // 歪みシェーダーの_DistortionFrequencyと同じ値
        private const float PitchVariationSemitones = 1.0f; // 1半音の変動
        private const float SemitoneRatio = 1.059463094359f; // 2^(1/12) ≈ 1.059（1半音の周波数比）
        
        // ピッチ変動中のSE用AudioSourceのリスト
        private List<AudioSource> pitchVariationAudioSources = new List<AudioSource>();

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
            // 保存された音量を復元（デフォルトは1.0）
            float savedVolume = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
            bgmAudioSource.volume = savedVolume;
            bgmAudioSource.outputAudioMixerGroup = bgmMixerGroup;
            
            // Webビルドではローパスフィルターが動作しないため、フィルターコンポーネントは追加しない
            if (!isWebBuild)
            {
                bgmLowPassFilter = bgmObject.AddComponent<AudioLowPassFilter>();
                bgmLowPassFilter.cutoffFrequency = LowPassNormalCutoff;
                bgmLowPassFilter.enabled = true;
            }
            
            // クレジットBGM専用のGameObject
            GameObject creditBgmObject = new GameObject("CreditBGMPlayer");
            creditBgmObject.transform.SetParent(this.transform);
            creditBgmAudioSource = creditBgmObject.AddComponent<AudioSource>();
            creditBgmAudioSource.playOnAwake = false;
            // BGM音量を適用（保存された音量を復元）
            float savedBGMVolume = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
            creditBgmAudioSource.volume = savedBGMVolume;
            creditBgmAudioSource.outputAudioMixerGroup = bgmMixerGroup;
            
            // 効果音専用のGameObject
            GameObject sfxObject = new GameObject("SFXPlayer");
            sfxObject.transform.SetParent(this.transform);
            sfxAudioSource = sfxObject.AddComponent<AudioSource>();
            sfxAudioSource.playOnAwake = false;
            // 保存されたSE音量を復元（デフォルトは1.0）
            float savedSEVolume = PlayerPrefs.GetFloat("SEVolume", 1.0f);
            sfxAudioSource.volume = savedSEVolume;
            sfxAudioSource.outputAudioMixerGroup = sfxMixerGroup;
            
            // 環境音専用のGameObject
            GameObject ambientObject = new GameObject("AmbientPlayer");
            ambientObject.transform.SetParent(this.transform);
            ambientAudioSource = ambientObject.AddComponent<AudioSource>();
            ambientAudioSource.playOnAwake = false;
            ambientAudioSource.volume = 0.5f;
            ambientAudioSource.loop = true;
            ambientAudioSource.outputAudioMixerGroup = ambientMixerGroup;
        }

        public void SetAudioClips(
            AudioClip[] wordGetSounds, 
            AudioClip wordGetIncreaseSound,
            AudioClip wordGetDecreaseSound,
            AudioClip creditsBGM, 
            AudioClip selectionBGM, 
            AudioClip selectionBGMMuffled,
            AudioClip typewriterSound, 
            AudioClip lostLetterSound, 
            AudioClip sparkleSound, 
            AudioClip buttonHoverSound, 
            AudioClip thunderSound, 
            AudioClip truthDoorUnlockSound,
            AudioClip[] ambientSounds)
        {
            this.wordGetSounds = wordGetSounds;
            this.wordGetIncreaseSound = wordGetIncreaseSound;
            this.wordGetDecreaseSound = wordGetDecreaseSound;
            this.creditsBGM = creditsBGM;
            this.selectionBGM = selectionBGM;
            this.selectionBGMMuffled = selectionBGMMuffled;
            this.typewriterSound = typewriterSound;
            this.lostLetterSound = lostLetterSound;
            this.sparkleSound = sparkleSound;
            this.buttonHoverSound = buttonHoverSound;
            this.thunderSound = thunderSound;
            this.truthDoorUnlockSound = truthDoorUnlockSound;
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
                    PlaySoundWithPitchVariation(selectedSound);
                    FadeOutAmbientSound();
                }
            }
        }

        /// <summary>
        /// ワードゲット数が増える時の効果音を再生
        /// </summary>
        public void PlayWordGetIncreaseSound()
        {
            if (wordGetIncreaseSound != null && sfxAudioSource != null)
            {
                PlaySoundWithPitchVariation(wordGetIncreaseSound);
                FadeOutAmbientSound();
            }
        }

        /// <summary>
        /// ワードゲット数が減る時の効果音を再生
        /// </summary>
        public void PlayWordGetDecreaseSound()
        {
            if (wordGetDecreaseSound != null && sfxAudioSource != null)
            {
                PlaySoundWithPitchVariation(wordGetDecreaseSound);
            }
        }

        /// <summary>
        /// ワードゲット音を逆再生（ダークモードでワードが奪われる時の効果音）
        /// </summary>
        public void PlayWordGetSoundReversed()
        {
            if (wordGetSounds != null && wordGetSounds.Length > 0 && sfxAudioSource != null)
            {
                int randomIndex = Random.Range(0, wordGetSounds.Length);
                AudioClip selectedSound = wordGetSounds[randomIndex];
                if (selectedSound != null)
                {
                    // 逆再生用のAudioSourceを作成
                    GameObject reversedSoundObject = new GameObject("ReversedWordGetSound");
                    reversedSoundObject.transform.SetParent(this.transform);
                    AudioSource reversedAudioSource = reversedSoundObject.AddComponent<AudioSource>();
                    reversedAudioSource.playOnAwake = false;
                    reversedAudioSource.volume = sfxAudioSource.volume;
                    reversedAudioSource.outputAudioMixerGroup = sfxMixerGroup;
                    reversedAudioSource.clip = selectedSound;
                    
                    // 逆再生: timeSamplesを最後から開始し、pitchを負の値に設定
                    reversedAudioSource.timeSamples = selectedSound.samples - 1;
                    reversedAudioSource.pitch = -1f;
                    
                    bool isDarkMode = GameManager.Instance != null && GameManager.Instance.IsDarkMode();
                    if (isDarkMode)
                    {
                        // ダークモード時はピッチ変動を適用
                        pitchVariationAudioSources.Add(reversedAudioSource);
                        StartCoroutine(ApplyPitchVariationToReversedSE(reversedAudioSource, selectedSound.length));
                    }
                    else
                    {
                        reversedAudioSource.Play();
                    }
                    
                    // 再生終了後にGameObjectを削除
                    StartCoroutine(DestroyAfterPlay(reversedSoundObject, selectedSound.length));
                }
            }
        }

        /// <summary>
        /// 逆再生SEのピッチを歪みに合わせて揺らすコルーチン
        /// </summary>
        private IEnumerator ApplyPitchVariationToReversedSE(AudioSource audioSource, float duration)
        {
            float startTime = Time.time;
            
            while (audioSource != null && audioSource.isPlaying && (Time.time - startTime) < duration)
            {
                float time = Time.time * DistortionSpeed;
                float pitchVariation = Mathf.Sin(time * DistortionFrequency) * 0.5f + 
                                       Mathf.Cos(time * DistortionFrequency * 0.7f) * 0.5f;
                float pitchOffset = pitchVariation * PitchVariationSemitones;
                float pitchMultiplier = Mathf.Pow(SemitoneRatio, pitchOffset);
                
                // 逆再生なので、ベースピッチを-1.0にして変動を適用
                audioSource.pitch = -1.0f * pitchMultiplier;
                
                yield return null;
            }
            
            if (audioSource != null)
            {
                pitchVariationAudioSources.Remove(audioSource);
            }
        }

        /// <summary>
        /// 再生終了後にGameObjectを削除するコルーチン
        /// </summary>
        private IEnumerator DestroyAfterPlay(GameObject obj, float duration)
        {
            yield return new WaitForSeconds(duration);
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        public void PlaySparkleSound()
        {
            if (sparkleSound != null && sfxAudioSource != null)
            {
                PlaySoundWithPitchVariation(sparkleSound);
            }
        }

        public void PlayHoverSound()
        {
            if (buttonHoverSound != null && sfxAudioSource != null)
            {
                PlaySoundWithPitchVariation(buttonHoverSound);
            }
        }

        public void PlayThunderSound()
        {
            if (thunderSound != null && sfxAudioSource != null)
            {
                PlaySoundWithPitchVariation(thunderSound);
            }
        }

        public void PlayTruthDoorUnlockSound()
        {
            if (truthDoorUnlockSound != null && sfxAudioSource != null)
            {
                PlaySoundWithPitchVariation(truthDoorUnlockSound);
            }
        }

        /// <summary>
        /// SEを再生し、ダークモード時はピッチを歪みに合わせて揺らす
        /// </summary>
        private void PlaySoundWithPitchVariation(AudioClip clip)
        {
            bool isDarkMode = GameManager.Instance != null && GameManager.Instance.IsDarkMode();
            
            if (isDarkMode)
            {
                // ダークモード時は、専用のAudioSourceを作成してピッチ変動を適用
                GameObject seObject = new GameObject("SEPlayer_PitchVariation");
                seObject.transform.SetParent(this.transform);
                AudioSource seAudioSource = seObject.AddComponent<AudioSource>();
                seAudioSource.playOnAwake = false;
                seAudioSource.volume = sfxAudioSource.volume;
                seAudioSource.outputAudioMixerGroup = sfxMixerGroup;
                seAudioSource.clip = clip;
                seAudioSource.Play();
                
                pitchVariationAudioSources.Add(seAudioSource);
                StartCoroutine(ApplyPitchVariationToSE(seAudioSource, clip.length));
            }
            else
            {
                // 通常モード時は通常通り再生
                sfxAudioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// SEのピッチを歪みに合わせて揺らすコルーチン
        /// </summary>
        private IEnumerator ApplyPitchVariationToSE(AudioSource audioSource, float duration)
        {
            float startTime = Time.time;
            
            while (audioSource != null && audioSource.isPlaying && (Time.time - startTime) < duration)
            {
                // 歪みシェーダーと同じ時間計算を使用
                float time = Time.time * DistortionSpeed;
                
                // 歪みシェーダーと同じ周波数でピッチを変動させる
                // sinとcosを組み合わせて、より自然な揺らぎを作る
                float pitchVariation = Mathf.Sin(time * DistortionFrequency) * 0.5f + 
                                       Mathf.Cos(time * DistortionFrequency * 0.7f) * 0.5f;
                
                // 1半音の変動幅に変換（±1半音 = ±5.9%）
                // pitchVariationは-1～1の範囲なので、それを±1半音に変換
                float pitchOffset = pitchVariation * PitchVariationSemitones;
                
                // 半音を周波数比に変換（1半音上 = ×1.059, 1半音下 = ÷1.059）
                float pitchMultiplier = Mathf.Pow(SemitoneRatio, pitchOffset);
                
                // ピッチを適用（1.0を基準に変動）
                audioSource.pitch = pitchMultiplier;
                
                yield return null;
            }
            
            // 再生終了後、AudioSourceをクリーンアップ
            if (audioSource != null)
            {
                pitchVariationAudioSources.Remove(audioSource);
                Destroy(audioSource.gameObject);
            }
        }

        public void PlayCreditsBGM()
        {
            if (creditsBGM != null && creditBgmAudioSource != null)
            {
                // 既存のBGMフェードアウトコルーチンを停止
                StopFadeOutBGM();
                
                // 既存のBGMを即座に停止（クレジットBGMと重ならないように）
                if (bgmAudioSource != null && bgmAudioSource.isPlaying)
                {
                    bgmAudioSource.Stop();
                }
                
                // 既存のフェードインコルーチンも停止
                if (fadeInCoroutine != null)
                {
                    StopCoroutine(fadeInCoroutine);
                    fadeInCoroutine = null;
                }
                
                // 音量復元コルーチンも停止
                if (selectionBGMVolumeCoroutine != null)
                {
                    StopCoroutine(selectionBGMVolumeCoroutine);
                    selectionBGMVolumeCoroutine = null;
                }
                
                FadeOutAmbientSound();
                creditBgmAudioSource.clip = creditsBGM;
                creditBgmAudioSource.loop = true;
                creditBgmAudioSource.volume = 1f;
                creditBgmAudioSource.Play();
            }
        }

        public void StopCreditBgm()
        {
            if (creditBgmAudioSource != null && creditBgmAudioSource.isPlaying)
            {
                creditBgmAudioSource.Stop();
            }
        }

        public void FadeOutCreditBGM(float duration)
        {
            if (creditBgmAudioSource != null && creditBgmAudioSource.isPlaying)
            {
                StartCoroutine(FadeOutCreditAudioCoroutine(duration));
            }
        }

        private IEnumerator FadeOutCreditAudioCoroutine(float duration)
        {
            float startVolume = creditBgmAudioSource.volume;
            // 設定されたBGM音量を保存（フェード終了後に復元するため）
            float savedVolume = GetBGMVolume();
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                creditBgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            creditBgmAudioSource.Stop();
            // 設定された音量を復元（フェード前の音量ではなく、ユーザーが設定した音量）
            creditBgmAudioSource.volume = savedVolume;
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
            if (bgmAudioSource.isPlaying)
            {
                float startVolume = bgmAudioSource.volume;
                // 設定されたBGM音量を保存（フェード終了後に復元するため）
                float savedVolume = GetBGMVolume();
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                    yield return null;
                }

                bgmAudioSource.Stop();
                // 設定された音量を復元（フェード前の音量ではなく、ユーザーが設定した音量）
                bgmAudioSource.volume = savedVolume;
                fadeOutCoroutine = null;
                StartCoroutine(CheckAndFadeInAmbientAfterBGM());
            }
        }

        private IEnumerator CheckAndFadeInAmbientAfterBGM()
        {
            yield return new WaitForSeconds(0.1f);
            if (!IsAnyAudioPlaying() && ambientAudioSource != null && ambientAudioSource.isPlaying)
            {
                ambientFadeInCoroutine = StartCoroutine(FadeInAmbientSoundCoroutine(1f));
            }
        }

        public void FadeOutAudioOnSceneChange(bool resetBgmFilter = true)
        {
            if (sfxAudioSource != null && sfxAudioSource.isPlaying)
            {
                if (sfxFadeOutCoroutine != null) StopCoroutine(sfxFadeOutCoroutine);
                sfxFadeOutCoroutine = StartCoroutine(FadeOutSfxAudioCoroutine(0.5f));
            }
            
            // BGMのフィルターをリセットするかどうか（プロフィール/実績/もうひとつ画面ではfalse）
            if (resetBgmFilter)
            {
                if (!isWebBuild && bgmLowPassFilter != null)
                {
                    if (lowPassFadeCoroutine != null) { StopCoroutine(lowPassFadeCoroutine); lowPassFadeCoroutine = null; }
                    bgmLowPassFilter.cutoffFrequency = LowPassNormalCutoff;
                }
                
                // Webビルドの場合、フィルター済み音源から通常音源に戻す
                if (isWebBuild && bgmAudioSource != null && bgmAudioSource.clip == selectionBGMMuffled)
                {
                    float currentTime = bgmAudioSource.time;
                    bgmAudioSource.clip = selectionBGM;
                    bgmAudioSource.time = currentTime;
                }
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
            // Webビルドでも通常音源で開始（フィルターが必要な時だけ切り替える）
            if ((bgmAudioSource.clip == selectionBGM || bgmAudioSource.clip == selectionBGMMuffled) && bgmAudioSource.isPlaying) return;

            // 既存のフェードアウトを停止
            StopFadeOutBGM();

            bgmAudioSource.clip = selectionBGM;
            bgmAudioSource.loop = true;
            bgmAudioSource.time = selectionBGMPausedTime;
            bgmAudioSource.volume = 0f;
            bgmAudioSource.Play();
            
            if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);
            fadeInCoroutine = StartCoroutine(FadeInAudioCoroutine(2f));
            FadeOutAmbientSound();
        }

        public void PauseSelectionBGM()
        {
            if (bgmAudioSource != null && (bgmAudioSource.clip == selectionBGM || bgmAudioSource.clip == selectionBGMMuffled) && bgmAudioSource.isPlaying)
            {
                StartCoroutine(FadeOutAndPauseSelectionBGM(1f));
            }
        }

        private IEnumerator FadeOutAndPauseSelectionBGM(float duration)
        {
            float startVolume = bgmAudioSource.volume;
            // 設定されたBGM音量を保存（フェード終了後に復元するため）
            float savedVolume = GetBGMVolume();
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            // 設定された音量を復元
            bgmAudioSource.volume = savedVolume;
            selectionBGMPausedTime = bgmAudioSource.time;
        }

        private IEnumerator FadeInAudioCoroutine(float duration)
        {
            // 設定されたBGM音量を取得
            float targetVolume = GetBGMVolume();
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bgmAudioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }
            bgmAudioSource.volume = targetVolume;
            fadeInCoroutine = null;
        }

        public void LowerSelectionBGMVolume()
        {
            if (bgmAudioSource != null && (bgmAudioSource.clip == selectionBGM || bgmAudioSource.clip == selectionBGMMuffled) && bgmAudioSource.isPlaying)
            {
                // 音量は維持したまま、フィルターだけをかける（音量を下げない）
                bool isDarkMode = GameManager.Instance != null && GameManager.Instance.IsDarkMode();
                if (isDarkMode)
                {
                    if (pitchFadeCoroutine != null) StopCoroutine(pitchFadeCoroutine);
                    pitchFadeCoroutine = StartCoroutine(FadePitch(LoweredPitch, 1f));
                }
                else
                {
                    if (isWebBuild)
                    {
                        // Webビルドの場合、フィルター済み音源に切り替え（音量は維持）
                        StartCoroutine(SwitchToMuffledBGM(1f));
                    }
                    else
                    {
                        if (lowPassFadeCoroutine != null) StopCoroutine(lowPassFadeCoroutine);
                        lowPassFadeCoroutine = StartCoroutine(FadeLowPassFilter(LowPassMuffledCutoff, 1f));
                    }
                }
            }
        }

        /// <summary>
        /// シナリオ選択BGMの音量を復元（プロフィール/実績画面から戻る時用）
        /// フィルターを解除する（音量は既に維持されているので変更なし）
        /// </summary>
        public void RestoreSelectionBGMVolume()
        {
            if (bgmAudioSource != null && (bgmAudioSource.clip == selectionBGM || bgmAudioSource.clip == selectionBGMMuffled) && bgmAudioSource.isPlaying)
            {
                // 音量は既に維持されているので、フィルターだけを解除する
                // ピッチとローパスフィルターを復元
                if (pitchFadeCoroutine != null) StopCoroutine(pitchFadeCoroutine);
                pitchFadeCoroutine = StartCoroutine(FadePitch(NormalPitch, 1f));

                if (isWebBuild)
                {
                    // Webビルドの場合、通常音源に戻す
                    StartCoroutine(SwitchToNormalBGM(1f));
                }
                else
                {
                    if (lowPassFadeCoroutine != null) StopCoroutine(lowPassFadeCoroutine);
                    lowPassFadeCoroutine = StartCoroutine(FadeLowPassFilter(LowPassNormalCutoff, 1f));
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

        /// <summary>
        /// Webビルド用: フィルター済み音源に切り替える（ローパスフィルターの代わり）
        /// 音量は維持したまま切り替える
        /// </summary>
        private IEnumerator SwitchToMuffledBGM(float duration)
        {
            if (bgmAudioSource == null || selectionBGMMuffled == null) yield break;
            if (bgmAudioSource.clip == selectionBGMMuffled) yield break; // 既にフィルター済み音源が再生中
            
            float currentVolume = bgmAudioSource.volume;
            float currentTime = bgmAudioSource.time;
            
            // 音量を維持したまま、音源を切り替え（フェードなし）
            bgmAudioSource.clip = selectionBGMMuffled;
            bgmAudioSource.time = currentTime;
            bgmAudioSource.volume = currentVolume;
            
            yield return null;
        }

        /// <summary>
        /// Webビルド用: 通常音源に戻す（ローパスフィルター解除の代わり）
        /// 音量は維持したまま切り替える
        /// </summary>
        private IEnumerator SwitchToNormalBGM(float duration)
        {
            if (bgmAudioSource == null || selectionBGM == null) yield break;
            if (bgmAudioSource.clip == selectionBGM) yield break; // 既に通常音源が再生中
            
            float currentVolume = bgmAudioSource.volume;
            float currentTime = bgmAudioSource.time;
            
            // 音量を維持したまま、音源を切り替え（フェードなし）
            bgmAudioSource.clip = selectionBGM;
            bgmAudioSource.time = currentTime;
            bgmAudioSource.volume = currentVolume;
            
            yield return null;
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
        
        /// <summary>
        /// エンドクレジットBGMのAudioSourceを取得
        /// </summary>
        public AudioSource GetCreditBgmAudioSource() => creditBgmAudioSource;

        /// <summary>
        /// BGM音量を設定（0.0～1.0）
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (bgmAudioSource != null)
            {
                bgmAudioSource.volume = volume;
            }
            if (creditBgmAudioSource != null)
            {
                creditBgmAudioSource.volume = volume;
            }
            // 音量をPlayerPrefsに保存
            PlayerPrefs.SetFloat("BGMVolume", volume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// BGM音量を取得（0.0～1.0）
        /// フェード中でも設定された音量を返す（PlayerPrefsから読み込む）
        /// </summary>
        public float GetBGMVolume()
        {
            // PlayerPrefsから読み込み（デフォルトは1.0）
            // フェード中でも設定された音量を返すため、AudioSourceのvolumeではなくPlayerPrefsから取得
            return PlayerPrefs.GetFloat("BGMVolume", 1.0f);
        }

        /// <summary>
        /// SE音量を設定（0.0～1.0）
        /// </summary>
        public void SetSEVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (sfxAudioSource != null)
            {
                sfxAudioSource.volume = volume;
            }
            // 音量をPlayerPrefsに保存
            PlayerPrefs.SetFloat("SEVolume", volume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// SE音量を取得（0.0～1.0）
        /// </summary>
        public float GetSEVolume()
        {
            if (sfxAudioSource != null)
            {
                return sfxAudioSource.volume;
            }
            // PlayerPrefsから読み込み（デフォルトは1.0）
            return PlayerPrefs.GetFloat("SEVolume", 1.0f);
        }

        public AudioClip GetCreditsBGM() => creditsBGM;
        public AudioClip GetSelectionBGM() => selectionBGM;
        public AudioClip GetTypewriterSound() => typewriterSound;
        public AudioClip GetLostLetterSound() => lostLetterSound;

        /// <summary>
        /// 外部からSEを再生する際に使用（ピッチ変動対応）
        /// </summary>
        public void PlaySEWithPitchVariation(AudioClip clip)
        {
            if (clip != null)
            {
                PlaySoundWithPitchVariation(clip);
            }
        }
    }
}
