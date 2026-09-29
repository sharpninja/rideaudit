#!/usr/bin/bash
# Install or reuse official ngrok linux amd64. No secrets. Not GHCR.
# SPDX-License-Identifier: GPL-2.0-only
set -euo pipefail

mkdir -p "$HOME/.local/bin" /tmp/rideaudit-ngrok

if command -v ngrok >/dev/null 2>&1; then
  echo "NGROK_BIN=$(command -v ngrok)"
  ngrok version || true
  exit 0
fi

if [ -x "$HOME/.local/bin/ngrok" ]; then
  echo "NGROK_BIN=$HOME/.local/bin/ngrok"
  "$HOME/.local/bin/ngrok" version || true
  exit 0
fi

TGZ="/tmp/rideaudit-ngrok/ngrok-v3-stable-linux-amd64.tgz"
URL="https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-linux-amd64.tgz"

if [ ! -s "$TGZ" ]; then
  if command -v curl >/dev/null 2>&1; then
    curl -fsSL "$URL" -o "$TGZ"
  elif command -v wget >/dev/null 2>&1; then
    wget -q "$URL" -O "$TGZ"
  else
    echo "NGROK_NEED_TARBALL=1"
    exit 0
  fi
fi

if [ ! -s "$TGZ" ]; then
  echo "NGROK_NEED_TARBALL=1"
  exit 0
fi

extract_dir="/tmp/rideaudit-ngrok/extract"
rm -rf "$extract_dir"
mkdir -p "$extract_dir"
tar -xzf "$TGZ" -C "$extract_dir"
if [ -f "$extract_dir/ngrok" ]; then
  mv "$extract_dir/ngrok" "$HOME/.local/bin/ngrok"
else
  found="$(find "$extract_dir" -type f -name ngrok | head -n 1)"
  if [ -z "$found" ]; then
    echo "NGROK_TARBALL_MISSING_BINARY=1"
    exit 1
  fi
  mv "$found" "$HOME/.local/bin/ngrok"
fi
chmod +x "$HOME/.local/bin/ngrok"
echo "NGROK_BIN=$HOME/.local/bin/ngrok"
"$HOME/.local/bin/ngrok" version || true
