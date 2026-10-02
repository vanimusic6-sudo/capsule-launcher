# Capsule onboarding backend

Capsule should eventually replace Playnite's desktop first-run wizard, but it must reuse the existing add-on delivery and compatibility pipeline.

## Current Playnite behavior

The desktop first-run wizard:

1. requests the default integration catalog from `ServicesClient.GetDefaultExtensions()`;
2. resolves the selected add-on with `ServicesClient.GetAddon()`;
3. asks the add-on manifest for the latest compatible package;
4. downloads that package to Playnite's temp directory;
5. queues it through `ExtensionInstaller`;
6. installs the queue;
7. loads library plugins and shows each plugin's settings/authentication UI.

The fullscreen app currently refuses first startup until that desktop wizard has completed.

## Shared package installer

`Playnite.Plugins.AddonPackageInstaller` now owns the repeated package download/queue step.

The existing desktop first-run wizard uses this shared implementation instead of carrying its own downloader code.

This matters for Capsule because onboarding can use the exact same compatibility and queue behavior rather than maintaining a second copy.

## Capsule provisioner

`CapsuleIntegrationProvisioner` is the backend-facing adapter intended for the future Capsule onboarding UI.

It can:

- fetch the recommended library + generic add-on catalog in one backend request;
- queue selected add-ons through the shared installer;
- install the queued packages using `ExtensionInstaller`.

The provisioner does not contain any UI and does not own account credentials.

## What remains intentionally unchanged

The fullscreen startup guard is still active.

Do not bypass `FirstTimeWizardComplete` until Capsule has a complete flow that can:

- show recommended libraries;
- download/install the selected integrations;
- host plugin-owned authentication/settings;
- commit/cancel each plugin's settings safely;
- import libraries;
- recover from network/package failures;
- mark first-run complete only after a usable state exists.

Until then, the existing desktop wizard remains the safe fallback.
