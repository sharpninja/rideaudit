# Bill of materials

Copyright (C) 2026 RideAudit contributors  
License: GPL-2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).  
Not Apache-2.0. Not MIT.

One shared landscape cradle. Two identical post blocks. No rail, no cradle clip, no stop pin.

## Printed parts

| Part | STL | Qty | Notes |
| --- | --- | --- | --- |
| Fit coupon | `exports/fit-coupon.stl` | 1 | Print before the blocks. Round bore, same diameter as the blocks. |
| Post block | `exports/post-block.stl` | 2 | 36.0 × 211.2 × 44.0 mm print. Horizontal arm, vertical bore. |
| Phone cradle | `exports/phone-cradle.stl` | 1 | 200.0 × 33.5 × 101.8 mm. Rear entry, one row of M5 holes in the roof. |

Regenerate every STL from `headrest-phone-mount.scad` with `./export-stls.sh` after a parameter change. Do not edit the meshes. `exports/headrest-phone-mount.stl` is the assembly preview only.

## Metal hardware

| Item | Qty | Notes |
| --- | --- | --- |
| M5×20 thumbscrew | 2 | One per arm. From below, through that arm's slot, into a tapped hole in the receiver roof. |
| M5×12 socket set screw or button head | 2 | One pinch screw in the top of each post block. |
| M5×0.8 tap | 1 | Chase the hole row. Holes are modeled at 4.2 mm tap-drill. |

No nuts. The thread is in the receiver roof.

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
