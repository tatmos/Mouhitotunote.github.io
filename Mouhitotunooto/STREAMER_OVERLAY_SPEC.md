

---

# STREAMER_OVERLAY_SPEC.md

**Streamer Overlay（外側世界実況者レイヤー）仕様書 v1.0**

## 0. 概要

本ゲーム（Normal/Dark/Third、Division A–E）の既存仕様は維持したまま、画面フレーム外の「Lo-fiガジェット部屋」にいる実況者（かわいい女の子）を追加する。
実況者はプレイヤー操作に同期してリアクションし、**同調／反対／ツッコミ**を返す。
混乱を増やさないため、実況は **低頻度・重要イベント中心**、かつ **クールダウン必須**とする。

---

## 1. 目的 / 非目的

### 1.1 目的

* 画面外に「もうひとつ外側の世界観」を追加し、体験の印象を刷新する。
* 既存のループ構造（Division B以降の崩れ）に合わせて、実況者の存在感を段階的に増減させる。
* “ゲームを理解すると面白い”に加え、**見ているだけでも楽しい**要素を作る（動画化・スクショ化の誘因）。

### 1.2 非目的

* ルール追加（分岐条件、スコア条件、クリア条件）の変更はしない。
* 本編UI/テキストの可読性を落とさない。
* 実況者の説明で物語を解説しすぎない（考察余地を残す）。

---

## 2. 表示仕様（UI Toolkit）

### 2.1 レイヤリング

* Overlay専用の `UIDocument` を追加し、本編UIの上に重ねる。
* 本編UIは極力改造しない。Overlayは「別レイヤー」で完結させる。

### 2.2 レイアウト方針

* デフォルト：右下 **PiP（小窓）** 常駐（Dark/Third中）
* 画面が狭い場合：PiPを縮小し、**吹き出しのみ**に縮退可能
* Overlayが本編の主要UI（選択肢・本文）を隠さないよう、**アンカー固定**＋**安全マージン**を持つ。

### 2.3 Overlay UI構成要素（UXML）

* `RoomImage`：部屋背景（差分）
* `GirlImage`：実況者立ち絵（表情差分）
* `BalloonRoot` / `BalloonLabel`：通常吹き出し
* `ThoughtBalloonRoot` / `ThoughtBalloonLabel`：心の声（点線枠）
* `PropsLayer`：付箋・マグカップ・ガジェット等（任意）
* `OverlayRoot`：全体ルート（フェード制御など）

---

## 3. 出現タイミング（フェーズ設計）

混乱回避のため、「いつ出るか」を Phase で統制する。

### 3.1 OverlayPhase

* `Hidden`：非表示
* `Presence`：気配のみ（短い反応、表示時間短）
* `Active`：常駐（ただし発話頻度は抑える）
* `Quiet`：終盤は静か（余韻）

### 3.2 推奨遷移

* **Phase 0** `Hidden`：Prologue〜Normal中盤
* **Phase 1** `Presence`：Division B確定直後〜Dark開始直前
  → “空気が変わる”程度の短い反応
* **Phase 2** `Active`：Dark〜Third中
  → 常駐。重要イベントのみ喋る
* **Phase 3** `Quiet`：Division D/E直前〜エンド
  → 喋らない/短い一言で余韻

---

## 4. イベント設計（本編→Overlay）

本編は最小改修でOverlayへ通知する。Overlay側が購読して反応を決定する。

### 4.1 必要イベント（最小）

* `ModeChanged(Normal/Dark/Third)`
* `DivisionEntered(A–E)`
* `ScenarioStarted(1..6)`
* `ScenarioCleared(1..6, gotMouhitotu: bool)`
* `MouhitotuResult(success: bool, reason, scenarioId)`
* `ChoiceSelected(choiceId, scenarioId)`
* `ReturnToScenarioSelect()`

### 4.2 本編差し込み原則

* 本編側は `OverlayEventHub.Publish(evt)` を呼ぶだけ。
* Overlayの内部事情（セリフ、表情、部屋差分）は本編に漏らさない。

---

## 5. AI向けCSファイル単位アーキテクチャ（SO/JSONなし）

### 5.1 フォルダ案

```
Assets/
  Scripts/
    Overlay/
      OverlayBootstrap.cs
      OverlayEventHub.cs
      OverlayEventTypes.cs
      OverlayState.cs
      OverlayAssets.cs
      OverlayViewBindings.cs
      OverlayPresenter_UITK.cs
      PlayerTelemetry.cs
      PlayerTypeClassifier.cs
      ReactionDirector.cs
      ReactionRules.cs
      ReactionLines_JP.cs
```

### 5.2 各ファイルの責務（固定）

* **OverlayBootstrap.cs**

  * Overlay初期化、UIDocument参照セット、購読開始
* **OverlayEventHub.cs**

  * staticイベントバス（Subscribe/Publish）
* **OverlayEventTypes.cs**

  * イベントrecord/struct定義
* **OverlayState.cs**

  * mode/division/phase/playerType/cooldown等のOverlay内部状態
* **OverlayAssets.cs**

  * 表情Sprite/部屋Texture等の参照解決（Addressables/Resources方針をここに閉じ込める）
* **OverlayViewBindings.cs**

  * UXML要素名 const、要素取得ユーティリティ
* **OverlayPresenter_UITK.cs**

  * UI Toolkitへ反映（画像・テキスト・フェード・アニメ）
* **PlayerTelemetry.cs**

  * プレイヤー癖計測（クリック間隔、スキップ率、ログ閲覧頻度など）
* **PlayerTypeClassifier.cs**

  * Telemetry→PlayerType判定（Fast/Careful/Explorer）
* **ReactionRules.cs**

  * リアクションの条件、優先度、クールダウン定義（C#内）
* **ReactionLines_JP.cs**

  * セリフ辞書（巨大化しやすいので分離）
* **ReactionDirector.cs**

  * イベント＋状態→最適Reactionを選ぶ（競合解決・頻度制御）

---

## 6. データ構造（C#定義）

### 6.1 enum

* `GameMode { Normal, Dark, Third }`
* `Division { None, A, B, C, D, E }`
* `PlayerType { Unknown, FastClicker, CarefulReader, Explorer }`
* `GirlExpression { Neutral, Smile, Laugh, Surprise, Thinking, Annoyed, Shock, Concern }`
* `RoomState { CleanDay, NightGlow, Messy, Glitchy, CalmMorning }`
* `OverlayPhase { Hidden, Presence, Active, Quiet }`

### 6.2 イベント型（record/struct）

`IOverlayEvent` を共通インターフェイスにし、`OverlayEventHub` が配信する。

例：

* `ModeChangedEvt(GameMode Mode)`
* `DivisionEnteredEvt(Division Division)`
* `ScenarioStartedEvt(int ScenarioId)`
* `ScenarioClearedEvt(int ScenarioId, bool GotMouhitotu)`
* `MouhitotuResultEvt(int ScenarioId, bool Success, string Reason)`
* `ChoiceSelectedEvt(int ScenarioId, string ChoiceId)`
* `ReturnToScenarioSelectEvt()`

### 6.3 Reaction（反応）定義

Reactionは「条件＋出力」を持つ。出力はUIに適用可能なPayloadへ落とす。

* `Id`：安定識別子
* `Priority`：競合時の勝ち
* `CooldownSeconds`：発話抑制
* `MinPhase/MaxPhase`：フェーズ範囲
* `Condition(ctx)`：条件式
* `PayloadFactory(ctx)`：実際に表示するセリフ・表情・部屋状態を生成

---

## 7. 発話頻度制御（混乱回避の最重要要件）

以下を **必須** とする。

* **CooldownSeconds**：最低でも 8–15 秒程度
* **MaxPerMinute**（内部で保持）：例 2回/分
* **OncePerKeyEvent**：初回のみ喋るイベントを明示
* 連打系（次送りなど）は原則反応対象から外す

> 実装上は `OverlayState` に「最近喋った時刻」「イベント別発話済みフラグ」を持つ。

---

## 8. セリフ設計（原則）

* 短い（1〜2文）
* 断定しない（考察余地）
* 本編の説明はしない（実況は“感想”寄り）
* 同じイベントでも PlayerType によって口調を変える

---

## 9. セリフ例（日本語：イベント×タイプ）

### 9.1 Division B（Presence）

* 共通

  * 「……今、空気変わったよね。」
  * 「あ、これ“戻れない”やつだ。」
* FastClicker

  * 「はい出た。取りすぎ警察。」
* CarefulReader

  * 「……修正、走った。たぶん。」
* Explorer

  * 「これ、裏側に踏み込めたってこと？」

### 9.2 Dark開始（Active）

* 共通

  * 「うわ、画面が“直されてる”。」
  * 「ねえ、今の音…混ざったよね。」
* FastClicker

  * 「スコア増えないの、ケチすぎ。」
* CarefulReader

  * 「“正しい遊び方”に戻されてる感じがする。」
* Explorer

  * 「じゃあ、逆に壊していこっか。」

### 9.3 「もうひとつ」成功（初回のみ）

* 共通

  * 「今の、取った。……取れた。」
  * 「見つけた…たぶん、見つけた。」
* FastClicker

  * 「そこで反応するの、素直すぎ。」
* CarefulReader

  * 「文字、戻る…？　いや、まだか。」
* Explorer

  * 「このバグ、育てられるかも。」

### 9.4 Division C（強制再起動）

* 共通

  * 「……あ、落ちる。これ落ちる。」
  * 「強制再起動って、言ったよね今。」
* FastClicker

  * 「修正って言いながら破壊してない？」
* CarefulReader

  * 「これは“戻す”じゃない。“作り直す”だ。」
* Explorer

  * 「じゃ、2周目じゃなくて…3周目？」

### 9.5 3周目開始（Active）

* 共通

  * 「最初から伏字…徹底してる。」
  * 「UIまで隠すの、やりすぎだって。」
* CarefulReader

  * 「復活条件…“見つける”か。」
* Explorer

  * 「拾い直すフェーズだね。回収しよ。」

### 9.6 終盤（Quiet）

* Division D寄り

  * 「……ここまで戻せたんだ。」
  * 「うん。これで、たぶん。」
* Division E寄り

  * 「……まだ触るの？」
  * 「やめとけ、って言うべきかな。……でも。」

---

## 10. アセット要件（最小）

### 10.1 実況者（立ち絵）

* MVP：4表情

  * Neutral / Surprise / Thinking / Smile
* 余裕：8表情（Shock/Annoyed/Concern/Laugh追加）

### 10.2 部屋背景

* MVP：2状態

  * CleanDay / NightGlow
* 余裕：5状態（Messy/Glitchy/CalmMorning追加）

### 10.3 吹き出し

* 通常・心の声（点線枠）2種

---

## 11. MVP（最小実装：これだけで成立）

### 11.1 反応イベント（5つ、全て初回のみ）

1. `DivisionEntered(B)`（Presence）
2. `ModeChanged(Dark)`（Active）
3. `MouhitotuResult(success=true)`（初回）
4. `DivisionEntered(C)`（強制再起動）
5. `ModeChanged(Third)`（3周目開始）

### 11.2 表示ルール

* Presenceは短く（1.5〜2.5秒）
* Activeは3〜5秒
* クールダウンは最低10秒

---

## 12. 実装チェックリスト（Agent用）

### 12.1 本編改修（最小）

* [ ] Division確定時に `DivisionEnteredEvt` をPublish
* [ ] Dark/Third切替時に `ModeChangedEvt` をPublish
* [ ] 「もうひとつ」成功/失敗時に `MouhitotuResultEvt` をPublish
* [ ] シナリオ開始/クリア時にイベントPublish
* [ ] シナリオ選択に戻る時に `ReturnToScenarioSelectEvt` をPublish

### 12.2 Overlay実装

* [ ] Overlay UIDocument追加（UXML/USS）
* [ ] ViewBindingsで要素取得
* [ ] ReactionDirectorで競合解決（Priority + Cooldown）
* [ ] TelemetryからPlayerType推定
* [ ] Presenterで画像/テキスト反映、フェード制御
* [ ] 画面狭い場合の縮退（任意）

### 12.3 品質

* [ ] 喋りすぎない（MaxPerMinute）
* [ ] 本編UIを隠さない
* [ ] エンド前はQuietに遷移して余韻が出る

---

## 13. 互換性 / 将来拡張

* 将来的にSO/JSONへ移行しても、`ReactionDirector` の入力は同じに保つ。
* Persona（実況者タイプ）を増やす場合は `ReactionLines_XX.cs` を増設し差し替え可能にする。

---

## 付録A：UXML要素名（推奨）

* `OverlayRoot`
* `RoomImage`
* `GirlImage`
* `BalloonRoot`
* `BalloonLabel`
* `ThoughtBalloonRoot`
* `ThoughtBalloonLabel`
* `PropsLayer`

---

