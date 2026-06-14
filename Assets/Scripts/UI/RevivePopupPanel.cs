using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VertigoCase.UI
{
    // Overlay panel displayed when the player hits a bomb.
    public class RevivePopupPanel : MonoBehaviour
    {
        // ── Inspector references ───────────────────────────────────────────────
        [Header("UI Elements (assign in Inspector)")]
        [SerializeField] private Image    _ui_image_revive_bomb_icon;
        [SerializeField] private TMP_Text _ui_text_revive_cost_value;
        [SerializeField] private Button   _ui_button_revive;
        [SerializeField] private Button   _ui_button_giveup;
        [SerializeField] private TMP_Text _ui_text_revive_button_cost_value;

        [Header("Animation Container")]
        [Tooltip("Child transform holding the Animator — must NOT be the root.")]
        [SerializeField] private RectTransform _ui_container_revive_anim;

        [Header("Animation Settings")]
        [SerializeField] private float _showDuration = 0.35f;
        [SerializeField] private float _hideDuration = 0.2f;

        private CanvasGroup _canvasGroup;

        // ── Initialisation (called by UIManager) ───────────────────────────────

        public void Initialize(Action onReviveClicked, Action onGiveUpClicked)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Start hidden
            _canvasGroup.alpha = 0f;
            _canvasGroup.SetInteractable(false);
            gameObject.SetActive(false);

            // Spec: listeners via code only
            _ui_button_revive.onClick.AddListener(() => onReviveClicked());
            _ui_button_giveup.onClick.AddListener(() => onGiveUpClicked());
        }

        // ── Public API ─────────────────────────────────────────────────────────

        private bool _isVisible;

        public void SetVisible(bool visible)
        {
            if (_isVisible == visible) return;
            _isVisible = visible;

            _canvasGroup.DOKill();
            _ui_container_revive_anim.DOKill();

            if (visible) Show();
            else Hide();
        }

        // Provides visual feedback when the player cannot afford a revive.
        public void ShakeReviveButton()
        {
            _ui_button_revive.transform
                .DOShakePosition(0.4f, strength: 8f, vibrato: 20, randomness: 0f);
        }

        public void UpdateReviveCost(int gold)
        {
            if (_ui_text_revive_cost_value != null)
            {
                _ui_text_revive_cost_value.text = gold.ToString();
            }

            if (_ui_text_revive_button_cost_value != null)
            {
                _ui_text_revive_button_cost_value.text = gold.ToString();
            }
        }

        // ── Animation ──────────────────────────────────────────────────────────

        private void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.SetInteractable(false);

            // Popup scale + fade
            _ui_container_revive_anim.localScale = Vector3.one * 0.7f;

            Sequence seq = DOTween.Sequence();
            seq.Append(_canvasGroup.DOFade(1f, _showDuration).SetEase(Ease.OutCubic));
            seq.Join(_ui_container_revive_anim.DOScale(Vector3.one, _showDuration).SetEase(Ease.OutBack));
            seq.OnComplete(() => _canvasGroup.SetInteractable(true));
        }

        private void Hide()
        {
            _canvasGroup.SetInteractable(false);

            _canvasGroup
                .DOFade(0f, _hideDuration)
                .SetEase(Ease.InCubic)
                .OnComplete(() => gameObject.SetActive(false));
        }

        // ── Editor auto-binding (direct children of anim container) ───────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_ui_container_revive_anim == null)
            {
                var t = transform.Find("ui_container_revive_anim");
                if (t != null) _ui_container_revive_anim = t as RectTransform;
                else Debug.LogWarning("[RevivePopupPanel] 'ui_container_revive_anim' child not found.", this);
            }

            var container = _ui_container_revive_anim != null ? _ui_container_revive_anim : transform;

            if (_ui_button_revive == null)
            {
                var t = container.Find("ui_button_revive");
                if (t != null) _ui_button_revive = t.GetComponent<Button>();
            }

            if (_ui_button_giveup == null)
            {
                var t = container.Find("ui_button_giveup");
                if (t != null) _ui_button_giveup = t.GetComponent<Button>();
            }

            if (_ui_text_revive_cost_value == null)
            {
                var t = container.Find("ui_text_revive_cost_value");
                if (t != null) _ui_text_revive_cost_value = t.GetComponent<TMP_Text>();
            }

            if (_ui_image_revive_bomb_icon == null)
            {
                var t = container.Find("ui_image_revive_bomb_icon");
                if (t != null) _ui_image_revive_bomb_icon = t.GetComponent<Image>();
            }

            if (_ui_text_revive_button_cost_value == null && _ui_button_revive != null)
            {
                var t = _ui_button_revive.transform.Find("Content/Cost");
                if (t == null) t = _ui_button_revive.transform.Find("Cost");
                if (t == null)
                {
                    var tmps = _ui_button_revive.GetComponentsInChildren<TMP_Text>();
                    foreach (var tmp in tmps)
                    {
                        if (tmp.name.Contains("Cost") || tmp.name.Contains("value"))
                        {
                            _ui_text_revive_button_cost_value = tmp;
                            break;
                        }
                    }
                }
                else
                {
                    _ui_text_revive_button_cost_value = t.GetComponent<TMP_Text>();
                }
            }
        }
#endif
    }
}
