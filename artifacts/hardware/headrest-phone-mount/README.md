# Headrest phone mount (3D)

**Artifact ID:** ART-RIDE-MOUNT-001  
**Kind:** mechanical-3d  
**Format:** OpenSCAD (source of truth) + generated STL  
**License:** GPL-2.0  
**Version:** 1.0.0 (geometry-verified; not a road release)

Copyright (C) 2026 RideAudit contributors. GPL-2.0-or-later. See [LICENSE](LICENSE) and [NOTICE](NOTICE). Apache-2.0 and MIT are not substitute licenses for this package.

## Purpose

One landscape phone, held on a vertical plate. Two post blocks, one per post. The post passes through the block. The block heel sits flush on the headrest pad. Each block has a 200 mm arm in a horizontal plane. Both arms slide into one shared cradle from the rear. Each arm has a longitudinal slot down its length, so the cradle can slide forward or back to set how far the phone sits from the pad. Its own M5 thumbscrew comes up from below, through a slot in the cradle bottom, through that arm slot, and into a tapped hole in the receiver roof.

The phone sits flush on the forward face of the vertical plate. Nothing in this package is a second cradle, a shared rail, a sliding clip, or an arm that rises toward the phone.

The blocks are independent. A single row of M5 holes, 10 mm apart, is the sideways lock. The longitudinal slot is the depth lock: slide the cradle along the arms, then tighten each arm's thumbscrew. The arm slot is wider than the hole pitch, so a small yaw still finds a hole.

![Complete assembly](verification/previews/assembly.png)

## Safety disclaimer

**This mount is not crash-tested and is not a certified automotive restraint, child seat, or OEM accessory.** It is a DIY audit fixture. Do not place it where it can interfere with airbags, the head restraint as the vehicle maker designed it, seatbelt geometry, or the driver's view. Do not cover airbag stitch lines, the headliner, or the headrest height lock. If the posts or the pad do not hold the blocks firmly, do not use the mount in a moving vehicle. Users assume all risk.

On the default model the block heels are flush on the pad, the arms run **200 mm** forward from the post axes, and the phone's front face is **220.7 mm** forward of the pad. The top of the cradle is **110.2 mm** above the bottom of the blocks. That forward reach has to stay clear of the driver, airbags, and the headrest release. Confirm the fit with [verification/vehicle-fit-checklist.md](verification/vehicle-fit-checklist.md) before any on-road use. Geometry checks in this repository are CAD checks, not a vehicle test.

## What you print

Print the part STLs in `exports/`. `headrest-phone-mount.stl` is an assembly preview, not a print file. Print `post-block.stl` twice; both blocks are the same part.

| Part | File | Qty | Notes |
| --- | --- | --- | --- |
| Fit coupon (print this first) | `exports/fit-coupon.stl` | 1 | 18 mm slice of the round post bore |
| Post block | `exports/post-block.stl` | 2 | Horizontal arm, vertical bore. No support rib. |
| Phone cradle | `exports/phone-cradle.stl` | 1 | Shared landscape holder. Arms enter from the rear. Bottom slots line up with the roof threads. |
| Thumbscrew | `exports/thumbscrew.stl` | 2 | Modeled M5 thumbscrew. Shown in the assembly. Metal M5×25 is the hardware match. |

The post block is **36.0 × 211.2 × 44.0 mm**. It needs about **215 mm** of travel on one axis. The cradle is **214.0 × 114.7 × 33.5 mm**. The thumbscrew is **26.0 × 16.0 × 26.1 mm**.

**Post block**

![Post block](verification/previews/post-block.png)

**Phone cradle**

![Phone cradle](verification/previews/phone-cradle.png)

**Fit coupon**

![Fit coupon](verification/previews/fit-coupon.png)

**Thumbscrew**

![Thumbscrew](verification/previews/thumbscrew.png)

## Print settings (starting point)

| Setting | Recommendation |
| --- | --- |
| Material | **PETG** (preferred) or ABS. PLA softens in a closed cabin. |
| Nozzle | 0.4 mm |
| Layer height | 0.2 mm |
| Perimeters | 5 on the post blocks and the cradle; 4 on the coupon |
| Infill | 40% gyroid or cubic |
| Top/bottom layers | 5 |
| Supports | None. The cradle pocket overhangs at 45°. |
| Bed | Block: arm and heel down, bore vertical. Cradle: rear edge down (bottom and roof stand as walls). Thumbscrew: head and wings down, shank up. Coupon: bore vertical. |

The block bore is a round vertical hole. The fit coupon is the same round bore; use it to judge the diameter.

The receiver roof carries the M5 threads. Holes are modeled at the M5×0.8 tap-drill diameter (4.2 mm). Chase every hole you might use with an M5 tap before assembly. The plastic thread is the lock; there is no nut pocket.

## Measure, then print the coupon

1. **Post spacing.** Center-to-center of the two posts. The hole row covers spacings from 110 mm to 170 mm. Set `post_spacing` to your measurement and re-export so the preview matches the car. The blocks themselves are independent; spacing is not a slot in a rail.
2. **Post diameter.** Outside diameter of one post. Set `post_diameter`. The bore is that diameter plus `clearance` (default 0.4 mm).
3. **Print `fit-coupon.stl`.** Slide it onto the post. The headrest may have to come out of the seat if the posts are captive. The coupon should start by hand and should not rattle. If it is tight, raise `clearance` toward 0.6 mm. If it is loose, lower `clearance` toward 0.2 mm.
4. **Phone, landscape.** Long edge is `phone_length_*` (140–172 mm). Short edge, the vertical one, is `phone_width_*` (70–85 mm). Thickness including a slim case is `phone_thickness_max` (up to 12 mm).
5. **Camera.** 18 mm square windows in both upper corners of the back plate. If the phone cameras face the pad, put those windows at the edge of the cushion so the lenses are not buried in foam. If the cameras face outward, the windows keep a corner camera bump off the plastic.
6. **Airbag / headrest.** The mount bears on the posts and on the headrest face at the block heels. It may not bear on an airbag cover. Keep the 220.7 mm forward reach and the 110.2 mm height off airbag covers, the driver, and the height lock.

Details: [dimensions.md](dimensions.md).

## Assembly

Hardware is listed in [bom.md](bom.md).

1. Slide each block onto a post until the flat heel is flush on the headrest pad. Print the same STL twice.
2. Slide both horizontal arms into the cradle from the rear, between the bottom plate and the roof. The phone plate stays vertical. Slide the cradle along the longitudinal slots until the phone is the depth you want.
3. From below, run one thumbscrew up through the cradle-bottom slot, through that arm's slot, and into the tapped hole the slot exposes. Tighten until that arm cannot shift. Each screw clamps only its own arm. The modeled screw is `thumbscrew.stl`; a metal M5×25 matches the 21 mm shank.
4. Run an M5 pinch screw into the top of each block until it bears on the post. That stops the block rotating after you have picked the holes.
5. Set the phone in from the top. The back of the phone sits flush on the vertical plate. The front lip keeps it from tipping out. A strap through the side slots is the backup retainer.

Foam thickness, per side, when the phone is under the maximum:

- Length: `(phone_length_max - actual length) / 2` against each side wall
- Short side: `phone_width_max - actual short side` under the strap, at the top of the pocket
- Thickness: `phone_thickness_max - actual thickness` against the back plate

To move the phone closer to the pad or farther out, loosen the two thumbscrews and slide the cradle along the arm slots, then retighten. The holes are 10 mm apart across the roof if an arm needs a neighboring hole. Then snug the pinch screws again.

## Exporting STL and previews

The `.scad` source is the source of truth under GPL-2.0. From this directory:

```text
./export-stls.sh
./export-previews.sh
```

`export-previews.sh` writes the isometric PNGs under `verification/previews/` for the post block, the cradle, the fit coupon, the thumbscrew, and the complete assembly.

Or one part at a time:

```text
openscad -o exports/post-block.stl --export-format binstl -D 'part="block"' headrest-phone-mount.scad
```

`part` is one of `assembly`, `block`, `tray`, `coupon`, `screw`.

Check the geometry (pocket, camera windows, post bore, rear entry, longitudinal arm slots, cradle-bottom slots, modeled thumbscrews, hole row, flush heels, no arm interference):

```text
python3 verify-geometry.py
```

That rewrites [verification/geometry-report.md](verification/geometry-report.md). OpenSCAD 2021 or newer is required. A failing assert in the `.scad` file means the parameters cannot satisfy the mount constraints.

## License (GPL-2.0)

```
RideAudit headrest phone mount
Copyright (C) 2026 RideAudit contributors

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, see <https://www.gnu.org/licenses/>.
```
