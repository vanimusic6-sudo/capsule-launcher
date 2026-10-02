# Capsule fullscreen layer

Capsule-specific fullscreen code belongs here when it is not a generic Playnite concern.

Planned boundaries:

- `Presentation/` — motion, visual-state orchestration, transitions and presentation helpers.
- `Experience/` — startup, home, game-focus, launch and return flows.
- `Features/` — Capsule-only features such as the drop-folder importer.
- `Infrastructure/` — thin adapters around Playnite services used by Capsule code.
  - `Infrastructure/Integrations/` — store/library discovery, client state and plugin-owned fullscreen auth/settings hosting.

Do not duplicate database, controller, plugin or SDK logic here. Prefer adapters around the existing Playnite implementation so the inherited foundation stays testable and replaceable.

The integration boundary is documented in `docs/CAPSULE_INTEGRATIONS.md`.
