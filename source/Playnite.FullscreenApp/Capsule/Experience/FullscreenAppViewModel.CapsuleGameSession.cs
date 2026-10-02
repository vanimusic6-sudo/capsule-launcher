using Playnite.FullscreenApp.Capsule.Experience;
using Playnite.SDK.Models;
using System;
using System.Linq;

namespace Playnite.FullscreenApp.ViewModels
{
    public partial class FullscreenAppViewModel
    {
        private Guid? capsuleReturnGameId;
        private bool capsuleReturnDetailsVisible;
        private bool capsuleReturnPointCaptured;

        public CapsuleGameSessionState CapsuleGameSession => app.GameSession;

        private void CaptureCapsuleReturnPoint(Guid launchedGameId)
        {
            // A second launch while another game is active must not replace the place
            // the user originally left Capsule from.
            if (capsuleReturnPointCaptured)
            {
                return;
            }

            // Keep the user's actual selection when possible. A launch can also arrive
            // from quick launch / URI paths where SelectedGame is temporarily null.
            capsuleReturnGameId = SelectedGame?.Game?.Id;
            if (capsuleReturnGameId == null || capsuleReturnGameId == Guid.Empty)
            {
                capsuleReturnGameId = launchedGameId == Guid.Empty ? (Guid?)null : launchedGameId;
            }

            capsuleReturnDetailsVisible = GameDetailsVisible;
            capsuleReturnPointCaptured = true;
        }

        private void RestoreCapsuleReturnPoint()
        {
            if (!capsuleReturnPointCaptured ||
                CapsuleGameSession?.Phase != CapsuleGameSessionPhase.Returning)
            {
                return;
            }

            if (capsuleReturnGameId.HasValue && GamesView?.CollectionView != null)
            {
                var returnEntry = GamesView.CollectionView
                    .Cast<object>()
                    .OfType<GamesCollectionViewEntry>()
                    .FirstOrDefault(a => a.Game?.Id == capsuleReturnGameId.Value);

                if (returnEntry != null)
                {
                    SelectedGame = returnEntry;
                }
            }

            if (GameDetailsVisible != capsuleReturnDetailsVisible)
            {
                GameDetailsVisible = capsuleReturnDetailsVisible;
            }

            GameDetailsFocused = capsuleReturnDetailsVisible;
            GameListFocused = !capsuleReturnDetailsVisible;

            capsuleReturnGameId = null;
            capsuleReturnPointCaptured = false;
            CapsuleGameSession.CompleteReturn();
        }
    }
}
