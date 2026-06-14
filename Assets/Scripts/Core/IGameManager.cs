using System;
using System.Collections.Generic;
using VertigoCase.Data;

namespace VertigoCase.Core
{
    // Abstraction of the game session controller.
    // Depend on this interface instead of the concrete <see cref="GameManager"/> to keep
    // subsystems (UIManager, future services) testable and decoupled from MonoBehaviour.
    public interface IGameManager
    {
        // ── State ──────────────────────────────────────────────────────────────
        GameState                          CurrentState      { get; }
        IReadOnlyList<CollectedReward>     CollectedRewards  { get; }
        int                                Cash              { get; }
        int                                Gold              { get; }

        // ── Events ─────────────────────────────────────────────────────────────
        event Action<GameState, GameState>                  OnStateChanged;
        event Action<int>                                   OnZoneChanged;
        event Action<CollectedReward>                       OnRewardCollected;
        event Action                                        OnBombHit;
        event Action<IReadOnlyList<CollectedReward>>        OnRewardsClaimed;
        event Action<int, int>                              OnCurrencyChanged;

        // ── Commands ───────────────────────────────────────────────────────────
        void RequestSpin();
        void RequestWalkAway();
        bool RequestRevive();
        void RequestGiveUp();
        void ConfirmClaim();
        int  GetCurrentReviveCost();

        // ── Animation callbacks ────────────────────────────────────────────────
        void NotifyBombAnimationComplete();
        void NotifyRewardAnimationComplete();
    }
}
