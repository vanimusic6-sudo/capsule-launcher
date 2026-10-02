# Capsule update policy

Capsule is a forked product and must never replace itself with an upstream Playnite program update.

The inherited Playnite update loop performs two independent jobs:

- application/program update checks;
- add-on update and blacklist checks.

Those responsibilities must remain separable.

## Fork rule

Capsule starts the inherited update checker with program updates disabled.

This applies to both the fullscreen runtime and the retained desktop/developer runtime in this fork.

Add-on update checks remain available because library integrations such as Steam, Epic and Xbox are intentionally kept outside Capsule core.

## Why this matters

Pointing a fork at the upstream program updater can overwrite customized binaries and invalidate the Capsule UI/runtime contract.

Disabling the whole checker would also be wrong because it would leave library plugins stale and skip the inherited add-on blacklist safety check.

The base update checker therefore accepts an explicit `checkProgramUpdates` flag and remembers it for periodic checks.

## Future Capsule updater

A Capsule-owned updater can later replace the program-update side with:

- Capsule release manifests;
- signed release artifacts;
- rollback/version channels;
- migration policy;
- release notes specific to Capsule.

Until that exists, program updates are disabled rather than redirected to an unowned endpoint.
