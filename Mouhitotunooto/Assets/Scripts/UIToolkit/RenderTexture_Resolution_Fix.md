# RenderTexture解像度とクリック位置ずれ問題 - 修正内容

## 問題
画面のクリック位置とボタンの反応位置がずれている

## 原因
**RenderTextureの解像度とPanelSettingsのReference Resolutionが一致していなかった**ことが原因でした。

### 問題の詳細
- **RenderTextureのサイズ**: 1920x1080
- **PanelSettingsのReference Resolution**: 960x540
- **CanvasScalerのreferenceResolution**: 1920x1080
- **CanvasScalerのmatchWidthOrHeight**: 0.5（バランス）

この不一致により、UI Toolkitの座標系とCanvas（RenderTextureを表示する）の座標系がずれ、クリック位置が正しく変換されていませんでした。

## 解決方法

### RenderTextureのサイズを960x540に変更

**修正ファイル**:
- `Assets/Scripts/UIToolkit/UIToolkitRenderTextureManager.cs`

**変更内容**:
1. **RenderTextureのサイズ**: 1920x1080 → **960x540**
   ```csharp
   [SerializeField] private int renderTextureWidth = 960;
   [SerializeField] private int renderTextureHeight = 540;
   ```

2. **CanvasScalerのmatchWidthOrHeight**: 0.5 → **0.0（幅基準）**
   ```csharp
   scaler.matchWidthOrHeight = 0.0f; // 幅基準（PanelSettingsと一致させる）
   ```

## 現在の設定

### PanelSettings
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
Reference Resolution: 960 x 540
Match Width Or Height: 0.0 (幅基準)
```

## 効果

RenderTextureのサイズをPanelSettingsのReference Resolutionと一致させることで：
- UI Toolkitの座標系とCanvasの座標系が一致する
- クリック位置が正しく変換される
- ボタンの反応位置が正しくなる

## 注意事項

- RenderTextureのサイズを変更した場合、既存のRenderTextureは再作成される必要があります
- ゲーム実行中に変更した場合は、UIToolkitRenderTextureManagerを再初期化する必要があります
- パフォーマンスへの影響は最小限です（解像度が下がるため、むしろ軽くなる可能性があります）

## 追加の確認事項

もし問題が解決しない場合は、以下を確認してください：

1. **UIToolkitRenderTextureManagerが正しく初期化されているか**: `SetupRenderTextureMode()`が呼ばれているか確認
2. **すべてのPanelSettingsが同じReference Resolutionを使用しているか**: 960x540に統一されているか確認
3. **EditorとWebGLで同じ設定が適用されているか**: ビルド設定を確認
