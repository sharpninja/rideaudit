#!/usr/bin/bash
# Start the RideAudit Octopus container stack. Secrets stay in .env (not git).
# FR-RIDE-063. SPDX-License-Identifier: GPL-2.0-only
set -eu
HERE="$(cd "$(dirname "$0")" && pwd)"
cd "$HERE"
test -f .env
docker compose --env-file .env -f compose.yaml up -d --pull missing
echo OCTOPUS_RIDEAUDIT_COMPOSE_UP
docker compose --env-file .env -f compose.yaml ps
