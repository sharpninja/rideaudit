# Closed Galaxy Z Fold 4 cradle (landscape, cameras forward)

Date: 2026-09-29T15:18:00Z. Host: PAYTON-LEGION2. Workspace: `F:\GitHub\rideaudit`. Artifact: ART-RIDE-MOUNT-001, version 1.1.0. SPDX: GPL-2.0-only.

This is a CAD change. It is not an on-vehicle print. HW1 stays open. The vehicle checklist is not filled in.

## Constraint

Samsung Galaxy Z Fold 4, model family SM-F936, closed, landscape, no case. Primary rear cameras face forward, out of the cradle opening, toward the road. The cover screen sits on the back plate. Portrait top (rear cluster) is the open end. Portrait bottom (USB-C) sits against the hook.

## Published body

Closed size is height 155.1 mm, width 67.1 mm, thickness 14.2 mm at the thin edge to 15.8 mm at the hinge. Landscape uses 155.1 mm as the long side and 67.1 mm as the vertical side. The pocket is 155.9 x 67.5 x 16.2 mm (0.40 mm at each end, 0.40 mm at the top, 0.40 mm in front of the hinge).

Samsung measures folded thickness from the cover display to the rear glass. 15.8 mm is the hinge. It is not the camera bump.

Sources:

- Samsung Australia, SM-F936B, folded HxWxD 155.1 x 67.1 x 15.8-14.2 mm: https://www.samsung.com/au/smartphones/galaxy-z/galaxy-z-fold4-phantom-black-256gb-sm-f936bzkaats/
- Samsung UK support, folded 155.1 x 67.1 x 14.2 (minimum) to 15.8: https://www.samsung.com/uk/support/mobile-devices/what-is-the-difference-between-galaxy-z-fold3-and-galaxy-z-fold4/
- Samsung Newsroom UK, folded thickness is cover display to rear glass, maximum is at the hinge: https://news.samsung.com/uk/from-17-1-mm-to-8-9-mm-the-galaxy-z-folds-journey-to-becoming-48-thinner
- Samsung Newsroom US, Fold4 folded thickness 14.2-15.8 mm: https://news.samsung.com/us/evolution-of-samsung-galaxy-z-fold-series-thinner-sturdier-compact-as-ever
- PhoneArena review, 15.8 mm hinge and 14.2 mm sag, rear module a vertical pill: https://www.phonearena.com/reviews/samsung-galaxy-z-fold-4-review_id5482
- GSMArena, folded 155.1 x 67.1 x 14.2-15.8 mm: https://www.gsmarena.com/samsung_galaxy_z_fold4-11737.php

## Not verified, so not modeled

No source above states the lens protrusion in millimetres or the cluster's offsets from the edges. Those numbers are not in the CAD. The back plate is solid. The forward face is open except a 12 mm hook, 2.4 mm thick, 0.35 mm past the hinge face, on the USB-C end only. That hook is 12 mm of a 155.1 mm body, at the opposite end from the rear cluster. Cradle front is 225.3 mm from the pad. Hinge face is 222.6 mm from the pad. Lens stand-out past that face is excluded. The operator measures it before calling the reach clear.

## CAD result

`verify-geometry.py` exited 0. OpenSCAD assertions passed. Probes: back plate solid, camera-end forward face open, center forward face open, USB-end hook solid, face beside the hook open. Cradle print mesh 242.0 x 104.9 x 36.5 mm. Roof 19.00 mm so the forward overhang (18.55 mm) prints at 45 degrees or shallower. Thumbscrews unchanged: M8x1.25, 12 mm in the roof, shank 30.5 mm. Post blocks unchanged.

STL and PNG files were regenerated from `headrest-phone-mount.scad`.
