# Capsule Launcher

> Experimental Windows gaming shell built on top of Playnite 10.

Capsule Launcher is a controller-first gaming environment for Windows. The project keeps Playnite's mature library, launch, metadata, plugin and controller foundations while replacing the presentation layer with a new Capsule experience focused on motion, sound, clarity and low background overhead while a game is running.

## Current status

Foundation stage. The upstream Playnite core is intentionally being kept stable while the fullscreen presentation layer is mapped and isolated for replacement.

Development branch: `next/capsule-foundation`.

## Architecture rule

We do **not** mass-rename or rewrite Playnite internals just for branding. The first goal is to preserve the reliable parts of Playnite and build Capsule around clear seams:

- **Keep stable:** game database, library/import plumbing, launch/session logic, plugin loading, controller input, migrations and low-level services.
- **Capsule-owned:** fullscreen layout, visual language, transitions, sound design, startup/return experience and Capsule-specific features.
- **Change carefully:** WPF controls with `PART_*` contracts, updater/services wiring, process names, IPC, paths and extension API surfaces.

See [docs/CAPSULE_FOUNDATION.md](docs/CAPSULE_FOUNDATION.md) before changing fullscreen or core code.

## Building

A GitHub Actions development build is provided in `.github/workflows/build.yml`. It uses only public dependencies and does not use the original Playnite AppVeyor/SignPath credentials.

The upstream build system is still used underneath so we keep a reproducible baseline while the fork evolves.

## Upstream and attribution

Capsule Launcher is currently a fork/derivative of [Playnite](https://github.com/JosefNemec/Playnite), created by Josef Nemec and contributors. Playnite is licensed under the MIT License. The original license and notices are retained in this repository.

Capsule-specific code and branding will be developed separately from the upstream Playnite identity.

## Security

Never commit API keys, signing certificates, access tokens or production service credentials. Development builds must remain reproducible without private Playnite infrastructure.
