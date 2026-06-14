using TMPro;
using UnityEngine;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    // Visual representation of a single zone cell in the zone progress bar.
    public class ZoneIndicatorItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _ui_text_zone_number_value;

        [Header("Text Highlight Colors")]
        [SerializeField] private Color _textColorStandard = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color _textColorSafe = new Color(0.2f, 0.8f, 0.4f); // Emerald green
        [SerializeField] private Color _textColorSuper = new Color(1f, 0.8f, 0.1f); // Gold/Yellow
        [SerializeField] private Color _textColorCurrent = Color.white;

        private CanvasGroup _canvasGroup;

        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void SetEmpty()
        {
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
        }

        public void SetZone(int zoneNumber, ZoneType zoneType, bool isCurrent)
        {
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;

            if (_ui_text_zone_number_value == null)
            {
                _ui_text_zone_number_value = GetComponentInChildren<TMP_Text>();
                if (_ui_text_zone_number_value == null)
                {
                    Debug.LogError($"[ZoneIndicatorItemUI] _ui_text_zone_number_value is not assigned and could not be found on child elements.", this);
                    return;
                }
            }

            _ui_text_zone_number_value.text = zoneNumber.ToString();

            if (isCurrent)
            {
                _ui_text_zone_number_value.color = _textColorCurrent;
            }
            else
            {
                _ui_text_zone_number_value.color = zoneType switch
                {
                    ZoneType.Safe  => _textColorSafe,
                    ZoneType.Super => _textColorSuper,
                    _              => _textColorStandard
                };
            }
        }

        // ── Editor auto-binding ────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_ui_text_zone_number_value == null)
            {
                var t = transform.Find("ui_text_zone_number_value");
                if (t != null) _ui_text_zone_number_value = t.GetComponent<TMP_Text>();
                else _ui_text_zone_number_value = GetComponentInChildren<TMP_Text>(); // Fallback to any child TMP text
            }
        }
#endif
    }
}
