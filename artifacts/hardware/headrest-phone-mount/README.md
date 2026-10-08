# Headrest phone mount (3D)

**Artifact ID:** ART-RIDE-MOUNT-001  
**Kind:** mechanical-3d  
**Format:** OpenSCAD (source of truth) + generated STL  
**License:** GPL-2.0  
**Version:** 1.1.0 (geometry-verified for a closed Galaxy Z Fold 4; not a road release)

Copyright (C) 2026 RideAudit contributors. GPL-2.0-or-later. See [LICENSE](LICENSE) and [NOTICE](NOTICE). Apache-2.0 and MIT are not substitute licenses for this package.

## Purpose

One landscape phone, held on a vertical plate. Two post blocks, one per post. Each block is one round collar, 25.5 mm outside and 33 mm thick along the post. The bore is the measured post plus 0.2-0.5 mm. The default export is a 14 mm post with 0.5 mm clearance, so the preview bore is 14.5 mm. The rear of the collar is tangent to the headrest pad. Each collar carries a 200 mm arm in a horizontal plane. The arm shares the collar's bottom face, and short blends rise into the 33 mm collar so the arm moment enters the tube around the post. Both arms slide into one shared cradle from the rear. Each arm has a longitudinal slot 10 mm wide, 1 mm clear of the M8 crest on each side, so the cradle can slide forward or back to set how far the phone sits from the pad. Its own M8×1.25 thumbscrew comes up from below, on that arm's centerline, through a slot in the cradle bottom, through that arm slot, and into a tap hole in the receiver roof. Fully seated, the head face clamps the bottom plate and the arm.

The closed Galaxy Z Fold 4 sits in that cradle in landscape. The cover screen is flush on the forward face of the vertical plate. The primary rear cameras face forward, out of the opening, toward the road. Nothing in this package is a second cradle, a shared rail, a sliding clip, or an arm that rises toward the phone.

The blocks are independent. Set `post_spacing` to the measured center distance (120-170 mm; 130, 150, and 160 mm are common) and re-export so each arm's tap hole sits on that arm. The longitudinal slot is only the depth lock: it is too tight to reach a neighboring hole. Set `post_od` to the measured post (presets 10, 12, 12.7, 13.8, and 14 mm) and reprint the coupon with the blocks.

![Complete assembly](verification/previews/assembly.png)

## Safety disclaimer

**This mount is not crash-tested and is not a certified automotive restraint, child seat, or OEM accessory.** It is a DIY audit fixture. Do not place it where it can interfere with airbags, the head restraint as the vehicle maker designed it, seatbelt geometry, or the driver's view. Do not cover airbag stitch lines, the headliner, or the headrest height lock. If the posts or the pad do not hold the blocks firmly, do not use the mount in a moving vehicle. Users assume all risk.

On the default model the collar rears are tangent to the pad, the arms run **200 mm** forward from the post axes, and the cradle front (the USB-end hook) is **225.3 mm** forward of the pad. The hinge face of the closed phone is **222.6 mm** forward of the pad. The top of the cradle is **98.7 mm** above the bottom of the blocks. That forward reach has to stay clear of the driver, airbags, and the headrest release. The camera lenses stand further out than the hinge face. Their protrusion is not a published dimension, so it is not added here. Confirm the fit with [verification/vehicle-fit-checklist.md](verification/vehicle-fit-checklist.md) before any on-road use. Geometry checks in this repository are CAD checks, not a vehicle test. HW1 on-vehicle print is not done.

## What you print

Print the part STLs in `exports/`. `headrest-phone-mount.stl` is an assembly preview, not a print file. Print `post-block.stl` twice; both blocks are the same part.

| Part | File | Qty | Notes |
| --- | --- | --- | --- |
| Fit coupon (print this first) | `exports/fit-coupon.stl` | 1 | The collar without the arm. Default file is 14.5 / 25.5 × 33 mm. Reprint it when `post_od` or `post_clearance` changes |
| Post block | `exports/post-block.stl` | 2 | Round 33 mm collar, horizontal arm, short blends into the collar. |
| Phone cradle | `exports/phone-cradle.stl` | 1 | Closed Fold 4, landscape, cameras forward. Arms enter from the rear. Bottom slots line up with the roof threads. |
| Thumbscrew | `exports/thumbscrew.stl` | 2 | Modeled M8×1.25 thumbscrew with an external thread. Shown in the assembly. Metal M8×30 is the hardware match. |

The post block prints **56.0 × 212.8 × 33.0 mm**. The 33 mm height is the collar. The blends stop at that top face. It needs about **215 mm** of travel on one axis. The cradle is **242.0 × 104.9 × 36.5 mm** and needs about **250 mm** on one axis. The fit coupon is **25.5 × 25.5 × 33.0 mm**. The thumbscrew is **32.0 × 22.0 × 36.5 mm**.

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

The receiver roof carries the M8 threads. Holes are modeled at the M8×1.25 tap-drill diameter (6.8 mm). The thumbscrew STL carries a modeled external thread (crest Ø 8 mm, pitch 1.25 mm). Chase every hole you might use with an M8×1.25 tap before assembly. The plastic thread is the lock; there is no nut pocket. Pinch screws in the post blocks stay M5.

## Measure, then print the coupon

1. **Post spacing.** Center-to-center of the two posts, from 120 mm to 170 mm. Common published centers are 130, 150, and 160 mm. Set `post_spacing` and re-export. Each cradle is cut for one spacing: the arm slot has only 1 mm of side clearance, so it cannot slide onto a different hole.
2. **Post diameter.** Set `post_od` to the measured post, from 10 mm to 14 mm, and `post_clearance` from 0.2 mm to 0.5 mm. Bore = `post_od + post_clearance`. The default is 14 mm with 0.5 mm clearance (bore 14.5 mm). A post larger than 14 mm does not enter this collar. Sources, presets, and the model chart are in [dimensions.md](dimensions.md).
3. **Print `fit-coupon.stl`.** Slide it onto the post. The headrest may have to come out of the seat if the posts are captive. It should start by hand on a post that matches `post_od`. Reprint the coupon when `post_od` or the clearance changes. The pinch screw stops rotation on the finished block.
4. **Phone.** Closed Galaxy Z Fold 4, no case, landscape. Long edge 155.1 mm. Short edge, vertical, 67.1 mm. Hinge 15.8 mm. Thin edge 14.2 mm.
5. **Cameras.** Cover screen flush on the back plate. Primary rear cameras face forward, out of the opening, toward the road. Put the rear-cluster end (portrait top) on the open end. Put the USB-C end against the hook. The back plate has no camera window.
6. **Airbag / headrest.** The mount bears on the posts and on the headrest face at the back of each collar. It may not bear on an airbag cover. Keep the 225.3 mm cradle front and the 98.7 mm height off airbag covers, the driver, and the height lock. Add the real lens bump, which is not in the 225.3 mm figure.

Details: [dimensions.md](dimensions.md).

## Assembly

Hardware is listed in [bom.md](bom.md).

1. Slide each collar onto a post until its rear is against the headrest pad. Print the same STL twice.
2. Slide both horizontal arms into the cradle from the rear, between the bottom plate and the roof. The phone plate stays vertical. Slide the cradle along the longitudinal slots until the phone is the depth you want.
3. From below, run one thumbscrew up through the cradle-bottom slot, through that arm's slot, and into the tapped hole the slot exposes. Tighten until the head face is seated on the cradle bottom. Each screw clamps only its own arm. The modeled screw is `thumbscrew.stl`; a metal M8×30 matches the 30.5 mm shank. A longer screw can break out of the 19.0 mm roof. The seated tip stays 7.0 mm below the pocket floor.
4. Run an M5 pinch screw through the outboard wall of each collar, at mid-height, until it bears on the post. That stops the block rotating.
5. Set the closed phone in from the top, landscape. Cover screen on the plate. Rear cameras toward the road. USB-C end against the hook. The 12 mm hook stops the phone sliding forward. A strap through the side slots is the backup retainer.

The pocket is cut for this phone only. Clearance is 0.40 mm at each end of the length, 0.40 mm at the top, and 0.40 mm in front of the hinge. Do not add a case. The 14.2 mm thin edge has extra air in front of the glass because the pocket is sized to the 15.8 mm hinge.

To move the phone closer to the pad or farther out, loosen the two thumbscrews and slide the cradle along the arm slots, then retighten. Sideways position is the `post_spacing` you exported, not a second hole. Then snug the pinch screws again.

## Clamp stack

Fully seated means the head bearing face is against the underside of the cradle bottom. The CAD model leaves a 0.12 mm gap there so the meshes do not share a face; that gap is not looseness in the clamp.

| Member | mm |
| --- | --- |
| Cradle bottom plate | 6 |
| Arm | 12 |
| Clamped stack | 18 |
| Slide clearance taken up (0.20 under the arm and 0.20 over it) | 0.40 |
| Thread left in the 19.0 mm roof | 12 |
| Head bearing face | Ø 22 |
| Bottom clearance slot | 9.0 wide × 14 along the arm |
| Roof hole | 6.8 tap drill; the Ø 8 crest bites that wall |
| Shank under the face | 30.5 |

There is one bottom slot per arm, so the Ø 22 face bears on the plate around that 9 mm slot. The cheeks hold the channel at a fixed height, so the 0.40 mm is the seating take-up. Each arm rail beside the 10 mm slot is 23 mm wide and 12 mm thick.

## Collar and arm joint

The collar is 33 mm along the post, 25.5 mm outside, with a 14.5 mm bore on the default 14 mm post. The 12 mm arm shares the bottom face of that collar, so a downward load at the phone puts compression straight into the tube. The collar stands 21 mm above the arm. A short center web and two side blends rise into that wall and taper onto the arm over 9 mm, stopping at the collar top and stopping before the slot. The moment goes into the tube around the post. The blends do not stand above the collar.

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

Check the geometry (Fold 4 pocket, open camera end, USB-end hook, default 14 mm post and 14.5 mm bore, 33 mm collar, rear entry, tight arm slots, root blends, cradle-bottom slots, modeled thumbscrews, flush heels, no arm interference):

```text
dotnet run --project ../../../tools/RideAudit.HeadrestGeometry -- --root .
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
