using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Data;

namespace VertigoCase.Wheel
{
    // Visual for a single wheel slice. Spawned and positioned by WheelController at runtime.
    public class WheelSliceUI : MonoBehaviour
    {
        [SerializeField] private Image    _ui_image_slice_icon;
        [SerializeField] private TMP_Text _ui_text_slice_amount_value;

        [Header("Layout Settings")]
        [SerializeField] private Vector2 _itemIconPosition = new Vector2(0f, 0f);
        [SerializeField] private Vector2 _itemIconSize = new Vector2(40f, 40f);
        [SerializeField] private Vector2 _itemTextPosition = new Vector2(0f, -55f);
        [SerializeField] private Vector2 _bombIconPosition = new Vector2(0f, 180f);
        [SerializeField] private Vector2 _bombIconSize = new Vector2(70f, 70f);

        public RectTransform RectTransform { get; private set; }
        private RectTransform _iconRt;
        private RectTransform _textRt;
        private LayoutGroup _layoutGroup;

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            _layoutGroup = GetComponent<LayoutGroup>();

            if (_ui_image_slice_icon != null)
            {
                _ui_image_slice_icon.preserveAspect = true;
                _iconRt = _ui_image_slice_icon.GetComponent<RectTransform>();
            }

            if (_ui_text_slice_amount_value != null)
            {
                _textRt = _ui_text_slice_amount_value.GetComponent<RectTransform>();
                _textRt.anchoredPosition = _itemTextPosition;
                _ui_text_slice_amount_value.alignment = TextAlignmentOptions.Center;
            }
        }

        // ── Public API ─────────────────────────────────────────────────────────

        // Configures the slice visual from data.
        public void Initialize(WheelSliceData sliceData, Sprite bombSprite, float rewardMultiplier = 1f)
        {
            // Disable LayoutGroup on the slice if it is a bomb, so custom positioning can work
            if (_layoutGroup != null)
            {
                _layoutGroup.enabled = !sliceData.IsBomb;
            }

            if (_iconRt != null)
            {
                _iconRt.anchorMin = new Vector2(0.5f, 0.5f);
                _iconRt.anchorMax = new Vector2(0.5f, 0.5f);
                _iconRt.pivot = new Vector2(0.5f, 0.5f);
            }

            if (sliceData.IsBomb)
            {
                _ui_image_slice_icon.sprite = bombSprite;
                _ui_text_slice_amount_value.gameObject.SetActive(false);

                if (_iconRt != null)
                {
                    _iconRt.anchoredPosition = Vector2.zero; // Perfect center alignment with parent ui_slice_x
                    _iconRt.sizeDelta = _bombIconSize;
                }
            }
            else
            {
                _ui_image_slice_icon.sprite = sliceData.Reward != null ? sliceData.Reward.Icon : null;
                int displayedAmount = Mathf.RoundToInt(sliceData.Amount * rewardMultiplier);
                _ui_text_slice_amount_value.text = "x" + displayedAmount.ToString();
                _ui_text_slice_amount_value.gameObject.SetActive(true);

                if (_iconRt != null)
                {
                    _iconRt.anchoredPosition = _itemIconPosition;
                    _iconRt.sizeDelta = _itemIconSize;
                }
            }

            // Ensure color is white to prevent icon discoloration
            _ui_image_slice_icon.color = Color.white;
        }
        // ── Editor auto-binding ────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Direct-child lookup only — fast O(n) on immediate children.
            if (_ui_image_slice_icon == null)
            {
                var t = transform.Find("ui_image_slice_icon");
                if (t != null) _ui_image_slice_icon = t.GetComponent<Image>();
                else Debug.LogWarning("[WheelSliceUI] 'ui_image_slice_icon' child not found.", this);
            }

            if (_ui_text_slice_amount_value == null)
            {
                var t = transform.Find("ui_text_slice_amount_value");
                if (t != null) _ui_text_slice_amount_value = t.GetComponent<TMP_Text>();
                else Debug.LogWarning("[WheelSliceUI] 'ui_text_slice_amount_value' child not found.", this);
            }
        }
#endif
    }
}
