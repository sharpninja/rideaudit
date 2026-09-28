# Dimensions

Copyright (C) 2026 RideAudit contributors  
License: GPL-2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).  
Not Apache-2.0. Not MIT.

All figures are millimetres. Defaults are the customizer values in `headrest-phone-mount.scad`. Derived sizes are what `verify-geometry.py` measures for those defaults; they change when you edit the parameters and re-export.

The mount is two identical post blocks and one cradle. Each post goes through one round collar. Both 200 mm arms lie in a horizontal plane, join that collar through gussets, and enter one receiver from the rear. A longitudinal slot in each arm lets the cradle slide to set depth. The slot is 1 mm clear of the M8 crest on each side. There is no beam and no sideways hole row.

## Parameters

| Parameter | Default | Meaning |
| --- | --- | --- |
| `post_spacing_min` | 120 | Narrow end of the usual 120–170 mm center range |
| `post_spacing_max` | 170 | Wide end of that range |
| `post_spacing` | 150 | Preview center distance. 130 and 160 mm are also common. Re-export for the measured spacing |
| `post_od` | 14 | Measured post diameter, 10–14 mm. Presets: 10, 12, 12.7, 13.8, 14 |
| `post_clearance` | 0.5 | Diametral clearance, 0.2–0.5 mm |
| `bore_id` | 14.5 | Derived: `post_od + post_clearance`. Default is 14 + 0.5 |
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

1. **Post spacing.** Center-to-center of the two vertical posts. The CAD range is 120–170 mm. Published centers are often 130, 150, or 160 mm. Set `post_spacing` and re-export. The cradle is cut for that spacing. Cradle depth is the slide along the longitudinal slot.
2. **Post diameter.** Measure the post and set `post_od` (10–14 mm) and `post_clearance` (0.2–0.5 mm). The bore is `post_od + post_clearance` on both the block and the fit coupon. The default export is a 14 mm post with 0.5 mm clearance, so the preview bore is 14.5 mm. Reprint the coupon when either parameter changes. Many 2013 and later generations are absent from the charts below; measure those posts and set `post_od`.
3. **Phone, landscape.** Long edge → `phone_length_*`. Short edge, the vertical one → `phone_width_*`. Thickness including the case → `phone_thickness_max`.
4. **Camera.** The windows are `camera_clearance` squares in both upper corners and pass through the back plate.
5. **Airbag and headrest.** The fixture bears on the posts and on the headrest face at the rear of each collar. Keep the 222.2 mm forward reach and the 114.0 mm height off airbag covers, the driver, the headliner, and the headrest height lock.

## Fit coupon

`exports/fit-coupon.stl` is the collar without the arm. The default file is a 14.5 / 25.5 × 10 mm collar (14 mm post, 0.5 mm clearance). Reprint it after any change to `post_od` or `post_clearance`. Slide it onto the post:

- On a post that matches `post_od` it should start by hand and then hold.
- The pinch screw is what locks rotation on the real block.
- If it will not start, the post is larger than `post_od`, or the clearance is tighter than the post allows. The design maximum is 14 mm. A larger post does not enter this collar.
- If it rattles, set `post_od` to the measured diameter (clearance stays inside 0.2–0.5 mm), reprint the coupon and the blocks, and tighten the radial pinch screw.

Print the arms only after the coupon fits.

## Headrest post diameters

The documented common range across Chevrolet, Ford, Hyundai, Honda, Toyota, and Nissan compact SUVs is about 10–14 mm. The sizes that appear most often in Rosen fitment for about 2005–2012 are 10, 12, 12.7, and 14 mm. The design maximum is 14 mm. The collar stays 25.5 mm outside and 10 mm thick; only the bore follows the post.

`post_od` is a customizer spinbox from 10 to 14 mm in 0.1 mm steps, so a measured size that is not one of the presets still works. Presets: **10, 12, 12.7, 13.8, 14**. `post_clearance` runs from 0.2 to 0.5 mm. Bore = `post_od + post_clearance`. The shipped preview uses the maximum: 14 mm post, 0.5 mm clearance, 14.5 mm bore. A smaller preset changes the bore only. The arm and the gussets stay on the 25.5 mm collar.

Center spacing in the same fitment notes is often 120–170 mm, commonly 130, 150, or 160 mm. The CAD range matches that span. The preview is 150 mm.

### Model examples

Rosen AV7500 headrest-availability sheet, dated 12.01.11.

| Vehicle | Years | Post OD |
| --- | --- | --- |
| Chevy Equinox | 2006–2008 | 10 mm |
| Chevy Equinox | 2010–2012 | 14 mm |
| Chevy Trailblazer | 2005–2009 | 10 mm |
| Ford Escape | 2008–2009 | 10 mm |
| Ford Escape Hybrid | 2009–2011 | 12.7 mm |
| Ford Escape | 2011–2012 | 12.7 mm |
| Hyundai Santa Fe | 2010–2012 | 10 mm |
| Hyundai Tucson | 2011–2012 | 12.7 mm |
| Honda CR-V | 2007–2011 | 12.7 mm |
| Honda Pilot | 2006–2008 | 10 mm |
| Honda Pilot | 2009–2012 | 12.7 mm |
| Toyota RAV4 | 2006–2012 | 12 mm |
| Toyota, some platforms | — | 14 mm |
| Nissan Rogue | 2008–2012 | 12.7 mm |

### Make-level chart

Philips JENHR1D vehicle preparation lists factory posts by make. Use it beside the Rosen model rows. Where the two sources overlap they agree on 10, 12, 12.7, and 14 mm.

| Make | Factory post diameters | Source |
| --- | --- | --- |
| Chevrolet / GM | 10, 11, 12, and 14 mm | Philips JENHR1D vehicle-preparation chart |
| Ford / Lincoln | 10, 12, and 12.7 mm | same chart |
| Honda / Acura | 10 and 12.7 mm | same chart |
| Hyundai | Santa Fe 2010–2012 is 10 mm; Tucson 2011–2012 is 12.7 mm | Rosen AV7500, 12.01.11 (Hyundai is absent from the Philips make chart) |
| Toyota / Lexus | 12 and 13.88 mm | Philips JENHR1D vehicle-preparation chart |
| Nissan / Infiniti | 12.7 mm | same chart |

The Philips adapter kit also stocks tubes at 12, 12.5, 12.7, 13.8, 14, and 16 mm. Preset 13.8 mm is that adapter tube. The Toyota/Lexus line on the same chart is 13.88 mm. Those are two published figures; measure the post before choosing one. A 16 mm tube is larger than the 14 mm design maximum and does not enter a bore of at most 14.5 mm.

Sources: [Rosen AV7500 headrest availability with post dimensions, 12.01.11](https://pdf.ampire.de/rosen_av7500_headrest.pdf). [Philips JENHR1D vehicle preparation](https://manualsdump.com/en/manuals/philips-jenhr1d/212963/7).
