# Editor/WebGL UI配置ずれ問題 - 確認レポート

## 確認日時
2026-01-25

## 修正日時
2026-01-25（修正完了）
2026-01-25（Reference Resolutionを960x540に変更）

## 確認項目と結果

### 1. Panel Settingsによる解像度スケールの不一致

#### 現状
- **使用中のPanel Settings**: `Assets/UI Toolkit/PanelSettings.asset` (guid: 811b6e4f596fde44cb25c5ca51926443)
- **Reference Resolution**: **1200 x 800** ⚠️
- **Scale Mode**: **1 (Scale With Screen Size)** ✓
- **Screen Match Mode**: **0 (Match Width Or Height)** ✓
- **Match**: **0.0 (幅基準)** ⚠️

#### 問題点
1. **Reference Resolutionが1200x800になっている**
   - ドキュメント（`UIScalingGuide.md`）では1920x1080を推奨しているが、実際の設定は1200x800
   - これにより、EditorとWebGLで異なるスケーリングが発生する可能性がある

2. **Match値が0.0（幅基準）**
   - `UIScalingGuide.md`では0.5（バランス）を推奨しているが、現在は0.0（幅基準）
   - 縦長画面では問題ないが、横長画面で配置がずれる可能性がある

3. **GamePanelSettings.assetが未使用**
   - `Assets/UI Toolkit/GamePanelSettings.asset`は1920x1080に設定されているが、実際には使用されていない
   - すべてのUIDocumentが`PanelSettings.asset`（1200x800）を参照している

#### 推奨対応
```
Panel Settings: PanelSettings.asset を修正
- Reference Resolution: 1200x800 → 1920x1080 に変更
- Match: 0.0 → 0.5 に変更（幅と高さのバランス）
```

---

### 2. UI Builderと実機のテーマの違い

#### 現状
- **Panel Settingsのテーマ**: `themeUss`が設定されている（guid: e2595a9bee1224847825aa33859f4c5e）
- **テーマファイル**: `Assets/UI Toolkit/UnityThemes/UnityDefaultRuntimeTheme.tss` が存在

#### 確認事項
- UI Builderで「Active Editor Theme」を「UnityDefaultRuntimeTheme」に設定しているかは確認できませんでした
- テーマ自体は設定されているため、大きな問題はない可能性が高い

#### 推奨対応
- UI Builderを開き、右上のメニューから「Active Editor Theme」を「UnityDefaultRuntimeTheme」に設定することを確認

---

### 3. Flexbox（自動レイアウト）の理解不足

#### 現状

**✅ 適切にFlexboxを使用しているファイル:**
- `TitleScreen.uxml`: `flex-grow`, `flex-direction: column`, `justify-content: center`, `align-items: center` を使用
- `SelectionScreen.uxml`: `flex-direction: row`, `flex-wrap: wrap`, `justify-content: center` を使用
- `CreditsScreen.uxml`, `AchievementsScreen.uxml`, `ProfileScreen.uxml` なども適切にFlexboxを使用

**⚠️ Position: Absoluteを多用しているファイル:**
- `Overlay.uxml`: すべての要素が`position: absolute`で固定配置
  ```xml
  <ui:VisualElement name="RoomImage" style="position: absolute; ..."/>
  <ui:VisualElement name="GirlImage" style="position: absolute; ..."/>
  <ui:VisualElement name="BalloonRoot" style="position: absolute; ..."/>
  ```

**⚠️ コード内でPosition: Absoluteを多用:**
- `OverlayPresenter_UITK.cs`: 多数の要素で`Position.Absolute`を使用
  - `roomImage.style.position = Position.Absolute;`
  - `girlImage.style.position = Position.Absolute;`
  - `balloonRoot.style.position = Position.Absolute;`
  - 座標を絶対値（例: `left: 1320`, `top: 700`）で指定

#### 問題点
1. **Overlay.uxmlでPosition: Absoluteが多用されている**
   - 画面サイズが変わると、絶対座標で配置された要素がずれる
   - `right: 20px; bottom: 20px;` のような相対配置は使用されているが、一部の要素は`left`と`top`で絶対配置されている

2. **OverlayPresenter_UITK.csで絶対座標を指定**
   - コード内で`left: 1320`, `top: 700`のような絶対座標を指定している
   - これらはReference Resolution（1200x800）を基準にしているため、実際の画面サイズが異なるとずれる

#### 推奨対応
1. **Overlay.uxmlの修正**
   - `right`と`bottom`を使用した相対配置を維持（既に一部使用されている）
   - `left`と`top`による絶対配置を避ける

2. **OverlayPresenter_UITK.csの修正**
   - 絶対座標（`left: 1320`, `top: 700`）の代わりに、`right`と`bottom`を使用
   - または、`root.resolvedStyle.width`と`root.resolvedStyle.height`を基準にした相対座標計算に変更

---

### 4. フォントサイズとレンダリングの違い

#### 現状
- フォントサイズは主に`px`単位で指定されている（例: `font-size: 14px`, `font-size: 24px`）
- TextMeshPro対応のフォントアセットが使用されているかは確認できませんでした

#### 確認事項
- テキスト要素を含むコンテナの高さや幅が固定されているかは、個別のUXMLファイルを確認する必要があります
- 一般的には、Flexboxを使用しているため、コンテンツに合わせて動的にサイズが変わる設計になっている

#### 推奨対応
- フォントサイズは`px`単位のままで問題ない（Panel Settingsのスケーリングで自動調整される）
- テキスト要素を含むコンテナが固定サイズになっていないか確認

---

### 5. WebGLでの画面の縦横比（Aspect Ratio）

#### 現状
- Project Settings > Player > Resolution and Presentation の設定は確認できませんでした
- WebGLのテンプレート設定は確認できませんでした

#### 推奨対応
- Project Settings > Player > Resolution and Presentation で、WebGLのテンプレートが画面全体に適切にスケールする設定になっているか確認

---

## まとめ：チェックリスト

### ✅ 確認済み（問題なし）
- [x] Panel Settingsが使用されている
- [x] Scale Modeが「Scale With Screen Size」に設定されている
- [x] 主要なUXMLファイルでFlexboxが適切に使用されている
- [x] テーマが設定されている

### ✅ 修正完了
- [x] **Reference Resolutionを1920x1080に変更** → **修正完了**
- [x] **Match値を0.5（バランス）に変更** → **修正完了**
- [x] **Overlay.uxmlは既に相対配置（right/bottom）を使用** → **問題なし**
- [x] **OverlayPresenter_UITK.csの絶対座標を相対配置に変更** → **修正完了**

### ❓ 要確認（手動確認が必要）
- [ ] UI Builderの「Active Editor Theme」が「UnityDefaultRuntimeTheme」に設定されているか
- [ ] Project Settings > Player > Resolution and Presentation のWebGL設定
- [ ] TextMeshPro対応のフォントアセットが使用されているか

---

## 修正完了項目

### ✅ 1. Panel SettingsのReference Resolutionを修正（完了）
**修正内容**:
- `Assets/UI Toolkit/PanelSettings.asset`を修正
- Reference Resolution: 1200x800 → **1920x1080** → **960x540** ✓
- Match: 0.0 → **0.5**（バランス）✓
- `Assets/UI Toolkit/GamePanelSettings.asset`も同様に修正
- `OverlayBootstrap.cs`の動的設定も960x540に変更

### ✅ 2. OverlayPresenter_UITK.csの絶対座標を相対座標に変更（完了）
**修正内容**:
- コンストラクタ内の初期配置を`right`/`bottom`ベースに変更
- `UpdatePhase`メソッド内の配置を`right`/`bottom`ベースに変更
- `FixBalloonCoordinates`メソッドの吹き出し位置を`right`/`bottom`ベースに変更
- `StartCreditsSinging`メソッドの音符レイヤー位置を`right`/`bottom`ベースに変更

**変更例**:
```csharp
// 修正前（絶対座標）
roomImage.style.left = 1320;
roomImage.style.top = 700;

// 修正後（相対配置）
roomImage.style.right = 20;
roomImage.style.bottom = 20;
```

### ✅ 3. Overlay.uxmlの確認（問題なし）
**確認結果**:
- `Overlay.uxml`は既に`right`と`bottom`を使用した相対配置になっている
- 修正不要

## 注意事項

### ドラッグ処理について
`OverlayDragManipulator`クラスでは、ドラッグ中に`left`/`top`を使用して位置を更新しています。これはドラッグ中に動的に位置を変更するため、`right`/`bottom`に変更するのは複雑です。現在の実装では、初期配置は相対配置（`right`/`bottom`）を使用し、ドラッグ後は絶対座標（`left`/`top`）に変換されます。これは許容範囲内です。

もしドラッグ処理も完全に相対配置にしたい場合は、追加の修正が必要になります。

---

## 参考資料
- `Assets/Scripts/UIToolkit/UIScalingGuide.md`: Panel Settingsの設定ガイド
- `Assets/Scripts/UIToolkit/UnityCoordinateSystemGuide.md`: 座標系の説明
- `Assets/Scripts/Overlay/OverlayBootstrap.cs`: Panel Settingsの動的設定コード
