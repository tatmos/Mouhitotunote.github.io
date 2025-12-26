using UnityEngine;

namespace NovelGame
{
    /// <summary>
    /// ゲームの初期化を行うコンポーネント
    /// シーンに1つ配置してください
    /// </summary>
    public class NovelGameInitializer : MonoBehaviour
    {
        private void Awake()
        {
            // GameManagerを探すか、作成する
            if (GameManager.Instance == null)
            {
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<GameManager>();
            }

            // ScenarioDataLoaderを探すか、作成する
            if (FindObjectOfType<ScenarioDataLoader>() == null)
            {
                GameObject dataLoaderObj = new GameObject("ScenarioDataLoader");
                dataLoaderObj.AddComponent<ScenarioDataLoader>();
            }
        }
    }
}


