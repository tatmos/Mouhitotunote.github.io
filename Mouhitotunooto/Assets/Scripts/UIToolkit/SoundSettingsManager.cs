using UnityEngine;
using UnityEngine.UIElements;

namespace NovelGame
{
    public class SoundSettingsManager
    {
        private VisualElement overlay;
        private Slider bgmSlider;
        private Label bgmValueLabel;
        private Slider seSlider;
        private Label seValueLabel;
        private Button closeButton;

        private System.Action onHoverSound;

        public SoundSettingsManager(VisualElement root, VisualTreeAsset uxml, System.Action onHoverSoundCallback = null)
        {
            onHoverSound = onHoverSoundCallback;
            Initialize(root, uxml);
        }

        private void Initialize(VisualElement root, VisualTreeAsset uxml)
        {
            if (uxml == null)
            {
                Debug.LogError("SoundSettingsManager: UXML is null! Please assign SoundSettingsPanel.uxml in the Inspector.");
                return;
            }

            var template = uxml.CloneTree();
            overlay = template.Q<VisualElement>("SoundSettingsOverlay");

            // もし名前で見つからない場合は最初の要素を試す
            if (overlay == null && template.childCount > 0)
            {
                overlay = template.ElementAt(0);
                Debug.Log($"[SoundSettingsManager] SoundSettingsOverlay not found by name, using first child: {overlay.name}");
            }
            
            if (overlay == null)
            {
                Debug.LogError("[SoundSettingsManager] SoundSettingsOverlay not found in UXML template");
                return;
            }

            // TemplateContainerから切り離してrootに直接追加
            overlay.RemoveFromHierarchy();
            root.Add(overlay);
            
            // overlayの初期スタイルを確認
            bgmSlider = overlay.Q<Slider>("BGMSlider");
            bgmValueLabel = overlay.Q<Label>("BGMValueLabel");
            seSlider = overlay.Q<Slider>("SESlider");
            seValueLabel = overlay.Q<Label>("SEValueLabel");
            closeButton = overlay.Q<Button>("CloseButton");

            var audioManager = AudioManager.Instance;

            if (bgmSlider != null)
            {
                float currentVolume = audioManager != null ? audioManager.GetBGMVolume() : 1.0f;
                bgmSlider.value = currentVolume;
                if (bgmValueLabel != null) bgmValueLabel.text = $"{Mathf.RoundToInt(currentVolume * 100)}%";

                bgmSlider.RegisterValueChangedCallback(evt =>
                {
                    if (audioManager != null) audioManager.SetBGMVolume(evt.newValue);
                    if (bgmValueLabel != null) bgmValueLabel.text = $"{Mathf.RoundToInt(evt.newValue * 100)}%";
                });
            }

            if (seSlider != null)
            {
                float currentVolume = audioManager != null ? audioManager.GetSEVolume() : 1.0f;
                seSlider.value = currentVolume;
                if (seValueLabel != null) seValueLabel.text = $"{Mathf.RoundToInt(currentVolume * 100)}%";

                seSlider.RegisterValueChangedCallback(evt =>
                {
                    if (audioManager != null) audioManager.SetSEVolume(evt.newValue);
                    if (seValueLabel != null) seValueLabel.text = $"{Mathf.RoundToInt(evt.newValue * 100)}%";
                });
            }

            if (closeButton != null)
            {
                closeButton.clicked += Hide;
                closeButton.RegisterCallback<PointerEnterEvent>(evt => onHoverSound?.Invoke());
            }

            // 背景クリックで閉じる
            overlay.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target == overlay)
                {
                    Hide();
                }
            });
        }

        public void Show(VisualElement root = null)
        {
            if (overlay != null)
            {
                // rootが指定されており、現在の親と異なる場合は再アタッチ
                if (root != null && overlay.parent != root)
                {
                    overlay.RemoveFromHierarchy();
                    root.Add(overlay);
                }

                overlay.style.display = DisplayStyle.Flex;
                overlay.BringToFront(); 
                
                // 表示時に最新の音量を反映
                var audioManager = AudioManager.Instance;
                if (audioManager != null)
                {
                    if (bgmSlider != null)
                    {
                        bgmSlider.value = audioManager.GetBGMVolume();
                        if (bgmValueLabel != null) bgmValueLabel.text = $"{Mathf.RoundToInt(bgmSlider.value * 100)}%";
                    }
                    if (seSlider != null)
                    {
                        seSlider.value = audioManager.GetSEVolume();
                        if (seValueLabel != null) seValueLabel.text = $"{Mathf.RoundToInt(seSlider.value * 100)}%";
                    }
                }
            }
        }

        public void Hide()
        {
            if (overlay != null)
            {
                overlay.style.display = DisplayStyle.None;
            }
        }

        public bool IsVisible()
        {
            return overlay != null && overlay.style.display == DisplayStyle.Flex;
        }
    }
}
