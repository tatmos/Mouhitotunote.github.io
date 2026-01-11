# リソース移行ガイド

## 完了した作業

### Phase 1: AudioClip ✅

AudioClipの移行が完了しました。すべてのオーディオファイルは`Assets/Resources/Audio/`に配置されています。

### Phase 2: Sprite（アイコン類） 🔄

Sprite（アイコン類）の移行を進めています。

**必要な作業：**
1. アイコンファイルを`Assets/Resources/UI/Icons/`に移動またはコピー
2. 以下のファイル名で配置：
   - `CreditsIcon.png` （エンドクレジット用のアイコン）
   - `AchievementsIcon.png` （実績用のアイコン）
   - `ClockIcon.png` （カウントダウン用のアイコン）
   - `SparkleIcon.png` （スパークル用のアイコン）
   - `SoundIcon.png` （サウンド設定用のアイコン）

**手順：**
1. Unityエディタで`Assets/Resources/`フォルダ内に`UI`フォルダを作成（まだない場合）
2. `UI`フォルダ内に`Icons`フォルダを作成
3. 上記のアイコンファイルを`Assets/Resources/UI/Icons/`に移動またはコピー

## 次のステップ

### Phase 3: Material（優先度：高）

`distortionMaterial`をResources.Loadに移行：
- `Assets/Resources/Materials/DistortionMaterial.mat`に配置

### Phase 4: VisualTreeAsset (UXML)（優先度：中）

UXMLファイルをResources.Loadに移行：
- `Assets/Resources/UI/UXML/`に配置

### Phase 5: Sprite（ボタン・背景画像）（優先度：中）

ボタン画像や背景画像をResources.Loadに移行：
- `Assets/Resources/UI/Buttons/`にボタン画像を配置
- `Assets/Resources/UI/Backgrounds/`に背景画像を配置
