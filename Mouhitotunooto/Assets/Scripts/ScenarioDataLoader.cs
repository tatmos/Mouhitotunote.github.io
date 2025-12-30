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
            // ScenarioDefinitionsを使用してシナリオデータを取得
            return ScenarioDefinitions.CreateScenarios();
        }
    }
}

