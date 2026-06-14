using System;
using System.Collections.Generic;
using UnityEngine;
using VertigoCase.Data;
using VertigoCase.Wheel;
using VertigoCase.UI;

namespace VertigoCase.Core
{
    public enum GameState
    {
        Idle,             // Wheel loaded, awaiting player action
        Spinning,         // Wheel is animating (input blocked)
        RewardCollected,  // Successful spin — reward animation in progress
        BombHit,          // Bomb landed — bomb animation in progress
        RevivePrompt,     // Bomb animation done — revive popup visible
        Claiming,         // Player chose to walk away — claim screen visible
        GameOver          // Bomb + no revive — session reset
    }

    // State machine that owns the game session. Coordinates Core and UI.
    public class GameManager : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ── Inspector dependencies (wired in Editor) ───────────────────────────
        [Header("Core Dependencies")]
        [SerializeField] private ZoneManager    _zoneManager;
        [SerializeField] private WheelController _wheelController;
        [SerializeField] private UIManager       _uiManager;

        // ── Currency (stub — replace with a proper CurrencyService if needed) ──
        [Header("Starting Currency")]
        [SerializeField] private int _startingCash = 0;
        [SerializeField] private int _startingGold = 500;

        private int _cash;
        private int _gold;
        public int Cash => _cash;
        public int Gold => _gold;

        // ── State ──────────────────────────────────────────────────────────────
        private GameState _state = GameState.Idle;
        public  GameState CurrentState => _state;

        private readonly List<CollectedReward> _collectedRewards = new();
        public IReadOnlyList<CollectedReward> CollectedRewards => _collectedRewards;

        // ── Events ─────────────────────────────────────────────────────────────
        // Fired on every state change. Args: (previousState, newState).
        public event Action<GameState, GameState> OnStateChanged;

        // Fired after the zone index changes (0-based new index).
        public event Action<int> OnZoneChanged;

        // Fired immediately when a non-bomb spin reward is resolved.
        public event Action<CollectedReward> OnRewardCollected;

        // Fired when the wheel lands on a bomb.
        public event Action OnBombHit;

        // Fired when the player confirms their walk-away claim.
        public event Action<IReadOnlyList<CollectedReward>> OnRewardsClaimed;

        // Fired when a game-over is confirmed (bomb + no revive).
        public event Action OnGameOver;

        // Fired when currency amounts change. (cash, gold)
        public event Action<int, int> OnCurrencyChanged; // (cash, gold)

        // ── Lifecycle ──────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _cash = _startingCash;
            _gold = _startingGold;

            // Subscribe to wheel
            _wheelController.OnSpinComplete += HandleSpinComplete;

            // Let UIManager hook into our events before we load the first zone
            _uiManager.Initialize(this);

            LoadCurrentZone();
            // Emit the initial zone so UIManager can render zone 1
            OnZoneChanged?.Invoke(_zoneManager.CurrentZoneIndex);
        }

        private void OnDestroy()
        {
            if (_wheelController != null)
                _wheelController.OnSpinComplete -= HandleSpinComplete;
        }

        // ── Public commands (called by UIManager button listeners) ─────────────

        public void RequestSpin()
        {
            if (_state != GameState.Idle) return;
            Transition(GameState.Spinning);
            _wheelController.Spin();
        }

        public void RequestWalkAway()
        {
            if (_state != GameState.Idle) return;
            Transition(GameState.Claiming);
        }

        public int GetCurrentReviveCost()
        {
            return _zoneManager.GetCurrentZone().ReviveCost;
        }

        // Attempt to pay the revive cost from gold reserves and continue. Returns false if cannot afford.
        public bool RequestRevive()
        {
            if (_state != GameState.RevivePrompt) return false;

            int cost = _zoneManager.GetCurrentZone().ReviveCost;
            if (_gold < cost)
            {
                Debug.Log($"[GameManager] Cannot afford revive. Need {cost} gold, have {_gold}.");
                return false;
            }

            _gold -= cost;
            OnCurrencyChanged?.Invoke(_cash, _gold);
            Transition(GameState.Idle);
            return true;
        }

        public void RequestGiveUp()
        {
            if (_state != GameState.RevivePrompt) return;
            _collectedRewards.Clear();
            Transition(GameState.GameOver);
            OnGameOver?.Invoke();
        }

        public void ConfirmClaim()
        {
            if (_state != GameState.Claiming) return;

            // Credit currency rewards to the player's account
            foreach (var reward in _collectedRewards)
            {
                if (reward.RewardData != null && reward.RewardData.Type == RewardType.Currency)
                {
                    if (string.Equals(reward.RewardData.RewardName, "Cash", StringComparison.OrdinalIgnoreCase))
                    {
                        _cash += reward.Amount;
                    }
                    else if (string.Equals(reward.RewardData.RewardName, "Gold", StringComparison.OrdinalIgnoreCase))
                    {
                        _gold += reward.Amount;
                    }
                }
            }
            OnCurrencyChanged?.Invoke(_cash, _gold);

            OnRewardsClaimed?.Invoke(_collectedRewards);
            ResetSession();
        }

        public void RestartAfterGameOver()
        {
            if (_state != GameState.GameOver) return;
            ResetSession();
        }

        // ── Callbacks from animated systems ───────────────────────────────────

        // Called when the bomb animation finishes, so the revive prompt appears.
        public void NotifyBombAnimationComplete()
        {
            if (_state == GameState.BombHit)
                Transition(GameState.RevivePrompt);
        }

        // Called when the reward fly-in animation finishes. Advances zone or transitions to Claiming.
        public void NotifyRewardAnimationComplete()
        {
            if (_state != GameState.RewardCollected) return;

            bool hasMore = _zoneManager.AdvanceZone();
            OnZoneChanged?.Invoke(_zoneManager.CurrentZoneIndex);

            if (hasMore)
            {
                LoadCurrentZone();
                Transition(GameState.Idle);
            }
            else
            {
                // All 60 zones completed — force claim
                Transition(GameState.Claiming);
            }
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private void HandleSpinComplete(WheelSliceData result)
        {
            if (result.IsBomb)
            {
                Transition(GameState.BombHit);
                OnBombHit?.Invoke();
            }
            else
            {
                var zone = _zoneManager.GetCurrentZone();
                int scaled = Mathf.RoundToInt(result.Amount * zone.RewardMultiplier);
                var reward = new CollectedReward(result.Reward, scaled, _zoneManager.GetCurrentZoneNumber());
                _collectedRewards.Add(reward);
                OnRewardCollected?.Invoke(reward);
                Transition(GameState.RewardCollected);
            }
        }

        private void LoadCurrentZone()
        {
            var zone = _zoneManager.GetCurrentZone();
            _wheelController.LoadWheel(zone.WheelConfig, _zoneManager.GetCurrentZoneNumber(), zone.RewardMultiplier);
        }

        private void Transition(GameState next)
        {
            if (_state == next) return;
            var prev = _state;
            _state = next;
            OnStateChanged?.Invoke(prev, next);
        }

        private void ResetSession()
        {
            _collectedRewards.Clear();
            _zoneManager.ResetToStart();
            LoadCurrentZone();
            OnZoneChanged?.Invoke(0);
            Transition(GameState.Idle);
        }

        // ── Editor validation ──────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_zoneManager == null)
                Debug.LogWarning("[GameManager] ZoneManager reference is missing.", this);
            if (_wheelController == null)
                Debug.LogWarning("[GameManager] WheelController reference is missing.", this);
            if (_uiManager == null)
                Debug.LogWarning("[GameManager] UIManager reference is missing.", this);
        }
#endif
    }
}
