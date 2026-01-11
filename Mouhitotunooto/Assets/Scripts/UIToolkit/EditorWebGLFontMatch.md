# エディタとWebGLでの文字表示を統一する方法

## 問題
UnityEditorで実行している時と、Webで見ているときの文字の見た目が異なる場合があります。

## 原因
TextMeshProのSDF（Signed Distance Field）フォントは、エディタとWebGLでレンダリング方法が若干異なることがあります。特に以下の設定が影響します：

1. **TextMeshProコンポーネントのExtra Settings**（Dilate、Softnessなど）
2. **フォントMaterialの設定**
3. **Canvasの設定**（Pixel Perfect、Render Modeなど）

## 解決方法

### 方法1: TextMeshProコンポーネントのExtra Settingsを調整（推奨）

各TextMeshProコンポーネントで、以下の設定を確認・調整してください：

1. シーンまたはPrefabで、TextMeshProコンポーネントが設定されているGameObjectを選択
2. Inspectorで、TextMeshProコンポーネントを展開
3. **Extra Settings**セクションを展開
4. 以下のパラメータを調整：
   - **Dilate**: 文字の太さを調整（範囲: -1.0 ～ 1.0、デフォルト: 0.0）
     - 値を大きくすると文字が太くなります
     - 値を小さくすると文字が細くなります
   - **Softness**: アンチエイリアスの強さを調整（範囲: 0.0 ～ 1.0、デフォルト: 0.0）
     - 値を大きくするとより滑らかに表示されます
     - WebGLで見た目を合わせるには、通常0.0～0.2程度で十分です
   - **Outline**: アウトラインの太さ（範囲: 0.0 ～ 0.6、デフォルト: 0.0）
   - **Underlay**: 下地の設定

**注意**: WebGLでの見た目に合わせて調整する場合：
- WebGLで表示されている画面を参考に、エディタでDilateやSoftnessを微調整してください
- 一度に大幅に変更せず、少しずつ調整することをお勧めします

### 方法2: フォントMaterialの設定を確認

1. Projectウィンドウで、使用しているフォントアセット（例: `Assets/TextMesh Pro/Resources/Fonts & Materials/ZenMaruGothic-Regular SDF.asset`）を選択
2. Inspectorで、フォントアセットの**Material Presets**を確認
3. Materialを選択して、以下のパラメータを確認・調整：
   - **Dilate**: フォント全体の太さを調整
   - **Softness**: フォント全体のアンチエイリアスを調整
   - **Outline Width**: アウトラインの太さ
   - **Face Dilate**: 文字面の太さ

**注意**: Materialを変更すると、そのMaterialを使用しているすべてのTextMeshProコンポーネントに影響します。

### 方法3: Canvasの設定を確認（uGUIを使用している場合）

uGUIのCanvasを使用している場合：

1. シーンでCanvasを選択
2. Inspectorで以下の設定を確認：
   - **Pixel Perfect**: チェックを外す、またはWebGLと同じ設定にする
   - **Render Mode**: Screen Space - Overlay、Screen Space - Camera、またはWorld Spaceのいずれかで、WebGLと同じ設定にする
   - **Reference Resolution**: WebGLと同じ解像度に設定

**注意**: Pixel Perfectを有効にすると、エディタとWebGLで見た目が異なる場合があります。

### 方法4: フォントアセットを再生成（最終手段）

上記の方法で解決しない場合、フォントアセットを再生成することで、エディタとWebGLの見た目を統一できる場合があります：

1. Unityエディタで `Window > TextMeshPro > Font Asset Creator` を開く
2. **Source Font File** に現在使用しているフォントファイルを選択
3. **Character Set** を **Characters from File** に設定
4. **Sampling Point Size** を適切な値に設定（例: 72）
5. **Padding** を設定（例: 9）
6. **Packing Method** を **Fast** に設定
7. **Atlas Resolution** を適切な値に設定（例: 2048 x 2048）
8. **Generate Font Atlas** をクリック
9. 既存のフォントアセットを上書き保存、または新しい名前で保存

**注意**: フォントアセットを再生成すると、すべてのTextMeshProコンポーネントで使用されているフォントが更新されます。

## 推奨される作業フロー

1. **まず方法1を試す**（個別のTextMeshProコンポーネントのExtra Settingsを調整）
   - これは最も安全で、他の部分に影響を与えません
   - 問題のあるTextMeshProコンポーネントのみを調整できます

2. **方法2を試す**（フォントMaterialの設定を調整）
   - 複数のTextMeshProコンポーネントで同じ問題がある場合に有効です
   - すべてのTextMeshProコンポーネントに影響します

3. **方法3を確認**（Canvasの設定を確認）
   - uGUIを使用している場合にのみ有効です

4. **方法4を検討**（フォントアセットを再生成）
   - 上記の方法で解決しない場合のみ検討してください

## トラブルシューティング

### 調整しても見た目が変わらない場合

1. **フォントアセットのMaterialが正しく設定されているか確認**
   - TextMeshProコンポーネントで、使用しているMaterialが正しいか確認してください
   - Materialが個別に設定されている場合、フォントアセットのデフォルトMaterialではなく、個別のMaterialが使用されています

2. **プレイモードで確認**
   - エディタのPlayモードで確認すると、実際のレンダリング結果が確認できます
   - エディタビューとPlayモードビューで見た目が異なる場合があります

3. **WebGLビルドを再実行**
   - 設定を変更した後、WebGLビルドを再実行して確認してください
   - 場合によっては、キャッシュが原因で変更が反映されないことがあります

## 参考情報

- TextMeshProの公式ドキュメント: https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest
- SDFフォントの詳細については、TextMeshProのドキュメントを参照してください
