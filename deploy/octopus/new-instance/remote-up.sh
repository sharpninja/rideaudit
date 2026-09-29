#!/usr/bin/bash
# Start the RideAudit Octopus container stack. Secrets stay in .env (not git).
# Modes: reset | server | tentacle
# FR-RIDE-063. SPDX-License-Identifier: GPL-2.0-only
set -eu
HERE="$(cd "$(dirname "$0")" && pwd)"
cd "$HERE"
test -f .env

MODE="${1:-server}"

gid="$(getent group docker | cut -d: -f3 || true)"
if [ -n "$gid" ]; then
  if grep -q '^DOCKER_GID=' .env; then
    sed -i '/^DOCKER_GID=/d' .env
  fi
  printf 'DOCKER_GID=%s\n' "$gid" >> .env
  echo "DOCKER_GID_SET=$gid"
fi

if [ "$MODE" = "reset" ]; then
  echo "RESET_OCTOPUS_RIDEAUDIT_DATA (octopus-legion2 volumes are not touched)"
  docker compose --env-file .env -f compose.yaml --profile with-tentacle down || true
  vols="$(docker volume ls -q --filter name=octopus-rideaudit_ || true)"
  if [ -n "$vols" ]; then
    # shellcheck disable=SC2086
    docker volume rm $vols
  fi
  MODE="server"
fi

if [ "$MODE" = "server" ]; then
  docker compose --env-file .env -f compose.yaml up -d --pull missing db octopus
  echo OCTOPUS_RIDEAUDIT_SERVER_UP
  docker compose --env-file .env -f compose.yaml ps
fi

if [ "$MODE" = "tentacle" ]; then
  docker compose --env-file .env -f compose.yaml --profile with-tentacle up -d --pull missing tentacle
  echo OCTOPUS_RIDEAUDIT_TENTACLE_UP
  docker compose --env-file .env -f compose.yaml --profile with-tentacle ps
fi
