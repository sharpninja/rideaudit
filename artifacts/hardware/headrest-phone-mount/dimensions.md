# Dimensions

Copyright (C) 2026 RideAudit contributors  
License: GPL-2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).  
Not Apache-2.0. Not MIT.

All figures are millimetres. Defaults are the customizer values in `headrest-phone-mount.scad`. Derived sizes are what `verify-geometry.py` measures for those defaults; they change when you edit the parameters and re-export.

The mount is two identical post blocks and one cradle. Each post goes through one round collar. Both 200 mm arms lie in a horizontal plane, join that collar through gussets, and enter one receiver from the rear. A longitudinal slot in each arm lets the cradle slide to set depth. The slot is 1 mm clear of the M8 crest on each side. There is no beam and no sideways hole row.

## Parameters

| Parameter | Default | Meaning |
| --- | --- | --- |
| `post_spacing_min` | 110 | Narrowest post center-to-center the receiver still accepts |
| `post_spacing_max` | 170 | Widest post center-to-center the receiver still accepts |
| `post_spacing` | 140 | Block positions in the assembly preview. Re-export for the measured spacing |
| `bore_id` | 14.5 | Inner diameter of the collar. Clears a 14 mm post by 0.5 mm |
| `block_od` | 25.5 | Outer diameter of the same collar |
| `block_t` | 10 | Axial thickness of that collar |
| `arm_length` | 200 | Post axis to arm tip, measured forward along the horizontal arm |
| `slot_span` | 10 | Arm slot width. M8 crest 8 mm plus 1 mm on each side |
| `slot_gap` | 14 | Length of each cradle-bottom slot along the arm |
| `phone_width_min` | 70 | Narrowest landscape short side |
| `phone_width_max` | 85 | Widest landscape short side (vertical in the cradle) |
| `phone_length_min` | 140 | Shortest landscape long side |
| `phone_length_max` | 172 | Longest landscape long side |
| `phone_thickness_max` | 12 | Thickest phone, including a slim case |
| `camera_clearance` | 18 | Square window, both upper corners of the back plate |
| `slot_radius` | 185 | Post axis to the preview thumbscrew, along the arm |
| `cradle_lip` | 3 | Front lip thickness. Lip height is `cradle_lip + 5` |

## Derived sizes at the defaults

| Item | Value |
| --- | --- |
| Post collar | One cylinder. Bore 14.5, outside 25.5, thickness 10, wall 5.5 |
| Arm section | 56 × 12. Each rail beside the 10 mm slot is 23 wide |
| Root gussets | Two ribs, 10 mm above the arm at the collar, tapering off before the slot. Side fillets span the collar-to-arm step |
| Arm slot | 171 long × 10.0 wide, 1.00 mm each side of the M8 crest, from 34 mm forward of the pad |
| Depth adjustment behind the preview screw | 164 |
| Block print size | 56.0 × 212.8 × 22.0. The 22 mm includes the gussets; the collar is 10 mm thick |
| Phone pocket (length × short side × thickness) | 173.6 × 85.8 × 12.5 |
| Cradle print size | 242.0 × 120.2 × 33.5 |
| Cradle bottom slots | 14 long × 9.0 wide, one under each arm |
| Phone front from the headrest face | 222.2 |
| Cradle top above the block bottom | 114.0 |
| Threaded holes | 2, one on each arm centerline, tap-drill 6.8 |
| Thumbscrews | 2 modeled M8×1.25, one per arm, from below, crest Ø 8, 30.5 mm shank under the head, head Ø 22 |
| Fit coupon | 25.5 × 25.5 × 10.0, the collar without the arm |

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
| Roof holes | One per arm, on the arm centerline. The two holes are a post-spacing apart, so the head cannot fall into the other slot |
| Shank under the face | 30.5. A metal M8×30 matches it. A longer screw can break out of the roof |

The channel height is fixed by the cheeks, so the screw does not close the 0.40 mm by stretching the plastic. That 0.40 mm is the clearance the seated head takes up.

## How to measure

1. **Post spacing.** Center-to-center of the two vertical posts. Set `post_spacing` and re-export. The cradle is cut for that spacing. Cradle depth is the slide along the longitudinal slot.
2. **Post diameter.** The printed bore is 14.5 mm on both the block and the fit coupon. It is not a per-car parameter. See the source table below.
3. **Phone, landscape.** Long edge → `phone_length_*`. Short edge, the vertical one → `phone_width_*`. Thickness including the case → `phone_thickness_max`.
4. **Camera.** The windows are `camera_clearance` squares in both upper corners and pass through the back plate.
5. **Airbag and headrest.** The fixture bears on the posts and on the headrest face at the rear of each collar. Keep the 222.2 mm forward reach and the 114.0 mm height off airbag covers, the driver, the headliner, and the headrest height lock.

## Fit coupon

`exports/fit-coupon.stl` is the same 14.5 / 25.5 × 10 mm collar, without the arm. Slide it onto the post:

- On a 14 mm post it should start by hand and then hold.
- The pinch screw, not the coupon, is what locks rotation on the real block.
- If it will not start, the post is larger than 14 mm. Do not open the bore in this package; this collar is sized for that maximum.
- If it rattles, the post is smaller than 14 mm. Tighten the radial pinch screw on the block. The bore stays 14.5 mm.

Do not print the 213 mm arms until the coupon fits.

## Factory post diameters

The bore is locked at 14.5 mm so the largest post in the chart below (14 mm) has 0.5 mm of diametral clearance. These figures are make-level, not measurements of a named compact SUV. No model-by-model caliper survey was found for Equinox, Escape, Tucson, CR-V, RAV4, or Rogue. Measure the post before printing.

| Make | Factory post diameters | Source |
| --- | --- | --- |
| Chevrolet / GM | 10, 11, 12, and 14 mm | Philips JENHR1D vehicle-preparation chart |
| Ford / Lincoln | 10, 12, and 12.7 mm | same chart |
| Honda / Acura | 10 and 12.7 mm | same chart |
| Toyota / Lexus | 12 and 13.88 mm | same chart |
| Nissan / Infiniti | 12.7 mm | same chart |
| Hyundai | not published in that chart | A Tucson-forum note guessed a guide hole near 1/2 in (12.7 mm). That is not a calipered post |

The Philips chart is reproduced in the JENHR1D user manual (vehicle preparation). The same manual's adapter kit also stocks 12, 12.5, 12.7, 13.8, 14, and 16 mm tubes. 16 mm is not assigned to these makes. A 16 mm post will not enter the 14.5 mm bore.

Sources: [Philips JENHR1D vehicle preparation](https://manualsdump.com/en/manuals/philips-jenhr1d/212963/7). Hyundai guide-hole guess: [Tucson forum, rear middle headrest](https://www.tucson-forum.com/threads/rear-middle-headrest.271/).
