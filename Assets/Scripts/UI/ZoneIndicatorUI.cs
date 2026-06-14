using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    // Horizontal zone progress bar displayed at the top of the screen.
    public class ZoneIndicatorUI : MonoBehaviour
    {
        // ── Inspector references ───────────────────────────────────────────────
        [Header("Cell Prefab & Container")]
        [SerializeField] private ZoneIndicatorItemUI _itemPrefab;
        [SerializeField] private RectTransform       _ui_container_zone_items;

        [Header("Config")]
        [SerializeField] private ZoneConfigData _zoneConfig;
        [Tooltip("How many zone cells are visible at once (odd number recommended so current is centred).")]
        [SerializeField] private int   _visibleCount   = 5;
        [SerializeField] private float _slideDuration  = 0.3f;

        private int                    _currentZoneIndex = 0;
        private ZoneIndicatorItemUI[]  _cellPool;

        private void Awake()
        {
            if (GetComponent<RectMask2D>() == null)
            {
                gameObject.AddComponent<RectMask2D>();
            }

            float cellSpacing = 25f;
            var layout = _ui_container_zone_items.GetComponent<HorizontalLayoutGroup>();
            if (layout != null)
            {
                layout.spacing = cellSpacing;
                layout.childAlignment = TextAnchor.MiddleCenter;
            }

            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null && _itemPrefab != null)
            {
                var cellRect = _itemPrefab.GetComponent<RectTransform>();
                if (cellRect != null)
                {
                    float cellWidth = cellRect.sizeDelta.x;
                    float targetWidth = _visibleCount * cellWidth + (_visibleCount - 1) * cellSpacing;

                    rectTransform.anchorMin = new Vector2(0.5f, rectTransform.anchorMin.y);
                    rectTransform.anchorMax = new Vector2(0.5f, rectTransform.anchorMax.y);
                    rectTransform.pivot = new Vector2(0.5f, rectTransform.pivot.y);
                    rectTransform.sizeDelta = new Vector2(targetWidth, rectTransform.sizeDelta.y);
                    rectTransform.anchoredPosition = new Vector2(0f, rectTransform.anchoredPosition.y);
                }
            }

            BuildCellPool();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        // Sets the current zone (0-based) and refreshes the displayed window.
        public void SetCurrentZone(int zeroBasedIndex)
        {
            int previous = _currentZoneIndex;
            _currentZoneIndex = zeroBasedIndex;

            RefreshCells();

            // Slide if advancing, otherwise snap immediately (e.g. at start or reset)
            if (zeroBasedIndex > previous && previous >= 0)
            {
                AnimateSlide();
            }
            else
            {
                SnapToCurrentZone();
            }
        }

        // ── Private ────────────────────────────────────────────────────────────

        private void BuildCellPool()
        {
            if (_itemPrefab == null || _ui_container_zone_items == null || _zoneConfig == null) return;

            // Clear any design-time placeholders/pre-existing children to prevent layout alignment issues
            for (int i = _ui_container_zone_items.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_ui_container_zone_items.GetChild(i).gameObject);
            }

            int count = _zoneConfig.TotalZones;
            _cellPool = new ZoneIndicatorItemUI[count];
            for (int i = 0; i < count; i++)
            {
                var cell = Instantiate(_itemPrefab, _ui_container_zone_items);
                cell.name = $"ui_zone_cell_{i}";
                
                int zoneNumber = i + 1;
                ZoneType type = ZoneConfigData.DetermineZoneType(zoneNumber);
                cell.SetZone(zoneNumber, type, false);
                
                _cellPool[i] = cell;
            }

            // Force layout rebuild so local positions are computed
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_ui_container_zone_items);
        }

        private void RefreshCells()
        {
            if (_cellPool == null || _zoneConfig == null) return;

            for (int i = 0; i < _cellPool.Length; i++)
            {
                int zoneNumber = i + 1;
                ZoneType type = ZoneConfigData.DetermineZoneType(zoneNumber);
                bool isCurrent = i == _currentZoneIndex;
                _cellPool[i].SetZone(zoneNumber, type, isCurrent);
            }
        }

        private void AnimateSlide()
        {
            if (_cellPool == null || _currentZoneIndex < 0 || _currentZoneIndex >= _cellPool.Length) return;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_ui_container_zone_items);

            float targetX = -_cellPool[_currentZoneIndex].RectTransform.localPosition.x;

            _ui_container_zone_items.DOKill();
            _ui_container_zone_items
                .DOAnchorPosX(targetX, _slideDuration)
                .SetEase(Ease.OutCubic);
        }

        private void SnapToCurrentZone()
        {
            if (_cellPool == null || _currentZoneIndex < 0 || _currentZoneIndex >= _cellPool.Length) return;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_ui_container_zone_items);

            float targetX = -_cellPool[_currentZoneIndex].RectTransform.localPosition.x;
            _ui_container_zone_items.DOKill();

            Vector2 pos = _ui_container_zone_items.anchoredPosition;
            pos.x = targetX;
            _ui_container_zone_items.anchoredPosition = pos;
        }

        // ── Editor auto-binding ────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_itemPrefab == null)
                Debug.LogWarning("[ZoneIndicatorUI] No ZoneIndicatorItemUI prefab assigned.", this);
            if (_zoneConfig == null)
                Debug.LogWarning("[ZoneIndicatorUI] No ZoneConfigData assigned.", this);
            if (_ui_container_zone_items == null)
                Debug.LogWarning("[ZoneIndicatorUI] 'ui_container_zone_items' not assigned.", this);
        }
#endif
    }
}
