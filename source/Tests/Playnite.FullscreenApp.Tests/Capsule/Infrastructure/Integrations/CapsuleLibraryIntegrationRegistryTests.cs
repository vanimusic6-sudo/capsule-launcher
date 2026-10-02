using NUnit.Framework;
using Playnite.FullscreenApp.Capsule.Infrastructure.Integrations;
using Playnite.SDK;
using System;
using System.Linq;

namespace Playnite.FullscreenApp.Tests.Capsule.Infrastructure.Integrations
{
    [TestFixture]
    public class CapsuleLibraryIntegrationRegistryTests
    {
        [Test]
        public void KnownIntegrationsUseStableBuiltInIds()
        {
            var integrations = CapsuleLibraryIntegrationRegistry.KnownIntegrations;

            Assert.AreEqual(
                BuiltinExtensions.GetIdFromExtension(BuiltinExtension.SteamLibrary),
                integrations.Single(a => a.Platform == CapsuleLibraryPlatform.Steam).PluginId);

            Assert.AreEqual(
                BuiltinExtensions.GetIdFromExtension(BuiltinExtension.EpicLibrary),
                integrations.Single(a => a.Platform == CapsuleLibraryPlatform.EpicGames).PluginId);

            Assert.AreEqual(
                BuiltinExtensions.GetIdFromExtension(BuiltinExtension.XboxLibrary),
                integrations.Single(a => a.Platform == CapsuleLibraryPlatform.Xbox).PluginId);
        }

        [Test]
        public void KnownIntegrationIdsAreUniqueAndNonEmpty()
        {
            var integrations = CapsuleLibraryIntegrationRegistry.KnownIntegrations;

            Assert.IsFalse(integrations.Any(a => a.PluginId == Guid.Empty));
            Assert.AreEqual(
                integrations.Count,
                integrations.Select(a => a.PluginId).Distinct().Count());
        }
    }
}
