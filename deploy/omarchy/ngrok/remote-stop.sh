#!/usr/bin/bash
# Stop the RideAudit ngrok tunnel. Do not advertise a URL after this.
# SPDX-License-Identifier: GPL-2.0-only
set -euo pipefail

STATE_DIR="$HOME/.local/state/rideaudit-ngrok"
PID_FILE="$STATE_DIR/ngrok.pid"
export XDG_RUNTIME_DIR="${XDG_RUNTIME_DIR:-/run/user/$(id -u)}"

if command -v systemctl >/dev/null 2>&1 && [ -d "$XDG_RUNTIME_DIR" ]; then
  systemctl --user stop rideaudit-ngrok.service >/dev/null 2>&1 || true
  systemctl --user disable rideaudit-ngrok.service >/dev/null 2>&1 || true
fi

if [ -f "$PID_FILE" ]; then
  old="$(cat "$PID_FILE" || true)"
  if [ -n "${old}" ] && kill -0 "$old" 2>/dev/null; then
    kill "$old" || true
    sleep 1
    if kill -0 "$old" 2>/dev/null; then
      kill -9 "$old" || true
    fi
  fi
  rm -f "$PID_FILE"
fi

pkill -f 'ngrok http 192.168.0.149:28080' >/dev/null 2>&1 || true
pkill -f 'ngrok http 127.0.0.1:18080' >/dev/null 2>&1 || true
echo "NGROK_STOPPED=1"
echo "PUBLIC_URL_ADVERTISED=0"
