using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Playnite.FullscreenApp.Capsule.Infrastructure.Integrations
{
    /// <summary>
    /// Capsule-facing view of Playnite library plugins.
    ///
    /// Vendor authentication and library import stay inside the library plugins.
    /// Capsule consumes those plugins through a small stable adapter so fullscreen
    /// presentation never needs to know Steam/Epic/Xbox-specific implementation details.
    /// </summary>
    public sealed class CapsuleLibraryIntegrationRegistry
    {
        private readonly ExtensionFactory extensions;

        public static IReadOnlyList<CapsuleLibraryIntegrationDefinition> KnownIntegrations { get; } =
            new List<CapsuleLibraryIntegrationDefinition>
            {
                BuiltIn(CapsuleLibraryPlatform.Steam, "Steam", BuiltinExtension.SteamLibrary),
                BuiltIn(CapsuleLibraryPlatform.Xbox, "Xbox", BuiltinExtension.XboxLibrary),
                BuiltIn(CapsuleLibraryPlatform.EpicGames, "Epic Games", BuiltinExtension.EpicLibrary),
                BuiltIn(CapsuleLibraryPlatform.Gog, "GOG", BuiltinExtension.GogLibrary),
                BuiltIn(CapsuleLibraryPlatform.Ubisoft, "Ubisoft Connect", BuiltinExtension.UplayLibrary),
                BuiltIn(CapsuleLibraryPlatform.EA, "EA app", BuiltinExtension.OriginLibrary),
                BuiltIn(CapsuleLibraryPlatform.BattleNet, "Battle.net", BuiltinExtension.BattleNetLibrary),
                BuiltIn(CapsuleLibraryPlatform.AmazonGames, "Amazon Games", BuiltinExtension.AmazonGamesLibrary),
                BuiltIn(CapsuleLibraryPlatform.ItchIo, "itch.io", BuiltinExtension.ItchioLibrary),
                BuiltIn(CapsuleLibraryPlatform.Humble, "Humble", BuiltinExtension.HumbleLibrary)
            };

        public CapsuleLibraryIntegrationRegistry(ExtensionFactory extensions)
        {
            this.extensions = extensions ?? throw new ArgumentNullException(nameof(extensions));
        }

        public IReadOnlyList<CapsuleLibraryIntegrationState> GetStates()
        {
            return BuildStates(extensions.LibraryPlugins);
        }

        public LibraryPlugin GetPlugin(Guid pluginId)
        {
            return extensions.LibraryPlugins.FirstOrDefault(a => a.Id == pluginId);
        }

        public bool TryOpenClient(Guid pluginId)
        {
            var plugin = GetPlugin(pluginId);
            var client = plugin?.Client;
            if (client?.IsInstalled != true)
            {
                return false;
            }

            client.Open();
            return true;
        }

        public bool TryShutdownClient(Guid pluginId)
        {
            var plugin = GetPlugin(pluginId);
            if (plugin?.Properties?.CanShutdownClient != true)
            {
                return false;
            }

            var client = plugin.Client;
            if (client == null)
            {
                return false;
            }

            client.Shutdown();
            return true;
        }

        internal static IReadOnlyList<CapsuleLibraryIntegrationState> BuildStates(
            IEnumerable<LibraryPlugin> plugins)
        {
            var loadedPlugins = (plugins ?? Enumerable.Empty<LibraryPlugin>())
                .Where(a => a != null)
                .GroupBy(a => a.Id)
                .Select(a => a.First())
                .ToDictionary(a => a.Id);

            var result = new List<CapsuleLibraryIntegrationState>();
            foreach (var definition in KnownIntegrations)
            {
                loadedPlugins.TryGetValue(definition.PluginId, out var plugin);
                result.Add(new CapsuleLibraryIntegrationState(definition, plugin));
                loadedPlugins.Remove(definition.PluginId);
            }

            foreach (var plugin in loadedPlugins.Values.OrderBy(a => a.Name))
            {
                var fallbackDefinition = new CapsuleLibraryIntegrationDefinition(
                    CapsuleLibraryPlatform.Other,
                    plugin.Name,
                    plugin.Id,
                    BuiltinExtensions.GetIsBuiltInPlugin(plugin.Id));

                result.Add(new CapsuleLibraryIntegrationState(fallbackDefinition, plugin));
            }

            return result;
        }

        private static CapsuleLibraryIntegrationDefinition BuiltIn(
            CapsuleLibraryPlatform platform,
            string displayName,
            BuiltinExtension extension)
        {
            return new CapsuleLibraryIntegrationDefinition(
                platform,
                displayName,
                BuiltinExtensions.GetIdFromExtension(extension),
                true);
        }
    }
}
