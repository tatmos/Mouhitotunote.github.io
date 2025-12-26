# Unityエディタでの設定ガイド

このドキュメントでは、ミニノベルゲームをUnityで動作させるための具体的な設定手順を説明します。

## 前提条件

- Unity 2021.3以降（推奨）
- TextMeshProがインポート済み（初回使用時に自動でインポートされます）

## 1. シーンの準備

### 1.1 シーンを開く
1. `Assets/Scenes/SampleScene.unity`を開くか、新しいシーンを作成
2. シーンに`NovelGameInitializer`コンポーネントを持つGameObjectを追加
   - Hierarchyで右クリック → Create Empty
   - 名前を「GameInitializer」に変更
   - Inspectorで`Add Component` → `Novel Game > Novel Game Initializer`を追加

## 2. Canvasの作成と設定

### 2.1 Canvasの作成
1. Hierarchyで右クリック → `UI > Canvas`
2. CanvasのInspectorで以下を設定：
   - **Render Mode**: Screen Space - Overlay
   - **Canvas Scaler**コンポーネントを追加（自動追加される場合あり）
     - **UI Scale Mode**: Scale With Screen Size
     - **Reference Resolution**: X=1920, Y=1080
     - **Match**: 0.5（Width/Height）

## 3. 選択画面（SelectionScreen）の作成

### 3.1 SelectionScreenの作成
1. Canvasの子として空のGameObjectを作成（右クリック → Create Empty）
2. 名前を「SelectionScreen」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Alt+Shift+クリックで「Stretch Stretch」を選択
   - **Left, Top, Right, Bottom**: すべて0

### 3.2 TitleTextの作成
1. SelectionScreenの子として`UI > Text - TextMeshPro`を作成
2. 名前を「TitleText」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Top Center
   - **Pos Y**: -50
   - **Width**: 600
   - **Height**: 60
4. TextMeshProUGUIの設定：
   - **Text**: 「ミニノベルゲーム」（スクリプトで上書きされます）
   - **Font Size**: 48
   - **Alignment**: Center

### 3.3 ScoreTextの作成
1. SelectionScreenの子として`UI > Text - TextMeshPro`を作成
2. 名前を「ScoreText」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Top Center
   - **Pos Y**: -120
   - **Width**: 600
   - **Height**: 30
4. TextMeshProUGUIの設定：
   - **Text**: 「スコア: 0 / 6」（スクリプトで上書きされます）
   - **Font Size**: 24
   - **Alignment**: Center

### 3.4 ScenarioButtonParentの作成
1. SelectionScreenの子として空のGameObjectを作成
2. 名前を「ScenarioButtonParent」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Center
   - **Width**: 1000
   - **Height**: 400（自動調整されます）
   - **注意**: GridLayoutGroupはスクリプトで自動追加されます

### 3.5 ScenarioButtonPrefabの作成
1. SelectionScreenの子として`UI > Button - TextMeshPro`を作成
2. 名前を「ScenarioButtonPrefab」に変更
3. RectTransformの設定：
   - **Width**: 300
   - **Height**: 100
4. Buttonの設定：
   - **Interactable**: ✓
5. Buttonの子要素（Text (TMP)）の設定：
   - **Text**: 「シナリオ1」（プレースホルダー）
   - **Font Size**: 24
   - **Alignment**: Center
6. **重要**: このボタンをProjectウィンドウにドラッグしてプレハブ化
   - プレハブ化後、Hierarchyから削除してもOK

### 3.6 ShowProfileButtonの作成
1. SelectionScreenの子として`UI > Button - TextMeshPro`を作成
2. 名前を「ShowProfileButton」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Center
   - **Pos Y**: -300（ScenarioButtonParentの下）
   - **Width**: 300
   - **Height**: 50
4. Buttonの子要素（Text (TMP)）の設定：
   - **Text**: 「登場人物プロフィールを見る」
   - **Font Size**: 20
   - **Alignment**: Center

## 4. シナリオ画面（ScenarioScreen）の作成

### 4.1 ScenarioScreenの作成
1. Canvasの子として空のGameObjectを作成
2. 名前を「ScenarioScreen」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Stretch Stretch
   - **Left, Top, Right, Bottom**: すべて0
4. **初期状態**: 無効（Inactive）に設定

### 4.2 ScenarioTitleTextの作成
1. ScenarioScreenの子として`UI > Text - TextMeshPro`を作成
2. 名前を「ScenarioTitleText」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Top Center
   - **Pos Y**: -50
   - **Width**: 800
   - **Height**: 50
4. TextMeshProUGUIの設定：
   - **Font Size**: 36
   - **Alignment**: Center

### 4.3 SetupTextの作成
1. ScenarioScreenの子として`UI > Text - TextMeshPro`を作成
2. 名前を「SetupText」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Top Center
   - **Pos Y**: -120
   - **Width**: 800
   - **Height**: 200
4. TextMeshProUGUIの設定：
   - **Font Size**: 20
   - **Alignment**: Top Center
   - **Wrapping**: ✓（折り返しを有効化）

### 4.4 ChoiceButtonParentの作成
1. ScenarioScreenの子として空のGameObjectを作成
2. 名前を「ChoiceButtonParent」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Center
   - **Width**: 800
   - **Height**: 400（自動調整されます）

### 4.5 ChoiceButtonPrefabの作成
1. ScenarioScreenの子として`UI > Button - TextMeshPro`を作成
2. 名前を「ChoiceButtonPrefab」に変更
3. RectTransformの設定：
   - **Width**: 780
   - **Height**: 120
4. Buttonの子要素を削除し、以下を作成：
   - **ChoiceText** (TextMeshProUGUI): 選択肢のテキスト
     - **Anchor Presets**: Top Stretch
     - **Pos Y**: -10
     - **Height**: 40
     - **Font Size**: 20
   - **PreviewText** (TextMeshProUGUI): プレビューテキスト
     - **Anchor Presets**: Bottom Stretch
     - **Pos Y**: 10
     - **Height**: 60
     - **Font Size**: 16
5. **重要**: このボタンをProjectウィンドウにドラッグしてプレハブ化

### 4.6 BackToSelectionButtonFromScenarioの作成
1. ScenarioScreenの子として`UI > Button - TextMeshPro`を作成
2. 名前を「BackToSelectionButtonFromScenario」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Bottom Center
   - **Pos Y**: 50
   - **Width**: 200
   - **Height**: 50
4. Buttonの子要素（Text (TMP)）の設定：
   - **Text**: 「選択画面に戻る」
   - **Font Size**: 18

## 5. 結果画面（ResultScreen）の作成

### 5.1 ResultScreenの作成
1. Canvasの子として空のGameObjectを作成
2. 名前を「ResultScreen」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Stretch Stretch
   - **Left, Top, Right, Bottom**: すべて0
4. **初期状態**: 無効（Inactive）に設定

### 5.2 ResultTextの作成
1. ResultScreenの子として`UI > Text - TextMeshPro`を作成
2. 名前を「ResultText」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Top Center
   - **Pos Y**: -50
   - **Width**: 800
   - **Height**: 300
4. TextMeshProUGUIの設定：
   - **Font Size**: 18
   - **Alignment**: Top Left
   - **Wrapping**: ✓

### 5.3 WordGetTextの作成
1. ResultScreenの子として`UI > Text - TextMeshPro`を作成
2. 名前を「WordGetText」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Center
   - **Pos Y**: -200
   - **Width**: 600
   - **Height**: 50
4. TextMeshProUGUIの設定：
   - **Font Size**: 24
   - **Alignment**: Center

### 5.4 EpilogueTextの作成
1. ResultScreenの子として`UI > Text - TextMeshPro`を作成
2. 名前を「EpilogueText」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Center
   - **Pos Y**: -100
   - **Width**: 800
   - **Height**: 200
4. TextMeshProUGUIの設定：
   - **Font Size**: 16
   - **Alignment**: Top Left
   - **Wrapping**: ✓

### 5.5 BackToSelectionButtonの作成
1. ResultScreenの子として`UI > Button - TextMeshPro`を作成
2. 名前を「BackToSelectionButton」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Bottom Center
   - **Pos Y**: 50
   - **Width**: 200
   - **Height**: 50
4. Buttonの子要素（Text (TMP)）の設定：
   - **Text**: 「シナリオ選択に戻る」
   - **Font Size**: 18

## 6. プロフィール画面（ProfileScreen）の作成

### 6.1 ProfileScreenの作成
1. Canvasの子として空のGameObjectを作成
2. 名前を「ProfileScreen」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Stretch Stretch
   - **Left, Top, Right, Bottom**: すべて0
4. **初期状態**: 無効（Inactive）に設定

### 6.2 ProfileSectionTitleの作成
1. ProfileScreenの子として`UI > Text - TextMeshPro`を作成
2. 名前を「ProfileSectionTitle」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Center（VerticalLayoutGroupが配置を制御します）
   - **Width**: 600
   - **Height**: 60
   - **注意**: Pos Yは設定しない（VerticalLayoutGroupが自動配置）
4. TextMeshProUGUIの設定：
   - **Text**: 「登場人物プロフィール」
   - **Font Size**: 36
   - **Alignment**: Center

### 6.3 ProfileParentの作成
1. ProfileScreenの子として空のGameObjectを作成
2. 名前を「ProfileParent」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Center（VerticalLayoutGroupが配置を制御します）
   - **Width**: 1000
   - **Height**: 600（自動調整されます）
   - **注意**: 
     - GridLayoutGroupはスクリプトで自動追加されます
     - Pos Yは設定しない（VerticalLayoutGroupが自動配置）

### 6.4 ProfileCardPrefabの作成
1. ProfileScreenの子として`UI > Image`を作成
2. 名前を「ProfileCardPrefab」に変更
3. RectTransformの設定：
   - **Width**: 300
   - **Height**: 380
4. Imageの設定：
   - **Color**: 白（後でスクリプトで変更されます）
5. 子要素として以下を作成：

   **NameText** (TextMeshProUGUI):
   - **Anchor Presets**: Top Stretch
   - **Pos Y**: -10
   - **Height**: 30
   - **Font Size**: 20
   - **Font Style**: Bold

   **InfoText** (TextMeshProUGUI):
   - **Anchor Presets**: Top Stretch
   - **Pos Y**: -50
   - **Height**: 150
   - **Font Size**: 14
   - **Wrapping**: ✓

   **QuoteText** (TextMeshProUGUI):
   - **Anchor Presets**: Top Stretch
   - **Pos Y**: -210
   - **Height**: 30
   - **Font Size**: 12
   - **Font Style**: Italic

   **EpilogueText** (TextMeshProUGUI):
   - **Anchor Presets**: Top Stretch
   - **Pos Y**: -250
   - **Height**: 80
   - **Font Size**: 12
   - **Wrapping**: ✓

   **ExpandButton** (Button):
   - **Anchor Presets**: Top Stretch
   - **Pos Y**: -340
   - **Height**: 25
   - **Text**: 「▶ 後日談の後日談を見る」
   - **Font Size**: 10

   **Epilogue2Text** (TextMeshProUGUI):
   - **Anchor Presets**: Top Stretch
   - **Pos Y**: -370
   - **Height**: 60
   - **Font Size**: 11
   - **Wrapping**: ✓
   - **初期状態**: 無効（Inactive）

   **HintText** (TextMeshProUGUI):
   - **Anchor Presets**: Top Stretch
   - **Pos Y**: -340
   - **Height**: 40
   - **Font Size**: 11
   - **Wrapping**: ✓
   - **初期状態**: 無効（Inactive）

   **LockedOverlay** (Image):
   - **Anchor Presets**: Stretch Stretch
   - **Color**: グレー（Alpha: 200）
   - **初期状態**: 無効（Inactive）

6. **重要**: このカードをProjectウィンドウにドラッグしてプレハブ化

### 6.5 BackToSelectionButtonFromProfileの作成
1. ProfileScreenの子として`UI > Button - TextMeshPro`を作成
2. 名前を「BackToSelectionButtonFromProfile」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Center（VerticalLayoutGroupが配置を制御します）
   - **Width**: 200
   - **Height**: 50
   - **注意**: Pos Yは設定しない（VerticalLayoutGroupが自動配置）
4. Buttonの子要素（Text (TMP)）の設定：
   - **Text**: 「選択画面に戻る」
   - **Font Size**: 18
5. **重要**: ProfileScreenの子要素の順番を確認
   - ProfileSectionTitle（最初）
   - ProfileParent（2番目）
   - BackToSelectionButtonFromProfile（最後）

## 7. UIManagerの設定

### 7.1 UIManagerコンポーネントの追加
1. Canvasまたは空のGameObjectに`UIManager`コンポーネントを追加
   - `Add Component` → `Novel Game > UI Manager`

### 7.2 UIManagerのInspector設定

**Selection Screen:**
- **Selection Screen**: SelectionScreenオブジェクトをドラッグ
- **Title Text**: TitleTextをドラッグ
- **Score Text**: ScoreTextをドラッグ
- **Scenario Button Parent**: ScenarioButtonParentをドラッグ
- **Scenario Button Prefab**: 作成したScenarioButtonPrefabプレハブをドラッグ
- **Show Profile Button**: ShowProfileButtonをドラッグ

**Scenario Screen:**
- **Scenario Screen**: ScenarioScreenオブジェクトをドラッグ
- **Scenario Title Text**: ScenarioTitleTextをドラッグ
- **Setup Text**: SetupTextをドラッグ
- **Choice Button Parent**: ChoiceButtonParentをドラッグ
- **Choice Button Prefab**: 作成したChoiceButtonPrefabプレハブをドラッグ
- **Back To Selection Button From Scenario**: BackToSelectionButtonFromScenarioをドラッグ

**Result Screen:**
- **Result Screen**: ResultScreenオブジェクトをドラッグ
- **Result Text**: ResultTextをドラッグ
- **Word Get Text**: WordGetTextをドラッグ
- **Epilogue Text**: EpilogueTextをドラッグ
- **Back To Selection Button**: BackToSelectionButtonをドラッグ

**Profile Screen:**
- **Profile Screen**: ProfileScreenオブジェクトをドラッグ
- **Profile Parent**: ProfileParentをドラッグ
- **Profile Card Prefab**: 作成したProfileCardPrefabプレハブをドラッグ
- **Profile Section Title**: ProfileSectionTitleをドラッグ
- **Back To Selection Button From Profile**: BackToSelectionButtonFromProfileをドラッグ

## 8. 初期状態の確認

以下のGameObjectが**無効（Inactive）**になっていることを確認：
- ScenarioScreen
- ResultScreen
- ProfileScreen

**有効（Active）**になっているもの：
- SelectionScreen
- Canvas

## 9. 実行とテスト

1. Playボタンを押してゲームを実行
2. 選択画面が表示されることを確認
3. シナリオボタンをクリックしてシナリオ画面に遷移することを確認
4. 選択肢を選んで結果画面が表示されることを確認
5. プロフィールボタンでプロフィール画面に遷移することを確認

## トラブルシューティング

### UI要素が重なって表示される
- 各画面のVerticalLayoutGroupが正しく設定されているか確認
- 各要素のRectTransformのAnchor設定を確認

### ボタンが表示されない
- プレハブが正しく作成されているか確認
- UIManagerのInspectorでプレハブがアサインされているか確認

### テキストが表示されない
- TextMeshProが正しくインポートされているか確認
- フォントが設定されているか確認（デフォルトフォントでOK）

### 画面遷移ができない
- 各画面のGameObjectが正しくアサインされているか確認
- ボタンのOnClickイベントがスクリプトで設定されているか確認（自動設定されます）

