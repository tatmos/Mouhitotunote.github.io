# ストリーマーオーバーレイ用画像リソース

このフォルダには、ストリーマーオーバーレイシステムで使用する画像リソースを配置します。

## フォルダ構造

```
Assets/Resources/Overlay/
├── Girl/          # 実況者の表情画像（Sprite）
│   ├── Neutral.png
│   ├── Smile.png
│   ├── Laugh.png
│   ├── Surprise.png
│   ├── Thinking.png
│   ├── Annoyed.png
│   ├── Shock.png
│   └── Concern.png
└── Room/          # 部屋背景画像（Texture2D）
    ├── CleanDay.png
    ├── NightGlow.png
    ├── Messy.png
    ├── Glitchy.png
    └── CalmMorning.png
```

## 必要な画像

### 1. 表情画像（Girl/）

実況者の表情を表現する画像です。各表情の説明：

- **Neutral.png** - 無表情・通常
- **Smile.png** - 笑顔
- **Laugh.png** - 大笑い
- **Surprise.png** - 驚き（⚠️ 現在不足）
- **Thinking.png** - 考えている
- **Annoyed.png** - 困った・イライラ
- **Shock.png** - ショック
- **Concern.png** - 心配

**推奨サイズ**: 200x300px 〜 400x600px（縦長）
**形式**: PNG（透明背景推奨）

### 2. 部屋背景画像（Room/）

実況者の部屋の背景を表現する画像です。各状態の説明：

- **CleanDay.png** - 清潔な昼間の部屋
- **NightGlow.png** - 夜の光る部屋（⚠️ 現在不足）
- **Messy.png** - 散らかった部屋
- **Glitchy.png** - グリッチが入った部屋
- **CalmMorning.png** - 穏やかな朝の部屋

**推奨サイズ**: 800x600px 〜 1920x1080px（横長）
**形式**: PNG（透明背景は不要）

## 画像の準備方法

### 方法1: 既存の画像を流用

既存のプロジェクト内の画像をコピーして使用できます。

### 方法2: 新しい画像を作成

以下のツールで作成できます：
- **AI画像生成ツール**（Stable Diffusion、Midjourney、DALL-Eなど）
- **イラストソフト**（Photoshop、Clip Studio Paint、Procreateなど）
- **フリー素材サイト**（イラストAC、いらすとやなど）

### 方法3: プレースホルダー画像を作成

一時的に警告を消すため、単色の画像や簡単な図形でも動作します。

## Unityでの設定

1. 画像を上記のフォルダ構造に配置
2. Unityエディタで画像を選択
3. **Inspector**で以下を設定：
   - **Texture Type**: `Sprite (2D and UI)`（表情画像の場合）
   - **Texture Type**: `Default`（部屋背景の場合）
   - **Max Size**: 適切なサイズに設定（2048など）
4. **Apply**をクリック

## 注意事項

- 画像ファイル名は**大文字小文字を区別**します（例: `Surprise.png` は `surprise.png` とは異なります）
- 画像は**Resourcesフォルダ内**に配置する必要があります
- 画像を追加・変更した後は、Unityエディタで**再インポート**されます

## 現在不足している画像

以下の画像が不足しているため、警告が表示されています：

1. **Overlay/Girl/Surprise.png** - 驚きの表情
2. **Overlay/Room/NightGlow.png** - 夜の光る部屋背景

これらの画像を用意して、上記のフォルダ構造に配置してください。

