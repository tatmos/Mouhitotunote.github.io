# リソース移行完了ガイド

## 完了した作業

すべてのリソースの移行コードが完了しました。以下のリソースがResources.Loadに移行されています：

### ✅ Phase 1: AudioClip
- `Assets/Resources/Audio/`に配置
- すべてのAudioClipフィールドを`[SerializeField]`から通常のフィールドに変更
- `LoadResources()`メソッドでResources.Loadを使用して読み込み

### ✅ Phase 2: Sprite（アイコン類）
- `Assets/Resources/UI/Icons/`に配置する必要があります
- 以下の5つのアイコン：
  - `creditsIcon.png`
  - `achievementsIcon.png`
  - `clockIcon.png`
  - `sparkleIcon.png`
  - `soundIcon.png`

### ✅ Phase 3: Material
- `Assets/Resources/Materials/DistortionMaterial.mat`に配置する必要があります
- `distortionMaterial`フィールドを`[SerializeField]`から通常のフィールドに変更

### ✅ Phase 4: VisualTreeAsset (UXML)
- `Assets/Resources/UI/UXML/`に配置する必要があります
- 以下の8つのUXMLファイル：
  - `SelectionScreen.uxml`
  - `ScenarioScreen.uxml`
  - `ResultScreen.uxml`
  - `ProfileScreen.uxml`
  - `CreditsScreen.uxml`
  - `AchievementsScreen.uxml`
  - `MouhitotsuScreen.uxml`
  - `SoundSettingsPanel.uxml`

### ✅ Phase 5: Sprite（ボタン・背景画像）
- `Assets/Resources/UI/Backgrounds/`に背景画像を配置する必要があります：
  - `Background_Scenario01_MysteryRequest.png`
  - `Background_Scenario02_MysteriousRestaurant.png`
  - `Background_Scenario03_TimeCapsule.png`
  - `Background_Scenario04_MagicSchool.png`
  - `Background_Scenario05_LastPiece.png`
  - `Background_Scenario06_TruthDoor.png`
  - `Background_SelectionScreen.png`
  - `Background_ProfileScreen.png`

- `Assets/Resources/UI/Buttons/`にボタン画像を配置する必要があります：
  - `scenarioButtonNormalImage.png`
  - `scenarioButtonCompletedImage.png`
  - `uiButtonNormalImage.png`
  - `uiButtonDarkImage.png`
  - `uiButtonIndigoImage.png`
  - `menuButtonImage.png`

- `Assets/Resources/UI/`にUI要素画像を配置する必要があります：
  - `titleImage.png`
  - `scoreDisplayBackgroundImage.png`

## 必要なファイル移動作業

Unityエディタで以下のファイルを移動またはコピーしてください：

### 1. アイコン画像
- 現在の場所: `Assets/Images/`
- 移動先: `Assets/Resources/UI/Icons/`
- ファイル: `creditsIcon.png`, `achievementsIcon.png`, `ClockIcon.png`, `sparkleIcon.png`, `SoundIcon.png`

### 2. Material
- 現在の場所: `Assets/Materials/DistortedMaterial.mat`
- 移動先: `Assets/Resources/Materials/DistortedMaterial.mat`

### 3. UXMLファイル
- 現在の場所: `Assets/Scripts/UIToolkit/`（推測）
- 移動先: `Assets/Resources/UI/UXML/`
- ファイル: `SelectionScreen.uxml`, `ScenarioScreen.uxml`, `ResultScreen.uxml`, `ProfileScreen.uxml`, `CreditsScreen.uxml`, `AchievementsScreen.uxml`, `MouhitotsuScreen.uxml`, `SoundSettingsPanel.uxml`

### 4. 背景画像
- 現在の場所: `Assets/Images/Backgrounds/`
- 移動先: `Assets/Resources/UI/Backgrounds/`
- ファイル: `Background_Scenario01_MysteryRequest.png` など

### 5. ボタン画像
- 現在の場所: `Assets/Images/`
- 移動先: `Assets/Resources/UI/Buttons/`
- ファイル: `scenarioButtonNormalImage.png`, `uiButtonNormalImage.png` など

### 6. UI要素画像
- 現在の場所: `Assets/Images/`
- 移動先: `Assets/Resources/UI/`
- ファイル: `titleImage.png`, `scoreDisplayBackgroundImage.png`

## 注意事項

1. **ファイル名の大文字小文字**: Resources.Loadは大文字小文字を区別しませんが、実際のファイル名に合わせてパスを指定しています。ファイル名が異なる場合は、コード内のパスを調整してください。

2. **拡張子**: Resources.Loadでは拡張子（.png, .mat, .uxmlなど）を除いたパスを指定します。

3. **ディレクトリ構造**: 以下の構造を作成してください：
   ```
   Assets/Resources/
   ├── Audio/
   ├── Materials/
   └── UI/
       ├── Icons/
       ├── UXML/
       ├── Backgrounds/
       ├── Buttons/
       └── (titleImage.png, scoreDisplayBackgroundImage.png)
   ```

4. **UIDocumentの設定**: UXMLファイルはUIDocumentコンポーネントからも参照されている可能性があります。UIDocumentのSource Assetは、Resourcesフォルダに移動後も自動的に更新されるはずです。

## 次のステップ

1. すべてのファイルを適切なResourcesフォルダに移動
2. Unityエディタで動作確認
3. エラーが表示された場合、ファイル名やパスを確認して調整

## 保持するリソース（SerializeField）

- `UIDocument` (9個) - シーンに配置が必要なため、引き続き`[SerializeField]`を使用
