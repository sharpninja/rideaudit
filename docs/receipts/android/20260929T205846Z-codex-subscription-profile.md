# aiUnit profile codex-subscription

TimestampUtc: 2026-09-29T20:58:46Z
Host: PAYTON-LEGION2
SPDX: GPL-2.0-only

SharpNinja.aiUnit 3.0.0 selects a named strategy. The profile name for RideAudit Android visual tests is `codex-subscription`. The package reads `AIUNIT_STRATEGY` first, then `AiUnit.ActiveStrategy` in `appsettings.aiunit.json`, then falls back to `claude`.

## Wiring

File: `tests/RideAudit.Client.Android.AiUnit.Tests/appsettings.aiunit.json`

- `AiUnit.ActiveStrategy` is `codex-subscription`.
- `Strategies.codex-subscription` is Kind `cli`, Command `codex`, Model `(cli-managed)`.
- `TimeoutSeconds` is 60. That is the harness fail-closed budget on this profile. The package sample uses 900.

`CodexSubscriptionProfile` sets `AIUNIT_STRATEGY` to `codex-subscription` when the variable is empty, before the fixture resolves. A different value throws when the test assembly loads.

`CodexVisualGate.RequireCodexSubscriptionProfile` asks `AiUnitStrategyLoader.ResolveActive` for the selected name. It fails closed unless that name is exactly `codex-subscription` and the settings stay Kind `cli`, Command `codex`, Model `(cli-managed)`. The name `codex` is a different strategy.

## How to fix a failed selection

1. Keep `appsettings.aiunit.json` in the test project and copy it to the test output. Set `ActiveStrategy` to `codex-subscription` and add that exact `Strategies` key.
2. Set `AIUNIT_STRATEGY` to `codex-subscription`, or unset it. Any other value replaces the profile. An empty `ActiveStrategy` with no env var selects `claude`.
3. Unset `AIUNIT_KIND`, `AIUNIT_COMMAND`, and `AIUNIT_MODEL`, or set them to `cli`, `codex`, and `(cli-managed)`.
4. Run `codex login` when the Codex CLI is not authenticated. A missing client stays fail-closed. It is not a skip.

## Check

`dotnet test tests/RideAudit.Client.Android.AiUnit.Tests/RideAudit.Client.Android.AiUnit.Tests.csproj --filter FullyQualifiedName~Codex_subscription_profile_is_the_active_strategy`

Passed 1, Failed 0, Skipped 0, Duration 101 ms. The resolved strategy name was `codex-subscription`, kind `cli`, model `(cli-managed)`, provider `codex-subscription:codex`.

This check does not rerun the Fold 4 screenshot suite. The 2026-09-29T20:47:07Z usability receipt still stands. Pixel agreement, usability agreement, and AC-UC-025-001 stay open. `isSatisfied` stays false.
