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
| `slot_span` | 16 | Width of the longitudinal slot across the arm |
| `slot_gap` | 12 | Length of each cradle-bottom slot along the arm |
| `slot_radius` | 185 | Post axis to the preview thumbscrew, along the arm |
| `hole_pitch` | 10 | Spacing of the M5 hole row |
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
| Arm section | 28 × 8 |
| Arm slot | 157 long × 16 wide, starting 46 mm forward of the pad |
| Depth adjustment behind the preview screw | 150 |
| Arm print length | 211.2 |
| Phone pocket (length × short side × thickness) | 173.6 × 85.8 × 12.5 |
| Cradle print size | 214.0 × 114.7 × 33.5 |
| Cradle bottom slots | 12 long, one under each threaded hole |
| Phone front from the headrest face | 220.7 |
| Cradle top above the block bottom | 110.2 |
| Threaded holes | 19 in one row at 10 mm, out to ±90, tap-drill 4.2 |
| Thumbscrews | 2 modeled, one per arm, from below, 21.1 mm shank under the head |
| Fit coupon | 36.0 × 30.4 × 18.0 |

The pocket is the **maximum** phone. Smaller phones in the same range sit in that pocket with foam, as described in the README. The block heels are coplanar at Y = 0. The phone is flush on the vertical plate, about 200 mm forward of that plane.

## How to measure

1. **Post spacing.** Center-to-center of the two vertical posts. Any value from `post_spacing_min` to `post_spacing_max` still puts each arm's slot over the hole row. Set `post_spacing` to the measured value before trusting the assembly preview. Cradle depth is the slide along the longitudinal slot, not a change in post spacing.
2. **Post diameter.** The printed bore is `post_diameter + clearance`. The block bore and the fit coupon are both round, because the bore prints vertical.
3. **Phone, landscape.** Long edge → `phone_length_*`. Short edge, the vertical one → `phone_width_*`. Thickness including the case → `phone_thickness_max`.
4. **Camera.** The windows are `camera_clearance` squares in both upper corners and pass through the back plate.
5. **Airbag and headrest.** The fixture bears on the posts and on the headrest face at the block heels. Keep the 220.7 mm forward reach and the 110.2 mm height off airbag covers, the driver, the headliner, and the headrest height lock.

## Fit coupon

`exports/fit-coupon.stl` is an 18 mm slice of the post block with the same round bore and the same heel. Slide it onto the post:

- It should start by hand and then hold.
- It should not spin freely once you are happy with the clearance. The pinch screw, not the coupon, is what locks rotation on the real block.
- If it will not start, increase `clearance` slightly and reprint the coupon only.
- If it rattles, reduce `clearance`.

Do not print the 211 mm blocks until the coupon fits.
