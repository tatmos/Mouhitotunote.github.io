# ストリーマーオーバーレイ デバッグガイド

## オーバーレイが表示される条件

オーバーレイは、以下の条件で表示されます：

### Phase（フェーズ）による表示制御

1. **Hidden（非表示）** - デフォルト状態
   - Prologue〜Normal中盤
   - Division A

2. **Presence（気配のみ）** - 短い反応のみ
   - Division B + Normalモード
   - 表示時間が短い

3. **Active（常駐）** - 常に表示される
   - Darkモード + Division C以前
   - Thirdモード + Division C以前

4. **Quiet（静か）** - 表示されるが発話しない
   - Dark/Thirdモード + Division D/E

## オーバーレイを表示する方法

### 方法1: Division Bに入る（Normalモード中）

1. ゲームを開始
2. シナリオをプレイしてDivision Bに到達
3. オーバーレイが右下に表示される（Presenceフェーズ）

### 方法2: Darkモードに入る

1. ゲームを開始
2. シナリオをクリアしてDarkモードに突入
3. オーバーレイが右下に常駐表示される（Activeフェーズ）

### 方法3: デバッグ用に強制表示（開発者向け）

Unityエディタで以下を実行：

```csharp
// OverlayBootstrapコンポーネントを取得
var overlay = FindFirstObjectByType<NovelGame.Overlay.OverlayBootstrap>();
if (overlay != null)
{
    // PhaseをActiveに強制設定
    var state = overlay.GetState(); // 注意: このメソッドは現在publicではない
}
```

## 確認すべき項目

### 1. OverlayBootstrapがシーンに配置されているか

- Unityエディタで `SampleScene` を開く
- Hierarchyで `OverlayBootstrap` GameObjectを確認
- `OverlayBootstrap` コンポーネントがアタッチされているか確認

### 2. Overlay.uxmlが設定されているか

- `OverlayBootstrap` コンポーネントの `Overlay UXML` フィールドに `Overlay.uxml` が設定されているか確認

### 3. 画像リソースが正しく配置されているか

- `Assets/Resources/Overlay/Girl/` に表情画像が配置されているか
- `Assets/Resources/Overlay/Room/` に部屋背景画像が配置されているか
- 画像のファイル名が正確か（大文字小文字を区別）

### 4. Phaseが正しく更新されているか

- Consoleログで `[OverlayBootstrap]` のメッセージを確認
- Division Bに入った時にPhaseが更新されているか確認

### 5. イベントが発火しているか

- `DivisionEnteredEvt` や `ModeChangedEvt` が正しく発火しているか確認
- Consoleログでイベント関連のメッセージを確認

## トラブルシューティング

### オーバーレイが表示されない場合

1. **PhaseがHiddenのまま**
   - Division Bに入るか、Darkモードに入る必要があります
   - ゲームを進めて、適切なPhaseに到達してください

2. **画像が見つからない警告が出る**
   - `Assets/Resources/Overlay/` 配下に画像を配置してください
   - ファイル名が正確か確認してください

3. **OverlayBootstrapが初期化されていない**
   - シーンに `OverlayBootstrap` GameObjectが存在するか確認
   - `Overlay.uxml` が設定されているか確認

4. **UIDocumentが正しく設定されていない**
   - `OverlayBootstrap` コンポーネントの `Overlay Document` フィールドを確認
   - 空の場合は、自動的に作成されますが、手動で設定することもできます

### オーバーレイは表示されるが、画像が表示されない場合

1. **画像のTexture Type設定**
   - 表情画像: `Sprite (2D and UI)`
   - 部屋背景: `Default` または `Sprite (2D and UI)`

2. **画像のパス**
   - `Resources/Overlay/Girl/` 配下に配置
   - `Resources/Overlay/Room/` 配下に配置

3. **画像のサイズ**
   - 画像が小さすぎる場合は、表示されない可能性があります
   - 推奨サイズ: 表情 200x300px以上、部屋背景 800x600px以上

## デバッグログの確認

Unity Consoleで以下のログを確認：

- `[OverlayBootstrap]` - 初期化関連
- `[OverlayAssets]` - 画像読み込み関連
- `[OverlayPresenter]` - 表示関連

警告やエラーが出ていないか確認してください。

