# リファクタリング計画

## 概要

UIManagerUIToolkit.cs（3607行）を整理するため、優先度の高い項目から実装を進めます。

## 実装順序

### 1. TitleScreenManager ✅ (完了)

既に実装済みです。

### 2. ScenarioScreenManager 🔄 (作業中)

**現状:**
- `ShowScenarioScreen()` メソッドが約240行（998-1240行目）
- 以下の処理を含む:
  - オーディオのフェードアウト・環境音の開始
  - 背景画像の設定
  - タイトルの設定
  - SetupTextの設定とタイプライター効果
  - 選択肢ボタンの作成（`CreateChoiceButtons`）
  - 選択肢の順次表示（`ShowChoicesSequentially` - コルーチン）
  - スコア表示の更新
  - トランジション開始

**依存関係:**
- `CreateChoiceButtons()` メソッド（2654行目～）
- `ShowChoicesSequentially()` メソッド（2677行目～）- コルーチン
- `OnChoiceSelected()` メソッド（2724行目～）
- `SetBackgroundImage()` メソッド（2750行目～）
- `ShakeAnimation()` メソッド（3472行目～）- コルーチン
- `ShowLetterGetAnimation()` メソッド（3954行目～）- コルーチン
- `wordFoundInCurrentScenario` フィールド（128行目）

**実装方針:**
1. `ScenarioScreenManager` クラスを作成
2. `ScenarioScreenSettings` と `ScenarioScreenActions` のstructを作成
3. コルーチンが必要な処理は、コールバック関数として渡す
4. `wordFoundInCurrentScenario` フラグは、ScenarioScreenManagerで管理するか、コールバック経由でUIManagerUIToolkitに通知

**課題:**
- コルーチン（`StartCoroutine`）が必要な処理が多数ある
- `wordFoundInCurrentScenario` フラグの管理方法を決める必要がある
- `CreateChoiceButtons` と `ShowChoicesSequentially` の移行方法を決める必要がある

### 3. ResultScreenManager ⏳ (未着手)

**現状:**
- `ShowResultScreen()` メソッドが非常に長い（1276行目～）
- 約500行以上の処理を含む

**依存関係:**
- `ShowWordGetWithEffect()` メソッド（3509行目～）- コルーチン
- `AnimateWordGetLabelFadeIn()` メソッド（3483行目～）- コルーチン
- `SetupWordGetLabelWithSparkle()` メソッド（3894行目～）
- `ShowSpecialCreditsTransition()` メソッド（3154行目～）- コルーチン
- `SetBackgroundImage()` メソッド
- その他多数の処理

**実装方針:**
- ScenarioScreenManagerの実装が完了してから着手
- 同様のパターンで実装

## 実装の注意事項

1. **コルーチンの扱い:**
   - コルーチンが必要な処理は、`System.Func<IEnumerator>` としてコールバックで渡す
   - または、MonoBehaviourが必要な場合は、UIManagerUIToolkitからコルーチンを実行

2. **フラグの管理:**
   - `wordFoundInCurrentScenario` は、ScenarioScreenManagerで管理するか、コールバック経由で通知

3. **段階的な実装:**
   - 一度にすべてを移行せず、段階的に実装
   - 各ステップで動作確認を実施

4. **後方互換性:**
   - 既存のコードを壊さないように、段階的に移行
   - 動作確認を十分に行う

## 次のステップ

1. ScenarioScreenManagerの基本構造を作成
2. ShowScenarioScreenの主要な処理を移行
3. 動作確認
4. ResultScreenManagerの実装に着手
