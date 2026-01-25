# RenderTextureが巨大に表示される問題 - 修正内容

## 問題
レンダリングだけが巨大になっている（UIが画面外にはみ出してしまうほど大きく表示されている）

## 原因
CanvasScalerのReference Resolutionを960x540に設定していたため、実際の画面サイズ（例：1920x1080）に対してRenderTextureが2倍に拡大されてしまっていました。

### 問題の詳細
- **RenderTextureのサイズ**: 960x540
- **PanelSettingsのReference Resolution**: 960x540
- **実際の画面サイズ**: 1920x1080など（より大きい）
- **CanvasScalerのReference Resolution**: 960x540

CanvasScalerが960x540をReference Resolutionとして使用しているため、実際の画面サイズ（1920x1080）に対してRenderTextureが2倍に拡大され、UIが巨大に表示されてしまっていました。

## 解決方法

### CanvasScalerを削除し、RawImageのuvRectを調整

**修正ファイル**:
- `Assets/Scripts/UIToolkit/UIToolkitRenderTextureManager.cs`

**変更内容**:
1. **CanvasScalerを削除**: CanvasScalerによるスケーリングを無効化
2. **RawImageのuvRectを調整**: RenderTextureのアスペクト比と画面のアスペクト比が異なる場合に備えて、アスペクト比を維持しながら画面全体に表示

```csharp
// CanvasScalerは使用しない
// RawImageのuvRectを調整して、RenderTextureを画面全体に適切に表示
float renderTextureAspect = (float)renderTextureWidth / renderTextureHeight;
float screenAspect = (float)Screen.width / Screen.height;

if (renderTextureAspect > screenAspect)
{
    // RenderTextureの方が横長の場合：高さを基準に表示
    float scale = (float)Screen.height / renderTextureHeight;
    float scaledWidth = renderTextureWidth * scale;
    float uvWidth = Screen.width / scaledWidth;
    float uvOffsetX = (1.0f - uvWidth) * 0.5f;
    uiDisplayImage.uvRect = new Rect(uvOffsetX, 0, uvWidth, 1.0f);
}
else
{
    // 画面の方が横長の場合：幅を基準に表示
    float scale = (float)Screen.width / renderTextureWidth;
    float scaledHeight = renderTextureHeight * scale;
    float uvHeight = Screen.height / scaledHeight;
    float uvOffsetY = (1.0f - uvHeight) * 0.5f;
    uiDisplayImage.uvRect = new Rect(0, uvOffsetY, 1.0f, uvHeight);
}
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

### Canvas（RenderTexture表示用）
```
CanvasScaler: なし（使用しない）
RawImage: 画面全体をカバー、uvRectでアスペクト比を調整
```

## 動作の仕組み

1. **UI Toolkit**: PanelSettingsのReference Resolution（960x540）を基準にUIを描画
2. **RenderTexture**: 960x540のサイズでUIを描画
3. **RawImage**: 画面全体をカバーするように設定
4. **uvRect調整**: RenderTextureのアスペクト比と画面のアスペクト比が異なる場合、アスペクト比を維持しながら画面全体に表示

これにより、RenderTextureが画面全体に適切に表示され、UIが巨大にならなくなります。

## 効果

CanvasScalerを削除し、RawImageのuvRectを調整することで：
- RenderTextureが画面全体に適切に表示される
- UIが巨大にならなくなる
- クリック位置も正しく動作する（PanelSettingsのReference Resolutionが960x540のままなので）

## 注意事項

- uvRectの調整により、アスペクト比が異なる場合に上下または左右に余白が生じる可能性があります
- これは、RenderTextureのサイズ（960x540）と実際の画面サイズのアスペクト比が異なる場合に発生します
