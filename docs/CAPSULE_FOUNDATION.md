# Capsule foundation map

This document is the guard rail for early Capsule Launcher work. The goal is to move quickly on the experience without throwing away the parts of Playnite that already solve difficult platform problems.

## Preserve first

Do not rewrite these areas during UI work:

- `source/Playnite/Database` — database, migrations and persistence.
- `source/Playnite/Controllers` — launch/install/uninstall plumbing.
- plugin and extension loading — library integrations and add-on compatibility.
- `source/Playnite/Input` and the SDL controller event loop in `FullscreenApplication`.
- `source/PlayniteSDK` — public contracts used by plugins.
- shared game/session/editor logic used by both desktop and fullscreen apps.

Changes here need a functional reason and regression testing.

## Capsule-owned presentation

The first implementation target is `source/Playnite.FullscreenApp`.

High-leverage areas:

- `Themes/Fullscreen/Default/Views/*.xaml`
- `Themes/Fullscreen/Default/Constants.xaml`
- `Themes/Fullscreen/Default/Media.xaml`
- fullscreen styles, templates and assets
- `SplashScreen.png`
- fullscreen audio and sound policy
- layout, transitions and visual states

The old `Default` path is retained temporarily so we do not create a giant path/namespace rename before the new UI exists.

## WPF template contracts

Fullscreen XAML is coupled to backing controls through named `PART_*` elements.

In `Controls/Views/Main.cs`, the most important contracts are:

- `PART_MainHost` — receives viewport dimensions.
- `PART_ViewHost` — owns global keyboard/controller bindings.
- `PART_ListGameItems` — receives the game collection, selection, focus, tile panel and activation bindings.
- `PART_ImageBackground` — selected-game background binding/effects.
- `PART_ElemGameDetails` and `PART_ElemGameStatus` — details/session overlays.

Most other parts are null-checked, so removing one usually removes the associated feature rather than immediately crashing. Rename a `PART_*` only when the backing control changes in the same commit.

`GameDetails.cs` uses the same contract pattern for cover, background, description, scroll view and action buttons.

## Cosmetic-looking things that are architectural

Do not casually rename these yet:

- executable/process names — instance detection checks `Playnite.DesktopApp` and `Playnite.FullscreenApp`;
- mutex/pipe/IPC identifiers;
- user-data/database paths;
- public SDK namespaces/types;
- theme/plugin API IDs;
- updater package format.

Visible Capsule branding can land before these internals are migrated.

## Fullscreen plumbing worth keeping

`FullscreenApplication` already gives us:

- SDL controller hot-plug handling;
- a dedicated SDL event thread;
- controller navigation mapping;
- interface/activation/background audio channels;
- active/background behavior;
- plugin loading before the main view;
- shared launch/session infrastructure.

The SDL loop intentionally stays on one thread. Do not turn it into a thread-pool/async loop.

## Viewbox warning

`Windows/MainWindow.xaml` wraps the main view in a `Viewbox`, giving themes a 1920×1080-style design surface scaled to the actual window.

Capsule may later move to a responsive layout, but removing the Viewbox is a migration. Validate 16:9, 16:10, ultrawide and non-integer Windows scaling before doing it.

## Desktop mode strategy

Keep the desktop app as a maintenance/settings escape hatch for the first Capsule versions. Rebuild the fullscreen consumer experience first instead of rebuilding every management dialog at once.

## Updater/services rule

Never use the original Playnite AppVeyor/SignPath secrets or release infrastructure. Development builds must rely on public dependencies and Capsule-owned CI only.

A Capsule updater/backend is a separate milestone. Never point a Capsule release at an upstream Playnite package just to make update UI work.

## First vertical slice

1. Capsule startup/splash.
2. Home screen with a small game rail.
3. selected-game hero/background transition.
4. controller-first focus motion and sound.
5. launch a game.
6. suspend expensive presentation work while the game is active.
7. return to the same selected game after exit.

Only then expand into deeper store-specific UX, social surfaces, achievements or large settings work.


## Inherited dependency debt

The first clean CI pass reports security advisories on inherited Playnite 10 dependencies, including AngleSharp 0.9.9, LiteDB 4.1.4 and Newtonsoft.Json 10.0.3.

Do not mass-upgrade these during UI work.

- LiteDB is part of persistence/database behavior. Any version migration must be tested against copied real libraries and rollback cases.
- Newtonsoft.Json is used broadly enough that a major jump needs serialization compatibility tests.
- Browser/HTML dependencies should be upgraded in an isolated security branch with startup, metadata, description and plugin regression checks.

Treat these advisories as tracked inherited debt, not as a reason to destabilize the foundation before the Capsule presentation layer is separated.
