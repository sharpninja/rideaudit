# Dimensions

Copyright (C) 2026 RideAudit contributors  
License: GPL-2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).  
Not Apache-2.0. Not MIT.

All figures are millimetres. Defaults are the customizer values in `headrest-phone-mount.scad`. Derived sizes are what `verify-geometry.py` measures for those defaults; they change when you edit the parameters and re-export.

The mount is two independent post blocks and one cradle. Each post goes through its block. Both 200 mm arms enter one receiver. There is no beam and no sliding clamp range.

## Parameters

| Parameter | Default | Meaning |
| --- | --- | --- |
| `post_spacing_min` | 110 | Narrowest post center-to-center whose arms can still meet |
| `post_spacing_max` | 170 | Widest post center-to-center whose arms can still meet |
| `post_spacing` | 140 | Block positions in the assembly preview |
| `post_diameter` | 14 | Nominal headrest-post outside diameter |
| `arm_length` | 200 | Post axis to arm tip |
| `slot_span` | 16 | Slot length across the arm width |
| `slot_gap` | 8 | Slot opening along the arm, wide enough for an M5 shank when the arms cross |
| `slot_radius` | 185 | Post axis to the slot center |
| `hole_pitch` | 10 | Spacing of the M5 hole grid |
| `hole_nx`, `hole_nz` | 5 × 3 | Grid size. Both counts are odd so a hole sits on the nominal overlap |
| `phone_width_min` | 70 | Narrowest landscape short side |
| `phone_width_max` | 85 | Widest landscape short side (vertical in the cradle) |
| `phone_length_min` | 140 | Shortest landscape long side |
| `phone_length_max` | 172 | Longest landscape long side |
| `phone_thickness_max` | 12 | Thickest phone, including a slim case |
| `camera_clearance` | 18 | Square window, both upper corners of the back plate |
| `jaw_wall` | 4 | Material outside the post bore, and in front of the teardrop |
| `cradle_lip` | 3 | Front lip thickness. Lip height is `cradle_lip + 5` |
| `clearance` | 0.4 | Extra diameter in the post bore |

## Derived sizes at the defaults

| Item | Value |
| --- | --- |
| Post bore | 14.40 |
| Heel behind the bore | 4 |
| Block depth off the pad | 22.4 |
| Arm section | 22 × 5 |
| Arm print length | 225.3 |
| Phone pocket (length × short side × thickness) | 173.6 × 85.8 × 12.5 |
| Cradle print size | 180.4 × 159.8 × 21.5 |
| Stand-off from the headrest face | 22.4 |
| Cradle top above the block bottom | 307.0 |
| Hole grid | 5 × 3 at 10 mm, tap-drill 4.2 |
| Fit coupon | 36.0 × 22.4 × 18.0 |

The pocket is the **maximum** phone. Smaller phones in the same range sit in that pocket with foam, as described in the README. The cradle back and both block heels are coplanar at Y = 0.

## How to measure

1. **Post spacing.** Center-to-center of the two vertical posts. Any value from `post_spacing_min` to `post_spacing_max` still lets the arms meet on the hole grid, because each block pivots on its own post. Set `post_spacing` to the measured value before trusting the assembly preview.
2. **Post diameter.** The printed bore is `post_diameter + clearance`. The block bore adds a teardrop toward the front face for printing; the inscribed circle is still the bore diameter. The fit coupon is round.
3. **Phone, landscape.** Long edge → `phone_length_*`. Short edge, the vertical one → `phone_width_*`. Thickness including the case → `phone_thickness_max`.
4. **Camera.** The windows are `camera_clearance` squares in both upper corners and pass through the back plate.
5. **Airbag and headrest.** The fixture bears on the posts and on the headrest face. Keep the 22.4 mm stand-off and the 307 mm height off airbag covers, the headliner, and the headrest height lock.

## Fit coupon

`exports/fit-coupon.stl` is an 18 mm slice of the post block with the same round bore and the same heel. Slide it onto the post:

- It should start by hand and then hold.
- It should not spin freely once you are happy with the clearance. The pinch screw, not the coupon, is what locks rotation on the real block.
- If it will not start, increase `clearance` slightly and reprint the coupon only.
- If it rattles, reduce `clearance`.

Do not print the 225 mm blocks until the coupon fits.
