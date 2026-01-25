# Unity UI Toolkit 座標系と表示システム完全ガイド

## 概要

このドキュメントは、Unity UI Toolkit（UIElements）における座標系と表示システムについて、実際のトラブルシューティング経験を基にまとめたものです。特に、**要素が表示されない**問題の解決に焦点を当てています。

---

## 1. Unityの座標系の基本

### 1.1 座標系の種類

Unityには複数の座標系が存在します：

#### スクリーン座標（Screen Coordinates）
- **原点**: 画面左下が (0, 0)
- **範囲**: X軸は右方向、Y軸は上方向
- **取得方法**: `Screen.width`, `Screen.height`
- **用途**: マウス位置、タッチ入力など

#### ワールド座標（World Coordinates）
- **原点**: シーンの原点
- **用途**: 3Dオブジェクトの位置

#### UI座標（UI Coordinates）
- **UI Toolkit**: `rootVisualElement` を基準とした座標
- **Canvas**: Canvasを基準とした座標
- **重要**: UI ToolkitとCanvasは**異なる座標系**を使用

### 1.2 UI Toolkitの座標系

UI Toolkitでは、`rootVisualElement` が座標系の基準となります：

```
rootVisualElement (0, 0)
├─ 左上が原点
├─ X軸: 右方向が正
└─ Y軸: 下方向が正（注意：スクリーン座標とは逆）
```

**重要**: UI ToolkitのY軸は**下方向が正**です。これはスクリーン座標（上方向が正）とは逆です。

---

## 2. PanelSettingsとreferenceResolution

### 2.1 PanelSettingsとは

`PanelSettings` は、UI Toolkitの表示設定を管理するScriptableObjectです。各`UIDocument`は`PanelSettings`を参照して、UIのスケーリングと座標変換を行います。

### 2.2 referenceResolutionの重要性

`referenceResolution` は、UIデザインの基準となる解像度です：

```csharp
panelSettings.referenceResolution = new Vector2Int(1920, 1080);
```

**重要なポイント**:
- `referenceResolution` が `(0, 0)` や不正な値の場合、UIが表示されない
- 実際の画面解像度と`referenceResolution`の比率でスケーリングされる
- `rootVisualElement`のサイズは`referenceResolution`に基づく

### 2.3 推奨設定

```csharp
// 推奨設定
panelSettings.referenceResolution = new Vector2Int(1920, 1080);
panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
panelSettings.match = 0.0f; // 0.0 = 幅基準、1.0 = 高さ基準、0.5 = バランス
panelSettings.scale = 1f;
```

**match値の意味**:
- `0.0`: 幅基準（縦長画面に対応しやすい）
- `1.0`: 高さ基準（横長画面に対応しやすい）
- `0.5`: 幅と高さのバランス

---

## 3. よくある問題と解決策

### 3.1 問題: 要素が表示されない

#### 症状
- `rootVisualElement`のサイズが`0x0`
- 要素を追加しても表示されない
- `resolvedStyle.width`や`resolvedStyle.height`が0

#### 原因
1. **PanelSettingsがnull**
2. **referenceResolutionが不正**（0以下など）
3. **UIDocumentが無効化されている**
4. **要素のdisplayプロパティが`DisplayStyle.None`**

#### 解決策

```csharp
// 1. PanelSettingsの確認と修正
if (overlayDocument.panelSettings == null)
{
    // 既存のPanelSettingsを取得または作成
    var panelSettings = FindFirstObjectByType<UIDocument>()?.panelSettings;
    if (panelSettings == null)
    {
        panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        panelSettings.referenceResolution = new Vector2Int(1920, 1080);
        panelSettings.match = 0.0f;
    }
    overlayDocument.panelSettings = panelSettings;
}

// 2. referenceResolutionの検証
if (overlayDocument.panelSettings.referenceResolution.x <= 0 || 
    overlayDocument.panelSettings.referenceResolution.y <= 0)
{
    overlayDocument.panelSettings.referenceResolution = new Vector2Int(1920, 1080);
}

// 3. UIDocumentの強制更新
overlayDocument.enabled = false;
overlayDocument.enabled = true;

// 4. 要素の表示確認
var root = overlayDocument.rootVisualElement;
if (root != null)
{
    root.style.display = DisplayStyle.Flex;
    root.style.visibility = Visibility.Visible;
}
```

### 3.2 問題: 座標が正しくない

#### 症状
- `left`/`top`で配置した要素が期待した位置にない
- `right`/`bottom`で配置した要素が画面外に出る

#### 原因
1. **座標系の混同**（スクリーン座標とUI座標）
2. **スケーリングの影響**
3. **`right`/`bottom`と`left`/`top`の同時使用**

#### 解決策

```csharp
// ❌ 悪い例: スクリーン座標を直接使用
element.style.left = Screen.width - 200; // これは間違い

// ✅ 良い例: UI座標を使用
var root = overlayDocument.rootVisualElement;
element.style.position = Position.Absolute;
element.style.left = root.resolvedStyle.width - 200; // UI座標系で計算

// ✅ より安全な方法: right/bottomを使用
element.style.position = Position.Absolute;
element.style.right = 20; // 右から20px
element.style.bottom = 20; // 下から20px

// ⚠️ 注意: left/topとright/bottomを同時に使用しない
// right/bottomを使用する場合は、left/topをクリア
element.style.left = StyleKeyword.Auto;
element.style.top = StyleKeyword.Auto;
```

### 3.3 問題: 要素のサイズが0

#### 症状
- `resolvedStyle.width`や`resolvedStyle.height`が0
- 要素が表示されない

#### 原因
1. **親要素のサイズが0**
2. **`width`/`height`が設定されていない**
3. **`display`が`DisplayStyle.None`**

#### 解決策

```csharp
// 1. 親要素のサイズ確認
var parent = element.parent;
if (parent != null)
{
    Debug.Log($"親要素サイズ: {parent.resolvedStyle.width}x{parent.resolvedStyle.height}");
}

// 2. 明示的にサイズを設定
element.style.width = 200;
element.style.height = 150;

// 3. displayを確認
element.style.display = DisplayStyle.Flex;
element.style.visibility = Visibility.Visible;

// 4. 強制的に再描画
element.MarkDirtyRepaint();
```

---

## 4. デバッグ方法

### 4.1 基本的なデバッグコード

```csharp
// rootVisualElementの状態確認
var root = overlayDocument.rootVisualElement;
if (root != null)
{
    Debug.Log($"rootサイズ: {root.resolvedStyle.width}x{root.resolvedStyle.height}");
    Debug.Log($"Screenサイズ: {Screen.width}x{Screen.height}");
    Debug.Log($"PanelSettings解像度: {overlayDocument.panelSettings.referenceResolution}");
    
    // スケール比の計算
    float scaleX = root.resolvedStyle.width / Screen.width;
    float scaleY = root.resolvedStyle.height / Screen.height;
    Debug.Log($"スケール比: X={scaleX:F3}, Y={scaleY:F3}");
}
```

### 4.2 要素の状態確認

```csharp
// 要素の詳細な状態確認
Debug.Log($"要素名: {element.name}");
Debug.Log($"表示状態: display={element.style.display.value}, visibility={element.style.visibility.value}");
Debug.Log($"位置: left={element.style.left.value.value}, top={element.style.top.value.value}");
Debug.Log($"サイズ: width={element.resolvedStyle.width}, height={element.resolvedStyle.height}");
Debug.Log($"実際の位置: worldBound={element.worldBound}");
```

### 4.3 テスト要素の追加

```csharp
// テスト用の目立つ要素を追加して表示を確認
var testElement = new VisualElement();
testElement.name = "DebugTestElement";
testElement.style.position = Position.Absolute;
testElement.style.left = 50;
testElement.style.top = 50;
testElement.style.width = 100;
testElement.style.height = 100;
testElement.style.backgroundColor = Color.magenta; // 目立つ色
root.Add(testElement);
```

---

## 5. 座標計算のベストプラクティス

### 5.1 右下に配置する場合

```csharp
// 方法1: right/bottomを使用（推奨）
element.style.position = Position.Absolute;
element.style.right = 20; // 右から20px
element.style.bottom = 20; // 下から20px

// 方法2: left/topを使用（計算が必要）
var root = overlayDocument.rootVisualElement;
element.style.position = Position.Absolute;
element.style.left = root.resolvedStyle.width - element.resolvedStyle.width - 20;
element.style.top = root.resolvedStyle.height - element.resolvedStyle.height - 20;
```

### 5.2 中央に配置する場合

```csharp
var root = overlayDocument.rootVisualElement;
element.style.position = Position.Absolute;
element.style.left = (root.resolvedStyle.width - element.resolvedStyle.width) / 2;
element.style.top = (root.resolvedStyle.height - element.resolvedStyle.height) / 2;
```

### 5.3 スケーリングを考慮した座標計算

```csharp
// スケール比を考慮した座標計算
var root = overlayDocument.rootVisualElement;
float scaleX = root.resolvedStyle.width / overlayDocument.panelSettings.referenceResolution.x;
float scaleY = root.resolvedStyle.height / overlayDocument.panelSettings.referenceResolution.y;

// referenceResolution基準の座標を実際の座標に変換
float actualX = targetX * scaleX;
float actualY = targetY * scaleY;
```

---

## 6. トラブルシューティングチェックリスト

要素が表示されない場合、以下の順序で確認してください：

- [ ] **PanelSettingsが設定されているか**
  ```csharp
  if (uidocument.panelSettings == null) { /* エラー */ }
  ```

- [ ] **referenceResolutionが正しいか**
  ```csharp
  var res = uidocument.panelSettings.referenceResolution;
  if (res.x <= 0 || res.y <= 0) { /* エラー */ }
  ```

- [ ] **UIDocumentが有効化されているか**
  ```csharp
  if (!uidocument.enabled) { /* エラー */ }
  ```

- [ ] **rootVisualElementが取得できるか**
  ```csharp
  var root = uidocument.rootVisualElement;
  if (root == null) { /* エラー */ }
  ```

- [ ] **rootVisualElementのサイズが0でないか**
  ```csharp
  if (root.resolvedStyle.width <= 0 || root.resolvedStyle.height <= 0) { /* エラー */ }
  ```

- [ ] **要素のdisplayプロパティが正しいか**
  ```csharp
  if (element.style.display.value == DisplayStyle.None) { /* エラー */ }
  ```

- [ ] **要素のvisibilityプロパティが正しいか**
  ```csharp
  if (element.style.visibility.value == Visibility.Hidden) { /* エラー */ }
  ```

- [ ] **要素が親に追加されているか**
  ```csharp
  if (element.parent == null) { /* エラー */ }
  ```

- [ ] **座標が画面内にあるか**
  ```csharp
  var bounds = element.worldBound;
  if (bounds.x < 0 || bounds.y < 0 || 
      bounds.xMax > Screen.width || bounds.yMax > Screen.height) { /* 警告 */ }
  ```

---

## 7. 実例: OverlayBootstrapでの問題と解決

### 7.1 発生した問題

`OverlayBootstrap.cs`で以下の問題が発生しました：

1. **rootVisualElementのサイズが0x0**
2. **要素を追加しても表示されない**
3. **座標計算が正しく動作しない**

### 7.2 原因

1. `PanelSettings`の`referenceResolution`が不正な値
2. `UIDocument`の初期化タイミングの問題
3. 座標系の混同（スクリーン座標とUI座標）

### 7.3 解決方法

```csharp
// 1. PanelSettingsの検証と修正
if (overlayDocument.panelSettings != null)
{
    var current = overlayDocument.panelSettings;
    if (current.referenceResolution.x <= 0 || current.referenceResolution.y <= 0)
    {
        current.referenceResolution = new Vector2Int(1920, 1080);
        current.match = 0.0f; // 幅基準
    }
}

// 2. UIDocumentの強制更新
overlayDocument.enabled = false;
overlayDocument.enabled = true;

// 3. 遅延後に状態確認
yield return new WaitForSeconds(0.5f);
var root = overlayDocument.rootVisualElement;
if (root != null && root.resolvedStyle.width > 0 && root.resolvedStyle.height > 0)
{
    // 正常に表示されている
}
```

---

## 8. まとめ

### 重要なポイント

1. **PanelSettingsとreferenceResolutionは必須**
   - `referenceResolution`が不正な値だとUIが表示されない

2. **座標系を混同しない**
   - スクリーン座標とUI座標は異なる
   - UI Toolkitでは`rootVisualElement`を基準にする

3. **right/bottomの使用を推奨**
   - `left`/`top`より`right`/`bottom`の方が安全
   - スケーリングの影響を受けにくい

4. **デバッグは段階的に**
   - `rootVisualElement`のサイズ確認
   - 要素の`display`/`visibility`確認
   - 座標とサイズの確認

5. **初期化タイミングに注意**
   - `UIDocument`の初期化は非同期
   - `StartCoroutine`で遅延初期化を検討

### 参考資料

- [Unity UI Toolkit Documentation](https://docs.unity3d.com/Manual/UIElements.html)
- [PanelSettings API Reference](https://docs.unity3d.com/ScriptReference/UIElements.PanelSettings.html)
- `UIScalingGuide.md` - UIスケーリング設定ガイド

---

**最終更新**: 2026-01-24  
**作成者**: AI Assistant (Claude)  
**プロジェクト**: Mouhitotunooto
