using System.Collections.Generic;
using UnityEngine;

namespace NovelGame
{
    public class ScenarioDataLoader : MonoBehaviour
    {
        [SerializeField] private NovelGameData gameDataAsset;
        private List<Scenario> defaultScenarios;

        private void Awake()
        {
            if (gameDataAsset == null)
            {
                // データが割り当てられていない場合、デフォルトデータを生成
                CreateDefaultGameData();
            }
            else
            {
                // ScriptableObjectからデータをロード
                defaultScenarios = gameDataAsset.scenarios;
            }
        }

        public List<Scenario> GetScenarios()
        {
            if (defaultScenarios == null || defaultScenarios.Count == 0)
            {
                CreateDefaultGameData();
            }
            return defaultScenarios;
        }

        private void CreateDefaultGameData()
        {
            defaultScenarios = CreateScenarios();
            if (gameDataAsset == null)
            {
                gameDataAsset = ScriptableObject.CreateInstance<NovelGameData>();
                gameDataAsset.scenarios = defaultScenarios;
            }
        }

        private List<Scenario> CreateScenarios()
        {
            var scenarios = new List<Scenario>();

            // シナリオ1: 謎の依頼
            var scenario1 = new Scenario
            {
                id = 1,
                title = "謎の依頼",
                setup = "あなたは探偵事務所で古い写真を手渡されました。",
                choices = new List<Choice>
                {
                    new Choice { id = 1, text = "写真の人物について調べる", preview = "私：「この人物は..." },
                    new Choice { id = 2, text = "写真の背景について調べる", preview = "私：「この建物は..." }
                }
            };
            scenario1.branches[1] = new Branch
            {
                text = "私：「この人物は10年前に失踪した田中様ですね」\n依頼人：「そう、でも私が知りたいのは・・・」\n私：「失礼しました。【もうひとつ】の手がかりを調べます。背景の建物ですね」\n\n(あなたは何かを見つけた気がした。写真の隅に小さな文字が...「も」)",
                hasWord = true,
                epilogue = "その後、あなたは背景の手がかりから事件を解決し、一人前の探偵として認められるようになりました。\n(何かが集まり始めている...「も」という文字を手に入れた)",
                epilogue2 = "もも子さんは、あなたの探偵としての成長を認めてくれました。\n「あなたは本当に優秀な探偵ね。また何かあったら、お願いします」\nそう言って、もも子さんは笑顔で帰っていきました。\nあなたは、写真の隅にあった小さな文字「も」を大切に保管することにしました。"
            };
            scenario1.branches[2] = new Branch
            {
                text = "私：「この建物は取り壊された旧市庁舎です」\n依頼人：「まさにそこ!ありがとう、探偵さん」",
                hasWord = false,
                epilogue = "事件は解決しましたが、何か物足りなさを感じました。\n依頼人は満足していましたが、あなたは写真の人物についてもっと調べたかったのです。\n(もしかしたら、もうひとつの手がかりがあったのかもしれない...)",
                hint = "💡 ヒント: もも子さんは「もうひとつ」の手がかりを求めていました。写真の「人物」だけでなく、「背景」にも注目してみましょう。"
            };
            scenario1.SerializeBranches();
            scenarios.Add(scenario1);

            // シナリオ2: 不思議なレストラン
            var scenario2 = new Scenario
            {
                id = 2,
                title = "不思議なレストラン",
                setup = "メニューには「本日のおすすめ」と「シェフのきまぐれ」があります。",
                choices = new List<Choice>
                {
                    new Choice { id = 1, text = "本日のおすすめを注文する", preview = "シェフ：「お待たせ..." },
                    new Choice { id = 2, text = "シェフのきまぐれを注文する", preview = "シェフ：「お待たせ..." }
                }
            };
            scenario2.branches[1] = new Branch
            {
                text = "シェフ：「お待たせしました」\n美味しい料理が運ばれてきた。\nシェフ：「お気に召しましたか?」\n私：「はい!」",
                hasWord = false,
                epilogue = "料理は美味しかったですが、シェフの「きまぐれ」が気になりました。\n次回は、きまぐれメニューも試してみようと心に決めました。\n(もしかしたら、あの空の皿には何か意味があったのかもしれない...)",
                hint = "💡 ヒント: うみシェフの「きまぐれ」には特別な意味があります。空の皿の「裏」や「底」に注目してみましょう。"
            };
            scenario2.branches[2] = new Branch
            {
                text = "シェフ：「お待たせしました」\n運ばれてきたのは空の皿。\n私：「あの・・・これは?」\nシェフ：にっこり笑って「今日のきまぐれは『想像の料理』です」\n私：「・・・?」\nシェフ：「冗談です。【もうひとつ】お持ちします」\n\n(皿の底に何かが刻まれていた...「う」)",
                hasWord = true,
                epilogue = "シェフのユーモアに笑いながら、あなたはこのレストランの常連客になりました。\n(また文字を手に入れた...「う」)",
                epilogue2 = "うみシェフは、あなたが皿の底の文字に気づいたことを喜んでいました。\n「よく気づいてくれたわね。あれは特別なお客様へのメッセージなの」\nあなたは、うみシェフの遊び心と温かさに、ますますこのレストランが好きになりました。\n「また来てね」と、うみシェフは手を振って見送ってくれました。"
            };
            scenario2.SerializeBranches();
            scenarios.Add(scenario2);

            // シナリオ3: タイムカプセル
            var scenario3 = new Scenario
            {
                id = 3,
                title = "タイムカプセル",
                setup = "20年前に埋めたタイムカプセルを掘り起こす日がきました。",
                choices = new List<Choice>
                {
                    new Choice { id = 1, text = "一人で開ける", preview = "中には手紙と壊れた..." },
                    new Choice { id = 2, text = "幼馴染を呼んで一緒に開ける", preview = "友：「開けよう!」..." }
                }
            };
            scenario3.branches[1] = new Branch
            {
                text = "中には手紙と壊れたキーホルダーが入っていた。\n手紙：「未来の僕へ。夢は叶った?」\n胸が締め付けられる。\n(あいつと一緒に埋めたのに・・・)",
                hasWord = false,
                epilogue = "一人でタイムカプセルを開けた後、後悔の念に襲われました。\n幼馴染を呼べば良かった...そう思うと、胸が痛みました。\n(もしかしたら、一緒に開けていれば、違う何かが見つかったかもしれない)",
                hint = "💡 ヒント: ひろは「もうひとつ」の手紙を入れていたと言っていました。一緒に開けることで、手紙の「裏」に何か見つかるかもしれません。"
            };
            scenario3.branches[2] = new Branch
            {
                text = "友：「開けよう!」\n中には手紙が二通。\n私：「あれ、一通しか入れてないよね?」\n友：ニヤリと笑って「実は僕も【もうひとつ】入れてたんだ。読んでみて」\n手紙には励ましの言葉が綴られていた。\n\n(手紙の裏に小さな文字が...「ひ」)",
                hasWord = true,
                epilogue = "幼馴染との絆を再確認したあなたは、二人で新しいタイムカプセルを埋めることにしました。\n(3つ目の文字を発見...「ひ」)",
                epilogue2 = "新しいタイムカプセルを埋めた後、ひろは「今度は20年後、また一緒に開けよう」と言いました。\n「約束だよ」と、あなたはひろと小指を絡めました。\n手紙の裏にあった文字「ひ」は、友情の証として、あなたの心に刻まれました。\n二人は、これからもずっと友達でいられることを確信しました。"
            };
            scenario3.SerializeBranches();
            scenarios.Add(scenario3);

            // シナリオ4: 魔法学校の試験
            var scenario4 = new Scenario
            {
                id = 4,
                title = "魔法学校の試験",
                setup = "試験官：「ウサギを出現させなさい」",
                choices = new List<Choice>
                {
                    new Choice { id = 1, text = "呪文を唱える", preview = "パッ!ウサギが2..." },
                    new Choice { id = 2, text = "帽子から取り出す", preview = "スッと帽子からウサ..." }
                }
            };
            scenario4.branches[1] = new Branch
            {
                text = "パッ!ウサギが2羽出現した。\n試験官：「おや?」\n私：「すみません、魔力が強すぎて・・・」\n試験官：「構いません。では【もうひとつ】消してください」\n私：冷や汗をかく。\n\n(消えたウサギの跡に文字が浮かんだ...「と」)",
                hasWord = true,
                epilogue = "あなたは魔法と手品の両方をマスターし、誰もが驚くエンターテイナーになりました。\n(4つ目の文字を手に入れた...「と」)",
                epilogue2 = "とおる試験官は、あなたの才能を高く評価してくれました。\n「魔法と手品、両方を使えるのは珍しい。君は特別な才能を持っている」\nあなたは、消えたウサギの跡に浮かんだ文字「と」を、魔法の証として大切にしました。\n「これからも、もうひとつの可能性を探し続けてほしい」と、とおる試験官は言いました。"
            };
            scenario4.branches[2] = new Branch
            {
                text = "スッと帽子からウサギを取り出した。\n試験官：「完璧です。合格」\n試験が終わってから気づいた。\n(あれ、魔法使ってないぞ?)",
                hasWord = false,
                epilogue = "試験には合格しましたが、手品でごまかしたことに罪悪感を覚えました。\n本当の魔法を習得するため、あなたは再び魔法学校の門を叩くことにしました。\n(もしかしたら、呪文を唱えていれば、何か特別なことが起きたかもしれない)",
                hint = "💡 ヒント: とおる試験官は「もうひとつ」消すよう指示しました。呪文を唱えて、ウサギが「もうひとつ」出現した時、何か特別なことが起きるかもしれません。"
            };
            scenario4.SerializeBranches();
            scenarios.Add(scenario4);

            // シナリオ5: 最後のピース
            var scenario5 = new Scenario
            {
                id = 5,
                title = "最後のピース",
                setup = "1000ピースのジグソーパズルがあと1ピースで完成します。",
                choices = new List<Choice>
                {
                    new Choice { id = 1, text = "最後のピースをはめる", preview = "カチッ。完成した絵は..." },
                    new Choice { id = 2, text = "周りを探してみる", preview = "床を這いつくばって探..." }
                }
            };
            scenario5.branches[1] = new Branch
            {
                text = "カチッ。完成した絵は美しい星空。\n達成感に満たされる。\nその時、床に何かが落ちているのに気づいた。\n(え・・・まさか)",
                hasWord = false,
                epilogue = "パズルは完成しましたが、床に落ちていたのは別のピースでした。\nもしかしたら、周りをもっと探していれば、恋人のサプライズに気づけたかもしれません。\n(次は、もっと注意深く観察しようと心に誓いました)",
                hint = "💡 ヒント: つばさは「もうひとつ」のパズルを用意していました。周りを探すことで、つばさのサプライズに気づけるかもしれません。箱の「蓋の内側」にも注目してみましょう。"
            };
            scenario5.branches[2] = new Branch
            {
                text = "床を這いつくばって探す。見つからない。\n恋人：「探し物?」\n私：「最後のピースが・・・」\n恋人：ポケットから取り出して「実は【もうひとつ】買っておいたんだ。君が完成させた時のために」\n箱には同じパズルが入っていた。\n\n(箱の蓋の内側に文字が...「つ」)",
                hasWord = true,
                epilogue = "恋人の優しさに感動したあなたは、二人で新しいパズルを完成させながら幸せな時間を過ごしました。\n(最後の文字を手に入れた...「つ」)",
                epilogue2 = "新しいパズルを完成させた後、つばさは「君のために、いつも準備してるんだよ」と微笑みました。\n箱の蓋の内側にあった文字「つ」は、つばさの愛情の証でした。\n「これからも、ずっと一緒にパズルを完成させようね」\nあなたは、つばさの優しさに包まれながら、幸せを噛みしめました。"
            };
            scenario5.SerializeBranches();
            scenarios.Add(scenario5);

            // シナリオ6: 真実の扉
            var scenario6 = new Scenario
            {
                id = 6,
                title = "真実の扉",
                setup = "5つの「もうひとつ」を集めたあなたの前に、古びた扉が現れました。\n扉には「もうひとつ」という文字が刻まれています。\n謎の声が響きます：「なぜ、あなたは『もうひとつ』を探し続けたのか？」",
                choices = new List<Choice>
                {
                    new Choice { id = 1, text = "「好奇心からです」と答える", preview = "私：「ただ、気になっただけ..." },
                    new Choice { id = 2, text = "「答えを知りたかったからです」と答える", preview = "声：「では、その答えを..." }
                }
            };
            scenario6.branches[1] = new Branch
            {
                text = "私：「ただ、気になっただけです」\n声：「なるほど...では、その好奇心があなたをここへ導いたのですね」\n扉がゆっくりと開く。\n中から光が溢れ出し、あなたの前に現れたのは...\n\n声：「実は、『もうひとつ』を探すことは、人生の縮図なのです」\n「いつも、もうひとつの選択肢、もうひとつの可能性、もうひとつの答えがある」\n「それを探し続けることが、成長への鍵なのです」\n\nあなたは深く頷いた。",
                hasWord = false,
                epilogue = "好奇心から始まった旅路でしたが、真実の一部しか見ることができませんでした。\nもしかしたら、「答えを知りたかった」と答えることで、もっと深い真実に辿り着けたかもしれません。\nそれでも、あなたは「もうひとつ」を探し続けることの大切さを学びました。"
            };
            scenario6.branches[2] = new Branch
            {
                text = "声：「では、その答えを教えましょう」\n扉が大きく開く。\n\n声：「あなたが集めたもの...それは何だったか覚えていますか?」\nあなたは手を伸ばす。集めた文字が浮かび上がる...\n\n「も」「う」「ひ」「と」「つ」\n\n声：「そう、『もうひとつ』です」\n「あなたは5つの物語を通じて、この言葉を集めました」\n「なぜ『もうひとつ』を探す必要があったのか...」\n「それは、あなた自身が『もうひとつ』の存在だからです」\n「この世界には、あなたという『もうひとつ』の可能性が必要だった」\n「選択肢が2つある時、常に『もうひとつ』の道を探すこと」\n「それが、あなたを特別な存在にするのです」\n\n光に包まれながら、あなたは全てを理解した。\n集めた文字が「もうひとつ」という言葉として輝いた。\nそして、扉の向こうには新しい世界が広がっていた。",
                hasWord = true,
                epilogue = "「もうひとつ」の真実を知ったあなたは、これからも常に新しい可能性を探し続けることを誓いました。\n\n(あなたが集めた文字「も」「う」「ひ」「と」「つ」は、\n「もうひとつ」という言葉として、あなたの心に刻まれました)"
            };
            scenario6.SerializeBranches();
            scenarios.Add(scenario6);

            return scenarios;
        }
    }
}

