#!/usr/bin/bash
# Start rideaudit-caddy. Does not stop Octopus, SQL, or the Octopus API Caddy.
# SPDX-License-Identifier: GPL-2.0-only
set -eu
cd /home/sharpninja/rideaudit-caddy
echo "HOST=$(hostname)"
echo "DATE=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
want='sha256:ae4458638da8e1a91aafffb231c5f8778e964bca650c8a8cb23a7e8ac557aa3c'
if ! docker image inspect 'caddy:2.10.0-alpine' >/dev/null 2>&1; then
  if ! docker ps --format '{{.Names}}' | grep -qx 'octopus-legion2-octopus-api-tls-1'; then
    echo "BLOCKER=caddy:2.10.0-alpine is untagged and the Octopus Caddy container is absent. Refusing to pull."
    exit 2
  fi
  src=$(docker inspect octopus-legion2-octopus-api-tls-1 --format '{{.Image}}')
  if [ "$src" != "$want" ]; then
    echo "BLOCKER=Octopus Caddy image is $src, expected $want. Refusing to pull."
    exit 2
  fi
  docker tag "$src" caddy:2.10.0-alpine
  echo "RETAG=local $src as caddy:2.10.0-alpine. No pull."
else
  echo "IMAGE_TAG=caddy:2.10.0-alpine already present"
fi
if docker ps --format '{{.Names}}' | grep -qx 'octopus-legion2-octopus-api-tls-1'; then
  echo "OCTOPUS_CADDY=left-running"
else
  echo "OCTOPUS_CADDY=not-running"
fi
if ! docker ps --format '{{.Names}}' | grep -qx 'rideaudit-octopus-admission-1'; then
  echo "BLOCKER=rideaudit-octopus-admission-1 is not running"
  exit 5
fi
if ! docker ps --format '{{.Names}}' | grep -qx 'rideaudit-octopus-counsel-1'; then
  echo "BLOCKER=rideaudit-octopus-counsel-1 is not running"
  exit 5
fi
docker network inspect rideaudit-caddy-edge >/dev/null 2>&1 || docker network create rideaudit-caddy-edge
attach_edge() {
  local name="$1"
  local nets
  nets=$(docker inspect "$name" --format '{{range $k, $v := .NetworkSettings.Networks}}{{$k}} {{end}}')
  if printf '%s' "$nets" | grep -qw rideaudit-caddy-edge; then
    echo "NET_$name=already-attached"
  else
    docker network connect rideaudit-caddy-edge "$name"
    echo "NET_$name=attached"
  fi
}
attach_edge rideaudit-octopus-admission-1
attach_edge rideaudit-octopus-counsel-1
for port in 28443 28444; do
  owner=$(ss -lnt "sport = :$port" | awk 'NR>1 {print}')
  if [ -n "$owner" ]; then
    if docker ps --format '{{.Names}}' | grep -qx 'rideaudit-caddy'; then
      echo "PORT_$port=held-by-rideaudit-caddy-will-recreate"
    else
      echo "BLOCKER=port $port is already listening and rideaudit-caddy is not the holder"
      ss -lnt "sport = :$port" || true
      exit 3
    fi
  else
    echo "PORT_$port=free"
  fi
done
docker compose -f compose.yaml up -d --pull never --force-recreate
sleep 2
if ! docker ps --filter name=rideaudit-caddy --filter status=running --format '{{.Names}}' | grep -qx 'rideaudit-caddy'; then
  echo "BLOCKER=rideaudit-caddy is not running"
  docker logs rideaudit-caddy --tail 80 || true
  exit 4
fi
docker ps --filter name=rideaudit-caddy --format '{{.Names}}|{{.Image}}|{{.Status}}'
echo UP_OK
