# リソースローディング戦略の分析と提案

## 現状の分析

### UIManagerUIToolkit の現状

`UIManagerUIToolkit`には大量の`[SerializeField]`フィールドがあり、インスペクターで設定する必要があります：

1. **UIDocument** (9個)
   - titleScreenDocument, selectionScreenDocument, scenarioScreenDocument, など

2. **VisualTreeAsset (UXML)** (8個)
   - selectionScreenUXML, scenarioScreenUXML, resultScreenUXML, など

3. **Sprite** (多数)
   - scenarioBackgrounds[6], selectionScreenBackground, profileScreenBackground
   - scenarioButtonNormalImage, scenarioButtonCompletedImage
   - uiButtonNormalImage, uiButtonDarkImage, uiButtonIndigoImage
   - titleImage, scoreDisplayBackgroundImage, menuButtonImage
   - creditsIcon, achievementsIcon, clockIcon, sparkleIcon, soundIcon

4. **AudioClip** (多数)
   - wordGetSounds[], wordGetIncreaseSound, wordGetDecreaseSound
   - creditsBGM, selectionBGM
   - typewriterSound, lostLetterSound, sparkleSound, buttonHoverSound
   - thunderSound, truthDoorUnlockSound
   - ambientSounds[]

5. **Material** (1個)
   - distortionMaterial

### 最近のパターン（Resources.Load使用）

以下のクラスでは`Resources.Load`を使用しています：

1. **CreditsScreenManager.cs**
   ```csharp
   TextAsset jsonAsset = Resources.Load<TextAsset>("Lyric/creditsBGM");
   ```

2. **OverlayAssets.cs**
   ```csharp
   Sprite sprite = Resources.Load<Sprite>(ExpressionPaths[index]);
   Texture2D texture = Resources.Load<Texture2D>(RoomPaths[index]);
   Sprite sprite = Resources.Load<Sprite>("Overlay/MusicNotes/BeamedNote");
   ```

## 比較：SerializeField vs Resources.Load

### SerializeField（インスペクター指定）のメリット

1. **型安全性**: コンパイル時に型チェックが可能
2. **エディタでの視覚的な確認**: インスペクターで設定値が一目でわかる
3. **エディタでの編集**: シーンごとに異なるリソースを設定可能
4. **参照の確実性**: アセットが存在しない場合、エディタで警告が表示される

### SerializeFieldのデメリット

1. **設定の手間**: インスペクターで多数の項目を設定する必要がある
2. **シーン依存**: シーンごとに設定が必要
3. **コードの肥大化**: 大量のフィールド定義が必要
4. **再利用性**: 他のシーンで使う際に再度設定が必要

### Resources.Loadのメリット

1. **設定不要**: コードで直接指定するため、インスペクター設定が不要
2. **コードの簡潔性**: フィールド定義が不要
3. **再利用性**: どのシーンでも同じコードで動作
4. **一貫性**: リソースのパスがコードで明示される

### Resources.Loadのデメリット

1. **実行時のオーバーヘッド**: リソースを毎回ロード（ただし、Unityは内部的にキャッシュ）
2. **パス管理**: 文字列でパスを指定するため、タイポのリスク
3. **エディタでの確認**: インスペクターでは表示されない
4. **ビルドサイズ**: Resourcesフォルダ内のすべてのアセットがビルドに含まれる

## 提案：ハイブリッドアプローチ

### 1. UIDocumentは保持（SerializeField）

理由：
- シーンに配置する必要がある
- 各UIDocumentはGameObjectとして存在する必要がある
- インスペクター設定が適切

### 2. 静的リソースはResources.Loadに移行

以下のリソースは`Resources.Load`に移行を検討：

#### 優先度：高
- **AudioClip**: 変更頻度が低く、複数シーンで使用される
- **Sprite（アイコン類）**: 変更頻度が低く、複数シーンで使用される
  - creditsIcon, achievementsIcon, clockIcon, sparkleIcon, soundIcon
- **Material**: 変更頻度が低い
  - distortionMaterial

#### 優先度：中
- **VisualTreeAsset (UXML)**: 変更頻度は中程度
- **Sprite（背景画像）**: 変更頻度は低いが、サイズが大きい
- **Sprite（ボタン画像）**: 変更頻度は低い

#### 優先度：低
- **Sprite（背景画像の配列）**: サイズが大きいため、Resourcesフォルダに置くのは注意が必要

### 3. 実装例

#### 現在（SerializeField）
```csharp
[Header("Audio")]
[SerializeField] private AudioClip creditsBGM;
[SerializeField] private AudioClip selectionBGM;
```

#### 提案（Resources.Load）
```csharp
private AudioClip creditsBGM;
private AudioClip selectionBGM;

private void Awake()
{
    creditsBGM = Resources.Load<AudioClip>("Audio/CreditsBGM");
    selectionBGM = Resources.Load<AudioClip>("Audio/SelectionBGM");
}
```

または、必要時にロード：
```csharp
private AudioClip GetCreditsBGM()
{
    return Resources.Load<AudioClip>("Audio/CreditsBGM");
}
```

### 4. 段階的な移行計画

#### Phase 1: AudioClipの移行
- すべてのAudioClipをResources/Audio/に配置
- UIManagerUIToolkitのAudioClipフィールドをResources.Loadに変更

#### Phase 2: アイコンSpriteの移行
- アイコン類（creditsIcon, achievementsIcon, clockIcon, sparkleIcon, soundIcon）をResources/Icons/に配置
- Resources.Loadに変更

#### Phase 3: ボタン画像Spriteの移行
- ボタン画像類をResources/UI/Buttons/に配置
- Resources.Loadに変更

#### Phase 4: 背景画像とUXMLの検討
- サイズや変更頻度を考慮して検討
- 必要性が高い場合のみ移行

### 5. 注意事項

1. **Resourcesフォルダの構造**: 
   - 明確なフォルダ構造を定義する
   - 例: `Resources/Audio/`, `Resources/UI/Icons/`, `Resources/UI/Buttons/`, `Resources/UI/UXML/`

2. **エラーハンドリング**:
   ```csharp
   private AudioClip LoadAudioClip(string path)
   {
       var clip = Resources.Load<AudioClip>(path);
       if (clip == null)
       {
           Debug.LogError($"AudioClip not found: {path}");
       }
       return clip;
   }
   ```

3. **パフォーマンス**:
   - 頻繁に使用されるリソースは、Awake()やStart()でキャッシュする
   - 使用頻度が低いリソースは必要時にロード

4. **既存コードとの互換性**:
   - 他のScreenManagerに渡す際の互換性を確認
   - 段階的に移行する

## 結論

現在のコードベースでは、`CreditsScreenManager`や`OverlayAssets`が`Resources.Load`を使用しているため、この方向性に統一することを推奨します。ただし、UIDocumentは引き続き`[SerializeField]`を使用し、静的リソース（AudioClip、Sprite、Material、UXMLなど）を段階的に`Resources.Load`に移行するのが適切です。

メリット：
- インスペクター設定の手間が減る
- コードの一貫性が向上
- シーン間での再利用性が向上
- 設定ミスの減少

デメリット（対策済み）：
- 実行時のオーバーヘッド → Awake()でキャッシュ
- パス管理のリスク → 定数で管理
- ビルドサイズ → Resourcesフォルダの構造を最適化
