using UnityEngine;
using UnityEngine.UI;

namespace NovelGame
{
    /// <summary>
    /// 選択画面のレイアウトを自動的に設定するヘルパークラス
    /// </summary>
    public class SelectionScreenLayoutHelper : MonoBehaviour
    {
        [ContextMenu("Setup Selection Screen Layout")]
        public void SetupLayout()
        {
            // 選択画面全体にVerticalLayoutGroupを追加
            var selectionScreen = transform;
            var verticalLayout = selectionScreen.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout == null)
            {
                verticalLayout = selectionScreen.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            verticalLayout.spacing = 20f;
            verticalLayout.padding = new RectOffset(20, 20, 20, 20);
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childAlignment = TextAnchor.UpperCenter;

            // ContentSizeFitterを追加
            var contentSizeFitter = selectionScreen.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                contentSizeFitter = selectionScreen.gameObject.AddComponent<ContentSizeFitter>();
            }
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            Debug.Log("選択画面のレイアウトを設定しました。");
        }
    }
}

