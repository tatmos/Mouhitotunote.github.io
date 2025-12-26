# 背景画像が表示されない場合の修正ガイド

ログは表示されているが画像が表示されない場合の対処法です。

## 確認手順

### 1. Hierarchyの順番を確認（最重要）

背景画像は**必ず最背面（最初の子要素）**に配置する必要があります。

#### SelectionScreenの場合：
1. HierarchyでSelectionScreenを展開
2. **BackgroundImageが最初（最上部）**にあることを確認
3. もし違う場合は、BackgroundImageをドラッグして最上部に移動

正しい順番（上から下へ）：
```
SelectionScreen
  ├── BackgroundImage ← 最初（最背面）
  ├── TitleText
  ├── ScoreText
  ├── ScenarioButtonParent
  └── ShowProfileButton
```

#### ProfileScreenの場合：
1. HierarchyでProfileScreenを展開
2. **BackgroundImageが最初（最上部）**にあることを確認
3. もし違う場合は、BackgroundImageをドラッグして最上部に移動

正しい順番（上から下へ）：
```
ProfileScreen
  ├── BackgroundImage ← 最初（最背面）
  ├── ProfileSectionTitle
  ├── ProfileParent
  └── BackToSelectionButtonFromProfile
```

### 2. Imageコンポーネントの設定を確認

1. BackgroundImageを選択
2. InspectorでImageコンポーネントを確認：
   - **Source Image**: 画像がアサインされているか
   - **Color**: R=255, G=255, B=255, A=255（白・不透明）になっているか
   - **Image Type**: Simple
   - **Preserve Aspect**: ✓（チェックを入れる）
   - **Enabled**: ✓（チェックが入っているか）

### 3. RectTransformの設定を確認

1. BackgroundImageを選択
2. InspectorでRectTransformを確認：
   - **Anchor Presets**: Stretch Stretchを選択
   - **Left**: 0
   - **Top**: 0
   - **Right**: 0
   - **Bottom**: 0
   - **Pos X**: 0
   - **Pos Y**: 0
   - **Width**: 自動的に設定される（0のままでOK）
   - **Height**: 自動的に設定される（0のままでOK）

### 4. Canvasの設定を確認

1. Canvasを選択
2. Inspectorで以下を確認：
   - **Render Mode**: Screen Space - Overlay
   - **Sort Order**: 0（他のCanvasと重なっていないか確認）

### 5. 画像のインポート設定を再確認

1. Projectウィンドウで背景画像を選択
2. Inspectorで以下を確認：
   - **Texture Type**: Sprite (2D and UI)
   - **Max Size**: 2048以上を推奨
   - **Compression**: None または High Quality
   - **Apply**をクリック

### 6. 一時的なテスト

背景画像が正しく表示されるかテストするため：

1. SelectionScreen内のBackgroundImageを選択
2. InspectorのImageコンポーネントで：
   - **Source Image**に直接画像をドラッグ（UIManager経由ではなく）
   - **Color**を白（255, 255, 255, 255）に設定
3. ゲームを実行して背景が表示されるか確認

表示される場合：UIManagerの設定は問題なし。Hierarchyの順番やRectTransformを確認。
表示されない場合：ImageコンポーネントやCanvasの設定を確認。

## よくある問題

### 問題1: 背景が他のUI要素の後ろに隠れている
**原因**: Hierarchyの順番が間違っている
**解決**: BackgroundImageを最上部（最背面）に移動

### 問題2: 画像の色が透明になっている
**原因**: ImageコンポーネントのColorのAlpha値が0
**解決**: ColorのAlpha値を255に設定

### 問題3: RectTransformのサイズが0
**原因**: Anchor Presetsが正しく設定されていない
**解決**: Anchor Presetsを「Stretch Stretch」に設定し、Left/Top/Right/Bottomを0に設定

### 問題4: Imageコンポーネントが無効
**原因**: ImageコンポーネントのEnabledがオフ
**解決**: InspectorでImageコンポーネントの✓を確認

### 問題5: GameObjectが無効
**原因**: BackgroundImageのGameObjectが無効になっている
**解決**: HierarchyでBackgroundImageの✓を確認

## デバッグ情報の確認

Consoleに表示されるデバッグ情報を確認：
- **Sprite**: 画像名が表示されているか
- **Enabled**: trueになっているか
- **Color**: (1.0, 1.0, 1.0, 1.0)になっているか
- **Size**: 適切なサイズ（例: (1920, 1080)）になっているか
- **GameObject Active**: trueになっているか

これらの情報から問題を特定できます。

