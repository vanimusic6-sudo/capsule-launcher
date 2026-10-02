using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;

namespace Playnite.FullscreenApp.Capsule.Infrastructure.Integrations
{
    public enum CapsuleLibraryPlatform
    {
        Steam,
        EpicGames,
        Xbox,
        Gog,
        Ubisoft,
        EA,
        BattleNet,
        AmazonGames,
        ItchIo,
        Humble,
        Other
    }

    public sealed class CapsuleLibraryIntegrationDefinition
    {
        public CapsuleLibraryPlatform Platform { get; }
        public string DisplayName { get; }
        public Guid PluginId { get; }
        public bool IsBuiltIn { get; }

        public CapsuleLibraryIntegrationDefinition(
            CapsuleLibraryPlatform platform,
            string displayName,
            Guid pluginId,
            bool isBuiltIn)
        {
            Platform = platform;
            DisplayName = displayName;
            PluginId = pluginId;
            IsBuiltIn = isBuiltIn;
        }
    }

    public sealed class CapsuleLibraryIntegrationState
    {
        public CapsuleLibraryIntegrationDefinition Definition { get; }
        public LibraryPlugin Plugin { get; }

        public bool IsPluginLoaded { get; }
        public bool HasSettings { get; }
        public bool HasClient { get; }
        public bool IsClientInstalled { get; }
        public bool CanShutdownClient { get; }

        public CapsuleLibraryIntegrationState(
            CapsuleLibraryIntegrationDefinition definition,
            LibraryPlugin plugin)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Plugin = plugin;

            IsPluginLoaded = plugin != null;
            HasSettings = plugin?.Properties?.HasSettings == true;
            CanShutdownClient = plugin?.Properties?.CanShutdownClient == true;

            var client = plugin?.Client;
            HasClient = client != null;
            IsClientInstalled = client?.IsInstalled == true;
        }
    }
}
