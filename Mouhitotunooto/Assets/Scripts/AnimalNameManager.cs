using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// 動物名管理クラス
    /// ランダムな動物名とその動物にゆかりのある話題を提供する
    /// </summary>
    public static class AnimalNameManager
    {
        private static List<AnimalData> animals = null;
        private static System.Random random = new System.Random();

        private class AnimalData
        {
            public string name;
            public string relatedTopic;
        }

        /// <summary>
        /// 動物リストを初期化
        /// </summary>
        private static void InitializeAnimals()
        {
            if (animals != null) return;

            animals = new List<AnimalData>
            {
                new AnimalData { name = "ウサギ", relatedTopic = "ウサギは月で餅をついていると言われています。また、ウサギの耳は長く、遠くの音を聞き分ける能力に優れています。" },
                new AnimalData { name = "ハムスター", relatedTopic = "ハムスターは頬袋に食べ物を詰め込む習性があります。その姿はとても愛らしく、ペットとしても人気が高いです。" },
                new AnimalData { name = "モルモット", relatedTopic = "モルモットは「ピーピー」と鳴くことが知られています。南米のアンデス地方が原産で、インカ帝国の時代から飼育されていました。" },
                new AnimalData { name = "リス", relatedTopic = "リスは木の実を地面に埋めて保存する習性があります。しかし、埋めた場所を忘れてしまうことも多く、それが森の木々の成長に役立っています。" },
                new AnimalData { name = "ネズミ", relatedTopic = "ネズミは十二支の最初の動物として知られています。繁殖力が非常に高く、世界中に生息しています。" },
                new AnimalData { name = "フェレット", relatedTopic = "フェレットはイタチの仲間で、細長い体と好奇心旺盛な性格が特徴です。かつてはウサギ狩りに使われていました。" },
                new AnimalData { name = "チンチラ", relatedTopic = "チンチラは南米のアンデス山脈に生息する、ふわふわの毛皮を持つ動物です。その毛皮は非常に柔らかく、1つの毛穴から60本以上の毛が生えています。" },
                new AnimalData { name = "デグー", relatedTopic = "デグーはチリのアンデス山脈原産のげっ歯類です。糖尿病になりやすいことで知られており、研究にも使われています。" },
                new AnimalData { name = "ハツカネズミ", relatedTopic = "ハツカネズミはその名の通り、妊娠期間が約20日と短いことが特徴です。実験動物としても広く使われています。" },
                new AnimalData { name = "シマリス", relatedTopic = "シマリスは背中に5本の縞模様があることが特徴です。冬眠する前に大量の食料を貯蔵する習性があります。" },
                new AnimalData { name = "プレーリードッグ", relatedTopic = "プレーリードッグは大きな群れで生活し、複雑なトンネルシステムを構築します。その鳴き声が犬に似ていることから名付けられました。" },
                new AnimalData { name = "スカンク", relatedTopic = "スカンクは危険を感じると強烈な臭いを放つことで知られています。その臭いは数キロメートル先まで届くと言われています。" },
                new AnimalData { name = "テン", relatedTopic = "テンは日本の森に生息するイタチの仲間です。冬になると毛色が黄色から白に変わる個体がおり、特に珍重されています。" },
                new AnimalData { name = "イタチ", relatedTopic = "イタチは機敏な動きで獲物を捕らえます。日本では「鼬の最後っ屁」という言葉があるように、追い詰められると臭いを放つことがあります。" },
                new AnimalData { name = "オコジョ", relatedTopic = "オコジョは冬になると真っ白な毛に変わることで知られています。その姿は雪の中に溶け込み、獲物に気づかれにくくなります。" },
                new AnimalData { name = "マーモット", relatedTopic = "マーモットは高山地帯に生息し、冬眠する習性があります。その鳴き声は警告の合図として使われ、仲間を危険から守ります。" },
                new AnimalData { name = "ビーバー", relatedTopic = "ビーバーはダムを作ることで知られています。その歯は非常に強く、大きな木を倒すことができます。" },
                new AnimalData { name = "カピバラ", relatedTopic = "カピバラは世界最大のげっ歯類です。非常に温厚な性格で、他の動物と一緒にいることが多く、動物園でも人気があります。" },
                new AnimalData { name = "ヤマネ", relatedTopic = "ヤマネは日本の固有種で、冬眠する習性があります。その姿は非常に小さく、手のひらに乗るほどです。" },
                new AnimalData { name = "トガリネズミ", relatedTopic = "トガリネズミは世界最小の哺乳類の一つです。その名の通り、とがった鼻が特徴的で、1日に自分の体重の3倍もの餌を食べます。" },
                new AnimalData { name = "コウモリ", relatedTopic = "コウモリは唯一空を飛べる哺乳類です。超音波を使って獲物を探し、暗闇でも自由に動き回ることができます。" },
                new AnimalData { name = "モグラ", relatedTopic = "モグラは地中にトンネルを掘って生活します。その前足は非常に発達しており、土を素早く掘ることができます。" },
                new AnimalData { name = "ハリネズミ", relatedTopic = "ハリネズミは背中に無数の針を持っています。危険を感じると体を丸めて、針で身を守ります。" },
                new AnimalData { name = "アルマジロ", relatedTopic = "アルマジロは硬い甲羅で身を守ります。危険を感じると体を丸めて、完全に防御態勢を取ります。" },
                new AnimalData { name = "ポッサム", relatedTopic = "ポッサムはオーストラリアに生息する有袋類です。危険を感じると死んだふりをすることがあり、その演技は非常に巧妙です。" },
                new AnimalData { name = "フクロモモンガ", relatedTopic = "フクロモモンガは皮膜を広げて滑空します。その姿は非常に愛らしく、ペットとしても人気があります。" },
                new AnimalData { name = "フクロネコ", relatedTopic = "フクロネコはオーストラリアに生息する小型の有袋類です。その名の通り、ネコのような見た目をしています。" },
                new AnimalData { name = "タヌキ", relatedTopic = "タヌキは日本の民話に多く登場します。「タヌキ寝入り」という言葉があるように、死んだふりが上手いことで知られています。" },
                new AnimalData { name = "キツネ", relatedTopic = "キツネは日本の民話で「狐憑き」や「狐火」など、神秘的な存在として描かれることが多いです。その美しい毛皮は古くから珍重されました。" },
                new AnimalData { name = "アナグマ", relatedTopic = "アナグマは地中に複雑な巣穴を掘ります。その巣は何世代にもわたって使われ、非常に広大になることがあります。" },
                new AnimalData { name = "ラッコ", relatedTopic = "ラッコは海に住む唯一のイタチの仲間です。お腹の上で石を使って貝を割る習性があり、非常に器用です。" },
                new AnimalData { name = "ミンク", relatedTopic = "ミンクは水辺に生息するイタチの仲間です。その毛皮は非常に高級で、コートなどに使われてきました。" },
                new AnimalData { name = "カワウソ", relatedTopic = "カワウソは水辺で生活し、魚を捕らえて食べます。その姿は非常に愛らしく、遊び好きな性格でも知られています。" },
                new AnimalData { name = "ニホンカモシカ", relatedTopic = "ニホンカモシカは日本の特別天然記念物です。その角は非常に強く、危険を感じると突進してきます。" },
                new AnimalData { name = "ヤマアラシ", relatedTopic = "ヤマアラシは背中に無数の針を持っています。その針は非常に鋭く、敵を撃退するのに使われます。" },
                new AnimalData { name = "パカ", relatedTopic = "パカは南米に生息するげっ歯類です。その鳴き声は「パカパカ」と聞こえ、それが名前の由来となっています。" },
                new AnimalData { name = "アグーチ", relatedTopic = "アグーチは中南米に生息する大型のげっ歯類です。その肉は食用とされ、現地では重要な食料源となっています。" },
                new AnimalData { name = "ヌートリア", relatedTopic = "ヌートリアは南米原産の大型のげっ歯類です。その毛皮は「ヌートリア」として知られ、コートなどに使われます。" },
                new AnimalData { name = "マスクラット", relatedTopic = "マスクラットは水辺に生息し、その名の通りマスクのような臭いを放ちます。その毛皮は高級で、帽子などに使われます。" },
                new AnimalData { name = "レミング", relatedTopic = "レミングは集団で移動する習性があります。その行動は時に大規模な移動となり、海に飛び込むこともあると言われています。" },
                new AnimalData { name = "ジリス", relatedTopic = "ジリスは北米の草原に生息します。その姿はリスに似ていますが、より地面で生活する習性があります。" },
                new AnimalData { name = "チップマンク", relatedTopic = "チップマンクは北米に生息する小型のリスです。その頬袋は非常に大きく、大量の食料を運ぶことができます。" },
                new AnimalData { name = "フクロウサギ", relatedTopic = "フクロウサギはオーストラリアに生息する有袋類です。その名の通り、ウサギのような見た目をしていますが、実際にはカンガルーの仲間です。" },
                new AnimalData { name = "バンディクート", relatedTopic = "バンディクートはオーストラリアに生息する有袋類です。その長い鼻は非常に敏感で、地中の虫を探すのに使われます。" },
                new AnimalData { name = "ビルビー", relatedTopic = "ビルビーはオーストラリアに生息する有袋類です。その長い耳はウサギに似ており、夜行性の動物です。" },
                new AnimalData { name = "クォッカ", relatedTopic = "クォッカはオーストラリアに生息する小型のカンガルーです。その笑顔のような表情から「世界一幸せな動物」と呼ばれています。" },
                new AnimalData { name = "ワラビー", relatedTopic = "ワラビーはオーストラリアに生息する小型のカンガルーです。その姿はカンガルーに似ていますが、より小型です。" },
                new AnimalData { name = "ポトルー", relatedTopic = "ポトルーはオーストラリアに生息する小型の有袋類です。その長い尾はバランスを取るのに使われます。" },
                new AnimalData { name = "フクロギツネ", relatedTopic = "フクロギツネはオーストラリアに生息する有袋類です。その名の通り、キツネのような見た目をしています。" },
                new AnimalData { name = "フクロネズミ", relatedTopic = "フクロネズミはオーストラリアに生息する有袋類です。その姿はネズミに似ていますが、実際には有袋類です。" },
                new AnimalData { name = "フクロアリクイ", relatedTopic = "フクロアリクイはオーストラリアに生息する有袋類です。その長い舌はアリを捕らえるのに使われます。" }
            };
        }

        /// <summary>
        /// ランダムな動物名を取得
        /// </summary>
        /// <returns>ランダムに選ばれた動物名</returns>
        public static string GetRandomAnimalName()
        {
            InitializeAnimals();
            
            if (animals == null || animals.Count == 0)
            {
                return "ウサギ"; // フォールバック
            }

            int index = random.Next(animals.Count);
            return animals[index].name;
        }

        /// <summary>
        /// 指定された動物名にゆかりのある話題を取得
        /// </summary>
        /// <param name="animalName">動物名</param>
        /// <returns>その動物にゆかりのある話題。見つからない場合はデフォルトの話題を返す</returns>
        public static string GetRelatedTopic(string animalName)
        {
            InitializeAnimals();
            
            if (animals == null || animals.Count == 0)
            {
                return "この動物について、もっと調べてみると面白い発見があるかもしれません。";
            }

            var animal = animals.FirstOrDefault(a => a.name == animalName);
            if (animal != null)
            {
                return animal.relatedTopic;
            }

            // 見つからない場合はデフォルトの話題
            return $"{animalName}について、もっと調べてみると面白い発見があるかもしれません。";
        }

        /// <summary>
        /// すべての動物名リストを取得（デバッグ用）
        /// </summary>
        /// <returns>動物名のリスト</returns>
        public static List<string> GetAllAnimalNames()
        {
            InitializeAnimals();
            return animals.Select(a => a.name).ToList();
        }
    }
}

