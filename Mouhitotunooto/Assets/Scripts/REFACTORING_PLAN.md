# リファクタリング計画

## 概要

UIManagerUIToolkit.cs（3758行）を整理するため、優先度の高い項目から実装を進めます。

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

### 3. ResultScreenManager ✅ (完了)

**実装済み:**
- `ResultScreenManager` クラスを作成
- `ResultScreenSettings` と `ResultScreenActions` のstructを作成
- `SetupEpilogue()` メソッドを実装（後日談の設定）
- `SetupWordGetDisplay()` メソッドを実装（ワードゲット表示の設定）
- `SetupBackButton()` メソッドを実装（戻るボタンの設定）
- `SetupResultText()` メソッドを実装（結果テキストの設定 - 約400行の複雑な処理を移行）
- `ShowResultScreen()` メソッドを `ResultScreenManager` を使用するように変更

**実装方針:**
- コールバック関数を使用してUIManagerUIToolkitとの依存関係を管理
- コルーチンが必要な処理は、`coroutineRunner.StartCoroutine`を使用

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

## 完了したタスク

### 高優先度
- ✅ **TitleScreenManager** - 完了（既に実装済み）
- ✅ **ScenarioScreenManager** - 完了（実装済み）
- ✅ **ResultScreenManager** - 完了（後日談、ワードゲット表示、戻るボタン、結果テキスト設定のすべてを実装）

### 中優先度
- ✅ **UIConstants** - 完了（定数クラスを作成）
  - カラー定数（DarkBrown、BrightText）
  - フォントサイズ定数（FontSizeNormal、FontSizeMedium、FontSizeTitle）
  - オーバーレイ関連の定数
  - フェード関連の定数
  - デフォルトのテキストシャドウ設定
- ✅ **UIButtonHelper** - 完了（ボタンヘルパークラスを作成）
  - `ApplyButtonImage` メソッド
  - `SetupButtonWithIcon` メソッド（プレースホルダー）
  - `SetupButtonWithEvents` メソッド
- ✅ **UIConstantsとUIButtonHelperの使用** - 完了
  - `UIManagerUIToolkit.cs`内で`UIConstants`を使用するように更新（色の定数、フォントサイズ）
  - `UIManagerUIToolkit.cs`内で`UIButtonHelper.ApplyButtonImage`を使用するように更新
  - ハードコードされた値を定数に置き換え（16箇所の色、6箇所のフォントサイズ）
- ✅ **UIDialogHelper** - 完了（ダイアログヘルパークラスを作成）
  - `ShowConfirmationDialog` メソッドを実装
  - `UIManagerUIToolkit.cs`内の`ShowConfirmationDialog`を`UIDialogHelper`を使用するように変更

## 次のステップ

1. ✅ ScenarioScreenManagerの基本構造を作成（完了）
2. ✅ ShowScenarioScreenの主要な処理を移行（完了）
3. ✅ ResultScreenManagerの基本構造を作成（完了）
4. ✅ ResultScreenManagerの結果テキスト設定の実装（完了）
5. ✅ 定数の抽出（`UIConstants` の作成）- 完了
6. ✅ ヘルパークラスの作成（`UIButtonHelper`）- 完了
7. ✅ `UIConstants`と`UIButtonHelper`を`UIManagerUIToolkit.cs`内で使用するようにコードを更新（完了）
8. ✅ `UIDialogHelper`の作成（完了）

## 完了状況のまとめ

高優先度のリファクタリングはすべて完了しました：
- ✅ TitleScreenManager
- ✅ ScenarioScreenManager
- ✅ ResultScreenManager（SetupResultTextを含む完全実装）

中優先度のリファクタリングもすべて完了しました：
- ✅ UIConstants
- ✅ UIButtonHelper
- ✅ UIDialogHelper

**現状:**
- 主要なScreenManager（Title, Scenario, Result）はすべて分離済み
- 既存のScreenManager（Profile, Achievements, Credits, Selection, Mouhitotsu）は既に分離されている
- 定数とヘルパークラスも作成済み
- `UIManagerUIToolkit.cs`は大幅に縮小（約3230行、元の3607行から約377行削減）

**今後の検討事項:**
- 低優先度のリファクタリングが必要かどうか（現在のコードで問題がなければ、そのまま維持する）
- 他のヘルパークラス（UIStyleHelperなど）が必要かどうか
