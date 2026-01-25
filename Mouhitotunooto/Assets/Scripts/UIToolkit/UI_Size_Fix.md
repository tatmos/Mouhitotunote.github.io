# UIサイズ調整 - 修正内容

## 問題
Reference Resolutionを960x540に変更した後、一部のUIや文字が大きすぎる問題が発生。

## 原因
- **Scale Mode**が「Scale With Screen Size (1)」になっていた
- Reference Resolutionが960x540で、実際の画面サイズ（例：1920x1080）が2倍の場合、UI要素が2倍にスケールされてしまう

## 解決方法

### Scale Modeを「Constant Pixel Size」に変更

**修正ファイル**:
- `Assets/UI Toolkit/PanelSettings.asset`
- `Assets/UI Toolkit/GamePanelSettings.asset`

**変更内容**:
- `m_ScaleMode: 1` → `m_ScaleMode: 0`

**Scale Modeの説明**:
- **0 (Constant Pixel Size)**: 画面サイズに関係なく、常に同じピクセルサイズで表示
- **1 (Scale With Screen Size)**: 画面サイズに応じてUIがスケール
- **2 (Constant Physical Size)**: 物理的なサイズを維持（DPIに依存）

## 結果

「Constant Pixel Size」に変更することで：
- px単位で指定したフォントサイズやUI要素のサイズが、そのままのサイズで表示される
- Reference Resolution（960x540）に関係なく、指定したpx値がそのまま適用される
- 画面サイズが変わっても、UI要素のサイズは変わらない

## 注意事項

- 画面サイズに応じたスケーリングが必要な場合は、Scale Modeを「Scale With Screen Size」に戻し、フォントサイズやUI要素のサイズを調整する必要があります
- 現在の設定では、960x540を基準にデザインしたUIが、そのままのサイズで表示されます
