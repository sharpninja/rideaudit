#!/usr/bin/env bash
# RideAudit headrest phone mount — export isometric PNG previews.
# Copyright (C) 2026 RideAudit contributors
# SPDX-License-Identifier: GPL-2.0
# Regenerates verification/previews/*.png from headrest-phone-mount.scad.
# The .scad file is the source of truth. part= values match export-stls.sh.
set -euo pipefail
cd "$(dirname "$0")"
if ! command -v openscad >/dev/null 2>&1; then
  echo "OpenSCAD is required (https://openscad.org)." >&2
  exit 1
fi
mkdir -p verification/previews

# Shared camera: three-quarter / isometric, orthographic, light Tomorrow scheme.
# --viewall and --autocenter fit each part; distance is only a starting point.
CAMERA="0,0,0,55,0,25,240"
IMG="1400,1000"

render() {
  local part="$1"
  local out="$2"
  echo "openscad -D part=${part} -> ${out}"
  openscad -o "$out" \
    --imgsize="$IMG" \
    --projection=ortho \
    --viewall \
    --autocenter \
    --colorscheme=Tomorrow \
    --camera="$CAMERA" \
    --render \
    -D "part=\"${part}\"" \
    headrest-phone-mount.scad
}

render beam verification/previews/beam.png
render extension verification/previews/beam-extension.png
render clamp verification/previews/post-clamp.png
render tray verification/previews/phone-cradle.png
render clip verification/previews/cradle-clip.png
render clip_cabin verification/previews/cradle-clip-cabin.png
render pin verification/previews/stop-pin.png
render coupon verification/previews/fit-coupon.png
render assembly verification/previews/assembly-opposed.png
render assembly_forward verification/previews/assembly-dual-forward.png
render assembly_single verification/previews/assembly-single.png
echo "Wrote PNG previews under verification/previews/."
