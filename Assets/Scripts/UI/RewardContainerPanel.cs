using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    // Panel that accumulates and displays all rewards collected.
    public class RewardContainerPanel : MonoBehaviour
    {
        // ── Inspector references ───────────────────────────────────────────────
        [Header("Content Container")]
        [Tooltip("The 'Content' RectTransform inside the ScrollRect.")]
        [SerializeField] private RectTransform _ui_container_reward_items;

        [Header("Prefab")]
        [SerializeField] private RewardItemUI _rewardItemPrefab;

        [Header("Animation")]
        [SerializeField] private float _spawnPunchDuration = 0.35f;
        [SerializeField] private float _destroyFadeDuration = 0.6f;

        [Header("Scrollbar Offset Adjustment")]
        [SerializeField] private float _scrollbarWidthAdjustment = 30f;

        private readonly List<RewardItemUI> _items = new();
        private CanvasGroup _canvasGroup;
        private ScrollRect  _scrollRect;
        private RectTransform _rectTransform;
        private float _initialWidth;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null)
            {
                _initialWidth = _rectTransform.sizeDelta.x;
            }

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _scrollRect = GetComponentInChildren<ScrollRect>();

            if (_ui_container_reward_items != null)
            {
                // Align to top-stretch so that content fills from top to bottom
                _ui_container_reward_items.anchorMin = new Vector2(0f, 1f);
                _ui_container_reward_items.anchorMax = new Vector2(1f, 1f);
                _ui_container_reward_items.pivot = new Vector2(0.5f, 1f);
                _ui_container_reward_items.anchoredPosition = Vector2.zero;

                var grid = _ui_container_reward_items.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    grid.childAlignment = TextAnchor.UpperLeft;
                }

                var fitter = _ui_container_reward_items.GetComponent<ContentSizeFitter>();
                if (fitter == null)
                {
                    fitter = _ui_container_reward_items.gameObject.AddComponent<ContentSizeFitter>();
                }
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
        }

        // ── Public API ─────────────────────────────────────────────────────────

        // Spawns a new reward row, animates it in, then calls onAnimationComplete.
        public void AddReward(CollectedReward reward, Action onAnimationComplete)
        {
            var item = Instantiate(_rewardItemPrefab, _ui_container_reward_items);
            item.Initialize(reward);
            _items.Add(item);

            UpdateWidthAdjustment();

            // Smoothly scroll to the bottom to highlight the new reward
            if (_scrollRect != null)
            {
                _scrollRect.DOKill();
                _scrollRect.DOVerticalNormalizedPos(0f, 0.3f).SetEase(Ease.OutCubic);
            }

            // Scale-punch animation to make the new item pop in
            var rt = item.RectTransform;
            rt.localScale = Vector3.zero;

            rt.DOScale(Vector3.one, _spawnPunchDuration)
              .SetEase(Ease.OutBack)
              .OnComplete(() => onAnimationComplete?.Invoke());
        }

        // Plays a destruction animation (fade out + shake) on all current items.
        public void PlayBombDestroyAnimation(Action onComplete)
        {
            if (_items.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            // Shake the entire container then fade out
            var rt = _ui_container_reward_items;
            Sequence seq = DOTween.Sequence();
            seq.Append(rt.DOShakePosition(0.4f, strength: 20f, vibrato: 15));
            seq.Append(_canvasGroup.DOFade(0f, _destroyFadeDuration).SetEase(Ease.InCubic));
            seq.OnComplete(() =>
            {
                Clear();
                _canvasGroup.alpha = 1f;
                onComplete?.Invoke();
            });
        }        // Destroys all reward items immediately.
        public void Clear()
        {
            foreach (var item in _items)
                if (item != null) Destroy(item.gameObject);
            _items.Clear();
            UpdateWidthAdjustment();
        }

        // Clears current rows and rebuilds them instantly from a list of rewards.
        public void RestoreRewards(IReadOnlyList<CollectedReward> rewards)
        {
            Clear();
            _canvasGroup.alpha = 1f;
            foreach (var reward in rewards)
            {
                var item = Instantiate(_rewardItemPrefab, _ui_container_reward_items);
                item.Initialize(reward);
                _items.Add(item);
            }

            UpdateWidthAdjustment();

            if (_scrollRect != null)
            {
                _scrollRect.DOKill();
                _scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private void UpdateWidthAdjustment()
        {
            if (_rectTransform == null || _scrollRect == null || _scrollRect.viewport == null) return;

            bool isScrollable = false;

            if (_items.Count > 0)
            {
                Canvas.ForceUpdateCanvases();
                if (_ui_container_reward_items != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_ui_container_reward_items);
                    float contentHeight = _ui_container_reward_items.rect.height;
                    float viewportHeight = _scrollRect.viewport.rect.height;
                    isScrollable = contentHeight > viewportHeight;
                }
            }

            float targetWidth = _initialWidth + (isScrollable ? _scrollbarWidthAdjustment : 0f);

            _rectTransform.DOKill();
            _rectTransform.DOSizeDelta(new Vector2(targetWidth, _rectTransform.sizeDelta.y), 0.25f)
                          .SetEase(Ease.OutCubic);
        }
        // ── Editor auto-binding ────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_rewardItemPrefab == null)
                Debug.LogWarning("[RewardContainerPanel] No RewardItemUI prefab assigned.", this);

            if (_ui_container_reward_items == null)
                Debug.LogWarning("[RewardContainerPanel] Reward items content container not assigned.", this);
        }
#endif
    }
}
