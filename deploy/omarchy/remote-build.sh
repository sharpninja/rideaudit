#!/bin/bash
# Build admission/counsel images on PAYTON-OMARCHY. Does not compose up.
# SPDX-License-Identifier: GPL-2.0-only
set -eu
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"
echo "Building from $ROOT commit $(git rev-parse --short HEAD 2>/dev/null || echo unknown)"
docker build -f deploy/containers/admission/Dockerfile -t rideaudit-admission:local .
docker build -f deploy/containers/counsel/Dockerfile -t rideaudit-counsel:local .
echo "Tagged rideaudit-admission:local and rideaudit-counsel:local. Not a CD green. Containers were not started."
docker image inspect rideaudit-admission:local --format 'admission={{.Id}} created={{.Created}}'
docker image inspect rideaudit-counsel:local --format 'counsel={{.Id}} created={{.Created}}'