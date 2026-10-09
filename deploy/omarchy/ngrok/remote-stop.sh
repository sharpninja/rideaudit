#!/usr/bin/bash
# Stop the RideAudit ngrok tunnel. Do not advertise a URL after this.
# SPDX-License-Identifier: GPL-2.0-only
set -euo pipefail

STATE_DIR="$HOME/.local/state/rideaudit-ngrok"
PID_FILE="$STATE_DIR/ngrok.pid"
CONFIG_FILE="$STATE_DIR/ngrok.config"
CONFIG="${RIDEAUDIT_NGROK_CONFIG:-$HOME/.config/ngrok/ngrok.yml}"
ADDR_FILE="$STATE_DIR/ngrok.addr"
LOG_FILE="$STATE_DIR/ngrok.log"
export XDG_RUNTIME_DIR="${XDG_RUNTIME_DIR:-/run/user/$(id -u)}"

# remote-start.sh records the config the agent was launched with. Ownership follows that
# recorded path as well as the current one, so a changed RIDEAUDIT_NGROK_CONFIG cannot
# orphan a running tunnel.
RECORDED_CONFIG=""
if [ -f "$CONFIG_FILE" ]; then
  RECORDED_CONFIG="$(cat "$CONFIG_FILE" || true)"
fi

# remote-start.sh writes this unit when it uses systemd. Its agent logs to stdout, so the
# --log check in is_our_ngrok cannot see it; the unit's exact ExecStart identifies it instead.
UNIT_FILE="$HOME/.config/systemd/user/rideaudit-ngrok.service"
UNIT_EXEC=""
if [ -f "$UNIT_FILE" ]; then
  UNIT_EXEC="$(sed -n 's/^ExecStart=//p' "$UNIT_FILE" | head -n 1)"
  UNIT_EXEC="${UNIT_EXEC//%h/$HOME}"
fi

# Backends the sweep searches: the canonical :28080 and prior interim :18080, plus any address
# this tree actually used. A RIDEAUDIT_NGROK_ADDR override reaches the unit's ExecStart (systemd
# mode, no PID file) or ngrok.addr (nohup mode), so both are read, as is the current override.
SWEEP_BACKENDS="192.168.1.182:28080 127.0.0.1:18080"
add_backend() {
  local addr="$1" known
  [ -n "$addr" ] || return 0
  for known in $SWEEP_BACKENDS; do
    [ "$known" = "$addr" ] && return 0
  done
  SWEEP_BACKENDS="$SWEEP_BACKENDS $addr"
}
if [ -n "$UNIT_EXEC" ]; then
  add_backend "$(printf '%s\n' "$UNIT_EXEC" | sed -n 's/.* http \([^ ]*\) .*/\1/p')"
fi
if [ -f "$ADDR_FILE" ]; then
  add_backend "$(cat "$ADDR_FILE" || true)"
fi
add_backend "${RIDEAUDIT_NGROK_ADDR:-}"

# A systemd failure is recorded, not returned at once: remote-start.sh can fall back to a nohup
# agent while leaving the unit file behind, so the PID-file and sweep cleanup below must still run.
# The recorded failure decides the final result.
SYSTEMD_FAIL=""
stop_systemd_unit() {
  local unit_pid unit_state unit_cmd stop_rc
  # Our unit is installed: every systemd step must succeed, or we cannot prove the tunnel stopped.
  if ! unit_pid="$(systemctl --user show -p MainPID --value rideaudit-ngrok.service 2>/dev/null)"; then
    SYSTEMD_FAIL="ngrok_systemd_query_failed"
    # A failed query does not prove the user manager is gone. Still ask it to stop and disable the
    # unit, so it does not restart (Restart=on-failure) the agent the sweep below kills.
    systemctl --user stop rideaudit-ngrok.service >/dev/null 2>&1 || true
    systemctl --user disable rideaudit-ngrok.service >/dev/null 2>&1 || true
    return
  fi
  stop_rc=0
  systemctl --user stop rideaudit-ngrok.service >/dev/null 2>&1 || stop_rc=$?
  # 5 = unit not loaded (unit file written but never loaded), so there is nothing to stop.
  if [ "$stop_rc" -ne 0 ] && [ "$stop_rc" -ne 5 ]; then
    SYSTEMD_FAIL="ngrok_systemd_stop_failed rc=${stop_rc}"
    return
  fi
  systemctl --user disable rideaudit-ngrok.service >/dev/null 2>&1 || true
  if ! unit_state="$(systemctl --user show -p ActiveState --value rideaudit-ngrok.service 2>/dev/null)"; then
    SYSTEMD_FAIL="ngrok_systemd_query_failed"
    return
  fi
  case "$unit_state" in
    inactive|failed) ;;
    *)
      SYSTEMD_FAIL="ngrok_systemd_unit_still_active state=${unit_state}"
      return
      ;;
  esac
  if [ -n "${unit_pid}" ] && [ "${unit_pid}" != "0" ] && kill -0 "$unit_pid" 2>/dev/null; then
    unit_cmd="$(tr '\0' ' ' < "/proc/$unit_pid/cmdline" 2>/dev/null || true)"
    case "$unit_cmd" in
      *ngrok\ http\ *)
        SYSTEMD_FAIL="ngrok_systemd_agent_still_running pid=${unit_pid}"
        ;;
    esac
  fi
}

if command -v systemctl >/dev/null 2>&1 && [ -d "$XDG_RUNTIME_DIR" ]; then
  if [ -f "$UNIT_FILE" ]; then
    stop_systemd_unit
  else
    systemctl --user stop rideaudit-ngrok.service >/dev/null 2>&1 || true
    systemctl --user disable rideaudit-ngrok.service >/dev/null 2>&1 || true
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
  # The systemd-mode agent: its command line is exactly the installed unit's ExecStart.
  if [ -n "$UNIT_EXEC" ] && [ "${cmd% }" = "$UNIT_EXEC" ]; then
    return 0
  fi
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

is_recorded_agent() {
  # The agent named by the PID file, identified by the backend recorded in ngrok.addr and
  # this tree's --log path. This covers an agent launched under a config that is neither
  # current nor recorded (one started before ngrok.config existed, then RIDEAUDIT_NGROK_CONFIG
  # changed), so its state is not discarded and its tunnel is not orphaned.
  local pid="$1" cmd="" rec_addr=""
  if [ -f "$ADDR_FILE" ]; then
    rec_addr="$(cat "$ADDR_FILE" || true)"
  fi
  [ -n "$rec_addr" ] || return 1
  if [ -r "/proc/$pid/cmdline" ]; then
    cmd="$(tr '\0' ' ' < "/proc/$pid/cmdline" 2>/dev/null || true)"
  fi
  case "$cmd" in
    *"ngrok http $rec_addr "*) ;;
    *) return 1 ;;
  esac
  case "$cmd" in
    *" --log $LOG_FILE "*) return 0 ;;
  esac
  return 1
}

our_agents() {
  # Agents this script family started, with or without a PID file, on any backend in
  # SWEEP_BACKENDS. Every candidate passes is_our_ngrok.
  local backend pid
  for backend in $SWEEP_BACKENDS; do
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
  if [ -n "${old}" ] && kill -0 "$old" 2>/dev/null && { is_our_ngrok "$old" || is_recorded_agent "$old"; }; then
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

if [ -n "$SYSTEMD_FAIL" ]; then
  # systemd could not be proven to have stopped the unit, so it may still restart the agent the
  # sweep killed. Each kill of a restarted agent can schedule another restart, so watch up to
  # three RestartSec windows (plus margin). In each window, retry stop and a runtime mask, and kill
  # any restart. Report a supervisor that is still restarting after the last window.
  restart_sec="$(sed -n 's/^RestartSec=\([0-9][0-9]*\)$/\1/p' "$UNIT_FILE" 2>/dev/null | head -n 1)"
  restarts=""
  quiet=0
  for window in 1 2 3; do
    sleep $(( ${restart_sec:-3} + 2 ))
    restarted="$(our_agents | tr '\n' ' ')"
    if [ -z "${restarted// /}" ]; then
      quiet=1
      break
    fi
    restarts="$restarts $restarted"
    systemctl --user stop rideaudit-ngrok.service >/dev/null 2>&1 || true
    systemctl --user mask --runtime rideaudit-ngrok.service >/dev/null 2>&1 || true
    for pid in $restarted; do
      kill -9 "$pid" 2>/dev/null || true
    done
  done
  if [ -n "${restarts// /}" ]; then
    SYSTEMD_FAIL="$SYSTEMD_FAIL ngrok_restarted_by_supervisor pids=${restarts# }"
    if [ "$quiet" -eq 0 ]; then
      # The last window still saw a restart, so supervision is active and the tunnel may return.
      SYSTEMD_FAIL="$SYSTEMD_FAIL ngrok_supervisor_still_restarting"
    fi
  fi
fi

remaining="$(our_agents | tr '\n' ' ')"
if [ -n "${old:-}" ] && kill -0 "$old" 2>/dev/null && { is_our_ngrok "$old" || is_recorded_agent "$old"; }; then
  remaining="$remaining $old"
fi
if [ -n "${remaining// /}" ]; then
  # Keep the state files so a later stop can still identify the agent.
  if [ -n "$SYSTEMD_FAIL" ]; then
    echo "FAIL_CLOSED=${SYSTEMD_FAIL}"
  fi
  echo "FAIL_CLOSED=ngrok_still_running pids=${remaining}"
  exit 1
fi
if [ -n "$SYSTEMD_FAIL" ]; then
  # The nohup agents are gone, but the systemd unit could not be proven stopped. Keep the state files.
  echo "FAIL_CLOSED=${SYSTEMD_FAIL}"
  exit 1
fi

rm -f "$PID_FILE" "$CONFIG_FILE" "$ADDR_FILE"
echo "NGROK_STOPPED=1"
echo "PUBLIC_URL_ADVERTISED=0"
