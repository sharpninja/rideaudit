#!/usr/bin/bash
# Stop the RideAudit ngrok tunnel. Do not advertise a URL after this.
# SPDX-License-Identifier: GPL-2.0-only
set -euo pipefail

STATE_DIR="$HOME/.local/state/rideaudit-ngrok"
PID_FILE="$STATE_DIR/ngrok.pid"
CONFIG="${RIDEAUDIT_NGROK_CONFIG:-$HOME/.config/ngrok/ngrok.yml}"
LOG_FILE="$STATE_DIR/ngrok.log"
export XDG_RUNTIME_DIR="${XDG_RUNTIME_DIR:-/run/user/$(id -u)}"

if command -v systemctl >/dev/null 2>&1 && [ -d "$XDG_RUNTIME_DIR" ]; then
  systemctl --user stop rideaudit-ngrok.service >/dev/null 2>&1 || true
  systemctl --user disable rideaudit-ngrok.service >/dev/null 2>&1 || true
fi

is_our_ngrok() {
  # True only for the agent this script starts: an ngrok http tunnel using this
  # script's --config and --log paths. Any other process (including another
  # ngrok tunnel under the same account) is never signaled.
  local pid="$1" cmd=""
  if [ -r "/proc/$pid/cmdline" ]; then
    cmd="$(tr '\0' ' ' < "/proc/$pid/cmdline" 2>/dev/null || true)"
  fi
  case "$cmd" in
    *ngrok\ http\ *) ;;
    *) return 1 ;;
  esac
  case "$cmd" in
    *" --config $CONFIG "*) ;;
    *) return 1 ;;
  esac
  case "$cmd" in
    *" --log $LOG_FILE "*) return 0 ;;
  esac
  return 1
}

if [ -f "$PID_FILE" ]; then
  old="$(cat "$PID_FILE" || true)"
  # Only signal the recorded PID when it is still an ngrok http tunnel (PIDs get reused).
  if [ -n "${old}" ] && kill -0 "$old" 2>/dev/null && is_our_ngrok "$old"; then
    kill "$old" || true
    sleep 1
    if kill -0 "$old" 2>/dev/null; then
      kill -9 "$old" || true
    fi
  fi
  rm -f "$PID_FILE"
fi
rm -f "$STATE_DIR/ngrok.addr"

pkill -f 'ngrok http 192.168.1.182:28080' >/dev/null 2>&1 || true
pkill -f 'ngrok http 127.0.0.1:18080' >/dev/null 2>&1 || true
echo "NGROK_STOPPED=1"
echo "PUBLIC_URL_ADVERTISED=0"
