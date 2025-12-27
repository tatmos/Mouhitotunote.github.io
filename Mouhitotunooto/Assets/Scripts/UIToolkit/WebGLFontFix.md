# WebGLビルド時の日本語フォント表示問題の解決方法

## 問題
WebGLビルド時に日本語が「?」や「□」と表示される問題が発生しています。

**関連する問題**: 一部の文字に四角いグレーの背景が表示される問題については、`WebGLFontGrayBackgroundFix.md` を参照してください。

## 原因
UI ToolkitのPanelSettingsに日本語フォントが設定されていないため、WebGLビルド時に日本語が正しく表示されません。

## 解決方法

### 1. フォントアセットの作成（UI Toolkit用）

**重要: エラーが発生している場合の対処**

エラーが発生している場合は、以下の手順でFont Asset Creatorウィンドウをリセットしてください：

1. **Font Asset Creatorウィンドウを閉じる**
2. Unityエディタを再起動（オプション、通常は不要）
3. 再度 `Window > TextMeshPro > Font Asset Creator` を開く

**方法A: Characters from Fileを使用（強く推奨）**

この方法が最も安全で、エラーが発生しにくいです。

1. Unityエディタで `Window > TextMeshPro > Font Asset Creator` を開く
2. **Source Font File** に日本語フォント（例: Noto Sans JP、Zen Maru Gothicなど）を選択
3. **Character Set** を **Characters from File** に設定
   - この方法では、フォントファイルに含まれるすべての文字が自動的に含まれます
   - Unicode Rangeの入力エラーを完全に回避できます
4. **Sampling Point Size** を適切な値に設定（例: 72）
5. **Padding** を設定（例: 9）
6. **Packing Method** を **Fast** に設定
7. **Atlas Resolution** を適切な値に設定（例: 2048 x 2048）
8. **Generate Font Atlas** をクリック
9. **Save** または **Save as...** をクリックして、`Assets/UI Toolkit/Fonts/` フォルダに保存（例: `NotoSansJP-Regular SDF.asset`）

**方法B: Unicode Rangeを使用する場合（エラーが発生しやすいため非推奨）**

⚠️ **注意**: この方法はエラーが発生しやすいため、方法Aを使用することを強く推奨します。

もしUnicode Rangeを使用する必要がある場合：

1. **Character Set** を **Unicode Range** に設定する前に、**Unicode Range** フィールドを完全にクリアしてください
2. **Unicode Range** フィールドに、**1行に1つの範囲**で入力します：
   ```
   0020-007F
   00A0-00FF
   0100-017F
   3040-309F
   30A0-30FF
   4E00-9FFF
   ```
   - 注意: `U+` プレフィックスは**絶対に使用しないでください**
   - 注意: カンマ区切りではなく、**必ず改行で区切ってください**
   - 注意: スペースやその他の文字を含めないでください
   - 注意: 形式は `XXXX-XXXX` のみ（Xは16進数）
3. エラーが発生した場合は、すぐに方法Aに切り替えてください

### 2. Text Settings アセットの作成

1. Projectウィンドウで `Assets/UI Toolkit/` フォルダ内で右クリック
2. `Create > UI Toolkit > Text Settings` を選択
3. 名前を「GameTextSettings」に変更
4. Inspectorで以下を設定：
   - **Font Asset**: 上で作成したTextMeshProのFont Asset（SDFアセット）をアサイン
     - 注意: UI ToolkitのText Settingsでは、TextMeshProのFont Assetを使用します
   - **Default Font Size**: 16（デフォルト値）
   - **Default Font Style**: Normal

### 3. PanelSettingsにText Settingsを設定

1. Projectウィンドウで `Assets/UI Toolkit/GamePanelSettings.asset` を選択
2. Inspectorで **Text Settings** に上で作成した `GameTextSettings` をアサイン
3. すべてのUIDocumentがこのPanelSettingsを使用していることを確認

### 4. フォントファイルをResourcesフォルダに配置（オプション）

WebGLビルド時にフォントが確実に含まれるようにするため：

1. `Assets/Resources/` フォルダを作成（存在しない場合）
2. フォントアセット（.assetファイル）を `Assets/Resources/` にコピーまたは移動
3. または、フォントアセットのInspectorで **Include Font Data** を有効にする

### 5. ビルド設定の確認

1. `File > Build Settings` を開く
2. **Player Settings** をクリック
3. **Other Settings** セクションで以下を確認：
   - **Scripting Backend**: IL2CPP または Mono
   - **Api Compatibility Level**: .NET Standard 2.1 または .NET Framework

### 6. ビルドとテスト

1. WebGLビルドを実行
2. ブラウザで実行して日本語が正しく表示されることを確認

## 注意事項

- フォントアセットのサイズが大きい場合、ビルドサイズが増加します
- 必要な文字のみを含めるようにUnicode Rangeを調整することで、ファイルサイズを削減できます
- 複数のフォントを使用する場合は、フォールバックフォントを設定できます

## トラブルシューティング

### Unicode Rangeの入力エラーが発生する場合

- **FormatException** が発生した場合、Unicode Rangeの形式が正しくありません
- **解決策（優先順位順）**: 
  1. **Characters from File** を使用する（最も安全で推奨）
  2. Font Asset Creatorウィンドウを閉じて再開する
  3. Unityエディタを再起動する（必要に応じて）
  4. Unicode Rangeを使用する場合は、以下を確認：
     - `U+` プレフィックスを**絶対に使用しない**
     - カンマ区切りではなく、**必ず改行で区切る**
     - 形式は `XXXX-XXXX` のみ（Xは16進数、例: `0020-007F`）
     - スペースやその他の文字を含めない
  5. それでもエラーが発生する場合は、方法A（Characters from File）を使用してください

### まだ文字が表示されない場合

1. ブラウザのコンソールでエラーを確認
2. フォントアセットが正しくビルドに含まれているか確認
3. PanelSettingsのText Settingsが正しくアサインされているか確認
4. すべてのUIDocumentが同じPanelSettingsを使用しているか確認
5. フォントアセットのInspectorで **Include Font Data** が有効になっているか確認

### フォントが大きすぎる場合

- フォントアセットのAtlas Resolutionを調整
- 必要な文字のみを含めるようにUnicode Rangeを調整
- **Characters from File** を使用する場合、フォントファイル自体が大きい可能性があります

