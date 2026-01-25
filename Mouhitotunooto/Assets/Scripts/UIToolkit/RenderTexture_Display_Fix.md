# RenderTextureが画面からはみ出す問題 - 最終修正

## 問題
UIが画面外にはみ出してしまうほど大きく表示されている

## 原因
CanvasScalerのReference Resolutionを実際の画面サイズ（例：1920x1080）に設定したことで、960x540のRenderTextureが1920x1080の基準でスケーリングされ、逆に大きくなってしまっていました。

## 解決方法

### CanvasScalerのReference ResolutionをRenderTextureのサイズに戻す

**修正ファイル**:
- `Assets/Scripts/UIToolkit/UIToolkitRenderTextureManager.cs`

**変更内容**:
```csharp
// 修正前（問題あり）
scaler.referenceResolution = new Vector2(Screen.width, Screen.height); // 実際の画面サイズ

// 修正後（正しい設定）
scaler.referenceResolution = new Vector2(renderTextureWidth, renderTextureHeight); // 960x540
scaler.matchWidthOrHeight = 0.0f; // 幅基準（PanelSettingsと一致）
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
Reference Resolution: 960 x 540（RenderTextureのサイズ）
Match Width Or Height: 0.0 (幅基準)
```

## 動作の仕組み

1. **UI Toolkit**: PanelSettingsのReference Resolution（960x540）を基準にUIを描画
2. **RenderTexture**: 960x540のサイズでUIを描画
3. **CanvasScaler**: Reference Resolutionを960x540に設定し、画面サイズに応じてRenderTextureをスケーリング
4. **RawImage**: 画面全体をカバーするように設定されており、スケーリングされたRenderTextureが表示される

これにより、RenderTextureが画面全体に適切に表示され、UIがはみ出さなくなります。

## 効果

CanvasScalerのReference ResolutionをRenderTextureのサイズ（960x540）に設定することで：
- RenderTextureが画面全体に適切にスケーリングされる
- UIがはみ出さなくなる
- クリック位置も正しく動作する（PanelSettingsのReference Resolutionが960x540のままなので）

## 注意事項

- CanvasScalerのReference ResolutionをRenderTextureのサイズに設定することで、RenderTextureが画面サイズに応じて適切にスケーリングされます
- 幅基準（matchWidthOrHeight = 0.0）のスケーリングにより、画面の幅に合わせて一貫したスケーリングが行われます
