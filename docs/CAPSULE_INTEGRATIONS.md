# Capsule library integrations

Capsule Launcher keeps store-specific authentication and library discovery outside the fullscreen experience.

The rule is simple:

> Capsule owns the experience. Library plugins own vendor protocols.

This lets Capsule feel like one console while Steam, Epic, Xbox, GOG, Ubisoft and other providers remain isolated behind Playnite's existing `LibraryPlugin` contract.

## Why this boundary exists

Store APIs and login flows change independently. Reimplementing each provider inside Capsule would create a permanent maintenance and security burden.

The inherited Playnite foundation already provides:

- extension discovery/loading through `ExtensionFactory`;
- a common `LibraryPlugin` contract;
- game import through `GameDatabase.ImportGames`;
- per-plugin client launch/shutdown through `LibraryClient`;
- plugin-owned settings and authentication surfaces;
- game launch/install/uninstall controllers that remain independent from Capsule presentation.

Capsule should compose those pieces instead of replacing them.

## Capsule integration layer

The first adapter lives in:

`source/Playnite.FullscreenApp/Capsule/Infrastructure/Integrations/`

It contains three small concepts.

### Integration definitions and state

`CapsuleLibraryIntegrationDefinition` maps the well-known PC libraries to stable built-in plugin IDs.

Initial platforms:

- Steam
- Xbox
- Epic Games
- GOG
- Ubisoft Connect
- EA app
- Battle.net
- Amazon Games
- itch.io
- Humble

Loaded third-party/community library plugins are not rejected. They are surfaced dynamically as `Other`, so the Capsule UI can support future integrations without a core update.

`CapsuleLibraryIntegrationState` is a snapshot intended for presentation. It exposes whether a plugin is loaded, whether it has settings, whether its client is installed and whether the plugin can close that client.

### Registry

`CapsuleLibraryIntegrationRegistry` adapts `ExtensionFactory.LibraryPlugins`.

The fullscreen UI should ask this registry for store state instead of reaching into `ExtensionFactory` itself. It can also open or shut down a provider client through the plugin's own `LibraryClient`.

Library refresh remains owned by the existing fullscreen view model for now:

`MainViewModelBase.UpdateLibrary(LibraryPlugin plugin)`

Do not duplicate import logic in Capsule.

### Fullscreen settings/auth session

Playnite's normal `Plugin.OpenSettingsView()` intentionally does not open in fullscreen mode.

`CapsulePluginSettingsSession` provides the missing orchestration without copying vendor auth logic. It requests the plugin's own `GetSettings()` and `GetSettingsView()`, applies the expected edit/verify/commit lifecycle and returns the `UserControl` for a Capsule-owned fullscreen host.

This is the preferred v0.x path for account connection:

1. Capsule opens its own "Connect libraries" screen.
2. User chooses Steam/Epic/Xbox/etc.
3. Capsule creates a `CapsulePluginSettingsSession`.
4. The plugin's existing account/login control is hosted inside a Capsule modal/page.
5. Capsule commits or cancels through the plugin's own settings contract.
6. Capsule calls the existing single-library refresh.
7. The user returns to Capsule UI without ever entering Playnite desktop settings.

Provider-specific Capsule adapters should only be added when a plugin cannot expose a usable fullscreen settings surface.

## First-run blocker

The inherited fullscreen startup currently refuses to start before Playnite's first-time wizard is complete and redirects to the desktop app.

That is acceptable while Capsule is still a development shell, but it is not acceptable for the console experience.

A future Capsule onboarding milestone must replace that redirect with:

- language/basic preferences;
- library selection;
- account connection;
- first library import;
- controller confirmation;
- optional startup-with-Windows / shell-like behavior.

Do not remove the old first-run guard until that flow exists.

## Packaging and updates

The provider implementations are separate extensions, not Capsule core.

For development, Capsule should work with installed compatible library plugins. For distribution, decide explicitly which integrations are shipped or installed during onboarding and review their licenses/dependencies before bundling them.

Do not hard-code Capsule against an external repository URL or vendor API just to make onboarding look integrated.

## Security rules

- Never store Steam/Epic/Xbox/etc passwords in Capsule.
- Never duplicate plugin session tokens into Capsule settings.
- Let provider plugins own their existing credential/session storage.
- Never log cookies, auth codes, refresh tokens or complete redirect URLs containing credentials.
- Capsule may store only presentation-level state such as "show this provider on Home" or "last refresh time".
- Treat a provider plugin failure as isolated: one broken store must not prevent Capsule from starting or showing games from other stores.

## Next backend milestones

1. Wire the registry into a Capsule fullscreen view model.
2. Add a generic fullscreen host for `CapsulePluginSettingsSession.View`.
3. Add one-library refresh and progress state.
4. Replace the inherited desktop first-run redirect with Capsule onboarding.
5. Add integration health/error states per provider.
6. Decide packaging/update policy for the selected official integrations.
7. Add launch-return polish: suppress expensive Capsule visuals while a game is active and restore the exact previous selection after exit.
