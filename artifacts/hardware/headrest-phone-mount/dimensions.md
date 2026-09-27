# Default dimensions (mm)

License: GPL-2.0

| Parameter | Default (mm) | Meaning |
|-----------|--------------|---------|
| `post_spacing_min` | 110 | Minimum center-to-center distance between headrest posts |
| `post_spacing_max` | 170 | Maximum center-to-center distance (sliding clamp travel) |
| `post_diameter` | 14 | Nominal headrest post diameter |
| `phone_width_min` | 70 | Narrowest phone body width (landscape short side) |
| `phone_width_max` | 85 | Widest phone body width in cradle |
| `phone_thickness_max` | 12 | Max phone thickness including slim case |
| `clamp_depth` | 35 | How far clamp jaws engage along the post axis |
| `camera_clearance` | 18 | Cutout / standoff so lenses are not occluded |
| `dual_cradle` | true | If true, two side-by-side cradles; if false, single cradle |

Additional derived defaults in the `.scad` file: beam thickness, jaw wall, cable notch width.

## How to measure

1. **Post spacing:** Measure center-to-center of the two vertical headrest posts with calipers or a ruler. Set `post_spacing_min` slightly below and `post_spacing_max` slightly above your measurement so the sliding clamp can tighten.
2. **Post diameter:** Measure post OD; add 0.4 to 0.8 mm clearance in the model if your filament shrinks tightly (parameter is nominal; jaws use a small clearance factor).
3. **Phone width / thickness:** Measure the phone in the case you will use, in landscape orientation (width = shorter face edge of the body).
4. **Camera clearance:** Note lens bump height and horizontal offset; increase `camera_clearance` if the cradle lip covers the lens.
5. **Airbag / headrest:** Confirm the mount sits only on the posts and does not cover airbag stitch lines or block headrest height locks.

Print a small clamp-ring test coupon before a full mount if you are unsure of post fit.
