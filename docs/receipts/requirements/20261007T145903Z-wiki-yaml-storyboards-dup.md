# wiki.yaml Storyboards path duplicate fix

Written: 2026-10-07 09:59 CT (America/Chicago).
Machine: PAYTON-LEGION2
Path: F:\GitHub\rideaudit\docs\wiki.yaml

## Observation (before)
MCP error: navigation[6].children[0].path and children[1].path duplicate navigation path Storyboards.
Parsed: navigation[6] title=Storyboards path=Storyboards children=2
- children[0] title=Mobile Dual-Phone path=Storyboards
- children[1] title=Review App path=Storyboards

### Raw path: Storyboards lines before
- L139: `  path: Storyboards`
- L142: `    path: Storyboards`
- L151: `    path: Storyboards`

## Fix
Removed only the child-level duplicate path keys (4-space indent `path: Storyboards`).
Kept parent `path: Storyboards` and both children (Mobile Dual-Phone, Review App) with their document lists.
Did not invent new child path strings.
- removed L142: path: Storyboards
- removed L151: path: Storyboards

## After
- L139:   path: Storyboards
Child path fields now: [, ] (empty expected).


## generateDocument result

```
type: error
payload:
  requestId: req-20261007T145919Z-2ef7
  code: method_invocation_error
  message: 'Invalid docs/wiki.yaml: navigation[6].children[0].path is required.; navigation[6].children[1].path is required. (stage: config load)'
  retryable: false
  details:
    methodName: workflow.requirements.generateDocument
    exceptionType: McpServer.Client.McpValidationException

---


```

## STOP (Payton decision required)

After removing the duplicate child `path: Storyboards` keys, `generateDocument` returned a **new** error (not the original duplicate):

`navigation[6].children[0].path is required.; navigation[6].children[1].path is required.`

Per orders: do not invent unique child paths. Current on-disk state: parent keeps `path: Storyboards`; both children keep titles + document lists but have **no** path field.

Options for Payton (not applied):
- (A) Flatten: put all `document:` entries directly under the Storyboards node; drop nested Mobile Dual-Phone / Review App path nodes.
- (B) Unique child paths using existing titles as path strings (would be new path values).
- (C) Other structure Payton names.

