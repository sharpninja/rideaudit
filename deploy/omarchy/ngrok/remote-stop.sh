#!/usr/bin/bash
# Stop the RideAudit ngrok tunnel. Do not advertise a URL after this.
# SPDX-License-Identifier: GPL-2.0-only
set -euo pipefail

STATE_DIR="$HOME/.local/state/rideaudit-ngrok"
PID_FILE="$STATE_DIR/ngrok.pid"
CONFIG_FILE="$STATE_DIR/ngrok.config"
CONFIG="${RIDEAUDIT_NGROK_CONFIG:-$HOME/.config/ngrok/ngrok.yml}"
LOG_FILE="$STATE_DIR/ngrok.log"
export XDG_RUNTIME_DIR="${XDG_RUNTIME_DIR:-/run/user/$(id -u)}"

# remote-start.sh records the config the agent was launched with. Ownership follows that
# recorded path as well as the current one, so a changed RIDEAUDIT_NGROK_CONFIG cannot
# orphan a running tunnel.
RECORDED_CONFIG=""
if [ -f "$CONFIG_FILE" ]; then
  RECORDED_CONFIG="$(cat "$CONFIG_FILE" || true)"
fi

if command -v systemctl >/dev/null 2>&1 && [ -d "$XDG_RUNTIME_DIR" ]; then
  # The unit's agent logs to stdout, so is_our_ngrok below cannot see it. Track it by the
  # unit's MainPID instead and fail closed if the unit or that process survives the stop.
  unit_pid="$(systemctl --user show -p MainPID --value rideaudit-ngrok.service 2>/dev/null || true)"
  systemctl --user stop rideaudit-ngrok.service >/dev/null 2>&1 || true
  systemctl --user disable rideaudit-ngrok.service >/dev/null 2>&1 || true
  if systemctl --user is-active --quiet rideaudit-ngrok.service 2>/dev/null; then
    echo "FAIL_CLOSED=ngrok_systemd_unit_still_active"
    exit 1
  fi
  if [ -n "${unit_pid}" ] && [ "${unit_pid}" != "0" ] && kill -0 "$unit_pid" 2>/dev/null; then
    unit_cmd="$(tr '\0' ' ' < "/proc/$unit_pid/cmdline" 2>/dev/null || true)"
    case "$unit_cmd" in
      *ngrok\ http\ *)
        echo "FAIL_CLOSED=ngrok_systemd_agent_still_running pid=${unit_pid}"
        exit 1
        ;;
    esac
  fi
fi

is_our_ngrok() {
  # True only for the agent this script family starts: an ngrok http tunnel using the
  # current or recorded --config path and this tree's --log path. Any other process
  # (including another ngrok tunnel under the same account) is never signaled.
  local pid="$1" cmd="" cfg
  if [ -r "/proc/$pid/cmdline" ]; then
    cmd="$(tr '\0' ' ' < "/proc/$pid/cmdline" 2>/dev/null || true)"
  fi
  case "$cmd" in
    *ngrok\ http\ *) ;;
    *) return 1 ;;
  esac
  case "$cmd" in
    *" --log $LOG_FILE "*) ;;
    *) return 1 ;;
  esac
  for cfg in "$CONFIG" "$RECORDED_CONFIG"; do
    [ -n "$cfg" ] || continue
    case "$cmd" in
      *" --config $cfg "*) return 0 ;;
    esac
  done
  return 1
}

our_agents() {
  # Agents this script family started, with or without a PID file (canonical :28080 and
  # the prior interim :18080). Every candidate passes is_our_ngrok.
  local backend pid
  for backend in 192.168.1.182:28080 127.0.0.1:18080; do
    for pid in $(pgrep -f "ngrok http $backend " 2>/dev/null || true); do
      if is_our_ngrok "$pid"; then
        echo "$pid"
      fi
    done
  done
}

if [ -f "$PID_FILE" ]; then
  old="$(cat "$PID_FILE" || true)"
  # Only signal the recorded PID when it is still our ngrok agent (PIDs get reused).
  if [ -n "${old}" ] && kill -0 "$old" 2>/dev/null && is_our_ngrok "$old"; then
    kill "$old" || true
    sleep 1
    if kill -0 "$old" 2>/dev/null; then
      kill -9 "$old" || true
    fi
  fi
fi

for pid in $(our_agents); do
  kill "$pid" 2>/dev/null || true
done
sleep 1
for pid in $(our_agents); do
  kill -9 "$pid" 2>/dev/null || true
done

remaining="$(our_agents | tr '\n' ' ')"
if [ -n "${old:-}" ] && kill -0 "$old" 2>/dev/null && is_our_ngrok "$old"; then
  remaining="$remaining $old"
fi
if [ -n "${remaining// /}" ]; then
  # Keep the state files so a later stop can still identify the agent.
  echo "FAIL_CLOSED=ngrok_still_running pids=${remaining}"
  exit 1
fi

rm -f "$PID_FILE" "$CONFIG_FILE" "$STATE_DIR/ngrok.addr"
echo "NGROK_STOPPED=1"
echo "PUBLIC_URL_ADVERTISED=0"
