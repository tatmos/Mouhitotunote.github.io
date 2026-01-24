using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// オーディオレベルメータ（左右チャンネル）を表示するクラス
    /// ParticleSystemを使ったLED風のVFX効果付き
    /// </summary>
    public class AudioLevelMeter : MonoBehaviour
    {
        private AudioSource audioSource;
        private Coroutine updateCoroutine;
        
        // レベルメータの設定
        private const int MeterSegments = 32; // LEDセグメント数
        private const float MeterWidth = 0.1f; // 各セグメントの幅（ピクセル）- 極端に狭く設定（1/10）
        private const float UpdateInterval = 0.05f; // 更新間隔（秒）
        
        // ゲームのテーマカラーに合わせたLED風の色設定（ブラウン/セピア/ベージュ系）
        private readonly Color[] ledColors = new Color[]
        {
            new Color(0.396f, 0.263f, 0.129f, 1f),    // 暗めのブラウン（低レベル）- rgb(101, 67, 33)
            new Color(0.722f, 0.529f, 0.345f, 1f),    // セピア/ベージュ（中レベル）- rgb(184, 135, 88)
            new Color(0.929f, 0.843f, 0.710f, 1f),   // 明るいベージュ（高レベル）- rgb(237, 215, 181)
            new Color(0.8f, 0.4f, 0.2f, 1f)           // オレンジがかったブラウン（クリップ）
        };
        
        // ParticleSystem
        private ParticleSystem leftParticleSystem;
        private ParticleSystem rightParticleSystem;
        private ParticleSystem.Particle[] leftParticles;
        private ParticleSystem.Particle[] rightParticles;
        
        private Material ledMaterial;
        private Camera uiCamera;
        
        /// <summary>
        /// レベルメータを初期化
        /// </summary>
        public void Initialize(AudioSource source, Camera targetCamera = null)
        {
            audioSource = source;
            uiCamera = targetCamera;
            
            // 既存のコルーチンを停止
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
            }
            
            // 既存のParticleSystemをクリーンアップ
            CleanupParticleSystems();
            
            // LED Materialを作成
            CreateLEDMaterial();
            
            // ParticleSystemを作成
            CreateParticleSystems();
            
            // 更新を開始
            if (audioSource != null)
            {
                updateCoroutine = StartCoroutine(UpdateLevelMeter());
            }
            else
            {
                Debug.LogWarning("AudioSourceがnullです。レベルメータの更新を開始できませんでした。");
            }
        }
        
        /// <summary>
        /// LED Materialを作成
        /// </summary>
        private void CreateLEDMaterial()
        {
            // ParticleTestSimpleと同じシェーダーを使用
            Shader ledShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (ledShader == null)
            {
                ledShader = Shader.Find("Particles/Standard Unlit");
            }
            if (ledShader == null)
            {
                // フォールバック：標準のParticleシェーダー
                ledShader = Shader.Find("Sprites/Default");
            }
            
            if (ledShader == null)
            {
                Debug.LogError("Particleシェーダーが見つかりません！");
                return;
            }
            
            ledMaterial = new Material(ledShader);
            ledMaterial.SetColor("_Color", Color.white);
        }
        
        /// <summary>
        /// ParticleSystemを作成
        /// </summary>
        private void CreateParticleSystems()
        {
            // 左チャンネル用ParticleSystem
            leftParticleSystem = CreateChannelParticleSystem(true);
            
            // 右チャンネル用ParticleSystem
            rightParticleSystem = CreateChannelParticleSystem(false);
            
            // Particle配列を初期化
            leftParticles = new ParticleSystem.Particle[MeterSegments];
            rightParticles = new ParticleSystem.Particle[MeterSegments];
        }
        
        /// <summary>
        /// チャンネル用のParticleSystemを作成
        /// </summary>
        private ParticleSystem CreateChannelParticleSystem(bool isLeft)
        {
            GameObject psObject = new GameObject($"LEDMeter_{(isLeft ? "Left" : "Right")}");
            psObject.transform.SetParent(transform, false);
            
            ParticleSystem ps = psObject.AddComponent<ParticleSystem>();
            
            // Main Module
            var main = ps.main;
            main.startLifetime = float.MaxValue; // 永久に表示
            main.startSpeed = 0f; // 移動しない
            main.startSize = 1f;
            main.startColor = new Color(0.5f, 0.5f, 0.5f, 1f); // テスト用に明るく設定（後でUpdateParticlesで更新される）
            main.maxParticles = MeterSegments;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            
            // Emission Module（無効化 - 手動でParticleを配置）
            var emission = ps.emission;
            emission.enabled = false;
            
            // Shape Module（無効化）
            var shape = ps.shape;
            shape.enabled = false;
            
            // Renderer Module
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (ledMaterial != null)
            {
                renderer.material = ledMaterial;
            }
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "UI"; // UIレイヤーに設定
            renderer.sortingOrder = 2; // Render Texture方式では、UI Display Canvas (0) より高い値に設定
            
            // 初期Particleを配置
            SetupParticles(ps, isLeft);
            
            // ParticleSystemを再生
            ps.Play();
            
            return ps;
        }
        
        /// <summary>
        /// Particleを初期配置
        /// </summary>
        private void SetupParticles(ParticleSystem ps, bool isLeft)
        {
            float screenHeight = Screen.height;
            float screenWidth = Screen.width;
            float segmentHeight = screenHeight / MeterSegments;
            
            // カメラを取得（UI CameraまたはMain Camera）
            Camera cam = uiCamera != null ? uiCamera : Camera.main;
            if (cam == null)
            {
                cam = Camera.main;
            }
            
            if (cam == null)
            {
                Debug.LogError("カメラが見つかりません。Particleを配置できません。");
                return;
            }
            
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[MeterSegments];
            
            // Render Texture方式では、カメラの前方に配置する必要がある
            float distanceFromCamera = 5f; // カメラからの距離
            
            for (int i = 0; i < MeterSegments; i++)
            {
                // 画面座標を計算（画面の左右端、極端に狭く配置）
                float edgeOffset = MeterWidth / 2f; // 画面端からのオフセット
                float xPos = isLeft ? edgeOffset : screenWidth - edgeOffset;
                float yPos = i * segmentHeight + segmentHeight / 2f;
                
                // 画面座標を正規化（0-1の範囲）
                float normalizedX = xPos / screenWidth;
                float normalizedY = yPos / screenHeight;
                
                // カメラの視野角を使ってワールド座標を計算
                float viewportX = normalizedX * 2f - 1f; // -1 to 1
                float viewportY = (1f - normalizedY) * 2f - 1f; // -1 to 1 (Y軸を反転)
                
                // カメラの前方に配置
                Vector3 worldPos = cam.transform.position + cam.transform.forward * distanceFromCamera;
                
                // カメラの右方向と上方向を使って位置を調整
                float halfHeight = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * distanceFromCamera;
                float halfWidth = halfHeight * cam.aspect;
                
                worldPos += cam.transform.right * (viewportX * halfWidth);
                worldPos += cam.transform.up * (viewportY * halfHeight);
                
                // Particleを設定
                    particles[i].position = worldPos;
                    particles[i].startLifetime = float.MaxValue;
                    particles[i].remainingLifetime = float.MaxValue;
                    // サイズを極端に狭く設定（画面端の細いライン）
                    float particleSize = Mathf.Max(MeterWidth, 0.5f); // 極端に狭く、最小0.5ピクセル
                    particles[i].startSize = particleSize;
                    // 初期色を透明に設定（後でUpdateParticlesで更新される）
                    particles[i].startColor = new Color(0f, 0f, 0f, 0f);
                    particles[i].velocity = Vector3.zero;
            }
            
            ps.SetParticles(particles, MeterSegments);
        }
        
        /// <summary>
        /// レベルメータを更新するコルーチン
        /// </summary>
        private IEnumerator UpdateLevelMeter()
        {
            float[] leftChannel = new float[1024];
            float[] rightChannel = new float[1024];
            
            // AudioSourceが再生開始するまで少し待つ
            int waitFrames = 0;
            while (audioSource != null && !audioSource.isPlaying && waitFrames < 60)
            {
                waitFrames++;
                yield return null;
            }
            
            if (audioSource == null)
            {
                Debug.LogWarning("AudioSourceがnullになりました。");
                yield break;
            }
            
            if (!audioSource.isPlaying)
            {
                Debug.LogWarning($"AudioSourceが再生されていません。isPlaying={audioSource.isPlaying}, clip={audioSource.clip?.name}");
            }
            
            while (audioSource != null && audioSource.isPlaying)
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    try
                    {
                        // オーディオデータを取得
                        audioSource.GetOutputData(leftChannel, 0);  // 左チャンネル
                        audioSource.GetOutputData(rightChannel, 1);  // 右チャンネル
                        
                        // RMS（Root Mean Square）を計算してレベルを取得
                        float leftLevel = CalculateRMS(leftChannel);
                        float rightLevel = CalculateRMS(rightChannel);
                        
                        // レベルを0-1の範囲に正規化（感度調整）
                        float leftNormalized = Mathf.Clamp01(leftLevel * 10f);
                        float rightNormalized = Mathf.Clamp01(rightLevel * 10f);
                        
                        // Particleを更新
                        UpdateParticles(leftParticleSystem, leftParticles, leftNormalized, true);
                        UpdateParticles(rightParticleSystem, rightParticles, rightNormalized, false);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"オーディオデータの取得に失敗しました: {e.Message}");
                        // WebGLではGetOutputDataが動作しない可能性があるため、フォールバック処理
                        // とりあえず固定値でテスト表示
                        UpdateParticles(leftParticleSystem, leftParticles, 0.5f, true);
                        UpdateParticles(rightParticleSystem, rightParticles, 0.5f, false);
                    }
                }
                else
                {
                    Debug.LogWarning($"AudioSourceが再生されていません。isPlaying: {audioSource?.isPlaying}, audioSource: {audioSource != null}");
                }
                
                yield return new WaitForSeconds(UpdateInterval);
            }
        }
        
        /// <summary>
        /// RMS（Root Mean Square）を計算
        /// </summary>
        private float CalculateRMS(float[] samples)
        {
            float sum = 0f;
            int count = 0;
            
            for (int i = 0; i < samples.Length; i++)
            {
                float sample = samples[i];
                sum += sample * sample;
                count++;
            }
            
            if (count > 0)
            {
                return Mathf.Sqrt(sum / count);
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Particleを更新（LED風の表示）
        /// </summary>
        private void UpdateParticles(ParticleSystem ps, ParticleSystem.Particle[] particles, float normalizedLevel, bool isLeft)
        {
            if (ps == null || particles == null) return;
            
            // 現在のParticleを取得
            int count = ps.GetParticles(particles);
            
            // アクティブなセグメント数を計算
            int activeSegments = Mathf.RoundToInt(normalizedLevel * MeterSegments);
            
            float screenHeight = Screen.height;
            float segmentHeight = screenHeight / MeterSegments;
            
            // カメラを取得
            Camera cam = uiCamera != null ? uiCamera : Camera.main;
            if (cam == null)
            {
                cam = Camera.main;
            }
            
            if (cam == null)
            {
                return;
            }
            
            // Render Texture方式では、カメラの前方に配置する必要がある
            float distanceFromCamera = 5f; // カメラからの距離
            float screenWidth = Screen.width;
            
            for (int i = 0; i < MeterSegments; i++)
            {
                if (i < count)
                {
                    // 画面座標を計算（画面の左右端、極端に狭く配置）
                    float edgeOffset = MeterWidth / 2f; // 画面端からのオフセット
                    float xPos = isLeft ? edgeOffset : screenWidth - edgeOffset;
                    float yPos = i * segmentHeight + segmentHeight / 2f;
                    
                    // 画面座標を正規化（0-1の範囲）
                    float normalizedX = xPos / screenWidth;
                    float normalizedY = yPos / screenHeight;
                    
                    // カメラの視野角を使ってワールド座標を計算
                    float viewportX = normalizedX * 2f - 1f; // -1 to 1
                    float viewportY = (1f - normalizedY) * 2f - 1f; // -1 to 1 (Y軸を反転)
                    
                    // カメラの前方に配置
                    Vector3 worldPos = cam.transform.position + cam.transform.forward * distanceFromCamera;
                    
                    // カメラの右方向と上方向を使って位置を調整
                    float halfHeight = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * distanceFromCamera;
                    float halfWidth = halfHeight * cam.aspect;
                    
                    worldPos += cam.transform.right * (viewportX * halfWidth);
                    worldPos += cam.transform.up * (viewportY * halfHeight);
                    
                    particles[i].position = worldPos;
                    // サイズを極端に狭く設定（画面端の細いライン）
                    float particleSize = Mathf.Max(MeterWidth, 0.5f); // 極端に狭く、最小0.5ピクセル
                    particles[i].startSize = particleSize;
                    
                    if (i < activeSegments)
                    {
                        // セグメントの位置に応じて色を決定（下から上へ：緑→黄→オレンジ→赤）
                        float segmentPosition = (float)i / MeterSegments;
                        Color segmentColor = GetColorForLevel(segmentPosition);
                        
                        // 輝きの強度を計算（LED風の明るさ）
                        float glowIntensity = Mathf.Lerp(2.0f, 4.0f, normalizedLevel);
                        segmentColor.r = Mathf.Clamp01(segmentColor.r * glowIntensity);
                        segmentColor.g = Mathf.Clamp01(segmentColor.g * glowIntensity);
                        segmentColor.b = Mathf.Clamp01(segmentColor.b * glowIntensity);
                        segmentColor.a = 1.0f; // 不透明度を最大に
                        
                        // パルス効果（高レベル時に点滅）
                        if (normalizedLevel > 0.8f)
                        {
                            float pulse = 1.0f + Mathf.Sin(Time.time * 8f) * 0.3f;
                            segmentColor.r = Mathf.Clamp01(segmentColor.r * pulse);
                            segmentColor.g = Mathf.Clamp01(segmentColor.g * pulse);
                            segmentColor.b = Mathf.Clamp01(segmentColor.b * pulse);
                        }
                        
                        particles[i].startColor = segmentColor;
                    }
                    else
                    {
                        // 非アクティブなセグメントは完全に透明に設定
                        particles[i].startColor = new Color(0f, 0f, 0f, 0f);
                    }
                }
            }
            
            // Particleを適用
            ps.SetParticles(particles, count);
        }
        
        /// <summary>
        /// レベルに応じた色を取得（ゲームのテーマカラー：ブラウン/セピア/ベージュ系）
        /// </summary>
        private Color GetColorForLevel(float level)
        {
            if (level < 0.5f)
            {
                // 低レベル：暗めのブラウン → セピア
                return Color.Lerp(ledColors[0], ledColors[1], level * 2f);
            }
            else if (level < 0.75f)
            {
                // 中レベル：セピア → 明るいベージュ
                return Color.Lerp(ledColors[1], ledColors[2], (level - 0.5f) * 4f);
            }
            else if (level < 0.95f)
            {
                // 高レベル：明るいベージュ → オレンジがかったブラウン
                return Color.Lerp(ledColors[2], ledColors[3], (level - 0.75f) * 5f);
            }
            else
            {
                // クリップ：オレンジがかったブラウン（パルス効果あり）
                return ledColors[3];
            }
        }
        
        /// <summary>
        /// レベルメータを停止
        /// </summary>
        public void Stop()
        {
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
                updateCoroutine = null;
            }
            
            // ParticleSystemを完全にクリーンアップ
            CleanupParticleSystems();
        }
        
        /// <summary>
        /// すべてのParticleを非アクティブにする
        /// </summary>
        private void SetAllParticlesInactive()
        {
            UpdateParticles(leftParticleSystem, leftParticles, 0f, true);
            UpdateParticles(rightParticleSystem, rightParticles, 0f, false);
        }
        
        /// <summary>
        /// 既存のParticleSystemをクリーンアップ
        /// </summary>
        private void CleanupParticleSystems()
        {
            if (leftParticleSystem != null)
            {
                Destroy(leftParticleSystem.gameObject);
                leftParticleSystem = null;
            }
            
            if (rightParticleSystem != null)
            {
                Destroy(rightParticleSystem.gameObject);
                rightParticleSystem = null;
            }
            
            leftParticles = null;
            rightParticles = null;
        }
        
        private void OnDestroy()
        {
            CleanupParticleSystems();
            
            if (ledMaterial != null)
            {
                Destroy(ledMaterial);
            }
        }
    }
}
