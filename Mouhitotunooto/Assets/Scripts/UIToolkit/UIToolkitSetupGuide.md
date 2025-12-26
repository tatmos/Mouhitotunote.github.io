# UI Toolkit移行ガイド

uGUIからUI Toolkit（UIElements）への移行手順です。

## 1. UI Toolkitパッケージの確認

1. Unityエディタで `Window > Package Manager` を開く
2. `UI Toolkit` パッケージがインストールされているか確認
3. インストールされていない場合は、`Unity Registry`からインストール

## 2. UIDocumentの作成

各画面用のUIDocumentを作成します。

### 2.1 SelectionScreenDocumentの作成

1. Hierarchyで右クリック → `UI Toolkit > UI Document`
2. 名前を「SelectionScreenDocument」に変更
3. Inspectorで以下を設定：
   - **Source Asset**: SelectionScreen.uxmlをアサイン
   - **Panel Settings**: 新規作成または既存のものを使用
   - **Sort Order**: 0
4. **初期状態**: 有効（Active）

### 2.2 ScenarioScreenDocumentの作成

1. Hierarchyで右クリック → `UI Toolkit > UI Document`
2. 名前を「ScenarioScreenDocument」に変更
3. Inspectorで以下を設定：
   - **Source Asset**: ScenarioScreen.uxmlをアサイン
   - **Panel Settings**: SelectionScreenDocumentと同じものを使用
   - **Sort Order**: 0
4. **初期状態**: 無効（Inactive）

### 2.3 ResultScreenDocumentの作成

1. Hierarchyで右クリック → `UI Toolkit > UI Document`
2. 名前を「ResultScreenDocument」に変更
3. Inspectorで以下を設定：
   - **Source Asset**: ResultScreen.uxmlをアサイン
   - **Panel Settings**: SelectionScreenDocumentと同じものを使用
   - **Sort Order**: 0
4. **初期状態**: 無効（Inactive）

### 2.4 ProfileScreenDocumentの作成

1. Hierarchyで右クリック → `UI Toolkit > UI Document`
2. 名前を「ProfileScreenDocument」に変更
3. Inspectorで以下を設定：
   - **Source Asset**: ProfileScreen.uxmlをアサイン
   - **Panel Settings**: SelectionScreenDocumentと同じものを使用
   - **Sort Order**: 0
4. **初期状態**: 無効（Inactive）

## 3. UXMLファイルの作成

### 3.1 SelectionScreen.uxml

1. Projectウィンドウで `Assets/Scripts/UIToolkit/` フォルダを作成
2. 右クリック → `Create > UI Toolkit > UXML Document`
3. 名前を「SelectionScreen」に変更
4. 作成されたUXMLファイルを開いて編集

### 3.2 他のUXMLファイルも同様に作成

- ScenarioScreen.uxml
- ResultScreen.uxml
- ProfileScreen.uxml

## 4. USSファイルの作成

1. Projectウィンドウで `Assets/Scripts/UIToolkit/` フォルダ内で右クリック
2. `Create > UI Toolkit > USS` を選択
3. 名前を「GameStyles」に変更
4. スタイルを編集（既に作成済みのGameStyles.ussを参照）

## 5. UIManagerUIToolkitの設定

1. 既存のUIManagerコンポーネントを削除または無効化
2. 新しいGameObjectに`UIManagerUIToolkit`コンポーネントを追加
3. Inspectorで以下を設定：

**UI Documents:**
- **Selection Screen Document**: SelectionScreenDocumentをアサイン
- **Scenario Screen Document**: ScenarioScreenDocumentをアサイン
- **Result Screen Document**: ResultScreenDocumentをアサイン
- **Profile Screen Document**: ProfileScreenDocumentをアサイン

**UXML Files:**
- **Selection Screen UXML**: SelectionScreen.uxmlをアサイン
- **Scenario Screen UXML**: ScenarioScreen.uxmlをアサイン
- **Result Screen UXML**: ResultScreen.uxmlをアサイン
- **Profile Screen UXML**: ProfileScreen.uxmlをアサイン

**Background Images:**
- **Scenario Backgrounds**: 6つのシナリオ背景画像を順番にアサイン
- **Selection Screen Background**: 選択画面用の背景画像をアサイン
- **Profile Screen Background**: プロフィール画面用の背景画像をアサイン

## 6. Panel Settingsの設定

1. Projectウィンドウで `Assets/Scripts/UIToolkit/` フォルダ内で右クリック
2. `Create > UI Toolkit > Panel Settings Asset` を選択
3. 名前を「GamePanelSettings」に変更
4. Inspectorで以下を設定：
   - **Target Texture**: None（Screen Space）
   - **Scale Mode**: Constant Pixel Size または Scale With Screen Size
   - **Reference Resolution**: 1920 x 1080

## 7. 画像のインポート設定

背景画像のインポート設定を確認：
1. Projectウィンドウで背景画像を選択
2. Inspectorで以下を設定：
   - **Texture Type**: Sprite (2D and UI)
   - **Max Size**: 2048
   - **Apply**をクリック

## 8. 移行のメリット

- **パフォーマンス**: UI ToolkitはuGUIより高速
- **スケーラビリティ**: 大量のUI要素でもパフォーマンスが安定
- **スタイリング**: CSSライクなUSSでスタイル管理が容易
- **レイアウト**: Flexboxライクなレイアウトシステム
- **デバッグ**: UI Builderで視覚的に編集可能

## 9. 注意事項

- UI ToolkitとuGUIは同時に使用できますが、別々のCanvas/UIDocumentで管理する必要があります
- 既存のuGUIベースのUIはそのまま残しておき、新しいUI ToolkitベースのUIに段階的に移行できます
- UI Builder（Window > UI Toolkit > UI Builder）を使用すると、UXMLファイルを視覚的に編集できます

## 10. トラブルシューティング

### UXMLファイルが読み込まれない
- UXMLファイルのパスが正しいか確認
- UIDocumentのSource Assetが正しくアサインされているか確認

### スタイルが適用されない
- USSファイルがUXMLファイルで正しく参照されているか確認
- USSファイルのクラス名が正しいか確認

### 画像が表示されない
- 画像のTexture Typeが「Sprite (2D and UI)」になっているか確認
- StyleBackgroundで正しく設定されているか確認

