#!/usr/bin/env bash
# RideAudit headrest phone mount — export print STLs from the OpenSCAD source.
# Copyright (C) 2026 RideAudit contributors
# SPDX-License-Identifier: GPL-2.0
# The .scad file is the source of truth. Re-run this after any parameter edit.
set -euo pipefail
cd "$(dirname "$0")"
if ! command -v openscad >/dev/null 2>&1; then
  echo "OpenSCAD is required (https://openscad.org)." >&2
  exit 1
fi
mkdir -p exports
render() {
  local part="$1"
  local out="$2"
  echo "openscad -D part=${part} -> ${out}"
  openscad -o "$out" --export-format binstl -D "part=\"${part}\"" headrest-phone-mount.scad
}
render assembly exports/headrest-phone-mount.stl
render block exports/post-block.stl
render block_outer exports/post-block-outer.stl
render tray exports/phone-cradle.stl
render coupon exports/fit-coupon.stl
echo "Exported STLs under exports/. Print the part files. The assembly STL is a preview."
