#!/usr/bin/env python3
# RideAudit headrest phone mount — geometry acceptance checks.
# Copyright (C) 2026 RideAudit contributors
# SPDX-License-Identifier: GPL-2.0
"""Export STLs from headrest-phone-mount.scad and check HW-AC-MOUNT geometry.

The OpenSCAD source is authoritative. This script fails if the model violates
the pocket, camera, post, spacing, or clearance checks, or if a print STL is
not a closed mesh sitting on the bed.
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
    "assembly_forward": "headrest-phone-mount-dual-forward.stl",
    "beam": "beam.stl",
    "extension": "beam-extension.stl",
    "clamp": "post-clamp.stl",
    "tray": "phone-cradle.stl",
    "clip": "cradle-clip.stl",
    "clip_cabin": "cradle-clip-cabin.stl",
    "pin": "stop-pin.stl",
    "coupon": "fit-coupon.stl",
}

RAW_PARTS = ("raw_beam", "raw_clamp", "raw_tray", "raw_clip", "raw_coupon")


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
        payload = line.split('CHECK ', 1)[1].rstrip('"')
        key, value = payload.split("=", 1)
        checks[key] = float(value)
    if "beam_length" not in checks:
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
    # Several ray directions. A single axis-aligned ray can slip through a
    # tessellated cylinder edge and mis-classify the center of a round hole.
    directions = (
        (0.31, 0.17, 1.0),
        (-0.22, 0.28, 1.0),
        (0.19, -0.27, 1.0),
        (1.0, 0.13, 0.21),
        (0.11, 1.0, 0.18),
    )
    votes = []
    for direction in directions:
        origin = (point[0] + direction[0] * 0.01, point[1] + direction[1] * 0.01, point[2] + direction[2] * 0.01)
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
            lines = openscad("pin", tmp_path / "pin-probe.stl")
            checks = parse_checks(lines)
            notes.append("OpenSCAD assertions accepted the default parameters.")

            for part, name in PRINT_PARTS.items():
                openscad(part, EXPORTS / name)
            for part in RAW_PARTS:
                openscad(part, tmp_path / f"{part}.stl")
            for part in ("fitcheck", "fitcheck_ext", "fitcheck_tongue"):
                openscad(part, tmp_path / f"{part}.stl")
            notes.append("Opposed clamps and clips do not intersect the beam.")
            notes.append("Dual-forward clips do not intersect the beam or extensions.")
            notes.append("Extension tongue fits the beam pocket with clearance.")

            meshes = {name: load_stl(EXPORTS / name) for name in PRINT_PARTS.values()}
            raw = {part: load_stl(tmp_path / f"{part}.stl") for part in RAW_PARTS}
            stats = {name: mesh_stats(tris) for name, tris in meshes.items()}

            for name, st in stats.items():
                expect(st["tris"] > 50, f"{name} is unexpectedly small")
                expect(st["open_edges"] == 0, f"{name} is not closed ({st['open_edges']} open edges)")
                expect(st["volume"] > 10, f"{name} has no volume")
                expect(st["min"][2] > -0.05, f"{name} extends below the bed ({st['min'][2]:.3f})")
                if name not in ("headrest-phone-mount.stl", "headrest-phone-mount-dual-forward.stl"):
                    expect(st["min"][2] < 0.05, f"{name} does not sit on the bed (min z {st['min'][2]:.3f})")
                    expect(st["min"][0] > -0.2 and st["min"][1] > -0.2, f"{name} is not in the positive XY octant")

            beam = stats["beam.stl"]
            tray = stats["phone-cradle.stl"]
            asm = stats["headrest-phone-mount.stl"]
            fwd = stats["headrest-phone-mount-dual-forward.stl"]
            expect(abs((beam["max"][0] - beam["min"][0]) - checks["beam_length"]) < 0.2, "beam STL length drifted")
            expect(abs((tray["max"][0] - tray["min"][0]) - checks["outer_x"]) < 0.2, "tray length is not the phone envelope")
            expect(abs((tray["max"][1] - tray["min"][1]) - checks["outer_z"]) < 0.2, "tray short-side is not the phone envelope")
            expect((tray["max"][2] - tray["min"][2]) < 28, "tray did not lie flat for printing")
            yspan = asm["max"][1] - asm["min"][1]
            expect(yspan > 1.8 * checks["forward_projection"], "opposed assembly does not show cradles on both faces")
            xspan = fwd["max"][0] - fwd["min"][0]
            expect(xspan > 1.8 * checks["outer_x"], "dual-forward assembly is not two landscape phones wide")

            c = checks
            expect(c["pocket_x"] >= c["phone_length_max"], "pocket shorter than the longest phone")
            expect(c["pocket_y"] >= c["phone_thickness_max"], "pocket shallower than the thickest phone")
            expect(c["pocket_z"] >= c["phone_width_max"], "pocket shorter than the widest short side")
            expect(c["bore_d"] > c["post_diameter"], "bore does not clear the post")
            expect(c["throat_w"] < c["post_diameter"] - 1, "throat will not snap")
            expect(c["camera_clearance"] >= 12, "camera window too small")
            expect(c["cradle_count"] == 2 and c["layout"] == 0, "default export is not the opposed dual cradle")
            expect(c["pin_x"] - 2.05 >= c["post_spacing_max"] / 2 + c["carriage_len"] / 2 - 0.2, "stop pin blocks max spacing")

            tray_m = raw["raw_tray"]
            clip_m = raw["raw_clip"]
            beam_m = raw["raw_beam"]
            clamp_m = raw["raw_clamp"]
            coupon_m = raw["raw_coupon"]

            probes = []
            half_l = c["phone_length_max"] / 2
            half_clear = (c["pocket_x"] - c["phone_length_max"]) / 2
            phone_y0 = c["pocket_y0"]
            phone_z0 = c["pocket_z0"]
            # Interior of a maximum phone seated on the lip and against the back plate.
            probes.append(classify(tray_m, (0.0, phone_y0 + 1.0, phone_z0 + c["phone_width_max"] / 2), False, "max-phone center is in the pocket"))
            probes.append(classify(
                tray_m,
                (half_l - 0.6, phone_y0 + 0.8, phone_z0 + c["phone_width_max"] - 0.6),
                False,
                "max-phone upper corner is not blocked",
            ))
            probes.append(classify(tray_m, (0.0, c["plate_y0"] + 1.5, c["outer_z"] / 2), True, "back plate is present"))
            win_x = -c["pocket_x"] / 2 + c["camera_clearance"] / 2
            win_z = c["pocket_z1"] - c["camera_clearance"] / 2
            probes.append(classify(tray_m, (win_x, c["plate_y0"] + 1.5, win_z), False, "camera window is open through the back plate"))
            probes.append(classify(tray_m, (-win_x, c["plate_y0"] + 1.5, win_z), False, "opposite camera window is open"))
            probes.append(classify(tray_m, (0.0, c["plate_y0"] + 1.5, win_z), True, "back plate remains between the camera windows"))
            probes.append(classify(tray_m, (0.0, c["pocket_y1"] + 1.0, c["front_lip_h"] / 2), True, "front retention lip is present"))
            probes.append(classify(tray_m, (0.0, c["pocket_y1"] + 1.0, c["front_lip_h"] + 8), False, "front of the cradle stays open above the lip"))
            probes.append(classify(tray_m, (c["screw_x"], c["plate_y0"] + 2.0, c["screw_z"]), False, "tray screw hole is clear"))
            probes.append(classify(beam_m, (0.0, c["rail_yc"], c["screw_z"]), True, "main rail exists at center"))
            probes.append(classify(beam_m, (0.0, 0.0, c["screw_z"]), True, "center link joins the two rails"))
            probes.append(classify(beam_m, (c["post_spacing_max"] / 2, c["rail_yc"], c["screw_z"]), True, "rail reaches post_spacing_max"))
            probes.append(classify(beam_m, (-c["post_spacing_min"] / 2, c["rail_yc"], c["screw_z"]), True, "rail reaches post_spacing_min"))
            probes.append(classify(clip_m, (0.0, c["rail_yc"], c["screw_z"]), False, "clip channel clears the rail"))
            probes.append(classify(clip_m, (0.0, c["beam_y1"] + 1.0, c["screw_z"]), True, "clip retains the outer rail face"))
            mid = c["clamp_depth"] if "clamp_depth" in c else 17.5
            # clamp_depth is not echoed; the bore is the full clamp height. Use a mid height inside 0..roof.
            bore_z = c["roof_top"] * 0.45
            probes.append(classify(clamp_m, (0.0, 0.0, bore_z), False, "clamp bore is open"))
            probes.append(classify(coupon_m, (0.0, -c["clamp_body_y"] / 2 + 2.0, 9.0), False, "coupon throat opens to the outside"))
            probes.append(classify(coupon_m, (c["clamp_body_x"] / 2 - 1.3, 0.0, 9.0), True, "coupon jaw surrounds the bore"))
            # Narrower phone still sits in the max pocket (air at its corners).
            narrow_x = c["phone_length_min"] / 2 - 0.5
            narrow_z = phone_z0 + min(c["phone_width_min"], c["pocket_z"]) - 0.8
            probes.append(classify(tray_m, (narrow_x, phone_y0 + 0.8, narrow_z), False, "minimum-length phone corner fits in the pocket"))
            expect(half_clear > 0.4, "length clearance is tighter than 0.4 mm per side")
            _ = mid
            notes.extend(probes)
    except Fail as exc:
        print(f"FAIL: {exc}", file=sys.stderr)
        return 1

    write_report(checks, stats, notes)
    print(f"PASS  beam {checks['beam_length']:.1f} mm  pocket {checks['pocket_x']:.1f}×{checks['pocket_z']:.1f}×{checks['pocket_y']:.1f} mm")
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
        f"| Post spacing covered | {c['post_spacing_min']:.0f} – {c['post_spacing_max']:.0f} mm center-to-center |",
        f"| Post bore | {c['bore_d']:.2f} mm (post {c['post_diameter']:.0f} mm + clearance) |",
        f"| Snap throat | {c['throat_w']:.2f} mm |",
        f"| Beam length (print X) | {c['beam_length']:.1f} mm |",
        f"| Phone pocket (L × short side × thickness) | {c['pocket_x']:.1f} × {c['pocket_z']:.1f} × {c['pocket_y']:.1f} mm |",
        f"| Phone envelope | length {c['phone_length_min']:.0f}–{c['phone_length_max']:.0f} mm, short side {c['phone_width_min']:.0f}–{c['phone_width_max']:.0f} mm, thickness ≤ {c['phone_thickness_max']:.0f} mm |",
        f"| Camera window | {c['camera_clearance']:.0f} mm square, both upper corners, through the back plate |",
        f"| Projection from post center, each face | {c['forward_projection']:.1f} mm |",
        f"| Cradle height above the clamp bottom | {c['cradle_top_z']:.1f} mm |",
        f"| Default cradles | {c['cradle_count']:.0f} (layout {c['layout']:.0f} = opposed) |",
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
