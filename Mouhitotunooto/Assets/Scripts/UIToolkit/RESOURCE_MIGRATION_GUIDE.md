# リソース移行ガイド

## 重要な注意事項

AudioClipをResources.Loadに移行するには、以下の手順が必要です：

### 1. AudioファイルをResourcesフォルダに移動

現在、Audioファイルは`Assets/Audio/`に配置されています。これらを`Resources/Audio/`に移動またはコピーする必要があります。

**手順：**
1. Unityエディタで`Assets/Audio/`フォルダ内のすべてのオーディオファイルを選択
2. `Assets/Resources/`フォルダ内に`Audio`フォルダを作成（まだない場合）
3. オーディオファイルを`Assets/Resources/Audio/`に移動またはコピー

**注意：**
- ファイル名にスペースが含まれている場合（例：`buttonHoverSound .wav`）は、スペースを削除するか、コードで正確なパスを指定してください
- `word_get_10.mp3`が存在しない場合、配列のインデックスを調整してください

### 2. ファイル名の確認

実際のファイル名を確認し、コード内のパスを調整してください：

- `word_get_1.mp3` → `Resources.Load<AudioClip>("Audio/word_get_1")`
- `wordGetIncreaseSound.wav` → `Resources.Load<AudioClip>("Audio/wordGetIncreaseSound")`
- `buttonHoverSound .wav` → `Resources.Load<AudioClip>("Audio/buttonHoverSound ")`（スペースに注意）

### 3. 動作確認

移行後、以下を確認してください：
- すべてのオーディオクリップが正常に読み込まれる
- エラーがコンソールに表示されない
- ゲームが正常に動作する

## 次のステップ

AudioClipの移行が完了したら、以下の順序で他のリソースも移行できます：

1. **Sprite（アイコン類）** - 優先度：高
2. **Material** - 優先度：高
3. **VisualTreeAsset (UXML)** - 優先度：中
4. **Sprite（ボタン・背景画像）** - 優先度：中
