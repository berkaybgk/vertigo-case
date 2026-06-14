using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VertigoCase.Data;

namespace VertigoCase.Wheel
{
    // Owns wheel spinning logic and slice layout.
    public class WheelController : MonoBehaviour
    {
        // ── Inspector references (set in Editor) ───────────────────────────────
        [Header("Wheel Visuals")]
        [SerializeField] private RectTransform _wheelBaseTransform;   // The rotating part
        [SerializeField] private Image         _wheelBaseImage;
        [SerializeField] private Image         _wheelIndicatorImage;

        [Header("Slice Prefab & Layout")]
        [SerializeField] private WheelSliceUI  _slicePrefab;
        [Tooltip("Distance from wheel centre to the centre of each slice icon, in pixels.")]
        [SerializeField] private float         _sliceIconRadius = 140f;

        [Header("Bomb Icon")]
        [Tooltip("Sprite shown on the bomb slice. Assign ui_card_icon_death.png here.")]
        [SerializeField] private Sprite        _bombSprite;

        // ── Public event ───────────────────────────────────────────────────────
        // Fired when the spin animation completes. Payload is the winning slice.
        public event Action<WheelSliceData> OnSpinComplete;

        // ── Private state ──────────────────────────────────────────────────────
        private WheelConfigData  _currentConfig;
        private WheelSliceData[] _activeSlices;
        private WheelSliceUI[]   _sliceInstances;
        private bool             _isSpinning;

        // ── Public API ─────────────────────────────────────────────────────────

        // Loads a new wheel configuration: swaps sprites, resets rotation, and rebuilds all slice UI elements.
        public void LoadWheel(WheelConfigData config, int zoneNumber, float rewardMultiplier = 1f)
        {
            if (config == null)
            {
                Debug.LogError("[WheelController] LoadWheel called with null config.");
                return;
            }

            _currentConfig = config;
            _activeSlices = config.GetSlicesForZone(zoneNumber);

            // Swap sprites
            _wheelBaseImage.sprite      = config.WheelBase;
            _wheelIndicatorImage.sprite = config.WheelIndicator;

            // Reset rotation instantly (no animation — loading happens off-screen or at start)
            _wheelBaseTransform.localEulerAngles = Vector3.zero;

            BuildSlices(rewardMultiplier);
        }

        private void OnDestroy()
        {
            DestroySlices();
            if (_wheelBaseTransform != null)
            {
                _wheelBaseTransform.DOKill();
            }
        }

        // Begins the spin. Selects the target slice, calculates rotation, and animates with DOTween.
        public void Spin()
        {
            if (_isSpinning)
            {
                Debug.LogWarning("[WheelController] Spin() called while already spinning.");
                return;
            }

            if (_currentConfig == null || _activeSlices == null || _activeSlices.Length == 0)
            {
                Debug.LogError("[WheelController] Cannot spin: no active slices loaded.");
                return;
            }

            int targetSliceIndex = SelectTargetSlice();
            PerformSpin(targetSliceIndex);
        }

        // ── Spin logic ─────────────────────────────────────────────────────────

        // Selects which slice the wheel will land on using a uniform random distribution.
        private int SelectTargetSlice()
        {
            return UnityEngine.Random.Range(0, _activeSlices != null ? _activeSlices.Length : 8);
        }

        private void PerformSpin(int targetSliceIndex)
        {
            _isSpinning = true;

            // ── Angle math ─────────────────────────────────────────────────────
            //
            // Slices are arranged clockwise starting from the top (0°):
            //   Slice 0 → 0°  (top)
            //   Slice 1 → 45°
            //   Slice N → N × 45°
            //
            // The indicator is fixed at the top.  To bring slice N to the top,
            // the wheel must rotate clockwise by (N × sliceAngle) degrees.
            // In Unity's local Z-axis: positive = CCW, so we negate.
            //
            // To prevent previous rotation accumulation, we calculate the absolute 
            // destination angle: targetSliceIndex * sliceAngle.
            // We subtract minRotations * 360 to ensure the wheel does full spins.
            // DOTween's RotateMode.FastBeyond360 will rotate clockwise since destinationZ < currentZ.

            int sliceCount = _activeSlices != null ? _activeSlices.Length : 8;
            float sliceAngle = sliceCount > 0 ? 360f / sliceCount : 45f;
            float destinationZ  = targetSliceIndex * sliceAngle - (_currentConfig.MinRotations + 1) * 360f;

            _wheelBaseTransform
                .DOLocalRotate(
                    new Vector3(0f, 0f, destinationZ),
                    _currentConfig.SpinDuration,
                    RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuart)
                .SetUpdate(UpdateType.Normal)
                .OnComplete(() => OnSpinFinished(targetSliceIndex));
        }

        private void OnSpinFinished(int winningSliceIndex)
        {
            _isSpinning = false;

            float z = _wheelBaseTransform.localEulerAngles.z % 360f;
            if (z < 0f) z += 360f;
            _wheelBaseTransform.localEulerAngles = new Vector3(0f, 0f, z);

            OnSpinComplete?.Invoke(_activeSlices[winningSliceIndex]);
        }

        // ── Slice construction ─────────────────────────────────────────────────

        private void BuildSlices(float rewardMultiplier)
        {
            if (_activeSlices == null) return;

            int count = _activeSlices.Length;
            float sliceAngle = count > 0 ? 360f / count : 45f;

            if (_sliceInstances == null || _sliceInstances.Length != count)
            {
                DestroySlices();
                _sliceInstances = new WheelSliceUI[count];
            }

            for (int i = 0; i < count; i++)
            {
                WheelSliceUI sliceUI = _sliceInstances[i];
                if (sliceUI == null)
                {
                    sliceUI = Instantiate(_slicePrefab, _wheelBaseTransform);
                    sliceUI.name = $"ui_slice_{i}";
                    _sliceInstances[i] = sliceUI;
                }

                // Position
                float angleDeg = i * sliceAngle;
                float angleRad = angleDeg * Mathf.Deg2Rad;

                var rt = sliceUI.RectTransform;
                rt.anchoredPosition = new Vector2(
                    Mathf.Sin(angleRad) * _sliceIconRadius,
                    Mathf.Cos(angleRad) * _sliceIconRadius
                );

                // Rotation of the icon
                rt.localEulerAngles = new Vector3(0f, 0f, -angleDeg);

                sliceUI.Initialize(_activeSlices[i], _bombSprite, rewardMultiplier);
            }
        }

        private void DestroySlices()
        {
            if (_sliceInstances == null) return;
            foreach (var s in _sliceInstances)
                if (s != null) Destroy(s.gameObject);
            _sliceInstances = null;
        }

        // ── Editor auto-binding & validation ───────────────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-bind: direct-child lookup only (non-recursive, fast).
            if (_wheelBaseTransform == null)
                _wheelBaseTransform = transform.Find("ui_image_spin_base") as RectTransform;

            if (_wheelBaseImage == null && _wheelBaseTransform != null)
                _wheelBaseImage = _wheelBaseTransform.GetComponent<Image>();

            if (_wheelIndicatorImage == null)
            {
                var t = transform.Find("ui_image_spin_indicator");
                if (t != null) _wheelIndicatorImage = t.GetComponent<Image>();
            }

            // Validation warnings
            if (_slicePrefab == null)
                Debug.LogWarning("[WheelController] No WheelSliceUI prefab assigned.", this);
            if (_bombSprite == null)
                Debug.LogWarning("[WheelController] No bomb sprite assigned (ui_card_icon_death).", this);
        }
#endif
    }
}
