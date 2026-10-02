# Capsule inherited dependency security

The Playnite foundation currently restores with NuGet security advisories for several inherited packages.

Observed by the Capsule CI restore:

| Package | Inherited version | NuGet severity seen by CI | Capsule treatment |
| --- | --- | --- | --- |
| LiteDB | 4.1.4 | Critical | Database migration project; do not bump blindly |
| Newtonsoft.Json | 10.0.3 | High | Upgrade separately with serialization compatibility tests |
| AngleSharp | 0.9.9 | Moderate | Upgrade separately with HTML/parser regression tests |

## Why CI does not suppress this globally

Release builds inherit `TreatWarningsAsErrors=True`. A direct `dotnet test -c Release` therefore turned NuGet audit warnings into restore failures before tests could run.

Capsule CI restores first so advisories remain visible, then compiles the .NET Framework test project with the same full Visual Studio MSBuild family required by Playnite's COM references.

This is not a declaration that the inherited versions are acceptable long term. It keeps the security signal visible while allowing tests to execute.

## Migration order

### Newtonsoft.Json

Treat as the first candidate because it is primarily serialization infrastructure rather than the database engine itself.

Before changing it:

- cover settings/config round trips;
- cover game/model JSON used by plugins and services;
- verify old saved JSON continues to deserialize;
- verify extension-facing serialization behavior.

### AngleSharp

Upgrade independently from JSON/database work.

Before changing it:

- locate all parser entry points;
- capture representative HTML fixtures;
- compare parsed links/text/metadata before and after;
- adapt API changes without mixing the work into UI changes.

### LiteDB

Highest migration risk because Playnite data persistence depends on it.

Do not replace or major-upgrade it until Capsule has:

- automatic database backup before migration;
- a copy-based migration test using real/representative Playnite databases;
- schema/index compatibility checks;
- rollback on failed open/migration;
- launch/import/playtime regression tests after migration.

Database integrity takes precedence over removing a warning quickly.

## CI policy

Security advisories must remain visible in restore output.

Do not add global `NoWarn=NU190x` or disable NuGet audit just to get a green pipeline.

A future dependency-security job can make selected advisories blocking again after each migration is complete.
