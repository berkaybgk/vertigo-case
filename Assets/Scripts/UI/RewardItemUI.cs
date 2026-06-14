using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    // Single collected-reward row displayed in the RewardContainerPanel.
    public class RewardItemUI : MonoBehaviour
    {
        [SerializeField] private Image    _ui_image_reward_icon;
        [SerializeField] private TMP_Text _ui_text_reward_amount_value;

        [Header("Dynamic Background Settings")]
        [Tooltip("Paddings for the black background to prevent it from bleeding past the rounded corners of the border.")]
        [SerializeField] private float _largeBackgroundInset = 4f;
        [SerializeField] private float _smallBackgroundInset = 2f;

        public RectTransform RectTransform { get; private set; }
        private RectTransform _iconRt;
        private RectTransform _textRt;
        private Transform _bgTransform;
        private Image _bgImage;
        private RectTransform _bgRt;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();

            if (_ui_image_reward_icon != null)
            {
                _ui_image_reward_icon.preserveAspect = true;
                _iconRt = _ui_image_reward_icon.GetComponent<RectTransform>();
            }

            if (_ui_text_reward_amount_value != null)
            {
                _textRt = _ui_text_reward_amount_value.GetComponent<RectTransform>();
            }
        }

        public void Initialize(CollectedReward reward, bool isLarge = false, bool showBackground = false)
        {
            if (showBackground)
            {
                if (_bgTransform == null)
                {
                    _bgTransform = transform.Find("ui_image_reward_background");
                    if (_bgTransform == null)
                    {
                        var bgGo = new GameObject("ui_image_reward_background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                        bgGo.transform.SetParent(transform, false);
                        _bgTransform = bgGo.transform;
                    }
                    _bgImage = _bgTransform.GetComponent<Image>();
                    _bgRt = _bgTransform as RectTransform;
                }

                _bgTransform.SetSiblingIndex(0); // Render behind other children
                _bgTransform.gameObject.SetActive(true);

                if (_bgImage != null)
                {
                    _bgImage.color = Color.black;
                }

                if (_bgRt != null)
                {
                    _bgRt.anchorMin = Vector2.zero;
                    _bgRt.anchorMax = Vector2.one;

                    // Inset to avoid corners sticking out and to fit nicely inside the border
                    float inset = isLarge ? _largeBackgroundInset : _smallBackgroundInset;
                    _bgRt.offsetMin = new Vector2(inset, inset);
                    _bgRt.offsetMax = new Vector2(-inset, -inset);
                }
            }
            else
            {
                if (_bgTransform == null)
                {
                    _bgTransform = transform.Find("ui_image_reward_background");
                }
                if (_bgTransform != null)
                {
                    _bgTransform.gameObject.SetActive(false);
                }
            }

            if (_ui_image_reward_icon != null)
            {
                _ui_image_reward_icon.gameObject.SetActive(true);
                _ui_image_reward_icon.sprite = reward.RewardData?.Icon;

                if (_iconRt != null)
                {
                    if (isLarge)
                    {
                        _iconRt.sizeDelta = new Vector2(110f, 110f);
                        _iconRt.anchoredPosition = new Vector2(0f, 20f);
                    }
                    else
                    {
                        _iconRt.sizeDelta = new Vector2(50f, 50f);
                        _iconRt.anchoredPosition = new Vector2(0f, 10f);
                    }
                }
            }

            if (_ui_text_reward_amount_value != null)
            {
                _ui_text_reward_amount_value.gameObject.SetActive(true);
                _ui_text_reward_amount_value.text = $"x{reward.Amount}";

                if (_textRt != null)
                {
                    if (isLarge)
                    {
                        _ui_text_reward_amount_value.fontSize = 28f;
                        _textRt.anchoredPosition = new Vector2(0f, 15f);
                    }
                    else
                    {
                        _ui_text_reward_amount_value.fontSize = 16f;
                        _textRt.anchoredPosition = new Vector2(0f, 6.8f);
                    }
                }
            }

            if (RectTransform != null)
            {
                float size = isLarge ? 200f : 100f;
                RectTransform.sizeDelta = new Vector2(size, size);
            }
        }

        // ── Editor auto-binding (direct children only) ─────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_ui_image_reward_icon == null)
            {
                var t = transform.Find("ui_image_reward_icon");
                if (t != null) _ui_image_reward_icon = t.GetComponent<Image>();
                else Debug.LogWarning("[RewardItemUI] 'ui_image_reward_icon' child not found.", this);
            }

            if (_ui_text_reward_amount_value == null)
            {
                var t = transform.Find("ui_text_reward_amount_value");
                if (t != null) _ui_text_reward_amount_value = t.GetComponent<TMP_Text>();
                else Debug.LogWarning("[RewardItemUI] 'ui_text_reward_amount_value' child not found.", this);
            }
        }
#endif
    }
}
