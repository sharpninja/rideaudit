# Frontier probe: codex-subscription returns JSON

Host: PAYTON-LEGION2. Worktree: `F:\GitHub\rideaudit-ui-font`, branch `cursor/dual-phone-fold-moto-8aa2`. Recorded 2026-09-30T17:25:06Z.

This receipt is an independent probe of the SharpNinja.aiUnit `codex-subscription` profile. It is not a Fold or Edge visual suite result. No FR or AC is satisfied. The device catalog was not run.

## Profile (unchanged)

File: `tests/RideAudit.Client.Android.AiUnit.Tests/appsettings.aiunit.json`

| Field | Value |
| --- | --- |
| ActiveStrategy | `codex-subscription` |
| Kind | `cli` |
| Command | `codex` |
| Model | `(cli-managed)` |
| TimeoutSeconds | `900` |
| Temperature | `0.0` |

`AIUNIT_STRATEGY`, `AIUNIT_KIND`, `AIUNIT_COMMAND`, `AIUNIT_MODEL`, `AIUNIT_TIMEOUT_SECONDS`, and `AIUNIT_BASE_URL` were unset in the probe shell. `~/.codex/config.toml` and `~/.codex/hooks.json` were not edited.

Codex CLI: `codex-cli 0.158.0` at `C:\Users\kingd\AppData\Local\Programs\OpenAI\Codex\bin\codex.exe`.

Package: SharpNinja.aiUnit 3.0.0. `CliFrontierClient` starts `codex exec --skip-git-repo-check` and passes the whole prompt as one argument. It does not redirect standard input for `codex`. Image bytes are written under `%TEMP%\aiunit-cli-*` and the prompt contains `Image attachment (name): <path>`. The package does not pass `-i`.

Resolved client on the text probe: `isResolved=True`, `provider=codex-subscription:codex`, `name=codex-subscription`, `kind=cli`, `model=(cli-managed)`.

Live stderr banner (text call): model `gpt-6-sol`, provider `openai`, approval `on-request`, sandbox `workspace-write`, reasoning effort `high`.

## What hung

`codex exec` help for 0.158.0: if stdin is piped and a prompt is also provided, stdin is appended until EOF.

| Probe | Stdin | Elapsed | Result |
| --- | --- | --- | --- |
| Same argv as the package, text prompt `{"ok":true,"n":2}` | closed | 7856 ms | exit 0, stdout `{"ok":true,"n":2}` |
| aiUnit `SendAsync`, same text, probe budget 120 s | closed | 9138 ms | exit 0, `error=` empty, text `{"ok":true,"n":2}` |
| aiUnit `SendAsync`, same text | left open | killed at 25146 ms | resolve lines only, no JSON |
| aiUnit `SendAsync` after `Console.In.Close()` | left open | killed at 45152 ms | no JSON |
| aiUnit `SendAsync` after `SetStdHandle(STD_INPUT_HANDLE, NUL)` | left open | 8935 ms | exit 0, text `{"ok":true,"n":2}` |
| Package-style path prompt for a PNG, no extra flags | closed | killed at 20390 ms | stderr ended at `hook: PreToolUse`, stdout empty |

`dotnet test` keeps a stdin pipe open. The package child inherits it, so `codex exec` waits for EOF until `TimeoutSeconds`. That matches the earlier device rows that expired at exactly `00:01:00`, `00:03:00`, and `00:15:00`.

A second hang remains after stdin is closed. `~/.codex/hooks.json` runs `C:\Users\kingd\.claude\hooks\block-gh-pr.ps1` on Bash `PreToolUse`. That script calls `[Console]::In.ReadToEnd()`. When the model shells out to read a PNG, stderr stops at `hook: PreToolUse` and the process does not exit. A text prompt that needs no tool does not hit the hook.

## What returns JSON

Flags that produced `{"color":"blue"}` for a 1x1 PNG named `pixel.png` (pixel bytes `0,0,255,255`), with stdin closed, in 33487 ms, exit 0, about 12793 tokens:

- `--dangerously-bypass-hook-trust`
- `--dangerously-bypass-approvals-and-sandbox`
- `-c features.hooks=false`
- `-i` after the prompt

Stderr for that call set `approval: never` and `sandbox: danger-full-access`, then ran a local PowerShell read of the PNG and returned `{"color":"blue"}`. The filename does not contain the color.

`--dangerously-bypass-hook-trust` alone still stopped at `hook: PreToolUse` on a later image attempt. `-c features.hooks=false` alone did not finish inside 40 s. The combination above did.

## Repo change

`appsettings.aiunit.json` is unchanged. `Command` stays `codex`.

`CodexCliStdin.PrepareForExec` runs immediately before `CodexVisualGate` calls `SendAsync`. It prepends `codex-shim\codex.exe` to `PATH` and points the test process standard input at `NUL`.

`tests/RideAudit.CodexShim` is that `codex.exe`. The package still launches a command named `codex` with `exec --skip-git-repo-check <prompt>`. The shim:

- closes the child standard input so an open test-host pipe cannot append forever
- inserts the three flags above
- adds `-i <path>` for each `Image attachment (...): <path>` line

Proof through the shim, parent stdin left open:

| Call | Elapsed | Stdout |
| --- | --- | --- |
| Text `{"ok":true,"n":2}` | 9303 ms, exit 0 | `{"ok":true,"n":2}` |
| `pixel.png` color | 36041 ms, exit 0 | `{"color":"blue"}` (tool reported `R=0 G=0 B=255 A=255`) |

Captured child command line for the text call:

`codex.exe exec --dangerously-bypass-hook-trust --skip-git-repo-check "Return this JSON and nothing else: {\"ok\":true,\"n\":2}"`

The image call also carried the approval bypass, `features.hooks=false`, and `-i`.

Headless `dotnet test --filter FullyQualifiedName!~Device_` after the change: Passed 22, Failed 0. That includes the shim dump test. It does not include a device screenshot.

## Not claimed

The Fold and Edge visual suites were not run. A wireframe judge prompt with two full screenshots was not run. `TimeoutSeconds` is still 900. Within-threshold pixel ratios are not a pass. No FR, AC, or plan checkbox is closed. Interactive Codex on this machine still uses the user config. The sandbox bypass exists only inside the test shim.
