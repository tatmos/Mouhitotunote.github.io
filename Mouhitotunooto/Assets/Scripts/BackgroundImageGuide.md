# 背景画像の設定ガイド

各シナリオと画面に背景画像を追加するためのガイドです。

## 1. 画像生成AI用プロンプト

### シナリオ1: 謎の依頼
**プロンプト（日本語）:**
```
探偵事務所の室内、古い木製の机、古い写真が置かれたデスク、柔らかい照明、ノスタルジックな雰囲気、温かみのある色調、アンティークな雰囲気、1920年代スタイル、詳細な背景、高品質
```

**プロンプト（英語）:**
```
Interior of a detective agency, old wooden desk, vintage photographs on the desk, soft lighting, nostalgic atmosphere, warm color tones, antique ambiance, 1920s style, detailed background, high quality
```

**ファイル名:** `Background_Scenario01_MysteryRequest.png`

---

### シナリオ2: 不思議なレストラン
**プロンプト（日本語）:**
```
レストランの室内、エレガントな雰囲気、温かい照明、テーブルクロス、空の皿が置かれたテーブル、シェフがいるキッチン、アートな雰囲気、モダンで洗練されたデザイン、詳細な背景、高品質
```

**プロンプト（英語）:**
```
Restaurant interior, elegant atmosphere, warm lighting, tablecloths, table with empty plates, chef in kitchen, artistic ambiance, modern and sophisticated design, detailed background, high quality
```

**ファイル名:** `Background_Scenario02_MysteriousRestaurant.png`

---

### シナリオ3: タイムカプセル
**プロンプト（日本語）:**
```
公園の風景、木々に囲まれた広場、タイムカプセルを埋める場所、青空、自然な光、ノスタルジックな雰囲気、温かみのある色調、詳細な背景、高品質
```

**プロンプト（英語）:**
```
Park landscape, square surrounded by trees, place to bury a time capsule, blue sky, natural light, nostalgic atmosphere, warm color tones, detailed background, high quality
```

**ファイル名:** `Background_Scenario03_TimeCapsule.png`

---

### シナリオ4: 魔法学校の試験
**プロンプト（日本語）:**
```
魔法学校の教室、古い石造りの壁、魔法の本が並ぶ本棚、試験官の机、神秘的な雰囲気、柔らかい魔法の光、ファンタジーな雰囲気、詳細な背景、高品質
```

**プロンプト（英語）:**
```
Magic school classroom, old stone walls, bookshelves with magic books, examiner's desk, mysterious atmosphere, soft magical light, fantasy ambiance, detailed background, high quality
```

**ファイル名:** `Background_Scenario04_MagicSchool.png`

---

### シナリオ5: 最後のピース
**プロンプト（日本語）:**
```
居間の室内、コーヒーテーブル、ジグソーパズルが広げられたテーブル、温かい家庭的な雰囲気、柔らかい照明、リラックスした雰囲気、詳細な背景、高品質
```

**プロンプト（英語）:**
```
Living room interior, coffee table, table with jigsaw puzzle spread out, warm homey atmosphere, soft lighting, relaxed ambiance, detailed background, high quality
```

**ファイル名:** `Background_Scenario05_LastPiece.png`

---

### シナリオ6: 真実の扉
**プロンプト（日本語）:**
```
古びた神秘的な扉、光が差し込む空間、神秘的な雰囲気、エフェクト光、ファンタジーな雰囲気、ドラマチックな照明、詳細な背景、高品質
```

**プロンプト（英語）:**
```
Ancient mysterious door, space with light streaming in, mysterious atmosphere, effect lights, fantasy ambiance, dramatic lighting, detailed background, high quality
```

**ファイル名:** `Background_Scenario06_TruthDoor.png`

---

### 選択画面用背景
**プロンプト（日本語）:**
```
ゲームのメインメニュー画面、エレガントなデザイン、温かい照明、ノスタルジックな雰囲気、選択肢が表示される画面、詳細な背景、高品質
```

**プロンプト（英語）:**
```
Game main menu screen, elegant design, warm lighting, nostalgic atmosphere, screen with selection options, detailed background, high quality
```

**ファイル名:** `Background_SelectionScreen.png`

---

### プロフィール画面用背景
**プロンプト（日本語）:**
```
プロフィール表示画面、カードが並ぶ画面、エレガントなデザイン、柔らかい照明、落ち着いた雰囲気、詳細な背景、高品質
```

**プロンプト（英語）:**
```
Profile display screen, screen with cards arranged, elegant design, soft lighting, calm atmosphere, detailed background, high quality
```

**ファイル名:** `Background_ProfileScreen.png`

---

## 2. 画像の仕様

- **解像度**: 1920x1080（推奨）または 16:9のアスペクト比
- **形式**: PNG（透明部分がある場合）または JPG
- **ファイルサイズ**: 2MB以下を推奨（Unityのインポート設定で最適化されます）

## 3. ファイルの配置場所

### 3.1 ディレクトリ構造
```
Mouhitotunooto/
└── Assets/
    └── Images/
        └── Backgrounds/
            ├── Background_SelectionScreen.png
            ├── Background_ProfileScreen.png
            ├── Background_Scenario01_MysteryRequest.png
            ├── Background_Scenario02_MysteriousRestaurant.png
            ├── Background_Scenario03_TimeCapsule.png
            ├── Background_Scenario04_MagicSchool.png
            ├── Background_Scenario05_LastPiece.png
            └── Background_Scenario06_TruthDoor.png
```

### 3.2 手順
1. Unityプロジェクトの`Assets`フォルダ内に`Images`フォルダを作成（まだない場合）
2. `Images`フォルダ内に`Backgrounds`フォルダを作成
3. 生成した画像ファイルを`Backgrounds`フォルダに配置
4. Unityエディタに戻ると、自動的にインポートされます

## 4. Unityでの設定

### 4.1 画像のインポート設定
1. Projectウィンドウで各背景画像を選択
2. Inspectorで以下を設定：
   - **Texture Type**: Sprite (2D and UI)
   - **Max Size**: 2048（必要に応じて調整）
   - **Compression**: 適切な品質を選択
   - **Apply**をクリック

### 4.2 背景画像用のGameObject作成
各画面（SelectionScreen、ProfileScreen、ScenarioScreen、ResultScreen）に背景画像を表示するImageコンポーネントを追加します。

#### SelectionScreenの背景
1. SelectionScreenの子として`UI > Image`を作成
2. 名前を「BackgroundImage」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Stretch Stretch
   - **Left, Top, Right, Bottom**: すべて0
   - **注意**: 最背面に配置（Hierarchyで最初に配置）
4. Imageの設定：
   - **Source Image**: Background_SelectionScreen.pngをドラッグ
   - **Color**: 白（Alpha: 255）
   - **Image Type**: Simple
   - **Preserve Aspect**: ✓（アスペクト比を維持）

#### ProfileScreenの背景
1. ProfileScreenの子として`UI > Image`を作成
2. 名前を「BackgroundImage」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Stretch Stretch
   - **Left, Top, Right, Bottom**: すべて0
   - **注意**: 最背面に配置（Hierarchyで最初に配置）
4. Imageの設定：
   - **Source Image**: Background_ProfileScreen.pngをドラッグ
   - **Color**: 白（Alpha: 255）
   - **Image Type**: Simple
   - **Preserve Aspect**: ✓（アスペクト比を維持）

#### ScenarioScreenの背景
1. ScenarioScreenの子として`UI > Image`を作成
2. 名前を「BackgroundImage」に変更
3. RectTransformの設定：
   - **Anchor Presets**: Stretch Stretch
   - **Left, Top, Right, Bottom**: すべて0
   - **注意**: 最背面に配置（Hierarchyで最初に配置）
4. Imageの設定：
   - **Source Image**: 対応するシナリオの背景画像をドラッグ
   - **Color**: 白（Alpha: 255）
   - **Image Type**: Simple
   - **Preserve Aspect**: ✓（アスペクト比を維持）

#### ResultScreenの背景
1. ResultScreenの子として`UI > Image`を作成
2. 名前を「BackgroundImage」に変更
3. 上記と同じ設定

## 5. スクリプトでの背景画像切り替え

UIManagerスクリプトを拡張して、シナリオに応じて背景画像を切り替える機能を追加します。

### 5.1 背景画像の参照を追加
UIManagerに以下のフィールドが追加されています（既に実装済み）：
```csharp
[Header("Background Images")]
[SerializeField] private Sprite[] scenarioBackgrounds = new Sprite[6];
[SerializeField] private Image scenarioBackgroundImage;
[SerializeField] private Image resultBackgroundImage;
[SerializeField] private Sprite selectionScreenBackground;
[SerializeField] private Image selectionBackgroundImage;
[SerializeField] private Sprite profileScreenBackground;
[SerializeField] private Image profileBackgroundImage;
```

### 5.2 背景画像の設定
Inspectorで以下を設定：
- **Scenario Backgrounds**: 6つの背景画像を順番にアサイン
  - Element 0: Scenario01の背景
  - Element 1: Scenario02の背景
  - Element 2: Scenario03の背景
  - Element 3: Scenario04の背景
  - Element 4: Scenario05の背景
  - Element 5: Scenario06の背景
- **Selection Screen Background**: 選択画面用の背景画像をアサイン
- **Selection Background Image**: SelectionScreen内のBackgroundImageをアサイン
- **Profile Screen Background**: プロフィール画面用の背景画像をアサイン
- **Profile Background Image**: ProfileScreen内のBackgroundImageをアサイン
- **Scenario Background Image**: ScenarioScreen内のBackgroundImageをアサイン
- **Result Background Image**: ResultScreen内のBackgroundImageをアサイン

### 5.3 背景画像の切り替え処理
ShowScenarioScreen()メソッドとShowResultScreen()メソッドで背景画像を切り替えます。

## 6. 画像生成AIの推奨設定

### Midjourneyの場合
- **Aspect Ratio**: `--ar 16:9`
- **Quality**: `--q 2`（高品質）
- **Style**: `--style raw`（よりリアルな表現）

### DALL-E 3の場合
- **Size**: 1024x1792 または 1792x1024（16:9に近い）
- **Quality**: Standard または HD
- **Style**: Natural（自然な表現）

### Stable Diffusionの場合
- **Resolution**: 1920x1080
- **Steps**: 30-50
- **CFG Scale**: 7-9
- **Sampler**: DPM++ 2M Karras または Euler a

## 7. 背景画像のカスタマイズ

各シナリオの内容に合わせて、プロンプトを調整できます：

- **時間帯**: 朝、昼、夕方、夜
- **天候**: 晴れ、曇り、雨
- **雰囲気**: 明るい、暗い、神秘的、温かい
- **スタイル**: リアル、アニメ風、絵画風

例：シナリオ1を夜の雰囲気にする場合
```
探偵事務所の室内、夜、古い木製の机、古い写真が置かれたデスク、柔らかい電灯の光、影が長く伸びる、ノスタルジックな雰囲気、温かみのある色調、アンティークな雰囲気、1920年代スタイル、詳細な背景、高品質
```

## 8. トラブルシューティング

### 画像が表示されない
- 画像のTexture Typeが「Sprite (2D and UI)」になっているか確認
- ImageコンポーネントのSource Imageが正しくアサインされているか確認
- Imageコンポーネントが有効になっているか確認

### 画像が歪んで表示される
- Imageコンポーネントの「Preserve Aspect」にチェックを入れる
- RectTransformのサイズを適切に設定

### 画像がぼやける
- 画像のMax Sizeを2048以上に設定
- Compression設定を確認（NoneまたはHigh Quality）

