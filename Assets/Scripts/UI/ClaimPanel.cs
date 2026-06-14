using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    // Walk-away / Game-Over summary screen.
    public class ClaimPanel : MonoBehaviour
    {
        // ── Inspector references ───────────────────────────────────────────────
        [Header("UI References (assign in Inspector)")]
        [SerializeField] private TMP_Text       _ui_text_claim_title_value;
        [SerializeField] private RectTransform  _ui_container_claim_rewards;
        [SerializeField] private Button         _ui_button_claim_confirm;
        [SerializeField] private RectTransform  _ui_container_claim_anim;

        [Header("Reward Row Prefab")]
        [SerializeField] private RewardItemUI   _rewardItemPrefab;

        [Header("Currency Reward References")]
        [Tooltip("Must match the RewardData assigned in GameManager. Used to aggregate totals on the claim screen.")]
        [SerializeField] private RewardData _cashRewardData;
        [SerializeField] private RewardData _goldRewardData;

        [Header("Animation")]
        [SerializeField] private float _showDuration = 0.4f;
        [SerializeField] private float _hideDuration = 0.25f;

        private CanvasGroup          _canvasGroup;
        private readonly List<RewardItemUI> _spawnedRows = new();

        // ── Initialisation ─────────────────────────────────────────────────────

        public void Initialize(Action onConfirmClicked)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _canvasGroup.alpha = 0f;
            _canvasGroup.SetInteractable(false);
            gameObject.SetActive(false);

            _ui_button_claim_confirm.onClick.AddListener(() => onConfirmClicked());
        }

        // ── Public API ─────────────────────────────────────────────────────────

        private bool _isVisible;

        public void SetVisible(bool visible)
        {
            if (_isVisible == visible) return;
            _isVisible = visible;

            _canvasGroup.DOKill();
            _ui_container_claim_anim.DOKill();

            if (visible) Show();
            else Hide();
        }

        public void ShowRewards(IReadOnlyList<CollectedReward> rewards)
        {
            _ui_text_claim_title_value.text = "Rewards Collected!";
            PopulateRewardRows(rewards);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private void PopulateRewardRows(IReadOnlyList<CollectedReward> rewards)
        {
            // Clear previous rows
            foreach (var row in _spawnedRows)
                if (row != null) Destroy(row.gameObject);
            _spawnedRows.Clear();

            int totalCash = 0;
            int totalGold = 0;

            // Tally totals using SO reference comparison — no magic strings.
            foreach (var reward in rewards)
            {
                if (reward.RewardData == null) continue;

                if (_cashRewardData != null && reward.RewardData == _cashRewardData)
                    totalCash += reward.Amount;
                else if (_goldRewardData != null && reward.RewardData == _goldRewardData)
                    totalGold += reward.Amount;
            }

            if (totalCash > 0 && _cashRewardData != null)
            {
                var row = Instantiate(_rewardItemPrefab, _ui_container_claim_rewards);
                row.Initialize(new CollectedReward(_cashRewardData, totalCash, 0), isLarge: true, showBackground: true);
                _spawnedRows.Add(row);
            }

            if (totalGold > 0 && _goldRewardData != null)
            {
                var row = Instantiate(_rewardItemPrefab, _ui_container_claim_rewards);
                row.Initialize(new CollectedReward(_goldRewardData, totalGold, 0), isLarge: true, showBackground: true);
                _spawnedRows.Add(row);
            }
        }

        private void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.SetInteractable(false);
            _ui_container_claim_anim.localScale = Vector3.one * 0.85f;

            Sequence seq = DOTween.Sequence();
            seq.Append(_canvasGroup.DOFade(1f, _showDuration).SetEase(Ease.OutCubic));
            seq.Join(_ui_container_claim_anim.DOScale(Vector3.one, _showDuration).SetEase(Ease.OutBack));
            seq.OnComplete(() => _canvasGroup.SetInteractable(true));
        }

        private void Hide()
        {
            _canvasGroup.SetInteractable(false);
            _canvasGroup
                .DOFade(0f, _hideDuration)
                .OnComplete(() => gameObject.SetActive(false));
        }

        // ── Editor auto-binding ────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_ui_container_claim_anim == null)
            {
                var t = transform.Find("ui_container_claim_anim");
                if (t != null) _ui_container_claim_anim = t as RectTransform;
            }

            var container = _ui_container_claim_anim != null ? _ui_container_claim_anim : transform;

            if (_ui_button_claim_confirm == null)
            {
                var t = container.Find("ui_button_claim_confirm");
                if (t != null) _ui_button_claim_confirm = t.GetComponent<Button>();
                else Debug.LogWarning("[ClaimPanel] 'ui_button_claim_confirm' not found.", this);
            }

            if (_ui_text_claim_title_value == null)
            {
                var t = container.Find("ui_text_claim_title_value");
                if (t != null) _ui_text_claim_title_value = t.GetComponent<TMP_Text>();
            }

            if (_ui_container_claim_rewards == null)
            {
                var t = container.Find("ui_container_claim_rewards");
                if (t != null) _ui_container_claim_rewards = t as RectTransform;
            }

            if (_rewardItemPrefab == null)
                Debug.LogWarning("[ClaimPanel] No RewardItemUI prefab assigned.", this);
            if (_cashRewardData == null)
                Debug.LogWarning("[ClaimPanel] Cash RewardData not assigned — cash rewards won't display on the claim screen.", this);
            if (_goldRewardData == null)
                Debug.LogWarning("[ClaimPanel] Gold RewardData not assigned — gold rewards won't display on the claim screen.", this);
        }
#endif
    }
}
