# ミニノベルゲーム Unity移植版

## 概要
index.htmlで作成されたミニノベルゲームをUnityに移植したバージョンです。

## セットアップ手順

### 1. シーンの設定

1. `Assets/Scenes/SampleScene.unity`を開くか、新しいシーンを作成
2. シーンに以下を追加：
   - `NovelGameInitializer`コンポーネントを持つGameObject（空のGameObjectを作成して追加）

### 2. UIの作成

#### Canvasの作成
1. Hierarchyで右クリック → UI → Canvas
2. Canvasの設定：
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler: UI Scale Mode = Scale With Screen Size
   - Reference Resolution: 1920 x 1080

#### 選択画面（Selection Screen）の作成
1. Canvasの子として空のGameObjectを作成し、「SelectionScreen」と命名
2. SelectionScreenの子要素として以下を作成：
   - **TitleText** (TextMeshProUGUI): タイトル「ミニノベルゲーム」
   - **ScoreText** (TextMeshProUGUI): スコア表示
   - **ScenarioButtonParent** (Empty GameObject): シナリオボタンの親
   - **ScenarioButtonPrefab** (Button):
     - 子要素にTextMeshProUGUIを追加
     - ボタンのテキストを設定

#### シナリオ画面（Scenario Screen）の作成
1. Canvasの子として空のGameObjectを作成し、「ScenarioScreen」と命名
2. ScenarioScreenの子要素として以下を作成：
   - **ScenarioTitleText** (TextMeshProUGUI): シナリオタイトル
   - **SetupText** (TextMeshProUGUI): シナリオの設定テキスト
   - **ChoiceButtonParent** (Empty GameObject): 選択肢ボタンの親
   - **ChoiceButtonPrefab** (Button):
     - 子要素に2つのTextMeshProUGUIを追加（1つ目: 選択肢テキスト、2つ目: プレビューテキスト）
   - **BackToSelectionButton** (Button): 選択画面に戻るボタン

#### 結果画面（Result Screen）の作成
1. Canvasの子として空のGameObjectを作成し、「ResultScreen」と命名
2. ResultScreenの子要素として以下を作成：
   - **ResultText** (TextMeshProUGUI): 結果テキスト
   - **WordGetText** (TextMeshProUGUI): ワードゲット表示
   - **EpilogueText** (TextMeshProUGUI): 後日談テキスト
   - **BackToSelectionButton** (Button): 選択画面に戻るボタン

### 3. UIManagerの設定

1. Canvasまたは空のGameObjectに`UIManager`コンポーネントを追加
2. Inspectorで以下を設定：
   - **Selection Screen**: SelectionScreenオブジェクトをアサイン
   - **Title Text**: TitleTextをアサイン
   - **Score Text**: ScoreTextをアサイン
   - **Scenario Button Parent**: ScenarioButtonParentをアサイン
   - **Scenario Button Prefab**: ScenarioButtonPrefabをアサイン
   - **Scenario Screen**: ScenarioScreenオブジェクトをアサイン
   - **Scenario Title Text**: ScenarioTitleTextをアサイン
   - **Setup Text**: SetupTextをアサイン
   - **Choice Button Parent**: ChoiceButtonParentをアサイン
   - **Choice Button Prefab**: ChoiceButtonPrefabをアサイン
   - **Back To Selection Button From Scenario**: シナリオ画面の選択画面に戻るボタンをアサイン
   - **Result Screen**: ResultScreenオブジェクトをアサイン
   - **Result Text**: ResultTextをアサイン
   - **Word Get Text**: WordGetTextをアサイン
   - **Epilogue Text**: EpilogueTextをアサイン
   - **Back To Selection Button**: 結果画面の選択画面に戻るボタンをアサイン

### 4. TextMeshProのセットアップ

初回使用時は、TextMeshProをインポートする必要があります：
1. TextMeshProコンポーネントを使用する場合、自動的にインポートダイアログが表示されます
2. 「Import TMP Essentials」をクリックしてインポート

### 5. 初期状態の設定

- SelectionScreenは有効（Active）
- ScenarioScreenは無効（Inactive）
- ResultScreenは無効（Inactive）

### 6. 実行

Playボタンを押してゲームを実行します。

## ゲームの機能

- 6つのシナリオから選択してプレイ
- 各シナリオで選択肢を選び、ストーリーを進める
- 「もうひとつ」という言葉を構成する文字（も、う、ひ、と、つ）を集める
- 最初の5つのシナリオをクリアすると、最後のシナリオ「真実の扉」が解放される
- スコアが異常な値になるとダークモードが有効になる

## 注意事項

- ボタンのプレハブは、Hierarchyで作成したものをProjectウィンドウにドラッグ＆ドロップしてプレハブ化してください
- TextMeshProを使用する場合は、フォントの設定が必要な場合があります
- UIのレイアウトは、1920x1080の解像度を基準に設計されています

