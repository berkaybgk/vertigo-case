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

        public void ShowGameOver()
        {
            _ui_text_claim_title_value.text = "All Rewards Lost!";
            PopulateRewardRows(new List<CollectedReward>()); // empty
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private void PopulateRewardRows(IReadOnlyList<CollectedReward> rewards)
        {
            // Clear previous rows
            foreach (var row in _spawnedRows)
                if (row != null) Destroy(row.gameObject);
            _spawnedRows.Clear();

            RewardData cashRewardData = null;
            RewardData goldRewardData = null;
            int totalCash = 0;
            int totalGold = 0;

            foreach (var reward in rewards)
            {
                if (reward.RewardData != null && reward.RewardData.Type == RewardType.Currency)
                {
                    if (string.Equals(reward.RewardData.RewardName, "Cash", System.StringComparison.OrdinalIgnoreCase))
                    {
                        cashRewardData = reward.RewardData;
                        totalCash += reward.Amount;
                    }
                    else if (string.Equals(reward.RewardData.RewardName, "Gold", System.StringComparison.OrdinalIgnoreCase))
                    {
                        goldRewardData = reward.RewardData;
                        totalGold += reward.Amount;
                    }
                }
            }

            // Spawn summary row for Cash if totalCash > 0
            if (totalCash > 0 && cashRewardData != null)
            {
                var row = Instantiate(_rewardItemPrefab, _ui_container_claim_rewards);
                row.Initialize(new CollectedReward(cashRewardData, totalCash, 0), isLarge: true, showBackground: true);
                _spawnedRows.Add(row);
            }

            // Spawn summary row for Gold if totalGold > 0
            if (totalGold > 0 && goldRewardData != null)
            {
                var row = Instantiate(_rewardItemPrefab, _ui_container_claim_rewards);
                row.Initialize(new CollectedReward(goldRewardData, totalGold, 0), isLarge: true, showBackground: true);
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
        }
#endif
    }
}
