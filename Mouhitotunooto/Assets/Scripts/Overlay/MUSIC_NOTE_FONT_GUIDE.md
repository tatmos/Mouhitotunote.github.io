# 音符文字（♫）の表示方法

## 概要

エンドクレジット時の音符エフェクトで使用する♫（U+266B）が文字化けする問題を解決する方法です。

**重要**: Zen Maru Gothicフォントファイル自体に♫が含まれていないため、フォントアセットに追加しても表示できません。以下の方法を使用してください。

## 解決方法

### 方法1: 画像として表示（推奨）

♫を画像（Sprite）として表示する方法です。最も確実で、フォントに依存しません。

#### 手順

1. **音符画像を用意**
   - ♫の画像を作成（PNG形式、透明背景推奨）
   - サイズ: 24x24px 〜 48x48px程度

2. **画像を配置**
   - `Assets/Resources/Overlay/MusicNotes/` フォルダを作成
   - 画像ファイル名を `BeamedNote.png` に変更して配置

3. **Unityでの設定**
   - Unityエディタで画像を選択
   - Inspectorで以下を設定：
     - **Texture Type**: `Sprite (2D and UI)`
     - **Max Size**: 適切なサイズに設定（例: 64）
     - **Apply** をクリック

4. **動作確認**
   - エンドクレジット画面で♫が画像として表示されることを確認

### 方法2: ♫を♪に置き換える（シンプル）

フォントに♫が含まれていないため、すべての音符を♪に統一する方法です。

#### 手順

1. `OverlayPresenter_UITK.cs` の248行目を以下のように変更：
   ```csharp
   string[] musicNotes = { "♪", "♪", "♪", "♪", "♪" }; // すべて♪に統一
   ```

### 方法3: 別のフォントを使用（非推奨）

♫を含む別のフォントを使用する方法ですが、フォントが混在するため推奨しません。

## 現在の実装

現在のコードでは、以下の優先順位で♫を表示します：

1. **画像が存在する場合**: `Resources/Overlay/MusicNotes/BeamedNote.png` を画像として表示
2. **画像が存在しない場合**: ♫を♪に置き換えてテキストとして表示

## 注意事項

- Zen Maru Gothicフォントファイル自体に♫が含まれていないため、フォントアセットに追加しても表示できません
- 画像を使用する場合は、`Assets/Resources/Overlay/MusicNotes/BeamedNote.png` に配置してください
- 画像が見つからない場合は、自動的に♪に置き換えられます

## 手順

### 1. Font Asset Creatorを開く

1. Unityエディタで `Window > TextMeshPro > Font Asset Creator` を開く

### 2. 既存のフォントアセットを開く

1. Projectウィンドウで `Assets/TextMesh Pro/Resources/Fonts & Materials/ZenMaruGothic-Regular SDF.asset` を選択
2. Font Asset Creatorウィンドウで、**Source Font File**にZenMaruGothic-Regularのフォントファイル（.ttfまたは.otf）を選択
   - フォントファイルが見つからない場合は、元のフォントファイルを再度選択してください

### 3. 文字セットを設定

**方法A: Custom Charactersを使用（推奨）**

1. **Character Set** を **Custom Characters** に設定
2. **Custom Character List** フィールドに以下の文字を入力：
   ```
   ♫
   ```
   - または、既存の文字に加えて `♫` を追加
3. **Sampling Point Size** を適切な値に設定（例: 72）
4. **Padding** を設定（例: 9）
5. **Packing Method** を **Fast** に設定
6. **Atlas Resolution** を適切な値に設定（例: 2048 x 2048）
7. **Generate Font Atlas** をクリック
8. **Save** をクリックして既存のアセットを上書き保存

**方法B: Unicode Rangeを使用**

1. **Character Set** を **Unicode Range** に設定
2. 既存のUnicode Rangeに加えて、以下の行を追加：
   ```
   266B-266B
   ```
   - 注意: `U+` プレフィックスは使用しない
   - 注意: 1行に1つの範囲を入力
3. **Generate Font Atlas** をクリック
4. **Save** をクリックして既存のアセットを上書き保存

### 4. UI Toolkit用のフォントアセットも更新（必要に応じて）

`ZenMaruGothic-Regular SDF_UI.asset` も同様に更新する必要がある場合があります。

1. `Assets/TextMesh Pro/Resources/Fonts & Materials/ZenMaruGothic-Regular SDF_UI.asset` を選択
2. 上記の手順を繰り返す

### 5. コードの確認

フォントアセットに♫が追加されれば、コード側の特別な処理は不要です。
`OverlayPresenter_UITK.cs`の音符エフェクトコードで、♫が正しく表示されるようになります。

## 注意事項

- フォントアセットを更新すると、既存の文字も再生成されるため、少し時間がかかる場合があります
- フォントアセットのサイズが大きくなる可能性がありますが、1文字追加するだけなので影響は最小限です
- WebGLビルド時にも正しく表示されることを確認してください

## トラブルシューティング

### フォントアセットが更新されない場合

1. Font Asset Creatorウィンドウを閉じて再開する
2. Unityエディタを再起動する
3. フォントファイルが正しく選択されているか確認する

### まだ文字化けする場合

1. フォントアセットが正しく保存されているか確認
2. GameTextSettingsのFont Assetが更新されたアセットを参照しているか確認
3. PanelSettingsのText Settingsが正しくアサインされているか確認
4. すべてのUIDocumentが同じPanelSettingsを使用しているか確認

