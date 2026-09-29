#!/usr/bin/bash
# Octopus target script: build RideAudit images on PAYTON-DESKTOP and run them.
# FR-RIDE-063 / UC-RIDE-032 / TR-RIDE-DEPLOY-001. No GHCR.
# SPDX-License-Identifier: GPL-2.0-only
set -eu

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

COMPOSE="$ROOT/deploy/octopus/compose.yaml"
ENVFILE="$ROOT/deploy/octopus/.env"
ADMISSION_DF="$ROOT/deploy/containers/admission/Dockerfile"
COUNSEL_DF="$ROOT/deploy/containers/counsel/Dockerfile"
RUNTIME_DF="$ROOT/deploy/omarchy/Dockerfile.runtime"
PUB="$ROOT/artifacts/omarchy-publish/admission"

if [ ! -f "$COMPOSE" ]; then
  echo "missing $COMPOSE — sync the tree from LEGION2 first." >&2
  exit 1
fi
if [ ! -f "$ENVFILE" ]; then
  cp "$ROOT/deploy/octopus/env.example" "$ENVFILE"
fi

echo "RIDEAUDIT_OCTOPUS_ROOT=$ROOT"
echo "RIDEAUDIT_OCTOPUS_HEAD=$(git rev-parse --short HEAD 2>/dev/null || echo unknown)"
echo "Building local images only. No GHCR pull or push."

assert_no_ghcr() {
  if docker image inspect "$1" --format '{{.RepoDigests}}' 2>/dev/null | grep -qi ghcr.io; then
    echo "refusing image $1 with a GHCR digest" >&2
    exit 2
  fi
}

build_from_containers() {
  docker build -f "$ADMISSION_DF" -t rideaudit-admission:octopus "$ROOT"
  docker build -f "$COUNSEL_DF" -t rideaudit-counsel:octopus "$ROOT"
}

build_from_runtime() {
  if [ ! -f "$PUB/RideAudit.Server.Admission.dll" ]; then
    echo "runtime fallback missing $PUB/RideAudit.Server.Admission.dll" >&2
    return 1
  fi
  docker build -f "$RUNTIME_DF" -t rideaudit-admission:octopus "$PUB"
  docker tag rideaudit-admission:octopus rideaudit-counsel:octopus
}

if [ -f "$ADMISSION_DF" ] && [ -f "$COUNSEL_DF" ]; then
  if ! build_from_containers; then
    echo "deploy/containers Dockerfiles failed; trying omarchy runtime image."
    build_from_runtime
  fi
else
  build_from_runtime
fi

assert_no_ghcr rideaudit-admission:octopus
assert_no_ghcr rideaudit-counsel:octopus

echo "Compose up rideaudit-octopus only. Other stacks stay up."
docker compose -f "$COMPOSE" --env-file "$ENVFILE" up -d --no-build --pull never

echo "Waiting for admission on 192.168.0.149:28080"
ok=0
for i in 1 2 3 4 5 6 7 8 9 10 11 12; do
  code="$(curl -s -o /tmp/rideaudit-octopus-admission-body.txt -w '%{http_code}' --max-time 5 http://192.168.0.149:28080/ || true)"
  echo "ADMISSION_HTTP=$code attempt=$i"
  if [ "$code" = "200" ]; then
    ok=1
    break
  fi
  sleep 5
done
if [ "$ok" != "1" ]; then
  echo "admission probe failed" >&2
  docker compose -f "$COMPOSE" ps || true
  exit 1
fi

echo "ADMISSION_BODY=$(tr '\n' ' ' </tmp/rideaudit-octopus-admission-body.txt)"
docker image inspect rideaudit-admission:octopus --format 'ADMISSION_IMAGE={{.Id}} ADMISSION_CREATED={{.Created}}'
docker image inspect rideaudit-counsel:octopus --format 'COUNSEL_IMAGE={{.Id}} COUNSEL_CREATED={{.Created}}'
docker compose -f "$COMPOSE" ps
echo "RIDEAUDIT_OCTOPUS_DEPLOY_OK"
