using Playnite.Controllers;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Playnite.FullscreenApp.Capsule.Experience
{
    public enum CapsuleGameSessionPhase
    {
        Idle,
        Starting,
        Running,
        Returning
    }

    /// <summary>
    /// Small Capsule-facing state machine around Playnite's game controller events.
    ///
    /// The inherited Playnite launch pipeline still owns process tracking, client startup,
    /// playtime and window minimize/restore. Capsule only exposes presentation state so
    /// expensive visuals can stop while a game owns the screen and resume after return.
    /// </summary>
    public sealed class CapsuleGameSessionState : ObservableObject, IDisposable
    {
        private readonly GameControllerFactory controllers;
        private readonly HashSet<Guid> activeGames = new HashSet<Guid>();
        private CapsuleGameSessionPhase phase = CapsuleGameSessionPhase.Idle;
        private Guid? primaryGameId;
        private bool disposed;

        public CapsuleGameSessionPhase Phase
        {
            get => phase;
            private set
            {
                if (phase == value)
                {
                    return;
                }

                phase = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPresentationSuspended));
            }
        }

        public Guid? PrimaryGameId
        {
            get => primaryGameId;
            private set
            {
                if (primaryGameId == value)
                {
                    return;
                }

                primaryGameId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True from the moment a launch begins until Capsule has restored its return point.
        /// Presentation code should use this to stop ambient animation/render loops.
        /// </summary>
        public bool IsPresentationSuspended => Phase != CapsuleGameSessionPhase.Idle;

        public int ActiveGameCount => activeGames.Count;

        public CapsuleGameSessionState(GameControllerFactory controllers)
        {
            this.controllers = controllers ?? throw new ArgumentNullException(nameof(controllers));

            controllers.Starting += Controllers_Starting;
            controllers.Started += Controllers_Started;
            controllers.Stopped += Controllers_Stopped;
            controllers.StartupCancelled += Controllers_StartupCancelled;
        }

        public void CompleteReturn()
        {
            if (activeGames.Count > 0)
            {
                Phase = CapsuleGameSessionPhase.Running;
                PrimaryGameId = activeGames.First();
                return;
            }

            PrimaryGameId = null;
            Phase = CapsuleGameSessionPhase.Idle;
        }

        internal void MarkStarting(Guid gameId)
        {
            if (gameId == Guid.Empty)
            {
                return;
            }

            activeGames.Add(gameId);
            PrimaryGameId = PrimaryGameId ?? gameId;
            Phase = CapsuleGameSessionPhase.Starting;
            OnPropertyChanged(nameof(ActiveGameCount));
        }

        internal void MarkRunning(Guid gameId)
        {
            if (gameId != Guid.Empty)
            {
                activeGames.Add(gameId);
                PrimaryGameId = PrimaryGameId ?? gameId;
            }

            Phase = CapsuleGameSessionPhase.Running;
            OnPropertyChanged(nameof(ActiveGameCount));
        }

        internal void MarkStopped(Guid gameId)
        {
            if (gameId != Guid.Empty)
            {
                activeGames.Remove(gameId);
            }
            else if (activeGames.Count == 1)
            {
                activeGames.Clear();
            }

            if (activeGames.Count == 0)
            {
                Phase = CapsuleGameSessionPhase.Returning;
            }
            else
            {
                PrimaryGameId = activeGames.First();
                Phase = CapsuleGameSessionPhase.Running;
            }

            OnPropertyChanged(nameof(ActiveGameCount));
        }

        internal void MarkStartupCancelled(Guid gameId)
        {
            MarkStopped(gameId);
        }

        private void Controllers_Starting(object sender, OnGameStartingEventArgs e)
        {
            MarkStarting(e?.Game?.Id ?? Guid.Empty);
        }

        private void Controllers_Started(object sender, GameStartedEventArgs e)
        {
            MarkRunning(e?.Source?.Game?.Id ?? PrimaryGameId ?? Guid.Empty);
        }

        private void Controllers_Stopped(object sender, GameStoppedEventArgs e)
        {
            MarkStopped(e?.Source?.Game?.Id ?? PrimaryGameId ?? Guid.Empty);
        }

        private void Controllers_StartupCancelled(object sender, OnGameStartupCancelledEventArgs e)
        {
            MarkStartupCancelled(e?.Game?.Id ?? PrimaryGameId ?? Guid.Empty);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            controllers.Starting -= Controllers_Starting;
            controllers.Started -= Controllers_Started;
            controllers.Stopped -= Controllers_Stopped;
            controllers.StartupCancelled -= Controllers_StartupCancelled;
            activeGames.Clear();
            PrimaryGameId = null;
            Phase = CapsuleGameSessionPhase.Idle;
            OnPropertyChanged(nameof(ActiveGameCount));
        }
    }
}
