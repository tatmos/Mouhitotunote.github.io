# 登場人物画像生成ガイド

プロフィール画面に表示する登場人物の画像を生成するためのガイドです。

## 1. 画像生成AI用プロンプト

### シナリオ1: 田中 もも子（依頼人）

**プロンプト（日本語）:**
```
会社員の女性、依頼人、探偵事務所に依頼を持参している、少し心配そうな表情、落ち着いた服装、古い写真を持っている、柔らかい照明、ノスタルジックな雰囲気、温かみのある色調、ポートレート、上半身、高品質
```

**プロンプト（英語）:**
```
Female office worker, client, bringing a request to a detective agency, slightly worried expression, calm clothing, holding an old photograph, soft lighting, nostalgic atmosphere, warm color tones, portrait, upper body, high quality
```

**ファイル名:** `Character_Momoko.png`

---

### シナリオ2: 海原 うみ（シェフ）

**プロンプト（日本語）:**
```
レストランのシェフ、ユーモア好き、シェフの帽子とコックコート、笑顔、空の皿を持っている、レストランのキッチン、温かい照明、アートな雰囲気、モダンで洗練されたデザイン、ポートレート、上半身、高品質
```

**プロンプト（英語）:**
```
Restaurant chef, humorous, chef's hat and cook's coat, smiling, holding an empty plate, restaurant kitchen, warm lighting, artistic ambiance, modern and sophisticated design, portrait, upper body, high quality
```

**ファイル名:** `Character_Umi.png`

---

### シナリオ3: 広瀬 ひろ（幼馴染）

**プロンプト（日本語）:**
```
フリーランスの女性、幼馴染、親しみやすい表情、カジュアルな服装、タイムカプセルを手にしている、公園の風景、自然な光、ノスタルジックな雰囲気、温かみのある色調、ポートレート、上半身、高品質
```

**プロンプト（英語）:**
```
Female freelancer, childhood friend, friendly expression, casual clothing, holding a time capsule, park landscape, natural light, nostalgic atmosphere, warm color tones, portrait, upper body, high quality
```

**ファイル名:** `Character_Hiro.png`

---

### シナリオ4: 遠藤 とおる（試験官）

**プロンプト（日本語）:**
```
魔法学校の試験官、厳格だが柔軟な表情、魔法のローブ、試験官の帽子、魔法の本を持っている、魔法学校の教室、神秘的な雰囲気、柔らかい魔法の光、ファンタジーな雰囲気、ポートレート、上半身、高品質
```

**プロンプト（英語）:**
```
Magic school examiner, strict but flexible expression, magic robe, examiner's hat, holding a magic book, magic school classroom, mysterious atmosphere, soft magical light, fantasy ambiance, portrait, upper body, high quality
```

**ファイル名:** `Character_Toru.png`

---

### シナリオ5: 月島 つばさ（恋人）

**プロンプト（日本語）:**
```
デザイナーの女性、恋人、優しい表情、気配り上手、エレガントな服装、パズルのピースを持っている、居間の室内、温かい家庭的な雰囲気、柔らかい照明、ポートレート、上半身、高品質
```

**プロンプト（英語）:**
```
Female designer, lover, gentle expression, considerate, elegant clothing, holding a puzzle piece, living room interior, warm homey atmosphere, soft lighting, portrait, upper body, high quality
```

**ファイル名:** `Character_Tsubasa.png`

---

### シナリオ6: 謎の声（真実の扉の守護者）

**プロンプト（日本語）:**
```
神秘的な存在、真実の扉の守護者、光に包まれた姿、幻想的な雰囲気、謎めいた表情、古びた神秘的な扉の前、神秘的な雰囲気、エフェクト光、ファンタジーな雰囲気、ドラマチックな照明、ポートレート、上半身、高品質
```

**プロンプト（英語）:**
```
Mysterious being, guardian of the truth door, figure enveloped in light, fantastic atmosphere, enigmatic expression, in front of an ancient mysterious door, mysterious atmosphere, effect lights, fantasy ambiance, dramatic lighting, portrait, upper body, high quality
```

**ファイル名:** `Character_Voice.png`

---

## 2. 画像の仕様

- **解像度**: 512x512（推奨）または 1:1のアスペクト比
- **形式**: PNG（透明部分がある場合）または JPG
- **ファイルサイズ**: 1MB以下を推奨（Unityのインポート設定で最適化されます）
- **構図**: ポートレート（上半身）、中央配置

## 3. ファイルの配置場所

### 3.1 ディレクトリ構造

```
Mouhitotunooto/
└── Assets/
    └── Resources/
        └── UI/
            └── Characters/
                ├── Character_Momoko.png
                ├── Character_Umi.png
                ├── Character_Hiro.png
                ├── Character_Toru.png
                ├── Character_Tsubasa.png
                └── Character_Voice.png
```

### 3.2 手順

1. Unityプロジェクトの`Assets/Resources/UI/`フォルダ内に`Characters`フォルダを作成（まだない場合）
2. 生成した画像ファイルを`Characters`フォルダに配置
3. Unityエディタに戻ると、自動的にインポートされます

## 4. Unityでの設定

### 4.1 画像のインポート設定

1. Projectウィンドウで各登場人物画像を選択
2. Inspectorで以下を設定：
   - **Texture Type**: `Sprite (2D and UI)`
   - **Sprite Mode**: `Single`
   - **Max Size**: 512（必要に応じて調整）
   - **Compression**: 適切な品質を選択
   - **Apply**をクリック

### 4.2 確認方法

画像が正しく配置され、設定されているか確認するには：

1. Unityエディタでプロフィール画面を開く
2. 各登場人物のプロフィールを選択
3. 画像が表示されることを確認

## 5. 実装について

- 画像は`CharacterProfileImageHelper.GetProfileImage()`で読み込まれます
- 画像が見つからない場合は、画像を表示せずにテキストのみが表示されます
- 画像は`ProfileScreenManager.CreateProfileDetail()`で表示されます
- 画像はアンロック済みのプロフィールにのみ表示されます

## 6. 注意事項

- 画像ファイル名は、コード内で指定された名前と完全に一致する必要があります
- 画像が存在しない場合でも、エラーは発生せず、画像なしでテキストのみが表示されます
- 画像を追加した後は、Unityエディタで再インポートが実行されるまで待ってください
