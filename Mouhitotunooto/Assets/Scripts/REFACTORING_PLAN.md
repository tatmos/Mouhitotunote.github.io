# リファクタリング計画

## 概要

UIManagerUIToolkit.cs（3607行）を整理するため、優先度の高い項目から実装を進めます。

## 実装順序

### 1. TitleScreenManager ✅ (完了)

既に実装済みです。

### 2. ScenarioScreenManager ✅ (完了)

**実装済み:**
- `ScenarioScreenManager` クラスを作成
- `ScenarioScreenSettings` と `ScenarioScreenActions` のstructを作成
- `ShowScenarioScreen()` メソッドを `ScenarioScreenManager` を使用するように変更
- コルーチンが必要な処理は、コールバック関数として渡す実装
- `wordFoundInCurrentScenario` フラグは、コールバック経由でUIManagerUIToolkitに通知

### 3. ResultScreenManager ✅ (部分的に完了)

**現状:**
- `ShowResultScreen()` メソッドが非常に長い（1123行目～）
- 約500行以上の処理を含む

**実装済み:**
- `ResultScreenManager` クラスを作成
- `ResultScreenSettings` と `ResultScreenActions` のstructを作成
- `SetupEpilogue()` メソッドを実装（後日談の設定）
- `SetupWordGetDisplay()` メソッドを実装（ワードゲット表示の設定）
- `SetupBackButton()` メソッドを実装（戻るボタンの設定）
- `ShowResultScreen()` メソッドを部分的に `ResultScreenManager` を使用するように変更

**残りのタスク:**
- `SetupResultText()` メソッドの実装（結果テキストの設定 - 非常に複雑な処理のため、現時点では `UIManagerUIToolkit` 内に残している）

**依存関係:**
- `ShowWordGetWithEffect()` メソッド（3509行目～）- コルーチン
- `AnimateWordGetLabelFadeIn()` メソッド（3483行目～）- コルーチン
- `SetupWordGetLabelWithSparkle()` メソッド（3894行目～）
- `ShowSpecialCreditsTransition()` メソッド（3154行目～）- コルーチン
- `SetBackgroundImage()` メソッド
- その他多数の処理

**実装方針:**
- 同様のパターンで実装（完了）
- 結果テキストの設定は将来的に移行を検討

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

1. ✅ ScenarioScreenManagerの基本構造を作成（完了）
2. ✅ ShowScenarioScreenの主要な処理を移行（完了）
3. ✅ ResultScreenManagerの基本構造を作成（部分的に完了）
4. ⏳ 定数の抽出（`UIConstants` の作成）- 中優先度
5. ⏳ ヘルパークラスの作成（`UIButtonHelper`、`UIDialogHelper` など）- 中優先度
6. ⏳ ResultScreenManagerの結果テキスト設定の実装（将来的に検討）
