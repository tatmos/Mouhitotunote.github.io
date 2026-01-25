# ボタンの反応位置ずれ問題 - 修正内容

## 問題
UIのボタンの反応位置がずれている

## 原因
Scale Modeを「Constant Pixel Size (0)」に変更したことで、表示位置とクリック判定の座標系がずれていました。

## 解決方法

### Scale Modeを「Scale With Screen Size」に戻す

**修正ファイル**:
- `Assets/UI Toolkit/PanelSettings.asset`
- `Assets/UI Toolkit/GamePanelSettings.asset`

**変更内容**:
- `m_ScaleMode: 0` → `m_ScaleMode: 1` (Scale With Screen Size)

## 現在の設定

```
Scale Mode: Scale With Screen Size (1)
Reference Resolution: 960 x 540
Screen Match Mode: Match Width Or Height
Match: 0.5 (バランス)
```

## 効果

「Scale With Screen Size」に戻すことで：
- 表示位置とクリック判定の座標系が一致する
- Reference Resolution（960x540）を基準に適切にスケーリングされる
- ボタンの反応位置が正しくなる

## 注意事項

- UIが大きすぎる場合は、フォントサイズやUI要素のサイズを調整する必要があります
- または、Scale値を調整することも可能です（現在は1.0）

## 追加の調整が必要な場合

もしUIが大きすぎる場合は、以下のいずれかの方法で調整できます：

1. **フォントサイズを調整**: UXMLファイルやコード内のフォントサイズを小さくする
2. **Scale値を調整**: Panel SettingsのScale値を0.5などに設定する（ただし、これはすべてのUI要素に影響します）
3. **Reference Resolutionを変更**: より大きな解像度（例：1280x720）に変更する
