# WebGLビルド時の文字のグレー背景問題の解決方法

## 問題
WebGLビルド時に一部の文字に四角いグレーの背景が表示され、文字が見づらくなる問題が発生しています。

**特定のフォント（例: ZenMaruGothic-Regular SDF_UI）で問題が発生している場合**:
この問題は、UI ToolkitのText Settingsで設定されているフォントアセット（`GameTextSettings.asset`）の設定が原因である可能性が高いです。

現在、`GameTextSettings.asset`には `ZenMaruGothic-Regular SDF_UI` が設定されています。

## 原因
この問題は、SDF（Signed Distance Field）フォントのレンダリング設定や、フォントアセットのPadding設定が不適切な場合に発生します。特にWebGLでは、フォントテクスチャのアルファチャンネル処理が異なるため、この問題が顕著に現れることがあります。

## 解決方法

### 方法0: 使用しているフォントアセットを特定する

1. Projectウィンドウで `Assets/UI Toolkit/GameTextSettings.asset` を選択
2. Inspectorで **Default Font Asset** を確認
3. このフォントアセットが問題の原因です

### 方法1: フォントアセットのPaddingを調整（推奨）

フォントアセットのPaddingが小さすぎると、文字の周囲にグレーの背景が表示されることがあります。

1. Projectウィンドウで `Assets/TextMesh Pro/Resources/Fonts & Materials/ZenMaruGothic-Regular SDF_UI.asset` を選択
2. Inspectorで **Padding** の値を確認
3. **Padding** を **9** から **12** または **15** に増やす
4. **Generate Font Atlas** をクリックして再生成
5. **Save** をクリックして保存
6. WebGLビルドを再実行して確認

### 方法2: フォントアセットのAtlas Resolutionを増やす

Atlas Resolutionが低いと、文字のエッジが不鮮明になり、グレーの背景が目立つことがあります。

1. Projectウィンドウで `Assets/TextMesh Pro/Resources/Fonts & Materials/ZenMaruGothic-Regular SDF_UI.asset` を選択
2. Inspectorで **Atlas Resolution** を確認
3. **Atlas Resolution** を **2048 x 2048** から **4096 x 4096** に増やす（必要に応じて）
4. **Generate Font Atlas** をクリックして再生成
5. **Save** をクリックして保存
6. WebGLビルドを再実行して確認

**注意**: Atlas Resolutionを増やすと、フォントアセットのファイルサイズが大きくなります。

### 方法3: フォントアセットのSampling Point Sizeを調整

Sampling Point Sizeが小さすぎると、文字の品質が低下し、グレーの背景が目立つことがあります。

1. `Window > TextMeshPro > Font Asset Creator` を開く
2. **Source Font File** に `Assets/Fonts/ZenMaruGothic-Regular.ttf` または元のフォントファイルを選択
3. 既存の `ZenMaruGothic-Regular SDF_UI` を上書きするか、新しい名前で保存する
3. **Sampling Point Size** を **72** から **96** または **128** に増やす
4. **Generate Font Atlas** をクリック
5. **Save** または **Save as...** をクリックして保存
6. WebGLビルドを再実行して確認

### 方法4: PanelSettingsのSDF Shader設定を確認

UI ToolkitのPanelSettingsでSDF Shaderが正しく設定されているか確認します。

1. Projectウィンドウで `Assets/UI Toolkit/GamePanelSettings.asset` を選択
2. Inspectorで **SDF Shader** が正しく設定されているか確認
3. 設定されていない場合は、デフォルトのSDF Shaderをアサイン

### 方法5: フォントアセットのマテリアル設定を確認

フォントアセットのマテリアルで、アルファチャンネルの処理が正しく設定されているか確認します。

1. フォントアセットを選択
2. Inspectorで **Material** を確認
3. マテリアルを選択して、Inspectorで以下を確認：
   - **Shader**: `TextMeshPro/Distance Field` または `TextMeshPro/Distance Field (UI)`
   - **Face Color** のアルファ値が適切に設定されているか
   - **Outline Color** が不要な場合は無効にする

### 方法6: USSで文字の背景色を明示的に設定

USSファイルで文字の背景色を明示的に設定することで、グレーの背景を隠すことができます。

`GameStyles.uss` に以下を追加：

```css
/* 文字の背景を透明にする */
Label {
    background-color: transparent;
}

/* または、特定のクラスに適用 */
.text-no-background {
    background-color: transparent;
}
```

ただし、この方法は根本的な解決にはなりません。

## 推奨される手順

1. **まず方法1を試す**（Paddingを増やす）
   - 最も簡単で効果的な方法です
2. **方法2を試す**（Atlas Resolutionを増やす）
   - Paddingを増やしても解決しない場合
3. **方法3を試す**（Sampling Point Sizeを増やす）
   - さらに品質を向上させたい場合

## 注意事項

- PaddingやAtlas Resolutionを増やすと、フォントアセットのファイルサイズが大きくなります
- WebGLビルドのサイズが増加する可能性があります
- すべての文字に適用されるため、必要な文字のみを含めるようにUnicode Rangeを調整することを検討してください

## トラブルシューティング

### まだグレーの背景が表示される場合

1. フォントアセットが正しくビルドに含まれているか確認
2. ブラウザのコンソールでエラーを確認
3. 別のブラウザでテストして、ブラウザ固有の問題でないか確認
4. フォントアセットのマテリアル設定を確認
5. PanelSettingsのSDF Shader設定を確認

### フォントアセットのサイズが大きすぎる場合

- PaddingやAtlas Resolutionを段階的に調整
- 必要な文字のみを含めるようにUnicode Rangeを調整
- 複数のフォントアセットを使用して、必要な文字のみを含める

