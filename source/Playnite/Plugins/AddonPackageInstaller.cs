using Playnite.Common;
using Playnite.Common.Web;
using Playnite.Services;
using System;
using System.IO;

namespace Playnite.Plugins
{
    /// <summary>
    /// Shared download/queue path for add-ons selected by onboarding experiences.
    /// Vendor/library discovery stays in ServicesClient while package compatibility
    /// and installation stay in the existing manifest/ExtensionInstaller pipeline.
    /// </summary>
    public sealed class AddonPackageInstaller
    {
        private readonly ServicesClient servicesClient;

        public AddonPackageInstaller(ServicesClient servicesClient)
        {
            this.servicesClient = servicesClient ?? throw new ArgumentNullException(nameof(servicesClient));
        }

        public AddonManifest QueueInstall(string addonId)
        {
            if (addonId.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Addon ID must be provided.", nameof(addonId));
            }

            var addon = servicesClient.GetAddon(addonId);
            if (addon == null)
            {
                throw new InvalidOperationException($"Addon '{addonId}' was not found.");
            }

            var package = addon.InstallerManifest?.GetLatestCompatiblePackage();
            if (package == null)
            {
                throw new InvalidOperationException($"Addon '{addonId}' has no compatible package.");
            }

            var localPath = addon.GetTargetDownloadPath();
            FileSystem.DeleteFile(localPath);
            FileSystem.PrepareSaveFile(localPath);

            if (package.PackageUrl.IsHttpUrl())
            {
                HttpDownloader.DownloadFile(package.PackageUrl, localPath);
            }
            else
            {
                File.Copy(package.PackageUrl, localPath, true);
            }

            ExtensionInstaller.QueuePackageInstall(localPath);
            return addon;
        }
    }
}
