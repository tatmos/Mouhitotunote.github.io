using System.Collections.Generic;
using UnityEngine;

namespace NovelGame
{
    public static class CharacterProfileManager
    {
        private static Dictionary<int, CharacterProfile> profiles = new Dictionary<int, CharacterProfile>();

        static CharacterProfileManager()
        {
            InitializeProfiles();
        }

        private static void InitializeProfiles()
        {
            // シナリオ1: もも子
            profiles[1] = new CharacterProfile
            {
                scenarioId = 1,
                name = "田中 もも子",
                role = "依頼人",
                job = "会社員",
                feature = "10年前に失踪した人物の写真を持参。探偵に「もうひとつ」の手がかりを求めている。写真の隅に何か隠されているかもしれない。",
                featureDarkMode = "バグによって「も」という文字を失った。写真は歪み、人物の姿が消えている。データの欠片となって崩壊しつつある。",
                quote = "「もっと深く調べてほしいの...」",
                quoteDarkMode = "「も...も...もど...」【エラー】",
                relationshipWithVoice = "もも子さんが探していた「もうひとつ」は、実は謎の声が導いた道標でした。謎の声は、もも子さんに真実を見つける力を与えた存在です。",
                bugDescription = "もも子のデータは破損しています。「も」という文字が消失し、存在が不安定になっています。シミュレーションの整合性が崩壊しています。",
                profileColor = new Color(1f, 0.9f, 0.9f), // ピンク系
                borderColor = new Color(1f, 0.7f, 0.8f)
            };

            // シナリオ2: うみ
            profiles[2] = new CharacterProfile
            {
                scenarioId = 2,
                name = "海原 うみ",
                role = "シェフ",
                job = "レストランシェフ",
                feature = "ユーモア好きで、時々「きまぐれ」なメニューを提供。空の皿を出すこともあるが、その裏には何か意味があるかもしれない。",
                featureDarkMode = "バグによって「う」という文字を失った。皿は空のまま、料理は生成されない。ユーモアは歪み、恐怖に変わっている。",
                quote = "「お客様には、もうひとつの楽しみを...」",
                quoteDarkMode = "「う...う...うま...」【エラー】",
                relationshipWithVoice = "うみシェフの「きまぐれ」は、謎の声からのメッセージでした。謎の声は、うみシェフを通じて「もうひとつ」の楽しみを伝えようとしていたのです。",
                bugDescription = "うみのデータは破損しています。「う」という文字が消失し、皿からは何も生成されません。シミュレーションの物理法則が崩壊しています。",
                profileColor = new Color(0.9f, 0.95f, 1f), // 青系
                borderColor = new Color(0.7f, 0.85f, 1f)
            };

            // シナリオ3: ひろ
            profiles[3] = new CharacterProfile
            {
                scenarioId = 3,
                name = "広瀬 ひろ",
                role = "幼馴染",
                job = "フリーランス",
                feature = "20年前に一緒にタイムカプセルを埋めた親友。実は、あなたが知らない「もうひとつ」の手紙を入れていた。手紙の裏には何か書かれているかもしれない。",
                featureDarkMode = "バグによって「ひ」という文字を失った。タイムカプセルは空で、手紙は文字化けしている。友情の記憶が消えつつある。",
                quote = "「実は、もうひとつ秘密があるんだ...」",
                quoteDarkMode = "「ひ...ひ...ひろ...」【エラー】",
                relationshipWithVoice = "ひろがタイムカプセルに入れた「もうひとつ」の手紙は、謎の声からの導きでした。謎の声は、友情を通じて「もうひとつ」の価値を教えてくれた存在です。",
                bugDescription = "ひろのデータは破損しています。「ひ」という文字が消失し、タイムカプセルの内容が空になっています。記憶の整合性が崩壊しています。",
                profileColor = new Color(1f, 1f, 0.9f), // 黄色系
                borderColor = new Color(1f, 0.95f, 0.7f)
            };

            // シナリオ4: とおる
            profiles[4] = new CharacterProfile
            {
                scenarioId = 4,
                name = "遠藤 とおる",
                role = "試験官",
                job = "魔法学校の試験官",
                feature = "厳格だが、予想外の結果にも柔軟に対応する。ウサギが2羽出現した時、「もうひとつ」消すよう指示。消えた跡には何かが残るかもしれない。",
                featureDarkMode = "バグによって「と」という文字を失った。ウサギは無限に増殖し、消すことができない。魔法の法則が崩壊している。",
                quote = "「もうひとつ、試してみなさい」",
                quoteDarkMode = "「と...と...とま...」【エラー】",
                relationshipWithVoice = "とおる試験官の「もうひとつ」という指示は、謎の声からの試練でした。謎の声は、とおる試験官を通じて、成長のための「もうひとつ」の道を示していたのです。",
                bugDescription = "とおるのデータは破損しています。「と」という文字が消失し、ウサギが無限増殖しています。シミュレーションの物理法則が完全に崩壊しています。",
                profileColor = new Color(0.95f, 0.9f, 1f), // 紫系
                borderColor = new Color(0.85f, 0.7f, 1f)
            };

            // シナリオ5: つばさ
            profiles[5] = new CharacterProfile
            {
                scenarioId = 5,
                name = "月島 つばさ",
                role = "恋人",
                job = "デザイナー",
                feature = "優しく気配り上手。パズルが完成することを見越して、「もうひとつ」のパズルを用意していた。箱の蓋の内側に何かメッセージがあるかもしれない。",
                featureDarkMode = "バグによって「つ」という文字を失った。パズルのピースは永遠に足りず、完成することはない。愛の記憶が消えつつある。",
                quote = "「君のために、もうひとつ準備してたんだ」",
                quoteDarkMode = "「つ...つ...つば...」【エラー】",
                relationshipWithVoice = "つばさが用意した「もうひとつ」のパズルは、謎の声からの贈り物でした。謎の声は、つばさを通じて、愛と気配りの「もうひとつ」を教えてくれた存在です。",
                bugDescription = "つばさのデータは破損しています。「つ」という文字が消失し、パズルは永遠に完成しません。愛のデータが欠損しています。",
                profileColor = new Color(0.9f, 1f, 0.9f), // 緑系
                borderColor = new Color(0.7f, 1f, 0.7f)
            };

            // シナリオ6: 謎の声
            profiles[6] = new CharacterProfile
            {
                scenarioId = 6,
                name = "謎の声",
                role = "真実の扉の守護者",
                job = "",
                feature = "5つの「もうひとつ」を集めた者に、真実を伝える存在。すべての登場人物との間に深い繋がりがある。",
                featureDarkMode = "",
                quote = "",
                quoteDarkMode = "",
                relationshipWithVoice = "",
                bugDescription = "",
                profileColor = new Color(0.9f, 0.9f, 1f), // インディゴ系
                borderColor = new Color(0.7f, 0.7f, 1f)
            };
        }

        public static CharacterProfile GetProfile(int scenarioId)
        {
            return profiles.ContainsKey(scenarioId) ? profiles[scenarioId] : null;
        }
    }
}

