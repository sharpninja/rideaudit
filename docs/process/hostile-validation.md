# Code generation and opposing-model hostile validation

**Author:** Sharp Ninja
**Status:** Process rule
**Scope:** Documentation, prompts, generated source, configuration, and any other generated project artifact.

## 1. Permitted code-generation models

Code generation may use **Grok `grok-4.6-xhigh`** or **GPT `gpt-6-sol` at `xhigh`**. The generator identity, provider, model, and reasoning setting must be recorded with the generated work. No other model is an approved generator unless this rule is amended in the repository.

## 2. Mandatory opposing-model hostile validation (HV)

Generated code **MUST** receive hostile validation from the opposing agent and model before it is accepted:

| Generator | Required hostile validator |
|---|---|
| Grok `grok-4.6-xhigh` | Codex / GPT `gpt-6-sol` at `xhigh` |
| GPT `gpt-6-sol` at `xhigh` | Grok `grok-4.6-xhigh` |

Self-review by the generating agent is not opposing-model HV and does not satisfy this rule. The validator must attack correctness, security, fail-closed behavior, data custody, requirements traceability, and other risks appropriate to the generated work. A failed, partial, unavailable, or unauthenticated validation is not a pass.

### Authentication limitation

Docs-only HV through ChatGPT-authenticated Codex **cannot use `gpt-6-sol` until API-key authentication is available**. Do not claim, summarize, or record a completed `gpt-6-sol` HV run when that authentication requirement is not met. An unavailable run may be recorded as unavailable or failed, but it must never be represented as a completed validation.

## 3. Immutable request/response retention

Every HV request and every HV response must be retained together as the exact JSON request/response pair. On receipt of the response, immediately write a UTF-8 JSON file under:

```text
docs/reviews/hv-pairs/
```

The pair must be committed to the repository immediately upon receipt, before relying on the result or accepting generated code. Do not replace the original response with a summary. If a tool returns metadata, errors, warnings, or refusal text, retain those in the response object as received and mark the run status accurately.

Use a filename containing the receipt timestamp and a topic slug, for example:

```text
docs/reviews/hv-pairs/2026-09-27T133600-0500-hostile-validation-auth.json
```

The timestamp must identify when the response was received; the topic slug must identify the generated work or review subject. Keep names filesystem-safe and unique. The commit containing the pair is the custody record; record its commit ID in the pair or associated review index when known.

## 4. Pair-file shape

Each pair file is a single JSON object with these required top-level members. The `request` and `response` objects preserve the provider payloads without prose rewriting; `generator` and `validator` identify the opposing roles.

```json
{
  "request": {
    "id": "hv-request-20260927T133600-0500-topic",
    "sent_at": "2026-09-27T13:36:00-05:00",
    "topic": "topic-slug",
    "payload": "exact request payload",
    "artifacts": ["path/to/generated/artifact"]
  },
  "response": {
    "id": "hv-response-20260927T133645-0500-topic",
    "received_at": "2026-09-27T13:36:45-05:00",
    "status": "completed",
    "payload": "exact response payload"
  },
  "generator": {
    "agent": "Grok",
    "provider": "xai",
    "model": "grok-4.6-xhigh",
    "reasoning_effort": "xhigh"
  },
  "validator": {
    "agent": "Codex",
    "provider": "openai",
    "model": "gpt-6-sol",
    "reasoning_effort": "xhigh"
  },
  "committed_at": "2026-09-27T13:36:50-05:00"
}
```

For an unavailable or failed run, use a truthful response status such as `unavailable` or `failed` and preserve the provider error. Never change that status to `completed` merely to satisfy the process.

## 5. Acceptance gate

A generated change is not accepted until the required opposing-model HV has been run (or explicitly recorded as unavailable/failed with no false pass), the exact pair is stored, and the pair is committed. Reviewers must check the pair path and commit before treating HV claims as evidence.

See [code-generation.md](code-generation.md) for the end-to-end generation checklist.
