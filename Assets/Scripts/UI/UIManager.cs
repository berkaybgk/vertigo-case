using UnityEngine;
using VertigoCase.Core;
using VertigoCase.Data;

namespace VertigoCase.UI
{
    // Central UI coordinator. Routes state changes to specific panels.
    public class UIManager : MonoBehaviour
    {
        // ── Panel references (assigned in Inspector) ───────────────────────────
        [Header("Panels")]
        [SerializeField] private WheelPanel           _wheelPanel;
        [SerializeField] private RewardContainerPanel _rewardContainerPanel;
        [SerializeField] private ZoneIndicatorUI      _zoneIndicatorUI;
        [SerializeField] private RevivePopupPanel     _revivePopupPanel;
        [SerializeField] private ClaimPanel           _claimPanel;
        [SerializeField] private TopBarUI             _topBarUI;

        // Typed as the interface so UIManager is decoupled from MonoBehaviour specifics.
        private IGameManager _gameManager;

        // ── Public initialisation (called by GameManager) ──────────────────────

        public void Initialize(IGameManager gameManager)
        {
            _gameManager = gameManager;

            // Subscribe to GameManager events
            _gameManager.OnStateChanged    += HandleStateChanged;
            _gameManager.OnZoneChanged     += HandleZoneChanged;
            _gameManager.OnRewardCollected += HandleRewardCollected;
            _gameManager.OnBombHit         += HandleBombHit;
            _gameManager.OnRewardsClaimed  += HandleRewardsClaimed;
            _gameManager.OnCurrencyChanged += HandleCurrencyChanged;

            // Wire all button listeners (spec: no Inspector OnClick)
            _wheelPanel.Initialize(
                onSpinClicked:     _gameManager.RequestSpin,
                onWalkAwayClicked: _gameManager.RequestWalkAway);

            _revivePopupPanel.Initialize(
                onReviveClicked:  () => { if (!_gameManager.RequestRevive()) _revivePopupPanel.ShakeReviveButton(); },
                onGiveUpClicked:  _gameManager.RequestGiveUp);

            // Give-up resets the session directly to Idle (no separate GameOver state).
            _claimPanel.Initialize(onConfirmClicked: _gameManager.ConfirmClaim);

            // Set initial UI state
            SetPanelVisibility(GameState.Idle);
            _topBarUI.UpdateCurrency(_gameManager.Cash, _gameManager.Gold);
        }

        private void OnDestroy()
        {
            if (_gameManager == null) return;
            _gameManager.OnStateChanged    -= HandleStateChanged;
            _gameManager.OnZoneChanged     -= HandleZoneChanged;
            _gameManager.OnRewardCollected -= HandleRewardCollected;
            _gameManager.OnBombHit         -= HandleBombHit;
            _gameManager.OnRewardsClaimed  -= HandleRewardsClaimed;
            _gameManager.OnCurrencyChanged -= HandleCurrencyChanged;
        }

        // ── Event handlers ─────────────────────────────────────────────────────

        private void HandleStateChanged(GameState previous, GameState next)
        {
            SetPanelVisibility(next);
            _wheelPanel.OnStateChanged(previous, next);

            if (next == GameState.Claiming)
                _claimPanel.ShowRewards(_gameManager.CollectedRewards);
            else if (next == GameState.RevivePrompt)
                _revivePopupPanel.UpdateReviveCost(_gameManager.GetCurrentReviveCost());

            // After a successful revive OR a give-up, restore the reward display.
            // CollectedRewards is already empty after a give-up, so this clears the panel.
            if (previous == GameState.RevivePrompt && next == GameState.Idle)
                _rewardContainerPanel.RestoreRewards(_gameManager.CollectedRewards);
        }

        private void HandleZoneChanged(int zoneIndex)
        {
            _zoneIndicatorUI.SetCurrentZone(zoneIndex);
            _wheelPanel.SetFirstZone(zoneIndex == 0);
        }

        private void HandleRewardCollected(CollectedReward reward)
        {
            _rewardContainerPanel.AddReward(reward, onAnimationComplete: _gameManager.NotifyRewardAnimationComplete);
        }

        private void HandleBombHit()
        {
            _rewardContainerPanel.PlayBombDestroyAnimation(onComplete: _gameManager.NotifyBombAnimationComplete);
        }

        private void HandleRewardsClaimed(System.Collections.Generic.IReadOnlyList<CollectedReward> rewards)
        {
            _claimPanel.ShowRewards(rewards);
            _rewardContainerPanel.Clear();
        }

        private void HandleCurrencyChanged(int cash, int gold)
        {
            _topBarUI.UpdateCurrency(cash, gold);
        }

        // ── Panel visibility routing ───────────────────────────────────────────

        // Shows/hides panels based on game state.
        private void SetPanelVisibility(GameState state)
        {
            bool reviveVisible = state == GameState.RevivePrompt;
            bool claimVisible  = state == GameState.Claiming;

            _revivePopupPanel.SetVisible(reviveVisible);
            _claimPanel.SetVisible(claimVisible);
        }

        // ── Editor validation ──────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_wheelPanel == null)
                Debug.LogWarning("[UIManager] WheelPanel reference is missing.", this);
            if (_rewardContainerPanel == null)
                Debug.LogWarning("[UIManager] RewardContainerPanel reference is missing.", this);
            if (_zoneIndicatorUI == null)
                Debug.LogWarning("[UIManager] ZoneIndicatorUI reference is missing.", this);
            if (_revivePopupPanel == null)
                Debug.LogWarning("[UIManager] RevivePopupPanel reference is missing.", this);
            if (_claimPanel == null)
                Debug.LogWarning("[UIManager] ClaimPanel reference is missing.", this);
            if (_topBarUI == null)
                Debug.LogWarning("[UIManager] TopBarUI reference is missing.", this);
        }
#endif
    }
}
