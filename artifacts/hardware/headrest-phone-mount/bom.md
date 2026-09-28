# Bill of materials

Copyright (C) 2026 RideAudit contributors  
License: GPL-2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).  
Not Apache-2.0. Not MIT.

One shared landscape cradle. Two post blocks. No rail, no cradle clip, no stop pin.

## Printed parts

| Part | STL | Qty | Notes |
| --- | --- | --- | --- |
| Fit coupon | `exports/fit-coupon.stl` | 1 | Print before the blocks. Round bore, same diameter as the blocks. |
| Post block, inner arm | `exports/post-block.stl` | 1 | 225.3 × 48.5 × 22.4 mm print. Snap off the rib under the arm. |
| Post block, outer arm | `exports/post-block-outer.stl` | 1 | Same print envelope. Arm stacks on the inner arm. Snap off the rib. |
| Phone cradle | `exports/phone-cradle.stl` | 1 | 180.4 × 159.8 × 21.5 mm. Receiver and 5×3 M5 hole grid. |

Regenerate every STL from `headrest-phone-mount.scad` with `./export-stls.sh` after a parameter change. Do not edit the meshes. `exports/headrest-phone-mount.stl` is the assembly preview only.

## Metal hardware

| Item | Qty | Notes |
| --- | --- | --- |
| M5×20 thumbscrew | 1 | Through both arm slots into one tapped hole in the cradle back. |
| M5×12 socket set screw or button head | 2 | One pinch screw in the front of each post block. |
| M5×0.8 tap | 1 | Chase the hole grid. Holes are modeled at 4.2 mm tap-drill. |

No nuts. The thread is in the solid back of the cradle.

## Soft goods

| Item | Qty | Notes |
| --- | --- | --- |
| Soft foam or rubber pad | 1 or 2 strips | Fill the gap when the phone is smaller than the max pocket. See README. |
| Velcro strap or reusable cable tie | 1 | Through the cradle side slots, over the phone. |

## Not included

- A second phone cradle
- Phones, chargers, or vehicle power taps
- Adhesive on airbag covers, the headliner, or the headrest pad
- Crash certification, airbag testing, or an OEM fit claim
