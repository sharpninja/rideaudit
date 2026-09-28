#!/bin/bash
# Build admission from a LEGION2 linux-x64 publish tree. Does not compose up.
# SPDX-License-Identifier: GPL-2.0-only
set -eu
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
PUB="$ROOT/artifacts/omarchy-publish/admission"
cd "$ROOT"
if [ ! -f "$PUB/RideAudit.Server.Admission.dll" ]; then
  echo "missing $PUB/RideAudit.Server.Admission.dll — run Publish-Admission.ps1 and Sync-Publish.ps1 first." >&2
  exit 1
fi
echo "Runtime-building from $PUB commit $(git rev-parse --short HEAD 2>/dev/null || echo unknown)"
docker build -f deploy/omarchy/Dockerfile.runtime -t rideaudit-admission:local "$PUB"
docker tag rideaudit-admission:local rideaudit-counsel:local
echo "Tagged rideaudit-admission:local and rideaudit-counsel:local from publish tree. Not a CD green. Containers were not started."
docker image inspect rideaudit-admission:local --format 'admission={{.Id}} created={{.Created}}'
