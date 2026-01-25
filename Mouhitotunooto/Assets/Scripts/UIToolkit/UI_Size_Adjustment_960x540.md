# UIサイズ調整 - 960x540対応

## 問題
Reference Resolutionを960x540に変更した後、UIが画面からはみ出してしまう

## 原因
ボタンやUI要素の最小サイズや固定サイズが、960x540の画面サイズに対して大きすぎた

## 解決方法

### 1. GameStyles.ussの調整

**修正内容**:
- `.scenario-button`: `width: 300px` → **240px**, `min-height: 180px` → **140px**, `font-size: 20px` → **18px**
- `.choice-button`: `min-width: 780px` → **600px**, `min-height: 120px` → **90px**, `font-size: 20px` → **18px**, `padding: 16px 24px` → **12px 18px**
- `Button`: `min-height: 50px` → **40px**, `padding: 10px` → **8px**
- `.card`: `padding: 40px` → **20px**, `margin: 30px` → **15px**, `border-radius: 16px` → **12px**

### 2. UXMLファイルの調整

#### TitleScreen.uxml
- StartButton: `min-width: 40%` → **200px**, `min-height: 30%` → **60px**, `font-size: 24px` → **20px**
- Content: `padding: 40px` → **20px**, `max-width: 1200px` → **800px**, `margin: 30px` → **20px**

#### SelectionScreen.uxml
- ボタン: `min-width: 250px` → **180px**, `min-height: 50px` → **40px**
- TitleText: `font-size: 48px` → **36px**
- ScoreText: `font-size: 24px` → **18px**
- Content: `padding: 40px` → **20px**, `max-width: 1200px` → **800px**, `margin: 30px` → **20px**
- ScenarioButtonContainer: `max-width: 960px` → **720px**, `gap: 20px` → **15px**, `margin-bottom: 30px` → **20px**
- MenuButtonContainer: `max-width: 1200px` → **800px**, `gap: 20px` → **15px**
- SoundButton: `width: 60px` → **48px**, `height: 60px` → **48px**, `font-size: 24px` → **20px**, `right: 40px` → **20px**, `bottom: 40px` → **20px**

#### ScenarioScreen.uxml
- ScenarioTitleText: `font-size: 36px` → **28px**
- SetupText: `font-size: 20px` → **16px**, `max-width: 800px` → **700px**, `margin-bottom: 30px` → **20px**
- WordFoundMessage/WordFailedMessage: `font-size: 24px` → **18px**, `margin-bottom: 30px` → **20px**
- ChoiceButtonContainer: `max-width: 800px` → **700px**, `gap: 15px` → **12px**
- Content: `padding: 40px` → **20px**, `max-width: 1000px` → **800px**, `margin: 30px` → **20px**

#### ResultScreen.uxml
- Content: `padding: 40px` → **20px**, `max-width: 800px` → **700px**, `margin: 30px` → **20px**
- ResultText: `font-size: 18px` → **16px**, `max-width: 800px` → **700px**, `margin-bottom: 30px` → **20px**

#### CreditsScreen.uxml
- Content: `padding: 40px` → **20px**, `max-width: 1400px` → **800px**, `margin: 30px` → **20px**
- CreditsTitle: `font-size: 60px` → **40px**, `margin-bottom: 48px` → **30px**

#### AchievementsScreen.uxml
- Content: `padding: 40px` → **20px**, `max-width: 1400px` → **800px**, `margin: 30px` → **20px**
- AchievementsTitle: `font-size: 36px` → **28px**, `margin-bottom: 30px` → **20px**
- AchievementsContainer: `padding: 20px` → **15px**
- BackToSelectionButtonFromAchievements: `margin-top: 30px` → **20px**

#### ProfileScreen.uxml
- Content: `padding: 40px` → **20px**, `max-width: 1400px` → **800px**, `margin: 30px` → **20px**
- ProfileSectionTitle: `font-size: 36px` → **28px**, `margin-bottom: 30px` → **20px**
- ProfileLayout: `gap: 20px` → **15px**, `min-height: 500px` → **400px**
- ProfileListContainer: `width: 250px` → **200px**
- ProfileList: `gap: 10px` → **8px**, `padding: 10px` → **8px**
- ProfileDetail: `padding: 20px` → **15px**
- BackToSelectionButtonFromProfile: `margin-top: 30px` → **20px**

#### MouhitotsuScreen.uxml
- Content: `padding: 40px` → **20px**, `max-width: 1400px` → **800px**, `margin: 30px` → **20px**
- MouhitotsuTitle: `font-size: 36px` → **28px**, `margin-bottom: 30px` → **20px**
- MouhitotsuContainer: `padding: 20px` → **15px**
- BackToSelectionButtonFromMouhitotsu: `margin-top: 30px` → **20px**

#### SoundSettingsPanel.uxml
- SoundSettingsPanel: `min-width: 400px` → **320px**, `padding: 30px` → **20px**
- タイトル: `font-size: 24px` → **20px**, `margin-bottom: 20px` → **15px**
- ラベル: `width: 100px` → **80px**, `font-size: 16px` → **14px**
- 値ラベル: `width: 50px` → **40px**, `margin-left: 10px` → **8px**
- コンテナ: `margin-bottom: 15px` → **12px**, `margin-bottom: 30px` → **20px**
- CloseButton: `min-width: 150px` → **120px**

## 調整の比率

960x540は1920x1080のちょうど半分なので、基本的にサイズを約80%に調整しました：
- 大きな要素（780px → 600px）: 約77%
- 中程度の要素（250px → 180px）: 約72%
- 小さな要素（50px → 40px）: 80%

## 効果

これらの調整により：
- UI要素が960x540の画面サイズに適切に収まる
- ボタンやテキストが適切なサイズで表示される
- UIが画面からはみ出さなくなる

## 注意事項

- フォントサイズも調整しているため、読みやすさを確認してください
- 必要に応じて、さらに細かい調整が必要な場合があります
- パーセンテージ指定の要素は、Reference Resolutionに応じて自動的にスケーリングされます
