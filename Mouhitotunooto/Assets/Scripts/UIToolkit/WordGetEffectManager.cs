using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    /// <summary>
    /// ワードゲット演出を管理するクラス
    /// </summary>
    public class WordGetEffectManager : MonoBehaviour
    {
        private GameManager gameManager;
        private AudioManager audioManager;
        private Sprite sparkleIcon;

        public void Initialize(GameManager gameManager, AudioManager audioManager, Sprite sparkleIcon)
        {
            this.gameManager = gameManager;
            this.audioManager = audioManager;
            this.sparkleIcon = sparkleIcon;
        }

        /// <summary>
        /// ワードゲットラベルにスパークルアイコンを追加
        /// </summary>
        public void SetupWordGetLabelWithSparkle(VisualElement container, Label label, string text, System.Action onSparkleClick = null)
        {
            if (container == null || label == null) return;
            
            // コンテナをクリア
            container.Clear();
            
            // 水平レイアウトコンテナを作成
            var horizontalContainer = new VisualElement();
            horizontalContainer.style.flexDirection = FlexDirection.Row;
            horizontalContainer.style.alignItems = Align.Center;
            horizontalContainer.style.justifyContent = Justify.Center;
            horizontalContainer.style.width = Length.Percent(100);
            
            // 左側のスパークルアイコン
            if (sparkleIcon != null)
            {
                var leftSparkle = new Image();
                leftSparkle.sprite = sparkleIcon;
                leftSparkle.style.width = 24f;
                leftSparkle.style.height = 24f;
                leftSparkle.style.marginRight = 8f;
                leftSparkle.RegisterCallback<ClickEvent>(evt => {
                    onSparkleClick?.Invoke();
                    if (audioManager != null)
                    {
                        audioManager.PlaySparkleSound();
                    }
                });
                horizontalContainer.Add(leftSparkle);
            }
            
            // テキストラベル
            label.text = text;
            horizontalContainer.Add(label);
            
            // 右側のスパークルアイコン
            if (sparkleIcon != null)
            {
                var rightSparkle = new Image();
                rightSparkle.sprite = sparkleIcon;
                rightSparkle.style.width = 24f;
                rightSparkle.style.height = 24f;
                rightSparkle.style.marginLeft = 8f;
                rightSparkle.RegisterCallback<ClickEvent>(evt => {
                    onSparkleClick?.Invoke();
                    if (audioManager != null)
                    {
                        audioManager.PlaySparkleSound();
                    }
                });
                horizontalContainer.Add(rightSparkle);
            }
            
            container.Add(horizontalContainer);
        }

        /// <summary>
        /// ワードゲット時の派手な演出を表示
        /// </summary>
        public IEnumerator ShowWordGetWithEffect(VisualElement root, bool isDarkMode, Vector2 clickPosition = default, System.Action onComplete = null)
        {
            // 演出用のオーバーレイを作成
            var effectOverlay = new VisualElement();
            effectOverlay.style.position = Position.Absolute;
            effectOverlay.style.left = 0;
            effectOverlay.style.top = 0;
            effectOverlay.style.right = 0;
            effectOverlay.style.bottom = 0;
            effectOverlay.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
            effectOverlay.pickingMode = PickingMode.Ignore;
            
            // clickPositionが指定されていない場合は中央
            if (clickPosition == default)
            {
                // 座標確定待ち
                yield return null;
                clickPosition = new Vector2(root.worldBound.width / 2f, root.worldBound.height / 2f);
            }
            
            root.Add(effectOverlay);
            
            // 座標確定待ち（worldBoundを取得するため）
            yield return null;
            
            // clickPositionをeffectOverlayのローカル座標に変換
            Vector2 localClickPos = clickPosition - root.worldBound.position;
            
            // パーティクル風の光のもやを生成（3Dっぽい光のエフェクト）
            int particleCount = 30;
            List<VisualElement> glowParticles = new List<VisualElement>();
            List<Vector2> particleVelocities = new List<Vector2>();
            List<float> particleSizes = new List<float>();
            
            for (int i = 0; i < particleCount; i++)
            {
                var particle = new VisualElement();
                float angle = (360f / particleCount) * i * Mathf.Deg2Rad;
                float distance = Random.Range(50f, 150f);
                float size = Random.Range(8f, 20f);
                
                particle.style.position = Position.Absolute;
                particle.style.width = size;
                particle.style.height = size;
                
                // 光るエフェクト（加算合成風の輝き）
                float glowIntensity = Random.Range(0.6f, 1.0f);
                Color glowColor = new Color(1f, 0.9f, 0.5f, glowIntensity);
                particle.style.backgroundColor = glowColor;
                
                // 円形にする
                float radius = size / 2f;
                particle.style.borderTopLeftRadius = radius;
                particle.style.borderTopRightRadius = radius;
                particle.style.borderBottomLeftRadius = radius;
                particle.style.borderBottomRightRadius = radius;
                
                // 初期位置（クリック位置から放射状に）
                Vector2 startPos = localClickPos + new Vector2(
                    Mathf.Cos(angle) * distance,
                    Mathf.Sin(angle) * distance
                );
                particle.style.left = startPos.x - size / 2f;
                particle.style.top = startPos.y - size / 2f;
                
                // 速度（中心に向かう）
                Vector2 velocity = (localClickPos - startPos).normalized * Random.Range(200f, 400f);
                particleVelocities.Add(velocity);
                particleSizes.Add(size);
                
                effectOverlay.Add(particle);
                glowParticles.Add(particle);
            }
            
            // 光輝く文字エフェクト（今回取得した文字と既に取得済みの文字）
            char collectedLetter = '\0';
            int currentScenarioId = 0;
            if (gameManager != null)
            {
                var currentScenario = gameManager.GetCurrentScenario();
                if (currentScenario != null && MouhitotsuWordManager.IsValidScenarioId(currentScenario.id))
                {
                    currentScenarioId = currentScenario.id;
                    collectedLetter = MouhitotsuWordManager.GetLetterByScenarioId(currentScenario.id);
                }
            }
            
            // 各文字の取得回数を計算
            Dictionary<char, int> letterCounts = new Dictionary<char, int>();
            var allLetters = MouhitotsuWordManager.GetAllLetters();
            var completedScenarios = gameManager?.GetCompletedScenarios() ?? new HashSet<int>();
            
            foreach (char letter in allLetters)
            {
                int count = 0;
                
                // この文字に対応するシナリオIDを取得
                for (int scenarioId = 1; scenarioId <= 5; scenarioId++)
                {
                    if (MouhitotsuWordManager.GetLetterByScenarioId(scenarioId) == letter)
                    {
                        // 1周目のクリア回数
                        if (completedScenarios.Contains(scenarioId))
                        {
                            count++;
                        }
                        
                        // 2周目以降のクリア回数を取得（3周目以降で同じシナリオをクリアした回数）
                        if (gameManager != null)
                        {
                            int thirdLoopCount = gameManager.GetScenarioThirdLoopCount(scenarioId);
                            count += thirdLoopCount;
                        }
                    }
                }
                
                letterCounts[letter] = count;
            }
            
            // 今回取得した文字の取得回数を増やす（今回を含める）
            if (collectedLetter != '\0' && currentScenarioId > 0)
            {
                // 今回が3周目以降のクリアの場合、thirdLoopCountはまだ更新されていない可能性があるため
                // 現在のカウントに1を追加（HandleChoiceの後なので、すでに更新されている可能性もあるが念のため）
                int currentCount = letterCounts.ContainsKey(collectedLetter) ? letterCounts[collectedLetter] : 0;
                
                // もし今回が3周目以降のクリアで、まだカウントが0の場合は1を追加
                // 1周目のクリアの場合は completedScenarios に既に含まれているので、追加しない
                bool isThirdLoop = gameManager != null && gameManager.IsThirdLoop();
                if (isThirdLoop && currentCount == 0)
                {
                    letterCounts[collectedLetter] = 1;
                }
                else if (currentCount > 0)
                {
                    // 既に取得済みの場合は、取得回数が増える（3周目以降の復活）
                    letterCounts[collectedLetter] = currentCount + 1;
                }
                else
                {
                    // 初回取得の場合
                    letterCounts[collectedLetter] = 1;
                }
            }
            
            List<VisualElement> glowingLetters = new List<VisualElement>();
            List<Vector2> letterStartPositions = new List<Vector2>();
            List<bool> isNewlyCollected = new List<bool>(); // 今回取得した文字かどうか
            List<int> letterAcquisitionCounts = new List<int>(); // 各文字の取得回数
            
            // 今回取得した文字（大きく表示）
            if (collectedLetter != '\0')
            {
                int count = letterCounts[collectedLetter];
                int letterCount = count; // この文字の取得回数
                
                // 取得回数分の文字を表示（重複表示で派手に）
                for (int i = 0; i < letterCount; i++)
                {
                    var letterElement = new Label(collectedLetter.ToString());
                    letterElement.style.position = Position.Absolute;
                    float fontSize = i == 0 ? 80f : 50f; // 最初の1つは大きく、他は小さめ
                    letterElement.style.fontSize = fontSize;
                    letterElement.style.color = new Color(1f, 0.95f, 0.7f, 0f); // 光る色
                    letterElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    
                    // 初期位置（クリック位置から少し離れた位置、放射状に配置）
                    float letterAngle = (360f / letterCount) * i * Mathf.Deg2Rad;
                    float letterDistance = 80f + i * 20f; // 取得回数が多いほど広がる
                    Vector2 letterStartPos = localClickPos + new Vector2(
                        Mathf.Cos(letterAngle) * letterDistance,
                        Mathf.Sin(letterAngle) * letterDistance
                    );
                    letterStartPositions.Add(letterStartPos);
                    isNewlyCollected.Add(true);
                    letterAcquisitionCounts.Add(letterCount);
                    
                    letterElement.style.left = letterStartPos.x;
                    letterElement.style.top = letterStartPos.y;
                    effectOverlay.Add(letterElement);
                    glowingLetters.Add(letterElement);
                }
            }
            
            // 既に取得済みの文字（小さめに表示、取得回数分）
            foreach (char letter in allLetters)
            {
                if (letter == collectedLetter) continue; // 今回取得した文字は既に追加済み
                
                int count = letterCounts.ContainsKey(letter) ? letterCounts[letter] : 0;
                if (count > 0)
                {
                    // 取得回数分の文字を表示
                    for (int i = 0; i < count; i++)
                    {
                        var letterElement = new Label(letter.ToString());
                        letterElement.style.position = Position.Absolute;
                        letterElement.style.fontSize = 36f; // 小さめ
                        letterElement.style.color = new Color(1f, 0.9f, 0.6f, 0f); // 少し控えめな色
                        letterElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                        
                        // 初期位置（クリック位置から少し離れた位置、外側に配置）
                        float letterAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                        float letterDistance = 150f + Random.Range(0f, 100f); // 外側に配置
                        Vector2 letterStartPos = localClickPos + new Vector2(
                            Mathf.Cos(letterAngle) * letterDistance,
                            Mathf.Sin(letterAngle) * letterDistance
                        );
                        letterStartPositions.Add(letterStartPos);
                        isNewlyCollected.Add(false);
                        letterAcquisitionCounts.Add(count);
                        
                        letterElement.style.left = letterStartPos.x;
                        letterElement.style.top = letterStartPos.y;
                        effectOverlay.Add(letterElement);
                        glowingLetters.Add(letterElement);
                    }
                }
            }
            
            // メインの光るエフェクト（中心）
            var mainGlow = new VisualElement();
            mainGlow.style.position = Position.Absolute;
            mainGlow.style.width = 100f;
            mainGlow.style.height = 100f;
            float mainRadius = 50f;
            mainGlow.style.borderTopLeftRadius = mainRadius;
            mainGlow.style.borderTopRightRadius = mainRadius;
            mainGlow.style.borderBottomLeftRadius = mainRadius;
            mainGlow.style.borderBottomRightRadius = mainRadius;
            mainGlow.style.backgroundColor = new Color(1f, 0.9f, 0.5f, 0f);
            mainGlow.style.left = localClickPos.x - 50f;
            mainGlow.style.top = localClickPos.y - 50f;
            effectOverlay.Add(mainGlow);
            
            // アニメーション
            float effectDuration = 1.5f;
            float elapsed = 0f;
            
            while (elapsed < effectDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / effectDuration;
                
                // パーティクルのアニメーション（集まっていく）
                for (int i = 0; i < glowParticles.Count; i++)
                {
                    var particle = glowParticles[i];
                    Vector2 velocity = particleVelocities[i];
                    
                    // 位置を更新（中心に向かう）
                    float currentX = particle.style.left.value.value + velocity.x * Time.deltaTime * (1f - t);
                    float currentY = particle.style.top.value.value + velocity.y * Time.deltaTime * (1f - t);
                    particle.style.left = currentX;
                    particle.style.top = currentY;
                    
                    // サイズを縮小（集まるときに小さくなる）
                    float currentSize = particleSizes[i] * (1f - t * 0.7f);
                    particle.style.width = currentSize;
                    particle.style.height = currentSize;
                    float currentRadius = currentSize / 2f;
                    particle.style.borderTopLeftRadius = currentRadius;
                    particle.style.borderTopRightRadius = currentRadius;
                    particle.style.borderBottomLeftRadius = currentRadius;
                    particle.style.borderBottomRightRadius = currentRadius;
                    
                    // 輝度を上げる（集まるときに明るくなる）
                    float intensity = Mathf.Lerp(0.6f, 1.5f, 1f - t);
                    Color currentColor = particle.style.backgroundColor.value;
                    currentColor.a = Mathf.Clamp01(intensity);
                    particle.style.backgroundColor = currentColor;
                }
                
                // 文字のアニメーション（集まっていく）
                for (int i = 0; i < glowingLetters.Count; i++)
                {
                    var letter = glowingLetters[i];
                    // 複数方向から集まる演出のため、最初の位置を使用
                    Vector2 letterStartPos = letterStartPositions.Count > i ? letterStartPositions[i] : localClickPos;
                    
                    bool isNew = isNewlyCollected.Count > i ? isNewlyCollected[i] : false;
                    int letterCount = letterAcquisitionCounts.Count > i ? letterAcquisitionCounts[i] : 1;
                    
                    // 中心に向かって移動（イージングで滑らかに）
                    float easedT = t * t * (3f - 2f * t); // SmoothStep
                    Vector2 currentPos = Vector2.Lerp(letterStartPos, localClickPos, easedT);
                    letter.style.left = currentPos.x;
                    letter.style.top = currentPos.y;
                    
                    // フェードインとスケールアップ（今回取得した文字は大きく、既存は小さめ）
                    float letterAlpha = Mathf.Lerp(0f, 1f, t);
                    float baseScale = isNew ? 0.5f : 0.3f; // 今回取得は大きく開始
                    float targetScale = isNew ? 1.8f : 1.0f; // 今回取得は大きく終了
                    float letterScale = Mathf.Lerp(baseScale, targetScale, easedT);
                    
                    // 取得回数が多いほど派手に（スケールを少し大きく）
                    if (letterCount > 1)
                    {
                        letterScale *= (1f + (letterCount - 1) * 0.1f); // 取得回数が多いほど大きく
                    }
                    
                    letter.style.opacity = letterAlpha;
                    letter.style.scale = new Scale(new Vector2(letterScale, letterScale));
                    
                    // 光る効果（加算合成風、今回取得した文字はより派手に）
                    float glowIntensity = isNew ? 1.0f : 0.7f; // 今回取得は明るく
                    float glow = Mathf.Sin(t * Mathf.PI * 4f) * 0.4f + glowIntensity;
                    
                    // 取得回数が多いほど明るく
                    if (letterCount > 1)
                    {
                        glow *= (1f + (letterCount - 1) * 0.15f);
                    }
                    
                    Color letterColor = isNew 
                        ? new Color(1f, 0.95f, 0.7f, letterAlpha * glow) 
                        : new Color(1f, 0.9f, 0.6f, letterAlpha * glow * 0.8f); // 既存は少し控えめ
                    letter.style.color = letterColor;
                }
                
                // メインの光るエフェクト
                float mainScale = Mathf.Lerp(0.5f, 3f, t);
                float mainAlpha = Mathf.Lerp(0.8f, 0f, t);
                mainGlow.style.width = 100f * mainScale;
                mainGlow.style.height = 100f * mainScale;
                float mainCurrentRadius = (100f * mainScale) / 2f;
                mainGlow.style.borderTopLeftRadius = mainCurrentRadius;
                mainGlow.style.borderTopRightRadius = mainCurrentRadius;
                mainGlow.style.borderBottomLeftRadius = mainCurrentRadius;
                mainGlow.style.borderBottomRightRadius = mainCurrentRadius;
                mainGlow.style.left = localClickPos.x - (100f * mainScale) / 2f;
                mainGlow.style.top = localClickPos.y - (100f * mainScale) / 2f;
                Color mainColor = mainGlow.style.backgroundColor.value;
                mainColor.a = mainAlpha;
                mainGlow.style.backgroundColor = mainColor;
                
                // 背景の明るさ
                float bgAlpha = Mathf.Lerp(0f, 0.2f, Mathf.Sin(t * Mathf.PI));
                effectOverlay.style.backgroundColor = new Color(1f, 0.95f, 0.8f, bgAlpha);
                
                yield return null;
            }
            
            // エフェクトを削除
            root.Remove(effectOverlay);
            
            // 一呼吸（0.3秒待つ）
            yield return new WaitForSeconds(0.3f);
            
            // ワードゲット表示を表示
            var wordGetContainer = root.Q<VisualElement>("WordGetContainer");
            if (wordGetContainer != null)
            {
                wordGetContainer.style.display = DisplayStyle.Flex;
            }
            
            // コールバックを呼び出して後続処理を実行
            onComplete?.Invoke();
        }

        /// <summary>
        /// ワード取得時に、結果画面からスコア表示へ光が飛んでいく派手な演出
        /// </summary>
        public IEnumerator ShowLetterGetAnimation(Vector2 startPos, VisualElement root)
        {
            if (root == null) yield break;

            // スコア表示（ScoreText）を探す
            var scoreLabel = root.Q<Label>("ScoreText");
            if (scoreLabel == null) yield break;

            // 座標確定待ち
            yield return null;
            Vector2 endPos = scoreLabel.worldBound.center;
            
            // startPosをrootのローカル座標に変換
            Vector2 localStartPos = startPos - root.worldBound.position;
            Vector2 localEndPos = endPos - root.worldBound.position;

            // 演出用コンテナ
            var effectContainer = new VisualElement();
            effectContainer.style.position = Position.Absolute;
            effectContainer.style.left = 0;
            effectContainer.style.top = 0;
            effectContainer.style.right = 0;
            effectContainer.style.bottom = 0;
            effectContainer.pickingMode = PickingMode.Ignore;
            root.Add(effectContainer);

            // 光の粒子演出（より派手に）
            int particleCount = 20;
            List<VisualElement> particles = new List<VisualElement>();
            List<Vector2> velocities = new List<Vector2>();
            
            for (int i = 0; i < particleCount; i++)
            {
                var p = new VisualElement();
                p.style.position = Position.Absolute;
                float size = Random.Range(6f, 15f);
                p.style.width = size;
                p.style.height = size;
                
                // 光る色（加算合成風）
                float hue = Random.Range(0.1f, 0.2f); // 黄色系
                Color particleColor = Color.HSVToRGB(hue, 0.8f, 1f);
                particleColor.a = 0.9f;
                p.style.backgroundColor = particleColor;
                
                // 円形
                float radius = size / 2f;
                p.style.borderTopLeftRadius = radius;
                p.style.borderTopRightRadius = radius;
                p.style.borderBottomLeftRadius = radius;
                p.style.borderBottomRightRadius = radius;
                
                p.style.left = localStartPos.x - size / 2f;
                p.style.top = localStartPos.y - size / 2f;
                
                // ランダムな初期速度（中心に向かう方向に）
                Vector2 direction = (localEndPos - localStartPos).normalized;
                Vector2 perpendicular = new Vector2(-direction.y, direction.x);
                float spread = Random.Range(-0.3f, 0.3f);
                Vector2 velocity = direction * Random.Range(300f, 500f) + perpendicular * spread * 100f;
                velocities.Add(velocity);
                
                effectContainer.Add(p);
                particles.Add(p);
            }

            if (audioManager != null)
            {
                audioManager.PlaySparkleSound();
            }

            float duration = 0.8f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                for (int i = 0; i < particles.Count; i++)
                {
                    var p = particles[i];
                    Vector2 velocity = velocities[i];
                    
                    // 物理的な動き（減速しながら）
                    float speedMultiplier = 1f - (t * t); // 二次的に減速
                    Vector2 currentPos = localStartPos + velocity * elapsed * speedMultiplier;
                    
                    p.style.left = currentPos.x - p.style.width.value.value / 2f;
                    p.style.top = currentPos.y - p.style.height.value.value / 2f;
                    
                    // フェードアウトとスケールダウン
                    float opacity = 1f - t;
                    float scale = 1f - t * 0.5f;
                    p.style.opacity = opacity;
                    p.style.scale = new Scale(new Vector2(scale, scale));
                    
                    // 輝度を上げる（加算合成風）
                    Color currentColor = p.style.backgroundColor.value;
                    currentColor.a = opacity * 1.2f; // 加算合成風に明るく
                    p.style.backgroundColor = currentColor;
                }
                
                yield return null;
            }
            
            // パーティクルを削除
            effectContainer.RemoveFromHierarchy();
        }

        /// <summary>
        /// ワードが失われた時の派手なアニメーション
        /// </summary>
        public IEnumerator PlayWordLostAnimation(int newScore, int oldScore, VisualElement root, Label scoreLabel)
        {
            if (root == null || scoreLabel == null) yield break;
            if (gameManager == null) yield break;
            
            // 座標確定待ち
            yield return null;
            
            // ワードゲット数が減る時の効果音を再生
            if (audioManager != null)
            {
                audioManager.PlayWordGetDecreaseSound();
            }
            
            // 失われた文字を取得（現在の失われた文字のセット）
            var currentLostLetters = gameManager.GetLostLetters();
            
            // 文字が奪われていく演出
            // 「もうひとつ」の各文字を順番にチェック
            char[] allLetters = MouhitotsuWordManager.GetAllLetters();
            
            // 失われた文字のみを抽出（実際に失われた文字）
            List<char> newlyLostLetters = new List<char>();
            foreach (char letter in allLetters)
            {
                if (currentLostLetters.Contains(letter))
                {
                    newlyLostLetters.Add(letter);
                }
            }
            
            // 失われた文字がない場合は何もしない
            if (newlyLostLetters.Count == 0)
            {
                yield break;
            }
            
            // 現在のテキストを取得（失われた文字を考慮）
            string baseText = $"{MouhitotsuWordManager.GetFormattedWord()}ワードゲット数";
            var collectedLetters = gameManager.GetCollectedLetters();
            var lostLettersForDisplay = new HashSet<char>(currentLostLetters);
            
            // 各失われた文字を順番に弾け飛ばす
            int lostCount = 0;
            foreach (char lostLetter in newlyLostLetters)
            {
                lostCount++;
                
                // スコア表示を更新（失われた文字を「※」に置き換え）
                // まず、この文字を一時的にlostLettersForDisplayから除外して、表示される状態にする
                lostLettersForDisplay.Remove(lostLetter);
                string displayText = TextFormatter.FormatMouhitotsuWord(baseText, collectedLetters, lostLettersForDisplay, true);
                lostLettersForDisplay.Add(lostLetter); // 戻す
                
                // スコアを徐々に減らす（実際のスコアに合わせる）
                int totalScenarios = gameManager.GetScenarios().Count;
                int displayScore = Mathf.Max(newScore, oldScore - lostCount);
                scoreLabel.text = $"{displayText}: {displayScore} / {totalScenarios}";
                
                // 座標確定待ち（スコアラベルの位置を取得）
                yield return null;
                Vector2 letterStartPos = scoreLabel.worldBound.center;
                
                // 文字が弾け飛ぶアニメーション（物理的な動き）
                yield return StartCoroutine(AnimateLetterExplosion(root, lostLetter.ToString(), letterStartPos));
                
                // 次の文字まで待機
                yield return new WaitForSeconds(0.15f);
            }
            
            // 最終的なスコア表示を更新
            string finalText = TextFormatter.FormatMouhitotsuWord(baseText, collectedLetters, currentLostLetters, true);
            int finalScore = gameManager.GetScore();
            int finalTotalScenarios = gameManager.GetScenarios().Count;
            scoreLabel.text = $"{finalText}: {finalScore} / {finalTotalScenarios}";
        }
        
        /// <summary>
        /// 文字が弾け飛ぶアニメーション（物理的な動き）
        /// </summary>
        private IEnumerator AnimateLetterExplosion(VisualElement root, string letter, Vector2 startPos)
        {
            // 座標確定待ち
            yield return null;
            
            // startPosをrootのローカル座標に変換
            Vector2 localStartPos = startPos - root.worldBound.position;
            
            // 演出用コンテナ
            var effectContainer = new VisualElement();
            effectContainer.style.position = Position.Absolute;
            effectContainer.style.left = 0;
            effectContainer.style.top = 0;
            effectContainer.style.right = 0;
            effectContainer.style.bottom = 0;
            effectContainer.pickingMode = PickingMode.Ignore;
            root.Add(effectContainer);
            
            // 文字を表示
            var letterElement = new Label(letter);
            letterElement.style.position = Position.Absolute;
            letterElement.style.fontSize = 36;
            letterElement.style.color = new Color(1f, 0.3f, 0.3f, 1f); // 赤みがかった色
            letterElement.style.unityFontStyleAndWeight = FontStyle.Bold;
            letterElement.style.left = localStartPos.x;
            letterElement.style.top = localStartPos.y;
            effectContainer.Add(letterElement);
            
            // 物理的な動き（ランダムな方向に弾け飛ぶ）
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float speed = Random.Range(300f, 600f);
            Vector2 velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
            float gravity = 500f; // 重力
            float rotationSpeed = Random.Range(-360f, 360f);
            
            float duration = 1.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // 物理的な動き（重力を考慮）
                velocity.y -= gravity * Time.deltaTime;
                Vector2 currentPos = localStartPos + velocity * elapsed;
                
                letterElement.style.left = currentPos.x;
                letterElement.style.top = currentPos.y;
                
                // 3D的な回転
                float rotation = rotationSpeed * elapsed;
                letterElement.style.rotate = new Rotate(new Angle(rotation));
                
                // スケールダウン（遠くに飛んでいく感じ）
                float scale = 1f - t;
                letterElement.style.scale = new Scale(new Vector2(scale, scale));
                
                // フェードアウト
                float opacity = 1f - (t * t); // 加速的にフェードアウト
                letterElement.style.opacity = opacity;
                
                // 画面外に出たら削除
                if (currentPos.x < -100f || currentPos.x > root.worldBound.width + 100f ||
                    currentPos.y < -100f || currentPos.y > root.worldBound.height + 100f)
                {
                    break;
                }
                
                yield return null;
            }
            
            // エフェクトを削除
            root.Remove(effectContainer);
        }
    }
}
