# Bill of materials

Copyright (C) 2026 RideAudit contributors  
License: GPL-2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).  
Not Apache-2.0. Not MIT.

Quantities are for the default opposed mount (road + cabin). Dual-forward changes are in the last column.

## Printed parts

| Part | STL | Opposed | Dual-forward | Notes |
| --- | --- | --- | --- | --- |
| Fit coupon | `exports/fit-coupon.stl` | 1 | 1 | Print before the full set. Snap-test on the actual post. |
| Beam | `exports/beam.stl` | 1 | 1 | 209.4 × 59.6 × 8.0 mm at the default parameters. Needs a 220 mm axis. |
| Post clamp | `exports/post-clamp.stl` | 2 | 2 | Snap jaw plus sliding shoe. |
| Phone cradle | `exports/phone-cradle.stl` | 2 | 2 | Landscape tray. One only if `dual_cradle` is false. |
| Cradle clip | `exports/cradle-clip.stl` | 1 | 2 | Road side. Open face toward the center link. |
| Cabin clip | `exports/cradle-clip-cabin.stl` | 1 | 0 | Mirror of the road clip. Omit for dual-forward. |
| Stop pin | `exports/stop-pin.stl` | 4 | 2 | Press into the rail holes after the sliders are on. |
| Beam extension | `exports/beam-extension.stl` | 0 | 2 | Dual-forward only. Mirror one copy in the slicer. |

Regenerate every STL from `headrest-phone-mount.scad` with `./export-stls.sh` after a parameter change. Do not edit the meshes.

## Metal hardware

| Item | Opposed qty | Notes |
| --- | --- | --- |
| M3×8 flat-head socket screw | 4 | Cradle to clip, two each. Heads sit in the back-plate countersinks. |
| M3×10 socket-head cap screw | 4 | Rail locks: one on each clamp, one on each clip. Tip bears on the rail. |
| M3 hex nut | 8 | Four in the clip pads, four in the rail-lock pockets. |
| 2.5 mm hex key | 1 | Drives both screw types. |

Dual-forward uses the same eight nuts and eight screws (two clips, two clamps, two cradles).

## Soft goods

| Item | Qty | Notes |
| --- | --- | --- |
| Soft foam or rubber pad | 2 to 4 strips | Fill the gap when the phone is smaller than the max pocket. See README. |
| Velcro strap or reusable cable tie | 2 | Through the cradle side slots, over the phone. Also for the charge lead. |

## Not included

- Phones, chargers, or vehicle power taps
- Adhesive on airbag covers, the headliner, or the headrest pad
- Crash certification, airbag testing, or an OEM fit claim
