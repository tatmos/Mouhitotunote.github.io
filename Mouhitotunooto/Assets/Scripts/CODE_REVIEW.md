# コードレビューと整理の提案

## 概要

プロジェクトのコードを確認した結果、いくつかの改善点が見つかりました。以下に整理の提案をまとめます。

## 主要な問題点

### 1. **UIManagerUIToolkit.cs が非常に大きい（3607行）**

`UIManagerUIToolkit.cs` が3607行と非常に大きくなっています。これは以下の問題を引き起こします：

- **可読性の低下**: ファイルが大きすぎて、コードを理解しにくい
- **保守性の低下**: 変更を加える際に、関連するコードを見つけるのが難しい
- **テストの困難**: 1つのクラスに多くの責任があるため、単体テストが難しい
- **マージの競合**: 複数の人が同じファイルを編集する際に、マージの競合が発生しやすい

### 2. **単一責任の原則違反**

`UIManagerUIToolkit` クラスが以下の複数の責任を持っています：

- 各画面（タイトル、選択、シナリオ、結果、プロフィール、クレジット、実績、もうひとつ）の表示管理
- ボタンイベントの処理
- ダイアログの表示
- スコア表示の更新
- 画面遷移の管理
- オーバーレイの管理
- 背景テクスチャキャッシュの管理

### 3. **長すぎるメソッド**

各 `ShowXXXScreen` メソッドが長すぎる可能性があります。特に：
- `ShowScenarioScreen()` 
- `ShowResultScreen()`
- `ShowSelectionScreen()`

などは、画面のセットアップ、イベントハンドラの設定、アニメーションの開始など、多くの処理を含んでいる可能性があります。

## 整理の提案

### 提案1: 画面管理クラスの分離（最優先）

各画面ごとに専用のマネージャークラスを作成し、画面固有の処理をそこに移行します。

**現状:**
- `ProfileScreenManager` - プロフィール画面の管理（既に存在）
- `AchievementsScreenManager` - 実績画面の管理（既に存在）
- `CreditsScreenManager` - クレジット画面の管理（既に存在）
- `SelectionScreenManager` - 選択画面の管理（既に存在）
- `MouhitotsuScreenManager` - もうひとつ画面の管理（既に存在）

**追加すべき:**
- `TitleScreenManager` - タイトル画面の管理
- `ScenarioScreenManager` - シナリオ画面の管理（選択肢の表示、タイプライター効果など）
- `ResultScreenManager` - 結果画面の管理（エピローグの表示、ワードゲット表示など）

**メリット:**
- 各画面のロジックが独立して管理される
- `UIManagerUIToolkit` が画面間の遷移のみを担当する
- 各画面マネージャーが独立してテストできる

**実装例:**

```csharp
// TitleScreenManager.cs
public class TitleScreenManager
{
    private GameManager gameManager;
    private AudioManager audioManager;
    private UIDocument titleScreenDocument;
    
    public void Initialize(GameManager gameManager, AudioManager audioManager, UIDocument document)
    {
        this.gameManager = gameManager;
        this.audioManager = audioManager;
        this.titleScreenDocument = document;
    }
    
    public void SetupTitleScreen()
    {
        var root = titleScreenDocument.rootVisualElement;
        // タイトル画面のセットアップ処理
    }
    
    public void OnStartButtonClicked()
    {
        // スタートボタンの処理
    }
}
```

### 提案2: 定数の抽出

マジックナンバーやハードコードされた文字列を定数クラスに抽出します。

**現在の問題:**
```csharp
overlay.style.opacity = 1f;
float fadeDuration = 1.5f;
label.style.fontSize = 20;
```

**改善案:**
```csharp
// UIConstants.cs
public static class UIConstants
{
    public const float OverlayOpacity = 1f;
    public const float DefaultFadeDuration = 1.5f;
    public const int DefaultFontSize = 20;
    public const int TitleFontSize = 36;
    public const float BackgroundOverlayOpacity = 0.6f;
    public const float BackgroundOverlayFadeDuration = 0.5f;
}
```

### 提案3: ヘルパークラスの作成

共通の処理をヘルパークラスに抽出します。

**例:**
- `UIDialogHelper` - ダイアログの作成と表示
- `UIButtonHelper` - ボタンの設定（画像の適用、イベントハンドラの設定など）
- `UIStyleHelper` - スタイルの適用

**実装例:**

```csharp
// UIButtonHelper.cs
public static class UIButtonHelper
{
    public static void SetupButtonWithIcon(
        Button button, 
        Sprite icon, 
        string tooltip, 
        System.Action onClick,
        System.Action onHover = null)
    {
        // ボタンの共通設定
        button.tooltip = tooltip;
        button.clicked += onClick;
        if (onHover != null)
        {
            button.RegisterCallback<PointerEnterEvent>(evt => onHover());
        }
    }
    
    public static void ApplyButtonImage(
        Button button, 
        Sprite buttonImage, 
        Color textColor)
    {
        // ボタン画像の適用
    }
}
```

### 提案4: インターフェースの導入

画面マネージャーに共通のインターフェースを導入し、統一的な管理を可能にします。

```csharp
// IScreenManager.cs
public interface IScreenManager
{
    void Initialize(GameManager gameManager, AudioManager audioManager, UIDocument document);
    void Setup();
    void Show();
    void Hide();
    void Cleanup();
}
```

### 提案5: イベントハンドラの整理

ボタンイベントハンドラを整理し、ラムダ式のネストを減らします。

**現在の問題:**
```csharp
button.clicked += () => {
    // 長い処理
    if (condition) {
        // さらに長い処理
    }
};
```

**改善案:**
```csharp
button.clicked += OnButtonClicked;

private void OnButtonClicked()
{
    // 処理をメソッドに分離
}
```

### 提案6: コメントの整理

重要なメソッドには XML コメントを追加し、不要なコメントは削除します。

**例:**
```csharp
/// <summary>
/// タイトル画面を表示します。
/// 背景画像の設定、ボタンのイベントハンドラの設定などを行います。
/// </summary>
public void ShowTitleScreen()
{
    // 実装
}
```

## 実装の優先順位

1. **高優先度:**
   - `TitleScreenManager` の作成
   - `ScenarioScreenManager` の作成
   - `ResultScreenManager` の作成

2. **中優先度:**
   - 定数の抽出（`UIConstants` の作成）
   - ヘルパークラスの作成（`UIButtonHelper`、`UIDialogHelper` など）

3. **低優先度:**
   - インターフェースの導入
   - コメントの整理

## 注意事項

- **既存のコードとの互換性**: 既存のコードを壊さないように、段階的にリファクタリングを行います
- **テスト**: リファクタリング後は、動作確認を必ず行います
- **コミット**: 1つの画面マネージャーを作成するたびにコミットし、問題が発生した場合にロールバックしやすくします

## 参考

- Single Responsibility Principle (単一責任の原則)
- Separation of Concerns (関心の分離)
- Clean Code (クリーンコード)
- Refactoring (リファクタリング)
