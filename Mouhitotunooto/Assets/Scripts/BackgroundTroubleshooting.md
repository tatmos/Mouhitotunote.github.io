# 背景画像が表示されない場合のトラブルシューティング

## 確認事項

### 1. Imageコンポーネントの確認

#### SelectionScreenの背景
1. HierarchyでSelectionScreenを展開
2. BackgroundImageを選択
3. Inspectorで以下を確認：
   - **Imageコンポーネントが存在する**: なければ`Add Component > UI > Image`を追加
   - **Imageコンポーネントが有効**: ✓がついているか確認
   - **Source Image**: 画像がアサインされているか確認（一時的に手動でアサインしてテスト）

#### ProfileScreenの背景
1. HierarchyでProfileScreenを展開
2. BackgroundImageを選択
3. 上記と同じ確認を行う

### 2. UIManagerのInspector設定確認

1. UIManagerコンポーネントを選択
2. Inspectorで「Background Images」セクションを確認：
   - **Selection Screen Background**: 画像がアサインされているか
   - **Selection Background Image**: SelectionScreen内のBackgroundImageがアサインされているか
   - **Profile Screen Background**: 画像がアサインされているか
   - **Profile Background Image**: ProfileScreen内のBackgroundImageがアサインされているか

### 3. 画像のインポート設定確認

1. Projectウィンドウで背景画像を選択
2. Inspectorで以下を確認：
   - **Texture Type**: `Sprite (2D and UI)`になっているか
   - **Max Size**: 適切な値（2048など）が設定されているか
   - **Apply**をクリックして設定を適用

### 4. Hierarchyの順番確認

背景画像は**最背面**に配置する必要があります。

#### SelectionScreenの子要素の順番（上から下へ）:
1. BackgroundImage（最初）
2. TitleText
3. ScoreText
4. ScenarioButtonParent
5. ShowProfileButton

#### ProfileScreenの子要素の順番（上から下へ）:
1. BackgroundImage（最初）
2. ProfileSectionTitle
3. ProfileParent
4. BackToSelectionButtonFromProfile

**重要**: BackgroundImageが最初（最背面）に来るように、Hierarchyでドラッグして順番を変更してください。

### 5. RectTransformの確認

BackgroundImageのRectTransformを確認：

1. **Anchor Presets**: Stretch Stretch
2. **Left, Top, Right, Bottom**: すべて0
3. **Pos X, Pos Y**: 0
4. **Width, Height**: 自動的に設定される（0のままでOK）

### 6. Imageコンポーネントの設定確認

BackgroundImageのImageコンポーネントを確認：

1. **Source Image**: 背景画像がアサインされているか
2. **Color**: 白（R:255, G:255, B:255, A:255）
3. **Image Type**: Simple
4. **Preserve Aspect**: ✓（チェックを入れる）

### 7. デバッグログの確認

1. UnityエディタでPlayモードに入る
2. Consoleウィンドウを開く（Window > General > Console）
3. 選択画面やプロフィール画面に遷移した時、以下のログが表示されるか確認：
   - "選択画面の背景画像を設定しました"
   - "プロフィール画面の背景画像を設定しました"
4. 警告メッセージが表示される場合、その内容に従って修正

### 8. 手動テスト

一時的に手動で背景画像を設定してテスト：

1. SelectionScreen内のBackgroundImageを選択
2. InspectorのImageコンポーネントで「Source Image」に直接画像をドラッグ
3. ゲームを実行して背景が表示されるか確認
4. 表示される場合：UIManagerの設定が問題
5. 表示されない場合：ImageコンポーネントやRectTransformの設定が問題

## よくある問題と解決方法

### 問題1: 背景が他のUI要素の後ろに隠れている
**解決方法**: HierarchyでBackgroundImageを最初（最背面）に移動

### 問題2: 画像がアサインされていない
**解決方法**: 
- UIManagerのInspectorで「Selection Screen Background」と「Profile Screen Background」に画像をアサイン
- または、BackgroundImageのImageコンポーネントに直接画像をアサイン

### 問題3: Imageコンポーネントが無効になっている
**解決方法**: InspectorでImageコンポーネントの✓を確認

### 問題4: 画像のTexture Typeが間違っている
**解決方法**: 
- Projectウィンドウで画像を選択
- Inspectorで「Texture Type」を「Sprite (2D and UI)」に変更
- 「Apply」をクリック

### 問題5: 画像の色が透明になっている
**解決方法**: 
- BackgroundImageのImageコンポーネントで「Color」を確認
- Alpha値を255（不透明）に設定

### 問題6: RectTransformの設定が間違っている
**解決方法**: 
- Anchor Presetsを「Stretch Stretch」に設定
- Left, Top, Right, Bottomをすべて0に設定

## デバッグ用コード

問題が解決しない場合、以下のコードをUIManagerに追加して詳細な情報を取得できます：

```csharp
private void DebugBackgroundSettings()
{
    Debug.Log("=== 背景画像設定のデバッグ ===");
    Debug.Log($"SelectionBackgroundImage: {(selectionBackgroundImage != null ? "設定済み" : "未設定")}");
    Debug.Log($"SelectionScreenBackground: {(selectionScreenBackground != null ? "設定済み" : "未設定")}");
    Debug.Log($"ProfileBackgroundImage: {(profileBackgroundImage != null ? "設定済み" : "未設定")}");
    Debug.Log($"ProfileScreenBackground: {(profileScreenBackground != null ? "設定済み" : "未設定")}");
    
    if (selectionBackgroundImage != null)
    {
        Debug.Log($"SelectionBackgroundImage.enabled: {selectionBackgroundImage.enabled}");
        Debug.Log($"SelectionBackgroundImage.sprite: {(selectionBackgroundImage.sprite != null ? selectionBackgroundImage.sprite.name : "null")}");
    }
    
    if (profileBackgroundImage != null)
    {
        Debug.Log($"ProfileBackgroundImage.enabled: {profileBackgroundImage.enabled}");
        Debug.Log($"ProfileBackgroundImage.sprite: {(profileBackgroundImage.sprite != null ? profileBackgroundImage.sprite.name : "null")}");
    }
}
```

このメソッドを`ShowSelectionScreen()`や`ShowProfileScreen()`の最初で呼び出すと、設定状況が確認できます。

