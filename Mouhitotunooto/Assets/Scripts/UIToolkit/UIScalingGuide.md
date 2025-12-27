# UIスケーリング設定ガイド

## 問題
画面を大きくしたときに文字も合わせて大きくなるようにしたい。

## 解決方法

### PanelSettingsのScale Modeを変更

1. Projectウィンドウで `Assets/UI Toolkit/GamePanelSettings.asset` を選択
2. Inspectorで以下を設定：
   - **Scale Mode**: **Scale With Screen Size** に変更
   - **Reference Resolution**: 1920 x 1080（基準解像度）
   - **Screen Match Mode**: **Match Width Or Height** に設定
   - **Match**: **0.5** に設定（幅と高さのバランス）

### Scale Modeの説明

- **Constant Pixel Size (0)**: 画面サイズに関係なく、常に同じピクセルサイズで表示
- **Scale With Screen Size (1)**: 画面サイズに応じてUIがスケール（推奨）
- **Constant Physical Size (2)**: 物理的なサイズを維持（DPIに依存）

### Screen Match Modeの説明

- **Match Width Or Height (0)**: 幅または高さに合わせてスケール
  - **Match**: 0.0 = 幅に合わせる、1.0 = 高さに合わせる、0.5 = バランス
- **Shrink**: 画面が小さい場合に縮小
- **Expand**: 画面が大きい場合に拡大

### 推奨設定

```
Scale Mode: Scale With Screen Size
Reference Resolution: 1920 x 1080
Screen Match Mode: Match Width Or Height
Match: 0.5
```

この設定により、画面サイズが大きくなると、文字やUI要素も比例して大きくなります。

## 注意事項

- Reference Resolutionは、デザインの基準となる解像度です
- Match値を0.5に設定すると、幅と高さの両方を考慮したスケーリングになります
- 画面のアスペクト比が大きく異なる場合、レイアウトが崩れる可能性があります

