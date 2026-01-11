# リソースインポート設定の修正方法

## 問題

ボタン画像が`Resources.Load<Sprite>()`で読み込めないエラーが発生しています。

## 原因

画像ファイルを移動した際、`.meta`ファイルの`textureType`が`0`（Default）のままで、`8`（Sprite (2D and UI)）に変更されていません。そのため、Unityが画像を`Texture2D`として認識し、`Sprite`として読み込めません。

## 解決方法

### 方法1: Unityエディタで再設定（推奨）

1. UnityエディタのProjectウィンドウで`Assets/Resources/UI/Buttons/`フォルダを開く
2. 以下の画像ファイルを1つずつ選択：
   - `scenarioButtonNormalImage.png`
   - `scenarioButtonCompletedImage.png`
   - `uiButtonNormalImage.png`
   - `uiButtonDarkImage.png`
   - `uiButtonIndigoImage.png`
   - `menuButtonImage.png`
3. Inspectorウィンドウで以下を設定：
   - **Texture Type**: `Sprite (2D and UI)`
   - **Sprite Mode**: `Single`
4. **Apply**ボタンをクリック
5. すべてのボタン画像について繰り返す

### 方法2: 一括設定

1. Projectウィンドウで`Assets/Resources/UI/Buttons/`フォルダを選択
2. Inspectorウィンドウで**Texture Type**を`Sprite (2D and UI)`に設定
3. すべての画像に適用されることを確認

### 確認方法

設定後、以下のコードで確認できます：

```csharp
var sprite = Resources.Load<Sprite>("UI/Buttons/scenarioButtonNormalImage");
Debug.Log(sprite != null ? "Found" : "Not found");
```

## 注意事項

- `.meta`ファイルを手動で編集すると、Unityのインポート設定が壊れる可能性があります
- 必ずUnityエディタのInspectorで設定を変更してください
- 設定変更後、Unityが自動的に再インポートします
