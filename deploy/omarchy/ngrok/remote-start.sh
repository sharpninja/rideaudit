#!/usr/bin/bash
# Start a durable ngrok HTTP tunnel to LAB-OMARCHY admission 192.168.1.182:28080.
# Fail closed if admission is down. No Python.
# SPDX-License-Identifier: GPL-2.0-only
set -euo pipefail

ADDR="${RIDEAUDIT_NGROK_ADDR:-192.168.1.182:28080}"
CONFIG="${RIDEAUDIT_NGROK_CONFIG:-$HOME/.config/ngrok/ngrok.yml}"
STATE_DIR="$HOME/.local/state/rideaudit-ngrok"
UNIT_DIR="$HOME/.config/systemd/user"
PID_FILE="$STATE_DIR/ngrok.pid"
ADDR_FILE="$STATE_DIR/ngrok.addr"
CONFIG_FILE="$STATE_DIR/ngrok.config"
LOG_FILE="$STATE_DIR/ngrok.log"
API_URL="http://127.0.0.1:4040/api/tunnels"

mkdir -p "$STATE_DIR" "$UNIT_DIR" "$HOME/.local/bin"

# The config a nohup agent was launched with, so ownership survives a changed
# RIDEAUDIT_NGROK_CONFIG (remote-stop.sh reads the same file).
RECORDED_CONFIG=""
if [ -f "$CONFIG_FILE" ]; then
  RECORDED_CONFIG="$(cat "$CONFIG_FILE" || true)"
fi

probe_local() {
  local code=""
  if command -v curl >/dev/null 2>&1; then
    code="$(curl -sS -o /tmp/rideaudit-ngrok-admission-body.txt -w '%{http_code}' --max-time 10 "http://${ADDR}/" || true)"
  elif command -v wget >/dev/null 2>&1; then
    if wget -q -O /tmp/rideaudit-ngrok-admission-body.txt --timeout=10 "http://${ADDR}/"; then
      code="200"
    else
      code="000"
    fi
  else
    echo "ADMISSION_PROBE_TOOL=missing"
    exit 1
  fi
  echo "ADMISSION_HTTP=${code}"
  if [ "$code" != "200" ]; then
    echo "FAIL_CLOSED=admission_not_up"
    exit 1
  fi
}

resolve_bin() {
  if command -v ngrok >/dev/null 2>&1; then
    command -v ngrok
    return
  fi
  if [ -x "$HOME/.local/bin/ngrok" ]; then
    echo "$HOME/.local/bin/ngrok"
    return
  fi
  echo ""
}

extract_public_url() {
  if command -v curl >/dev/null 2>&1; then
    curl -sS --max-time 3 "$API_URL" | grep -oE 'https://[A-Za-z0-9._-]+\.ngrok[^"]+' | head -n 1 || true
  fi
}

write_unit() {
  local bin="$1"
  cat > "$UNIT_DIR/rideaudit-ngrok.service" <<EOF
[Unit]
Description=RideAudit ngrok HTTP tunnel to LAB-OMARCHY admission 192.168.1.182:28080
After=network-online.target
Wants=network-online.target

[Service]
Type=simple
ExecStart=${bin} http ${ADDR} --config ${CONFIG} --log stdout --log-format logfmt
Restart=on-failure
RestartSec=3

[Install]
WantedBy=default.target
EOF
}

start_systemd() {
  local bin="$1"
  export XDG_RUNTIME_DIR="${XDG_RUNTIME_DIR:-/run/user/$(id -u)}"
  if [ ! -d "$XDG_RUNTIME_DIR" ]; then
    return 1
  fi
  if ! command -v systemctl >/dev/null 2>&1; then
    return 1
  fi
  write_unit "$bin"
  # remote-stop.sh may have runtime-masked the unit when systemd ignored its stop.
  systemctl --user unmask --runtime rideaudit-ngrok.service >/dev/null 2>&1 || true
  if ! systemctl --user daemon-reload; then
    return 1
  fi
  systemctl --user enable rideaudit-ngrok.service >/dev/null 2>&1 || true
  if ! systemctl --user restart rideaudit-ngrok.service; then
    return 1
  fi
  echo "NGROK_MODE=systemd-user"
  return 0
}

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

start_nohup() {
  local bin="$1"
  if [ -f "$PID_FILE" ]; then
    old="$(cat "$PID_FILE" || true)"
    # A PID file can outlive its process and the PID can be reused. Only treat it as
    # our agent when the live process is our ngrok http tunnel; otherwise drop the file.
    if [ -n "${old}" ] && kill -0 "$old" 2>/dev/null && ! is_our_ngrok "$old" && ! is_recorded_agent "$old"; then
      echo "NGROK_STALE_PID=discarded"
      rm -f "$PID_FILE" "$ADDR_FILE" "$CONFIG_FILE"
      old=""
    fi
    if [ -n "${old}" ] && kill -0 "$old" 2>/dev/null; then
      # Reuse the running agent only when it was started for this backend and its live
      # command line carries the current --config. Anything else (including a legacy agent
      # on an old config) is stopped below before a new one starts.
      old_cmd="$(tr '\0' ' ' < "/proc/$old/cmdline" 2>/dev/null || true)"
      if [ -f "$ADDR_FILE" ] && [ "$(cat "$ADDR_FILE" || true)" = "$ADDR" ] \
        && [[ "$old_cmd" == *" --config $CONFIG "* ]]; then
        printf '%s\n' "$CONFIG" > "$CONFIG_FILE"
        echo "NGROK_MODE=nohup-existing"
        return 0
      fi
      echo "NGROK_STALE_BACKEND=restart"
      kill "$old" 2>/dev/null || true
      j=0
      while kill -0 "$old" 2>/dev/null && [ $j -lt 10 ]; do
        j=$((j + 1))
        sleep 1
      done
      if kill -0 "$old" 2>/dev/null; then
        echo "FAIL_CLOSED=stale_ngrok_still_running"
        exit 1
      fi
    fi
  fi
  nohup "$bin" http "$ADDR" --config "$CONFIG" --log "$LOG_FILE" --log-format logfmt \
    >"$STATE_DIR/ngrok.out" 2>&1 &
  echo $! > "$PID_FILE"
  printf '%s\n' "$ADDR" > "$ADDR_FILE"
  printf '%s\n' "$CONFIG" > "$CONFIG_FILE"
  echo "NGROK_MODE=nohup"
}

probe_local

if [ ! -f "$CONFIG" ]; then
  echo "FAIL_CLOSED=missing_config"
  exit 1
fi

BIN="$(resolve_bin)"
if [ -z "$BIN" ]; then
  echo "FAIL_CLOSED=ngrok_binary_missing"
  exit 1
fi
echo "NGROK_BIN=$BIN"

if ! start_systemd "$BIN"; then
  start_nohup "$BIN"
fi

url=""
i=0
while [ $i -lt 30 ]; do
  url="$(extract_public_url || true)"
  if [ -n "$url" ]; then
    break
  fi
  i=$((i + 1))
  sleep 1
done

if [ -z "$url" ]; then
  echo "FAIL_CLOSED=no_public_url"
  if [ -f "$LOG_FILE" ]; then
    grep -Eiv 'authtoken|api_key|authorization' "$LOG_FILE" | grep -E 'eror|crit|ERR_NGROK|failed to start' | tail -n 8 || true
  fi
  exit 1
fi

echo "PUBLIC_URL=$url"
