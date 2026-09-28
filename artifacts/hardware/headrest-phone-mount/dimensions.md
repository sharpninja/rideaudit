# Dimensions

Copyright (C) 2026 RideAudit contributors  
License: GPL-2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).  
Not Apache-2.0. Not MIT.

All figures are millimetres. Defaults are the customizer values in `headrest-phone-mount.scad`. Derived sizes are what `verify-geometry.py` measures for those defaults; they change when you edit the parameters and re-export.

## Parameters

| Parameter | Default | Meaning |
| --- | --- | --- |
| `post_spacing_min` | 110 | Minimum post center-to-center the clamps can close to |
| `post_spacing_max` | 170 | Maximum post center-to-center the clamps can open to |
| `post_spacing` | 140 | Clamp positions in the assembly preview only |
| `post_diameter` | 14 | Nominal headrest-post outside diameter |
| `phone_width_min` | 70 | Narrowest landscape short side |
| `phone_width_max` | 85 | Widest landscape short side (vertical in the cradle) |
| `phone_length_min` | 140 | Shortest landscape long side |
| `phone_length_max` | 172 | Longest landscape long side (horizontal in the cradle) |
| `phone_thickness_max` | 12 | Thickest phone, including a slim case |
| `camera_clearance` | 18 | Square window, both upper corners of the back plate |
| `dual_cradle` | true | Two phones when true; one road phone when false |
| `cradle_layout` | 0 | 0 = opposed (road + cabin). 1 = dual-forward |
| `clamp_depth` | 35 | Jaw length along the post |
| `beam_thickness` | 8 | Rail height |
| `jaw_wall` | 4 | Material around the post bore |
| `cable_notch_w` | 10 | Cable-slot width (capped at 6.5 mm so the rail stays intact) |
| `cable_notch_d` | 6 | Tie-down depth used to size the shallow face groove |
| `cradle_lip` | 3 | Front lip thickness. Lip height is `cradle_lip + 5` |
| `clearance` | 0.4 | Extra diameter in the post bore |

`throat_ratio` (Hidden section, default 0.76) sets the snap opening to `throat_ratio * post_diameter`. Raise it only after the fit coupon will not start on the post.

## Derived sizes at the defaults

| Item | Value |
| --- | --- |
| Post bore | 14.40 |
| Snap throat | 10.64 |
| Beam print length | 209.4 |
| Beam print width × height | 59.6 × 8.0 |
| Phone pocket (length × short side × thickness) | 173.6 × 85.8 × 12.5 |
| Cradle print size | 180.4 × 89.8 × 20.5 |
| Projection from the post centerline, each face | 57.9 |
| Cradle top above the clamp bottom | 89.8 |
| Clamp print size | 22.0 × 44.3 × 35.0 |
| Extension print length | 31.3 |

The pocket is the **maximum** phone. Smaller phones in the same range sit in that pocket with foam, as described in the README. Both the maximum phone and a minimum-length phone were probed as empty space inside the tray by `verify-geometry.py`.

## How to measure

1. **Post spacing.** Center-to-center of the two vertical headrest posts. Set `post_spacing_min` slightly below and `post_spacing_max` slightly above that number so the clamps can slide and then lock. The rail is long enough for clamp centers at `post_spacing_max`, and the stop pins sit outside that travel.
2. **Post diameter.** Measure the post outside diameter. The printed bore is `post_diameter + clearance`. Print the fit coupon before the beam.
3. **Phone, landscape.** Long edge → `phone_length_*`. Short edge, the one that will be vertical → `phone_width_*`. Thickness including the case → `phone_thickness_max`.
4. **Camera.** The windows are `camera_clearance` squares in both upper corners and pass through the back plate. Increase the parameter if the lens island is larger than the window. The front lip stays below the windows.
5. **Airbag and headrest.** The fixture may load only the posts. Keep the 57.9 mm projection and the 89.8 mm height off airbag covers, the headliner, and the headrest height lock.

## Fit coupon

`exports/fit-coupon.stl` is an 18 mm slice of the clamp jaw with the same bore and throat as the full clamp. Snap it onto the post:

- It should start by hand and then hold.
- It should not spin freely.
- If it will not start, the throat is tight for that post: increase `throat_ratio` slightly and reprint the coupon only.
- If it rattles, reduce `clearance`.

Do not print the 209 mm beam until the coupon fits.
