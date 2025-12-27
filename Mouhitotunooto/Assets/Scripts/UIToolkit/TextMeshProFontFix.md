# TextMeshProの日本語フォント設定方法

## 問題
既存のuGUI/TextMeshProコンポーネント（Prefabなど）が日本語文字を表示しようとしていますが、`LiberationSans SDF`フォントアセットに日本語文字が含まれていないため、□（\u25A1）で置き換えられています。

## 解決方法

### 方法1: TMP Settingsでデフォルトフォントとフォールバックフォントを設定（推奨）

この方法は、すべてのTextMeshProコンポーネントに自動的に適用されます。

1. Projectウィンドウで `Assets/TextMesh Pro/Resources/TMP Settings.asset` を選択
2. Inspectorで以下を設定：
   - **Default Font Asset**: 日本語対応のフォントアセット（例: `ZenMaruGothic-Regular SDF`）をアサイン
   - **Fallback Font Assets**: 日本語対応のフォントアセットを追加
3. これで、フォントが明示的に設定されていないすべてのTextMeshProコンポーネントが、日本語対応フォントを使用するようになります

### 方法2: 各PrefabのTextMeshProコンポーネントに直接フォントを設定

個別にPrefabを修正する場合：

1. **ScenarioButtonPrefab.prefab** を開く
2. `Text (TMP)` オブジェクトを選択
3. Inspectorで **Font Asset** を日本語対応のフォントアセット（例: `ZenMaruGothic-Regular SDF`）に変更
4. **Apply** をクリックしてPrefabを保存
5. 同様に、**ProfileCardPrefab.prefab** と **ChoiceButtonPrefab.prefab** も修正

### 方法3: 日本語対応フォントアセットが存在しない場合

日本語対応のフォントアセットがまだ作成されていない場合：

1. `Window > TextMeshPro > Font Asset Creator` を開く
2. **Source Font File** に日本語フォント（例: Noto Sans JP、Zen Maru Gothicなど）を選択
3. **Character Set** を **Characters from File** に設定
4. **Sampling Point Size**: 72
5. **Padding**: 9
6. **Packing Method**: Fast
7. **Atlas Resolution**: 2048 x 2048
8. **Generate Font Atlas** をクリック
9. **Save** または **Save as...** をクリックして、`Assets/Fonts/` フォルダに保存（例: `NotoSansJP-Regular SDF.asset`）
10. 上記の方法1または方法2で、作成したフォントアセットを設定

## 推奨される手順

1. **まず方法1を実行**（TMP Settingsでデフォルトフォントを設定）
   - これにより、すべてのTextMeshProコンポーネントが自動的に日本語対応フォントを使用します
2. **既存のPrefabを確認**
   - Prefabで明示的にフォントが設定されている場合は、それも日本語対応フォントに変更
3. **シーン内のTextMeshProコンポーネントを確認**
   - シーン内に直接配置されているTextMeshProコンポーネントも確認

## 注意事項

- フォントアセットのサイズが大きい場合、ビルドサイズが増加します
- 日本語フォントアセットは通常、数MBから数十MBになります
- WebGLビルド時には、フォントアセットが確実にビルドに含まれるように、フォントアセットのInspectorで **Include Font Data** が有効になっていることを確認してください

## トラブルシューティング

### まだ文字が表示されない場合

1. フォントアセットに日本語文字が含まれているか確認
   - Font Asset Creatorで **Characters from File** を使用した場合、フォントファイルに含まれるすべての文字が含まれます
2. TextMeshProコンポーネントの **Font Asset** が正しく設定されているか確認
3. TMP Settingsの **Default Font Asset** が正しく設定されているか確認
4. フォントアセットのInspectorで **Include Font Data** が有効になっているか確認

