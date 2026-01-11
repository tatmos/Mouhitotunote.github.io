# リソースローディング移行計画

## 移行対象リソース

### Phase 1: AudioClip（優先度：高）

以下のAudioClipをResources.Loadに移行：

1. `wordGetSounds[]` - Resources/Audio/WordGet/に配置
2. `wordGetIncreaseSound` - Resources/Audio/WordGetIncrease
3. `wordGetDecreaseSound` - Resources/Audio/WordGetDecrease
4. `creditsBGM` - Resources/Audio/CreditsBGM
5. `selectionBGM` - Resources/Audio/SelectionBGM
6. `typewriterSound` - Resources/Audio/TypewriterSound
7. `lostLetterSound` - Resources/Audio/LostLetterSound
8. `sparkleSound` - Resources/Audio/SparkleSound
9. `buttonHoverSound` - Resources/Audio/ButtonHoverSound
10. `thunderSound` - Resources/Audio/ThunderSound
11. `truthDoorUnlockSound` - Resources/Audio/TruthDoorUnlockSound
12. `ambientSounds[]` - Resources/Audio/Ambient/Scenario1, Scenario2, ... に配置

### Phase 2: Sprite（アイコン類）（優先度：高）

1. `creditsIcon` - Resources/UI/Icons/CreditsIcon
2. `achievementsIcon` - Resources/UI/Icons/AchievementsIcon
3. `clockIcon` - Resources/UI/Icons/ClockIcon
4. `sparkleIcon` - Resources/UI/Icons/SparkleIcon
5. `soundIcon` - Resources/UI/Icons/SoundIcon

### Phase 3: Material（優先度：高）

1. `distortionMaterial` - Resources/Materials/DistortionMaterial

### Phase 4: VisualTreeAsset (UXML)（優先度：中）

1. `selectionScreenUXML` - Resources/UI/UXML/SelectionScreen
2. `scenarioScreenUXML` - Resources/UI/UXML/ScenarioScreen
3. `resultScreenUXML` - Resources/UI/UXML/ResultScreen
4. `profileScreenUXML` - Resources/UI/UXML/ProfileScreen
5. `creditsScreenUXML` - Resources/UI/UXML/CreditsScreen
6. `achievementsScreenUXML` - Resources/UI/UXML/AchievementsScreen
7. `mouhitotsuScreenUXML` - Resources/UI/UXML/MouhitotsuScreen
8. `soundSettingsPanelUXML` - Resources/UI/UXML/SoundSettingsPanel

### Phase 5: Sprite（ボタン・背景画像）（優先度：中）

1. `scenarioBackgrounds[]` - Resources/UI/Backgrounds/Scenario1, Scenario2, ...
2. `selectionScreenBackground` - Resources/UI/Backgrounds/SelectionScreen
3. `profileScreenBackground` - Resources/UI/Backgrounds/ProfileScreen
4. `scenarioButtonNormalImage` - Resources/UI/Buttons/ScenarioButtonNormal
5. `scenarioButtonCompletedImage` - Resources/UI/Buttons/ScenarioButtonCompleted
6. `uiButtonNormalImage` - Resources/UI/Buttons/UIButtonNormal
7. `uiButtonDarkImage` - Resources/UI/Buttons/UIButtonDark
8. `uiButtonIndigoImage` - Resources/UI/Buttons/UIButtonIndigo
9. `titleImage` - Resources/UI/TitleImage
10. `scoreDisplayBackgroundImage` - Resources/UI/ScoreDisplayBackground
11. `menuButtonImage` - Resources/UI/Buttons/MenuButton

## 保持するリソース（SerializeField）

- `UIDocument` (9個) - シーンに配置が必要なため

## 実装手順

1. Resourcesフォルダ構造の作成
2. 各リソースをResourcesフォルダに移動（またはコピー）
3. UIManagerUIToolkit.csのコードを変更
4. インスペクター設定を削除
5. テストと確認
