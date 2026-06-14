using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Core;

namespace VertigoCase.UI
{
    // Controls the wheel area panel.
    public class WheelPanel : MonoBehaviour
    {
        // ── Inspector references ───────────────────────────────────────────────
        [Header("Buttons (assign in Inspector)")]
        [SerializeField] private Button _ui_button_spin;
        [SerializeField] private Button _ui_button_walkaway;

        [Header("Wheel Animation Container")]
        [Tooltip("Child transform that holds the Animator component — NOT this root.")]
        [SerializeField] private RectTransform _ui_container_wheel_anim;

        private CanvasGroup _canvasGroup;
        private GameState _currentState = GameState.Idle;
        private bool _isFirstZone = true;

        // ── Initialisation (called by UIManager) ───────────────────────────────

        public void Initialize(Action onSpinClicked, Action onWalkAwayClicked)
        {
            // Spec: all listeners added via code — no Inspector OnClick
            // Clear any Inspector or previous listeners to prevent incorrect routing
            _ui_button_spin.onClick.RemoveAllListeners();
            _ui_button_walkaway.onClick.RemoveAllListeners();

            _ui_button_spin.onClick.AddListener(() => onSpinClicked());
            _ui_button_walkaway.onClick.AddListener(() => onWalkAwayClicked());

            _canvasGroup = GetComponent<CanvasGroup>();
        }

        // ── State reactions ────────────────────────────────────────────────────

        public void OnStateChanged(GameState previous, GameState next)
        {
            _currentState = next;
            UpdateButtonStates();
        }

        public void SetFirstZone(bool isFirstZone)
        {
            _isFirstZone = isFirstZone;
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            // Buttons are only interactive when the player is idle
            bool idle = _currentState == GameState.Idle;
            _ui_button_spin.interactable     = idle;
            _ui_button_walkaway.interactable = idle && !_isFirstZone;
        }

        // ── Editor auto-binding (non-recursive, direct children only) ──────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Check if null OR if the wrong button is assigned to the field (swapped references)
            if (_ui_button_spin == null || _ui_button_spin.name != "ui_button_spin")
            {
                var t = transform.Find("ui_button_spin");
                if (t != null) _ui_button_spin = t.GetComponent<Button>();
                else if (_ui_button_spin == null) Debug.LogWarning("[WheelPanel] 'ui_button_spin' child not found.", this);
            }

            if (_ui_button_walkaway == null || _ui_button_walkaway.name != "ui_button_walkaway")
            {
                var t = transform.Find("ui_button_walkaway");
                if (t != null) _ui_button_walkaway = t.GetComponent<Button>();
                else if (_ui_button_walkaway == null) Debug.LogWarning("[WheelPanel] 'ui_button_walkaway' child not found.", this);
            }

            if (_ui_container_wheel_anim == null)
            {
                var t = transform.Find("ui_container_wheel_anim");
                if (t != null) _ui_container_wheel_anim = t as RectTransform;
                else Debug.LogWarning("[WheelPanel] 'ui_container_wheel_anim' child not found. " +
                                      "Remember: Animator must NOT be on the root transform.", this);
            }
        }
#endif
    }
}
