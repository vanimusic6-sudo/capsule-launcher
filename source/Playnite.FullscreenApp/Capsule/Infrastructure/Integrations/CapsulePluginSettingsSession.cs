using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Playnite.FullscreenApp.Capsule.Infrastructure.Integrations
{
    /// <summary>
    /// Hosts an existing plugin settings surface inside a Capsule-owned fullscreen flow.
    ///
    /// This deliberately reuses the plugin's own authentication/settings implementation
    /// instead of duplicating vendor login logic inside Capsule.
    /// </summary>
    public sealed class CapsulePluginSettingsSession : IDisposable
    {
        private bool closed;

        public LibraryPlugin Plugin { get; }
        public ISettings Settings { get; }
        public UserControl View { get; }

        private CapsulePluginSettingsSession(
            LibraryPlugin plugin,
            ISettings settings,
            UserControl view)
        {
            Plugin = plugin;
            Settings = settings;
            View = view;

            View.DataContext = Settings;
            Settings.BeginEdit();
        }

        public static CapsulePluginSettingsSession TryCreate(
            LibraryPlugin plugin,
            bool firstRun)
        {
            if (plugin == null || plugin.Properties?.HasSettings != true)
            {
                return null;
            }

            var settings = plugin.GetSettings(firstRun);
            var view = plugin.GetSettingsView(firstRun);
            if (settings == null || view == null)
            {
                return null;
            }

            return new CapsulePluginSettingsSession(plugin, settings, view);
        }

        public bool TryCommit(out IReadOnlyList<string> errors)
        {
            EnsureOpen();

            if (!Settings.VerifySettings(out var validationErrors))
            {
                errors = validationErrors ?? new List<string>();
                return false;
            }

            Settings.EndEdit();
            closed = true;
            errors = Array.Empty<string>();
            return true;
        }

        public void Cancel()
        {
            if (closed)
            {
                return;
            }

            Settings.CancelEdit();
            closed = true;
        }

        public void Dispose()
        {
            Cancel();
        }

        private void EnsureOpen()
        {
            if (closed)
            {
                throw new InvalidOperationException("The plugin settings session has already been closed.");
            }
        }
    }
}
