# Dimensions

Copyright (C) 2026 RideAudit contributors  
License: GPL-2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).  
Not Apache-2.0. Not MIT.

All figures are millimetres. Defaults are the customizer values in `headrest-phone-mount.scad`. Derived sizes are what `verify-geometry.py` measures for those defaults; they change when you edit the parameters and re-export.

The mount is two identical post blocks and one cradle. Each post goes through its block. Both 200 mm arms lie in a horizontal plane and enter one receiver from the rear. A longitudinal slot in each arm lets the cradle slide to set depth. There is no beam and no sliding clamp range.

## Parameters

| Parameter | Default | Meaning |
| --- | --- | --- |
| `post_spacing_min` | 110 | Narrowest post center-to-center the hole row still covers |
| `post_spacing_max` | 170 | Widest post center-to-center the hole row still covers |
| `post_spacing` | 140 | Block positions in the assembly preview |
| `post_diameter` | 14 | Nominal headrest-post outside diameter |
| `arm_length` | 200 | Post axis to arm tip, measured forward along the horizontal arm |
| `slot_span` | 26 | Width of the longitudinal slot across the arm. Clears an M8 crest at half a hole pitch |
| `slot_gap` | 14 | Length of each cradle-bottom slot along the arm |
| `slot_radius` | 185 | Post axis to the preview thumbscrew, along the arm |
| `hole_pitch` | 16 | Spacing of the M8 hole row. Wide enough that the Ø 22 head bears on solid plate |
| `hole_x_max` | 90 | Half-width of the hole row |
| `phone_width_min` | 70 | Narrowest landscape short side |
| `phone_width_max` | 85 | Widest landscape short side (vertical in the cradle) |
| `phone_length_min` | 140 | Shortest landscape long side |
| `phone_length_max` | 172 | Longest landscape long side |
| `phone_thickness_max` | 12 | Thickest phone, including a slim case |
| `camera_clearance` | 18 | Square window, both upper corners of the back plate |
| `jaw_wall` | 4 | Nominal material outside the post bore |
| `front_wall` | 12 | Block material in front of the bore, where the arm roots |
| `cradle_lip` | 3 | Front lip thickness. Lip height is `cradle_lip + 5` |
| `clearance` | 0.4 | Extra diameter in the post bore |

## Derived sizes at the defaults

| Item | Value |
| --- | --- |
| Post bore | 14.40 |
| Heel behind the bore | 4 |
| Block depth off the pad | 30.4 |
| Arm section | 56 × 12. Each rail beside the slot is 15 wide |
| Arm slot | 157 long × 26 wide, starting 46 mm forward of the pad |
| Depth adjustment behind the preview screw | 150 |
| Arm print length | 211.2 |
| Block print size | 60.0 × 211.2 × 44.0 |
| Phone pocket (length × short side × thickness) | 173.6 × 85.8 × 12.5 |
| Cradle print size | 242.0 × 120.2 × 33.5 |
| Cradle bottom slots | 14 long × 9.0 wide, one under each threaded hole |
| Phone front from the headrest face | 220.7 |
| Cradle top above the block bottom | 114.0 |
| Threaded holes | 12 in one row at 16 mm, out to ±90, tap-drill 6.8 |
| Thumbscrews | 2 modeled M8×1.25, one per arm, from below, crest Ø 8, 30.5 mm shank under the head, head Ø 22 |
| Fit coupon | 60.0 × 30.4 × 18.0 |

The pocket is the **maximum** phone. Smaller phones in the same range sit in that pocket with foam, as described in the README. The block heels are coplanar at Y = 0. The phone is flush on the vertical plate, about 200 mm forward of that plane.

## Clamp stack

The preview screws are fully seated. The head bearing face sits on the underside of the cradle bottom (the 0.12 mm CAD gap is mesh clearance).

| Member | mm |
| --- | --- |
| Bottom plate, the bearing member | 6 |
| Arm | 12 |
| Clamped stack, gaps closed | 18 |
| Slide take-up, 0.20 under the arm and 0.20 over it | 0.40 |
| External thread in the roof | 12 (1.5 × the 8 mm major diameter) |
| Roof thickness | 16, so the tip stays about 4 mm below the phone pocket |
| Head | Ø 22 flat face around the 9 mm clearance slot |
| Thread | M8×1.25, crest Ø 8, valley held at Ø 5.8 so it stays inside the 6.8 tap drill |
| Bottom slot | 9.0 wide × 14 along the arm (major diameter plus clearance) |
| Roof hole | 6.8 tap drill. The modeled crest bites that wall. Chase it with an M8×1.25 tap for a metal screw |
| Hole pitch | 16, so the head does not fall into the neighboring slot |
| Shank under the face | 30.5. A metal M8×30 matches it. A longer screw can break out of the roof |

The channel height is fixed by the cheeks, so the screw does not close the 0.40 mm by stretching the plastic. That 0.40 mm is the clearance the seated head takes up.

## How to measure

1. **Post spacing.** Center-to-center of the two vertical posts. Any value from `post_spacing_min` to `post_spacing_max` still puts each arm's slot over the hole row. Set `post_spacing` to the measured value before trusting the assembly preview. Cradle depth is the slide along the longitudinal slot, not a change in post spacing.
2. **Post diameter.** The printed bore is `post_diameter + clearance`. The block bore and the fit coupon are both round, because the bore prints vertical.
3. **Phone, landscape.** Long edge → `phone_length_*`. Short edge, the vertical one → `phone_width_*`. Thickness including the case → `phone_thickness_max`.
4. **Camera.** The windows are `camera_clearance` squares in both upper corners and pass through the back plate.
5. **Airbag and headrest.** The fixture bears on the posts and on the headrest face at the block heels. Keep the 220.7 mm forward reach and the 114.0 mm height off airbag covers, the driver, the headliner, and the headrest height lock.

## Fit coupon

`exports/fit-coupon.stl` is an 18 mm slice of the post block with the same round bore and the same heel. Slide it onto the post:

- It should start by hand and then hold.
- It should not spin freely once you are happy with the clearance. The pinch screw, not the coupon, is what locks rotation on the real block.
- If it will not start, increase `clearance` slightly and reprint the coupon only.
- If it rattles, reduce `clearance`.

Do not print the 211 mm blocks until the coupon fits.
