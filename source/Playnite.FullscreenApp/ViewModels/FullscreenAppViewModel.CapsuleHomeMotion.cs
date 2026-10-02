using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite.FullscreenApp.ViewModels
{
    public partial class FullscreenAppViewModel
    {
        internal const int CapsuleLaunchCollapseDurationMs = 210;

        private bool capsuleLaunchTransitionActive;
        public bool CapsuleLaunchTransitionActive
        {
            get => capsuleLaunchTransitionActive;
            private set
            {
                if (capsuleLaunchTransitionActive == value)
                {
                    return;
                }

                capsuleLaunchTransitionActive = value;
                OnPropertyChanged();
            }
        }

        public string CapsuleSelectedGameDescription
        {
            get
            {
                var source = SelectedGame?.Description;
                if (string.IsNullOrWhiteSpace(source))
                {
                    return string.Empty;
                }

                var text = Regex.Replace(source, "<[^>]+>", " ");
                text = WebUtility.HtmlDecode(text);
                text = Regex.Replace(text, @"\s+", " ").Trim();

                const int maxLength = 190;
                if (text.Length > maxLength)
                {
                    text = text.Substring(0, maxLength).TrimEnd() + "…";
                }

                return text;
            }
        }

        public string CapsuleSelectedGameGenres
        {
            get
            {
                var genres = SelectedGame?.Genres;
                if (genres == null)
                {
                    return string.Empty;
                }

                return string.Join("  •  ",
                    genres
                        .Where(a => a != null && !string.IsNullOrWhiteSpace(a.Name))
                        .Take(3)
                        .Select(a => a.Name));
            }
        }

        private void NotifyCapsuleSelectionChanged()
        {
            OnPropertyChanged(nameof(CapsuleSelectedGameDescription));
            OnPropertyChanged(nameof(CapsuleSelectedGameGenres));
        }

        private async Task PlaySelectedWithCapsuleTransitionAsync()
        {
            if (CapsuleLaunchTransitionActive || SelectedGame?.IsInstalled != true)
            {
                return;
            }

            var game = SelectedGame.Game;
            CapsuleLaunchTransitionActive = true;

            // Let the selected cover finish its collapse before the launch pipeline
            // takes focus/minimizes fullscreen. This is deliberately short enough to
            // feel like one action rather than a blocking splash.
            await Task.Delay(CapsuleLaunchCollapseDurationMs);

            try
            {
                GamesEditor.PlayGame(game, true);
            }
            catch
            {
                CapsuleLaunchTransitionActive = false;
                throw;
            }

            // Defensive fallback for failures that never raise Starting/Cancelled.
            await Task.Delay(5000);
            if (app.GameSession?.Phase == Playnite.FullscreenApp.Capsule.Experience.CapsuleGameSessionPhase.Idle)
            {
                CapsuleLaunchTransitionActive = false;
            }
        }

        private void CompleteCapsuleLaunchTransition()
        {
            CapsuleLaunchTransitionActive = false;
        }
    }
}
