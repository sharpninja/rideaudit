# Bill of materials

Copyright (C) 2026 RideAudit contributors  
License: GPL-2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).  
Not Apache-2.0. Not MIT.

One shared landscape cradle. Two identical post blocks. No rail, no cradle clip, no stop pin.

## Printed parts

| Part | STL | Qty | Notes |
| --- | --- | --- | --- |
| Fit coupon | `exports/fit-coupon.stl` | 1 | Print before the blocks. Same bore as the blocks. Reprint when `post_od` or `post_clearance` changes. Default file is 14.5 / 25.5 × 33 mm. |
| Post block | `exports/post-block.stl` | 2 | 56.0 × 212.8 × 33.0 mm print. One round collar, default bore 14.5 mm (14 mm post + 0.5 mm clearance), outside 25.5 mm, 33 mm thick, 56 × 12 mm arm, short blends into the collar. |
| Phone cradle | `exports/phone-cradle.stl` | 1 | 242.0 × 104.9 × 36.5 mm. Closed Fold 4 landscape pocket 155.9 × 67.5 × 16.2 mm. Rear entry, one M8 tap hole per arm on the arm centerline. USB-end hook only. |
| Thumbscrew | `exports/thumbscrew.stl` | 2 | 32.0 × 22.0 × 36.5 mm. Modeled M8×1.25 thumbscrew with an external thread, used in the assembly preview. |

Regenerate every STL from `headrest-phone-mount.scad` with `./export-stls.sh` after a parameter change. Do not edit the meshes. `exports/headrest-phone-mount.stl` is the assembly preview only.

## Metal hardware

| Item | Qty | Notes |
| --- | --- | --- |
| M8×30 thumbscrew, M8×1.25 | 2 | Optional metal match for the modeled screw. One per arm. From below, through that arm's bottom slot and longitudinal slot, into a tapped hole in the roof. Shank under the head is 30.5 mm on the model. Do not substitute a longer screw; the tip would break out of the 19.0 mm roof. |
| M5×12 socket set screw or button head | 2 | One radial pinch screw in the outboard wall of each collar. |
| M8×1.25 tap | 1 | Chase the two roof holes. They are modeled at 6.8 mm tap-drill. The modeled crest is Ø 8 mm. |

No nuts. The thread is in the receiver roof.

## Soft goods

| Item | Qty | Notes |
| --- | --- | --- |
| Soft foam or rubber pad | 0 | Not used. The pocket is cut for one closed Fold 4. A case does not fit. |
| Velcro strap or reusable cable tie | 1 | Through the cradle side slots, over the phone. |

## Not included

- A second phone cradle
- Phones, chargers, or vehicle power taps
- Adhesive on airbag covers, the headliner, or the headrest pad
- Crash certification, airbag testing, or an OEM fit claim
