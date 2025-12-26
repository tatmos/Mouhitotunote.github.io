# ミニノベルゲーム Unity移植版（UI Toolkit版）

## 概要
index.htmlで作成されたミニノベルゲームをUnityに移植したバージョンです。
UI Toolkit（UIElements）を使用して実装されています。

## セットアップ手順

詳細なセットアップ手順は `UIToolkit/UIToolkitSetupGuide.md` を参照してください。

### 1. シーンの設定

1. `Assets/Scenes/SampleScene.unity`を開くか、新しいシーンを作成
2. シーンに以下を追加：
   - `NovelGameInitializer`コンポーネントを持つGameObject（空のGameObjectを作成して追加）

### 2. UI Toolkitのセットアップ

1. UI Toolkitパッケージがインストールされているか確認（`Window > Package Manager`）
2. 各画面用のUIDocumentを作成（`Window > UI Toolkit > UI Document`）
3. UXMLファイルを各UIDocumentにアサイン
4. `UIManagerUIToolkit`コンポーネントをGameObjectに追加
5. Inspectorで必要なUIDocument、UXMLファイル、背景画像をアサイン

詳細は `UIToolkit/UIToolkitSetupGuide.md` を参照してください。

### 3. 実行

Playボタンを押してゲームを実行します。

## ゲームの機能

- 6つのシナリオから選択してプレイ
- 各シナリオで選択肢を選び、ストーリーを進める
- 「もうひとつ」という言葉を構成する文字（も、う、ひ、と、つ）を集める
- 最初の5つのシナリオをクリアすると、最後のシナリオ「真実の扉」が解放される
- スコアが異常な値になるとダークモードが有効になる

## 主要なスクリプト

### UIManagerUIToolkit.cs
UI ToolkitベースのUI管理を行うメインスクリプトです。以下の機能を提供します：

- 選択画面の表示とシナリオボタンの生成
- シナリオ画面の表示と選択肢ボタンの生成
- 結果画面の表示
- スコア表示の更新
- プロフィール画面の表示とプロフィールカードの生成

**Inspectorで設定する項目:**
- Selection Screen Document: 選択画面のUIDocument
- Scenario Screen Document: シナリオ画面のUIDocument
- Result Screen Document: 結果画面のUIDocument
- Profile Screen Document: プロフィール画面のUIDocument
- Selection Screen UXML: 選択画面のUXMLファイル
- Scenario Screen UXML: シナリオ画面のUXMLファイル
- Result Screen UXML: 結果画面のUXMLファイル
- Profile Screen UXML: プロフィール画面のUXMLファイル
- Scenario Backgrounds: 6つのシナリオ背景画像
- Selection Screen Background: 選択画面用の背景画像
- Profile Screen Background: プロフィール画面用の背景画像

### GameManager.cs
ゲームの状態管理を行うシングルトンクラスです。

### ScenarioDataLoader.cs
シナリオデータを初期化するスクリプトです。

### CharacterProfileManager.cs
キャラクタープロフィールデータを管理するスクリプトです。

## 背景画像について

背景画像の生成プロンプトと配置方法については、`BackgroundImageGuide.md` を参照してください。

## UI Toolkitのメリット

- **パフォーマンス**: UI ToolkitはuGUIより高速
- **スケーラビリティ**: 大量のUI要素でもパフォーマンスが安定
- **スタイリング**: CSSライクなUSSでスタイル管理が容易
- **レイアウト**: Flexboxライクなレイアウトシステム
- **デバッグ**: UI Builderで視覚的に編集可能

## 注意事項

- UI Builder（`Window > UI Toolkit > UI Builder`）を使用すると、UXMLファイルを視覚的に編集できます
- 背景画像のTexture Typeは「Sprite (2D and UI)」に設定してください
- UIのレイアウトは、1920x1080の解像度を基準に設計されています
