# RenderTextureが画面からはみ出す問題 - 修正内容

## 問題
RenderTextureのサイズを960x540に変更した後、UIが画面からはみ出してしまっている

## 原因
RenderTextureのサイズ（960x540）と実際の画面サイズ（例：1920x1080）の比率が異なるため、CanvasScalerがRenderTextureを画面全体に表示しようとして、スケーリングが正しく行われていませんでした。

### 問題の詳細
- **RenderTextureのサイズ**: 960x540
- **PanelSettingsのReference Resolution**: 960x540
- **実際の画面サイズ**: 1920x1080など（より大きい）
- **CanvasScalerのReference Resolution**: 960x540（RenderTextureのサイズ）

CanvasScalerが960x540をReference Resolutionとして使用しているため、実際の画面サイズ（1920x1080）に対してRenderTextureが2倍に拡大され、UIがはみ出してしまっていました。

## 解決方法

### CanvasScalerのReference Resolutionを実際の画面サイズに変更

**修正ファイル**:
- `Assets/Scripts/UIToolkit/UIToolkitRenderTextureManager.cs`

**変更内容**:
```csharp
// 修正前
scaler.referenceResolution = new Vector2(renderTextureWidth, renderTextureHeight); // 960x540

// 修正後
scaler.referenceResolution = new Vector2(Screen.width, Screen.height); // 実際の画面サイズ
```

## 現在の設定

### PanelSettings（UI Toolkit内部）
```
Scale Mode: Scale With Screen Size (1)
Reference Resolution: 960 x 540
Screen Match Mode: Match Width Or Height
Match: 0.0 (幅基準)
```

### RenderTexture
```
Width: 960
Height: 540
```

### CanvasScaler（RenderTexture表示用）
```
Scale Mode: Scale With Screen Size
Reference Resolution: 実際の画面サイズ（例：1920x1080）
Match Width Or Height: 0.0 (幅基準)
```

## 動作の仕組み

1. **UI Toolkit**: PanelSettingsのReference Resolution（960x540）を基準にUIを描画
2. **RenderTexture**: 960x540のサイズでUIを描画
3. **CanvasScaler**: 実際の画面サイズをReference Resolutionとして使用し、RenderTextureを画面全体に適切に表示

これにより、RenderTextureが画面全体に表示され、UIがはみ出さなくなります。

## 効果

CanvasScalerのReference Resolutionを実際の画面サイズに変更することで：
- RenderTextureが画面全体に適切に表示される
- UIがはみ出さなくなる
- クリック位置も正しく動作する（PanelSettingsのReference Resolutionが960x540のままなので）

## 注意事項

- CanvasScalerのReference Resolutionは実行時に決定されるため、EditorとWebGLで異なる画面サイズを使用している場合、動作が異なる可能性があります
- もし固定のReference Resolutionが必要な場合は、`Screen.width`と`Screen.height`の代わりに固定値を使用できます

## 追加の調整が必要な場合

もし問題が解決しない場合は、以下を確認してください：

1. **実際の画面サイズの確認**: `Screen.width`と`Screen.height`の値を確認
2. **RenderTextureのサイズ**: 960x540が適切か確認
3. **PanelSettingsの設定**: Reference Resolutionが960x540になっているか確認
