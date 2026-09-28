#!/usr/bin/env python3
# RideAudit headrest phone mount — geometry acceptance checks.
# Copyright (C) 2026 RideAudit contributors
# SPDX-License-Identifier: GPL-2.0
"""Export STLs from headrest-phone-mount.scad and check the mount geometry.

The OpenSCAD source is authoritative. This script fails if the cradle, post
bore, arm slots, threaded-hole grid, or flush faces are wrong, or if a print
STL is not a closed mesh sitting on the bed.
"""

from __future__ import annotations

import struct
import subprocess
import sys
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent
SCAD = ROOT / "headrest-phone-mount.scad"
EXPORTS = ROOT / "exports"
REPORT = ROOT / "verification" / "geometry-report.md"

PRINT_PARTS = {
    "assembly": "headrest-phone-mount.stl",
    "block": "post-block.stl",
    "tray": "phone-cradle.stl",
    "coupon": "fit-coupon.stl",
    "screw": "thumbscrew.stl",
}

RAW_PARTS = ("raw_block", "raw_block_right", "raw_tray", "raw_coupon", "raw_screw")


class Fail(Exception):
    pass


def openscad(part: str, dest: Path) -> list[str]:
    dest.parent.mkdir(parents=True, exist_ok=True)
    cmd = [
        "openscad",
        "-o",
        str(dest),
        "--export-format",
        "binstl",
        "-D",
        f'part="{part}"',
        str(SCAD),
    ]
    proc = subprocess.run(cmd, capture_output=True, text=True)
    text = (proc.stdout or "") + (proc.stderr or "")
    if part.startswith("fitcheck"):
        if "Current top level object is empty." not in text:
            raise Fail(f"{part} found an interference or did not run:\n{text[-800:]}")
        return text.splitlines()
    if proc.returncode != 0:
        raise Fail(f"OpenSCAD failed for {part} ({proc.returncode}):\n{text[-1200:]}")
    return text.splitlines()


def parse_checks(lines: list[str]) -> dict[str, float]:
    checks: dict[str, float] = {}
    for line in lines:
        if 'ECHO: "CHECK ' not in line:
            continue
        payload = line.split("CHECK ", 1)[1].rstrip('"')
        key, value = payload.split("=", 1)
        checks[key] = float(value)
    if "arm_length" not in checks:
        raise Fail("OpenSCAD did not emit CHECK lines")
    return checks


def load_stl(path: Path) -> list[tuple]:
    data = path.read_bytes()
    if data[:5].lower() == b"solid" and b"facet" in data[:200]:
        verts = []
        for line in data.decode("utf-8", "ignore").splitlines():
            if "vertex" in line:
                parts = line.split()
                verts.append(tuple(float(x) for x in parts[-3:]))
        return [tuple(verts[i : i + 3]) for i in range(0, len(verts), 3)]
    count = struct.unpack_from("<I", data, 80)[0]
    tris = []
    off = 84
    for _ in range(count):
        vals = struct.unpack_from("<12fH", data, off)
        tris.append(tuple(tuple(vals[3 + 3 * k : 6 + 3 * k]) for k in range(3)))
        off += 50
    return tris


def mesh_stats(tris: list[tuple]) -> dict:
    edges: dict[tuple, int] = {}
    xs: list[float] = []
    ys: list[float] = []
    zs: list[float] = []
    volume = 0.0
    for tri in tris:
        a, b, c = tri
        volume += (
            a[0] * (b[1] * c[2] - b[2] * c[1])
            - a[1] * (b[0] * c[2] - b[2] * c[0])
            + a[2] * (b[0] * c[1] - b[1] * c[0])
        )
        for p in tri:
            xs.append(p[0])
            ys.append(p[1])
            zs.append(p[2])
        for e in ((a, b), (b, c), (c, a)):
            k = tuple(sorted((tuple(round(v, 4) for v in e[0]), tuple(round(v, 4) for v in e[1]))))
            edges[k] = edges.get(k, 0) + 1
    return {
        "tris": len(tris),
        "volume": abs(volume) / 6.0,
        "open_edges": sum(1 for n in edges.values() if n != 2),
        "min": (min(xs), min(ys), min(zs)),
        "max": (max(xs), max(ys), max(zs)),
    }


def _ray_hits(origin, direction, tri) -> bool:
    eps = 1e-8
    ox, oy, oz = origin
    dx, dy, dz = direction
    (ax, ay, az), (bx, by, bz), (cx, cy, cz) = tri
    abx, aby, abz = bx - ax, by - ay, bz - az
    acx, acy, acz = cx - ax, cy - ay, cz - az
    px = dy * acz - dz * acy
    py = dz * acx - dx * acz
    pz = dx * acy - dy * acx
    det = abx * px + aby * py + abz * pz
    if abs(det) < eps:
        return False
    inv = 1.0 / det
    sx, sy, sz = ox - ax, oy - ay, oz - az
    u = (sx * px + sy * py + sz * pz) * inv
    if u < -eps or u > 1 + eps:
        return False
    qx = sy * abz - sz * aby
    qy = sz * abx - sx * abz
    qz = sx * aby - sy * abx
    v = (dx * qx + dy * qy + dz * qz) * inv
    if v < -eps or u + v > 1 + eps:
        return False
    t = (acx * qx + acy * qy + acz * qz) * inv
    return t > eps


def _odd_hit(tris, origin, direction) -> bool:
    hits = sum(1 for tri in tris if _ray_hits(origin, direction, tri))
    return hits % 2 == 1


def inside(tris, point) -> bool:
    directions = (
        (0.31, 0.17, 1.0),
        (-0.22, 0.28, 1.0),
        (0.19, -0.27, 1.0),
        (1.0, 0.13, 0.21),
        (0.11, 1.0, 0.18),
    )
    votes = []
    for direction in directions:
        origin = (
            point[0] + direction[0] * 0.01,
            point[1] + direction[1] * 0.01,
            point[2] + direction[2] * 0.01,
        )
        votes.append(_odd_hit(tris, origin, direction))
    return sum(votes) >= 3


def expect(condition: bool, message: str) -> None:
    if not condition:
        raise Fail(message)


def classify(tris, point, should_be_solid: bool, label: str) -> str:
    solid = inside(tris, point)
    if solid != should_be_solid:
        kind = "solid" if solid else "air"
        want = "solid" if should_be_solid else "air"
        raise Fail(f"{label}: point {tuple(round(v, 2) for v in point)} is {kind}, expected {want}")
    return f"{'solid' if should_be_solid else 'air'}  {label}"


def main() -> int:
    if not SCAD.is_file():
        print("missing", SCAD, file=sys.stderr)
        return 1
    notes: list[str] = []
    try:
        with tempfile.TemporaryDirectory(prefix="rideaudit-mount-") as tmp:
            tmp_path = Path(tmp)
            lines = openscad("coupon", tmp_path / "echo.stl")
            checks = parse_checks(lines)
            notes.append("OpenSCAD assertions accepted the default parameters.")

            for part, name in PRINT_PARTS.items():
                openscad(part, EXPORTS / name)
            for part in RAW_PARTS:
                openscad(part, tmp_path / f"{part}.stl")
            for part in ("fitcheck", "fitcheck_arms"):
                openscad(part, tmp_path / f"{part}.stl")
            notes.append("The two arms do not intersect each other.")
            notes.append("The arms pass under the receiver roof and do not intersect the cradle.")

            meshes = {name: load_stl(EXPORTS / name) for name in PRINT_PARTS.values()}
            raw = {part: load_stl(tmp_path / f"{part}.stl") for part in RAW_PARTS}
            stats = {name: mesh_stats(tris) for name, tris in meshes.items()}
            raw_stats = {part: mesh_stats(tris) for part, tris in raw.items()}

            for name, st in stats.items():
                expect(st["tris"] > 50, f"{name} is unexpectedly small")
                expect(st["open_edges"] == 0, f"{name} is not closed ({st['open_edges']} open edges)")
                expect(st["volume"] > 10, f"{name} has no volume")
                expect(st["min"][2] > -0.05, f"{name} extends below the bed ({st['min'][2]:.3f})")
                if name != "headrest-phone-mount.stl":
                    expect(st["min"][2] < 0.05, f"{name} does not sit on the bed (min z {st['min'][2]:.3f})")
                    expect(st["min"][0] > -0.2 and st["min"][1] > -0.2, f"{name} is not in the positive XY octant")

            c = checks
            tray = stats["phone-cradle.stl"]
            block = stats["post-block.stl"]
            block_span = max(block["max"][i] - block["min"][i] for i in range(3))
            expect(abs((tray["max"][0] - tray["min"][0]) - c["rail_span"]) < 0.3, "cradle width is not the receiver span")
            expect(block_span > c["arm_length"] - 1, "post block print is shorter than the arm")
            expect(c["bore_d"] > c["post_diameter"], "bore does not clear the post")
            expect(c["arm_length"] > c["slot_radius"], "preview screw is past the arm tip")
            expect(c["slot_len"] > 100, "arm slot is not a longitudinal slot")
            expect(c["y_slot"] - c["slot_y0"] > 40, "cradle cannot slide back along the arm")
            expect(c["slot_y1"] > c["y_slot"] + 4, "arm slot does not contain the preview screw")
            expect(c["arm_rise"] == 0, "arms are not horizontal")
            expect(c["screw_count"] == 2, "model does not have one thumbscrew per arm")
            expect(c["hole_nz"] == 1, "threaded holes are not a single row")
            expect(c["pocket_x"] >= c["phone_length_max"], "pocket shorter than the longest phone")
            expect(c["pocket_y"] >= c["phone_thickness_max"], "pocket shallower than the thickest phone")
            expect(c["pocket_z"] >= c["phone_width_max"], "pocket shorter than the widest short side")
            expect(c["camera_clearance"] >= 12, "camera window too small")
            expect(c["cradle_count"] == 1, "model is not the single shared cradle")
            expect(c["slot_span"] / 2 + 0.01 >= c["hole_pitch"] / 2, "slot is narrower than the hole pitch")
            for spacing in (c["post_spacing_min"], c["post_spacing_max"], c["post_spacing"]):
                half_s = spacing / 2
                expect(c["hole_x_max"] + 0.01 >= half_s, f"hole row does not cover {spacing:.0f} mm spacing")
                expect(
                    c["rail_span"] / 2 + 0.01 >= half_s + c["arm_width"] / 2,
                    f"receiver roof does not cover an arm at {spacing:.0f} mm spacing",
                )
            notes.append(
                f"The hole row covers post centers from {c['post_spacing_min']:.0f} mm to {c['post_spacing_max']:.0f} mm."
            )
            notes.append("Arms lie in a horizontal plane and enter the receiver from the rear.")
            notes.append(
                "Each arm has a longitudinal slot. The cradle slides along it to set depth, then each thumbscrew locks."
            )
            notes.append(
                "Each thumbscrew comes up through a bottom slot, through that arm slot, and into the roof thread."
            )

            for part in ("raw_block", "raw_block_right", "raw_coupon"):
                st = raw_stats[part]
                expect(st["min"][1] > -0.05, f"{part} breaks the flush plane (min y {st['min'][1]:.3f})")
                expect(st["min"][1] < 0.05, f"{part} does not bear on Y=0 (min y {st['min'][1]:.3f})")
            tray_raw = raw_stats["raw_tray"]
            expect(tray_raw["min"][1] > c["rail_y0"] - 0.5, "cradle reaches back to the headrest pad")
            notes.append("Post-block heels bear on Y=0. The cradle sits at the forward end of the arms.")

            tray_m = raw["raw_tray"]
            block_m = raw["raw_block"]
            right_m = raw["raw_block_right"]
            coupon_m = raw["raw_coupon"]
            screw_m = raw["raw_screw"]
            probes = []
            plate_y = (c["y_plate0"] + c["y_plate1"]) / 2
            phone_y = c["y_plate1"] + 1.0
            phone_z = c["z_pocket0"] + c["phone_width_max"] / 2
            hole_z = c["rail_z0"] + 2.0
            beside = c["slot_span"] / 2 + 1.5
            probes.append(classify(tray_m, (0.0, phone_y, phone_z), False, "max-phone center is in the pocket"))
            probes.append(classify(
                tray_m,
                (c["phone_length_max"] / 2 - 0.6, phone_y, c["z_pocket0"] + c["phone_width_max"] - 0.6),
                False,
                "max-phone upper corner is not blocked",
            ))
            probes.append(classify(
                tray_m,
                (0.0, plate_y, c["z_pocket0"] + c["pocket_z"] / 2),
                True,
                "back plate is present behind the phone",
            ))
            probes.append(classify(tray_m, (-c["win_x"], plate_y, c["win_z"]), False, "camera window is open"))
            probes.append(classify(tray_m, (c["win_x"], plate_y, c["win_z"]), False, "opposite camera window is open"))
            probes.append(classify(
                tray_m,
                (0.0, plate_y, c["win_z"]),
                True,
                "back plate remains between the camera windows",
            ))
            lip_y = c["y_plate1"] + c["front_lip_t"] / 2
            lip_z = c["z_pocket0"] + c["front_lip_h"] / 2
            probes.append(classify(tray_m, (0.0, lip_y, lip_z), True, "front retention lip is present"))
            probes.append(classify(
                tray_m,
                (0.0, lip_y, c["z_pocket0"] + c["front_lip_h"] + 8),
                False,
                "front of the cradle stays open above the lip",
            ))
            probes.append(classify(
                tray_m,
                (-c["half"], c["y_slot"], hole_z),
                False,
                "left thumbscrew hole is open in the roof",
            ))
            probes.append(classify(
                tray_m,
                (c["half"], c["y_slot"], hole_z),
                False,
                "right thumbscrew hole is open in the roof",
            ))
            probes.append(classify(
                tray_m,
                (c["half"] + c["hole_pitch"] / 2, c["y_slot"], hole_z),
                True,
                "roof stays solid between threaded holes",
            ))
            probes.append(classify(
                tray_m,
                (c["half"] + c["hole_pitch"], c["y_slot"], hole_z),
                False,
                "neighbor threaded hole is open",
            ))
            probes.append(classify(
                tray_m,
                (-c["half"], c["y_slot"] - 4.0, c["rail_z0"] + c["rail_h"] / 2),
                True,
                "receiver roof is solid above the arm",
            ))
            probes.append(classify(
                tray_m,
                (-c["half"], c["rail_y0"] + 2.0, c["z_arm"]),
                False,
                "receiver is open at the rear on the arm centerline",
            ))
            bot_z = c["z_bot0"] + c["bottom_t"] / 2
            probes.append(classify(
                tray_m,
                (-c["half"], c["y_slot"], bot_z),
                False,
                "left bottom slot is open under the threaded hole",
            ))
            probes.append(classify(
                tray_m,
                (c["half"], c["y_slot"], bot_z),
                False,
                "right bottom slot is open under the threaded hole",
            ))
            probes.append(classify(
                tray_m,
                (c["half"] + c["hole_pitch"] / 2, c["y_slot"], bot_z),
                True,
                "cradle bottom stays solid between the screw slots",
            ))
            probes.append(classify(
                block_m,
                (-c["half"], c["bore_cy"], c["block_h"] / 2),
                False,
                "post bore passes through the block",
            ))
            probes.append(classify(
                block_m,
                (-c["half"], 1.2, c["block_h"] / 2),
                True,
                "flush heel is solid behind the bore",
            ))
            probes.append(classify(
                block_m,
                (-c["half"], c["bore_cy"] + c["bore_d"] / 2 + 2.0, c["z_arm"]),
                True,
                "arm root is solid in front of the bore",
            ))
            probes.append(classify(
                block_m,
                (-c["half"], c["slot_y0"] + 8.0, c["z_arm"]),
                False,
                "longitudinal slot is open near the arm root",
            ))
            probes.append(classify(
                block_m,
                (-c["half"], c["y_slot"], c["z_arm"]),
                False,
                "left arm slot is open at the preview screw",
            ))
            probes.append(classify(
                block_m,
                (-c["half"], c["slot_y1"] - 6.0, c["z_arm"]),
                False,
                "longitudinal slot is open near the arm tip",
            ))
            probes.append(classify(
                block_m,
                (-c["half"] + beside, c["slot_y0"] + 8.0, c["z_arm"]),
                True,
                "left arm rail is solid beside the slot near the root",
            ))
            probes.append(classify(
                block_m,
                (-c["half"] + beside, c["y_slot"], c["z_arm"]),
                True,
                "left arm rail is solid beside the slot, under the roof",
            ))
            probes.append(classify(
                block_m,
                (-c["half"] + beside, c["slot_y1"] - 6.0, c["z_arm"]),
                True,
                "left arm rail stays at the same height near the tip",
            ))
            probes.append(classify(
                block_m,
                (-c["half"] + beside, c["bore_cy"] + 80.0, c["z_arm"] + c["arm_thick"] / 2 + 2.0),
                False,
                "nothing rises above the horizontal arm",
            ))
            probes.append(classify(
                right_m,
                (c["half"], c["y_slot"], c["z_arm"]),
                False,
                "right arm slot is open on its own screw, not the left screw",
            ))
            probes.append(classify(
                right_m,
                (c["half"] - beside, c["y_slot"], c["z_arm"]),
                True,
                "right arm is solid beside its slot",
            ))
            probes.append(classify(
                coupon_m,
                (0.0, c["bore_cy"], 9.0),
                False,
                "fit coupon bore is open",
            ))
            probes.append(classify(
                coupon_m,
                (c["block_w"] / 2 - 1.4, c["bore_cy"], 9.0),
                True,
                "fit coupon wall surrounds the bore",
            ))
            probes.append(classify(
                screw_m,
                (0.0, 0.0, -2.0),
                True,
                "thumbscrew head is solid",
            ))
            probes.append(classify(
                screw_m,
                (c["screw_head_d"] / 2 + 2.0, 0.0, -4.0),
                True,
                "thumbscrew wing is solid",
            ))
            probes.append(classify(
                screw_m,
                (0.0, 0.0, 6.0),
                True,
                "thumbscrew shank is solid",
            ))
            narrow_x = c["phone_length_min"] / 2 - 0.5
            narrow_z = c["z_pocket0"] + min(c["phone_width_min"], c["pocket_z"]) - 0.8
            probes.append(classify(
                tray_m,
                (narrow_x, phone_y, narrow_z),
                False,
                "minimum-length phone corner fits in the pocket",
            ))
            notes.extend(probes)
    except Fail as exc:
        print(f"FAIL: {exc}", file=sys.stderr)
        return 1

    write_report(checks, stats, notes)
    print(
        f"PASS  horizontal arms {checks['arm_length']:.0f} mm  "
        f"pocket {checks['pocket_x']:.1f}×{checks['pocket_z']:.1f}×{checks['pocket_y']:.1f} mm  "
        f"reach {checks['standout_y']:.1f} mm"
    )
    print(f"report {REPORT}")
    return 0


def write_report(checks: dict[str, float], stats: dict, notes: list[str]) -> None:
    c = checks
    lines = [
        "# Geometry measurement log — ART-RIDE-MOUNT-001",
        "",
        "Generated by `verify-geometry.py` from `headrest-phone-mount.scad`.",
        "This is a CAD measurement, not an on-vehicle print. Physical fit stays on the operator checklist.",
        "",
        "License: GPL-2.0",
        "",
        "## Envelope",
        "",
        "| Check | Value |",
        "| --- | --- |",
        f"| Post spacing the hole row covers | {c['post_spacing_min']:.0f} – {c['post_spacing_max']:.0f} mm center-to-center |",
        f"| Assembly preview spacing | {c['post_spacing']:.0f} mm |",
        f"| Post bore | {c['bore_d']:.2f} mm (post {c['post_diameter']:.0f} mm + clearance) |",
        f"| Arm length | {c['arm_length']:.0f} mm from the post axis, horizontal (rise {c['arm_rise']:.0f}) |",
        f"| Arm section | {c['arm_width']:.0f} × {c['arm_thick']:.0f} mm |",
        f"| Arm slot, along the arm | {c['slot_len']:.0f} mm long, {c['slot_span']:.0f} mm wide, from {c['slot_y0']:.0f} to {c['slot_y1']:.0f} mm forward of the pad |",
        f"| Depth adjustment behind the preview screw | {c['y_slot'] - c['slot_y0']:.0f} mm |",
        f"| Cradle bottom slots | {c['slot_gap']:.0f} mm along the arm, one under each threaded hole |",
        f"| Threaded holes | {c['hole_nx']:.0f} in one row, pitch {c['hole_pitch']:.0f} mm, out to ±{c['hole_x_max']:.0f} mm, M5 tap-drill {c['m5_tap']:.1f} mm |",
        f"| Thumbscrews | {c['screw_count']:.0f} modeled, one per arm, from below, shank {c['screw_shank_l']:.1f} mm under the head |",
        f"| Phone pocket (L × short side × thickness) | {c['pocket_x']:.1f} × {c['pocket_z']:.1f} × {c['pocket_y']:.1f} mm |",
        f"| Phone envelope | length {c['phone_length_min']:.0f}–{c['phone_length_max']:.0f} mm, short side {c['phone_width_min']:.0f}–{c['phone_width_max']:.0f} mm, thickness ≤ {c['phone_thickness_max']:.0f} mm |",
        f"| Camera window | {c['camera_clearance']:.0f} mm square, both upper corners, through the back plate |",
        f"| Phone front from the headrest face | {c['standout_y']:.1f} mm |",
        f"| Cradle top above the block bottom | {c['z_pocket1']:.1f} mm |",
        f"| Cradles | {c['cradle_count']:.0f} shared landscape holder |",
        "",
        "## Print meshes",
        "",
        "| STL | Triangles | Volume mm³ | Size X×Y×Z mm |",
        "| --- | --- | --- | --- |",
    ]
    for name, st in stats.items():
        size = tuple(st["max"][i] - st["min"][i] for i in range(3))
        lines.append(
            f"| `{name}` | {st['tris']} | {st['volume']:.0f} | {size[0]:.1f} × {size[1]:.1f} × {size[2]:.1f} |"
        )
    lines.extend(["", "## Probe results", ""])
    for note in notes:
        lines.append(f"- {note}")
    lines.append("")
    REPORT.parent.mkdir(parents=True, exist_ok=True)
    REPORT.write_text("\n".join(lines), encoding="utf-8")


if __name__ == "__main__":
    sys.exit(main())
