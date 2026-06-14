using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    // Horizontal zone progress bar with a lazy-growing cell system.
    //
    // Instead of instantiating all 60 zones at the start, this script starts
    // by instantiating only 3 cell GameObjects. As the player advances, additional
    // cells are instantiated one-by-one on demand. Off-screen cells are automatically
    // deactivated to optimize layout and rendering.
    public class ZoneIndicatorUI : MonoBehaviour
    {
        // ── Inspector references ───────────────────────────────────────────────
        [Header("Cell Prefab & Container")]
        [SerializeField] private ZoneIndicatorItemUI _itemPrefab;
        [SerializeField] private RectTransform       _ui_container_zone_items;

        [Header("Config")]
        [SerializeField] private ZoneConfigData _zoneConfig;
        [Tooltip("How many zone cells are visible at once (odd number recommended so current is centred).")]
        [SerializeField] private int   _visibleCount  = 5;
        [Tooltip("Horizontal gap between cell centres in pixels.")]
        [SerializeField] private float _cellSpacing   = 25f;
        [SerializeField] private float _slideDuration = 0.3f;

        // ── State ──────────────────────────────────────────────────────────────
        private float                     _cellWidth;   // derived from prefab RectTransform.sizeDelta.x
        private float                     _cellStep;    // _cellWidth + _cellSpacing
        private List<ZoneIndicatorItemUI> _cells = new List<ZoneIndicatorItemUI>();
        private int                       _currentZoneIndex;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            if (GetComponent<RectMask2D>() == null)
                gameObject.AddComponent<RectMask2D>();

            // Setup container pivot & anchors to left-aligned with parent center
            if (_ui_container_zone_items != null)
            {
                _ui_container_zone_items.anchorMin = new Vector2(0.5f, 0.5f);
                _ui_container_zone_items.anchorMax = new Vector2(0.5f, 0.5f);
                _ui_container_zone_items.pivot     = new Vector2(0f, 0.5f);

                var layout = _ui_container_zone_items.GetComponent<HorizontalLayoutGroup>();
                if (layout != null) layout.enabled = false;
            }

            // Derive cell size from the prefab
            var cellRt = _itemPrefab != null ? _itemPrefab.GetComponent<RectTransform>() : null;
            _cellWidth = cellRt != null ? cellRt.sizeDelta.x : 60f;
            _cellStep  = _cellWidth + _cellSpacing;

            // Resize this rect to show exactly _visibleCount cells
            var rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                float visibleWidth  = _visibleCount * _cellWidth + (_visibleCount - 1) * _cellSpacing;
                rt.anchorMin        = new Vector2(0.5f, rt.anchorMin.y);
                rt.anchorMax        = new Vector2(0.5f, rt.anchorMax.y);
                rt.pivot            = new Vector2(0.5f, rt.pivot.y);
                rt.sizeDelta        = new Vector2(visibleWidth, rt.sizeDelta.y);
                rt.anchoredPosition = new Vector2(0f, rt.anchoredPosition.y);
            }

            BuildInitialPool();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void SetCurrentZone(int zeroBasedIndex)
        {
            int previous      = _currentZoneIndex;
            _currentZoneIndex = zeroBasedIndex;

            EnsureCellsInstantiated(zeroBasedIndex);
            UpdateActiveStatesAndHighlights();

            if (zeroBasedIndex > previous && previous >= 0)
                AnimateSlide();
            else
                SnapToCurrentZone();
        }

        // ── Initialization & Cell Management ────────────────────────────────────

        private void BuildInitialPool()
        {
            if (_itemPrefab == null || _ui_container_zone_items == null || _zoneConfig == null) return;

            // Clear design-time placeholder children
            for (int i = _ui_container_zone_items.childCount - 1; i >= 0; i--)
                Destroy(_ui_container_zone_items.GetChild(i).gameObject);

            _cells.Clear();

            // Instantiate initial cells (starts with 3 cells for Zone 1, 2, and 3)
            EnsureCellsInstantiated(0);
            UpdateActiveStatesAndHighlights();
            SnapToCurrentZone();
        }

        private void EnsureCellsInstantiated(int zoneIndex)
        {
            if (_itemPrefab == null || _ui_container_zone_items == null || _zoneConfig == null) return;

            int totalZones = _zoneConfig.TotalZones;

            // To support smooth sliding, we instantiate cells up to at least zoneIndex + 2
            // (1 visible centered, 1 visible right, plus 1 off-screen buffer right).
            // But we must also ensure we always instantiate at least 3 cells initially.
            int requiredCount = Mathf.Max(3, zoneIndex + 3);
            requiredCount = Mathf.Min(requiredCount, totalZones);

            while (_cells.Count < requiredCount)
            {
                int newIndex = _cells.Count;
                var cell = Instantiate(_itemPrefab, _ui_container_zone_items);
                cell.name = $"ui_zone_cell_{newIndex}";

                PositionCell(cell, newIndex);
                _cells.Add(cell);
            }
        }

        // Sets a cell's RectTransform so its centre sits at (zoneIndex * _cellStep, 0)
        // relative to the container's anchor.
        private void PositionCell(ZoneIndicatorItemUI cell, int zoneIndex)
        {
            var rt        = cell.RectTransform;
            rt.anchorMin  = new Vector2(0f, 0.5f);
            rt.anchorMax  = new Vector2(0f, 0.5f);
            rt.pivot      = new Vector2(0.5f, 0.5f);
            rt.sizeDelta  = new Vector2(_cellWidth, rt.sizeDelta.y);
            rt.anchoredPosition = new Vector2(zoneIndex * _cellStep, 0f);
        }

        // Deactivates cells that are too far from the visible window and updates highlights.
        private void UpdateActiveStatesAndHighlights()
        {
            if (_zoneConfig == null) return;

            int totalZones = _zoneConfig.TotalZones;
            int half       = _visibleCount / 2; // For _visibleCount = 3, half = 1

            // Visible range is [_currentZoneIndex - half, _currentZoneIndex + half]
            // We keep a buffer of 1 cell active on either side of the visible window
            int activeStart = _currentZoneIndex - half - 1;
            int activeEnd   = _currentZoneIndex + half + 1;

            for (int i = 0; i < _cells.Count; i++)
            {
                var cell = _cells[i];
                if (i >= activeStart && i <= activeEnd && i < totalZones)
                {
                    int zoneNum = i + 1;
                    bool isCurrent = i == _currentZoneIndex;
                    cell.SetZone(zoneNum, ZoneConfigData.DetermineZoneType(zoneNum), isCurrent);
                    cell.gameObject.SetActive(true);
                }
                else
                {
                    cell.gameObject.SetActive(false);
                }
            }
        }

        // ── Animation ──────────────────────────────────────────────────────────

        // Slides the container so the current zone cell appears at screen centre.
        private void AnimateSlide()
        {
            float targetX = -(_currentZoneIndex * _cellStep);
            _ui_container_zone_items.DOKill();
            _ui_container_zone_items.DOAnchorPosX(targetX, _slideDuration).SetEase(Ease.OutCubic);
        }

        private void SnapToCurrentZone()
        {
            float targetX = -(_currentZoneIndex * _cellStep);
            _ui_container_zone_items.DOKill();
            var pos = _ui_container_zone_items.anchoredPosition;
            pos.x   = targetX;
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
