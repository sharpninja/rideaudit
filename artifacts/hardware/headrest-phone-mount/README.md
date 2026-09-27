# Headrest phone mount (3D)

**Artifact ID:** ART-RIDE-MOUNT-001  
**Kind:** mechanical-3d  
**Format:** OpenSCAD  
**License:** GPL-2.0

## Purpose

Parametric dual-phone headrest mount for RideAudit rideshare audit capture. Clamps to two vertical headrest posts and holds one or two phones in landscape for forward/cabin or dual-forward layouts (configurable). Intended for use with the Android dual-phone client (Bluetooth driver-rider pairing).

## Design goals

- Clamps to two vertical headrest posts (adjustable spacing).
- Holds one or two phones in landscape (`dual_cradle`).
- Does not intentionally block airbags; keep clear of airbag cover zones.
- Tool-less friction/clamp adjust where practical.
- Cable routing notch for charging leads.
- Parametric dimensions for post diameter, phone size, and clearance.

## Print settings (starting point)

| Setting | Recommendation |
|---------|----------------|
| Material | **PETG** or **ABS** (preferred over PLA for cabin heat) |
| Layer height | 0.2 mm |
| Perimeters | 4+ |
| Infill | 40%+ gyroid or cubic |
| Supports | As needed for cradle overhangs |
| Orientation | Post clamps flat on bed when possible |

Measure your vehicle and phones; edit parameters in `headrest-phone-mount.scad` before printing. See [dimensions.md](dimensions.md).

## Bill of materials

See [bom.md](bom.md): printed parts plus optional foam pad and M3 hardware.

## Safety disclaimer

**This mount is not crash-tested and is not a certified automotive restraint or OEM accessory.** It is a DIY audit fixture. Do not place it where it can interfere with airbags, head restraints as designed by the vehicle maker, or safe driving. Users assume all risk. Adjust dimensions for your vehicle; if the fit is insecure, do not use it on public roads.

## Exporting STL

If OpenSCAD CLI is available:

```text
openscad -o exports/headrest-phone-mount.stl headrest-phone-mount.scad
```

On Windows (typical install):

```text
"C:\Program Files\OpenSCAD\openscad.com" -o exports/headrest-phone-mount.stl headrest-phone-mount.scad
```

The `.scad` source is always the source of truth under GPL-2.0 even when an STL export is present.

## License (GPL-2.0)

```
RideAudit headrest phone mount
Copyright (C) 2026 RideAudit contributors

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.
```
