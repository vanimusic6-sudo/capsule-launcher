using Playnite.Plugins;
using Playnite.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Playnite.FullscreenApp.Capsule.Infrastructure.Integrations
{
    public sealed class CapsuleRecommendedAddon
    {
        public string Name { get; }
        public string AddonId { get; }

        public CapsuleRecommendedAddon(string name, string addonId)
        {
            Name = name;
            AddonId = addonId;
        }
    }

    public sealed class CapsuleIntegrationRecommendations
    {
        public IReadOnlyList<CapsuleRecommendedAddon> Libraries { get; }
        public IReadOnlyList<CapsuleRecommendedAddon> GenericAddons { get; }

        public CapsuleIntegrationRecommendations(
            IReadOnlyList<CapsuleRecommendedAddon> libraries,
            IReadOnlyList<CapsuleRecommendedAddon> genericAddons)
        {
            Libraries = libraries ?? Array.Empty<CapsuleRecommendedAddon>();
            GenericAddons = genericAddons ?? Array.Empty<CapsuleRecommendedAddon>();
        }
    }

    /// <summary>
    /// Backend-only provisioning path for Capsule onboarding.
    ///
    /// It intentionally reuses Playnite's ServicesClient and ExtensionInstaller pipeline.
    /// Capsule can provide its own UI without owning package compatibility or downloads.
    /// </summary>
    public sealed class CapsuleIntegrationProvisioner
    {
        private readonly ServicesClient servicesClient;
        private readonly AddonPackageInstaller packageInstaller;

        public CapsuleIntegrationProvisioner()
            : this(new ServicesClient())
        {
        }

        public CapsuleIntegrationProvisioner(ServicesClient servicesClient)
        {
            this.servicesClient = servicesClient ?? throw new ArgumentNullException(nameof(servicesClient));
            packageInstaller = new AddonPackageInstaller(servicesClient);
        }

        public CapsuleIntegrationRecommendations GetRecommendations()
        {
            var recommended = servicesClient.GetDefaultExtensions();
            return new CapsuleIntegrationRecommendations(
                ToList(recommended?.Libraries),
                ToList(recommended?.Generic));
        }

        public void QueueInstall(string addonId)
        {
            packageInstaller.QueueInstall(addonId);
        }

        public IReadOnlyList<ExtensionInstallResult> InstallQueuedPackages()
        {
            return ExtensionInstaller.InstallExtensionQueue();
        }

        private static IReadOnlyList<CapsuleRecommendedAddon> ToList(
            Dictionary<string, string> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<CapsuleRecommendedAddon>();
            }

            return source
                .Where(a => !string.IsNullOrWhiteSpace(a.Key) && !string.IsNullOrWhiteSpace(a.Value))
                .OrderBy(a => a.Key)
                .Select(a => new CapsuleRecommendedAddon(a.Key, a.Value))
                .ToList();
        }
    }
}
