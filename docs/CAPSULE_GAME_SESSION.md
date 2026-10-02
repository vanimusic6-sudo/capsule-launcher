# Capsule game-session lifecycle

Capsule keeps Playnite's mature launch pipeline and adds only the presentation state needed for a console-like experience.

## Ownership boundary

Playnite continues to own:

- play/install/uninstall controllers;
- process tracking;
- launcher/client startup;
- playtime and running-state persistence;
- minimizing fullscreen after launch;
- restoring the fullscreen window after the game exits;
- post-game scripts and client shutdown.

Capsule does not duplicate any of that.

Capsule owns:

- whether expensive presentation work should be suspended;
- the UI return point that existed before launch;
- restoring the selected game and list/details mode after return;
- future launch/return transition presentation.

## Runtime state

`CapsuleGameSessionState` is created by `FullscreenApplication` and observes `GameControllerFactory`.

Its phases are:

- `Idle` — normal Capsule rendering.
- `Starting` — launch has begun.
- `Running` — game is running.
- `Returning` — the last game stopped and Capsule is restoring its UI return point.

`IsPresentationSuspended` is true for every phase except `Idle`.

Future animated Capsule surfaces should bind their expensive render/timer loops to this property. Do not merely reduce opacity while a game is active; stop the work.

## Return point

The inherited `FullscreenAppViewModel` is a partial class. Capsule-specific return-point behavior is isolated in:

`Capsule/Experience/FullscreenAppViewModel.CapsuleGameSession.cs`

Before launch it captures:

- selected game ID;
- whether the user was in the details view.

After Playnite restores the fullscreen window, Capsule restores that selection/view and only then marks the presentation as active again.

This intentionally keeps the inherited view-model changes to two calls:

- capture before launch UI focus is removed;
- restore after the normal stopped-game handling.

## Multiple running games

The session state tracks a set of active game IDs. Stopping one game does not resume Capsule presentation while another tracked game remains active.

## Design contract

When Capsule visuals are implemented:

- ambient shaders/particle fields/video loops must stop when `IsPresentationSuspended == true`;
- one-shot launch animations may finish before minimization, but must not leave a background render loop alive;
- returning from a game should animate from the already restored game card, not rebuild Home from zero;
- background library refresh must not intentionally replace the stored return point;
- startup cancellation uses the same return path as a normal game exit.
