// RideAudit headrest phone mount — ART-RIDE-MOUNT-001
// Copyright (C) 2026 RideAudit contributors
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, see <https://www.gnu.org/licenses/>.
// SPDX-License-Identifier: GPL-2.0
//
// Source of truth. STL files under exports/ are generated; do not hand-edit them.
//
// Two post blocks, one shared vertical cradle.
// Each post passes through its block. The block is one round collar, 25.5 mm
// outside and 33 mm thick along the post. The bore is
// post_od plus 0.2–0.5 mm. The default is a 14 mm post, so the preview bore
// is 14.5 mm. Its rear is tangent to the headrest (Y = 0). Each collar carries
// a ~200 mm arm in a horizontal plane (constant Z). The arm shares the collar's
// bottom face. Short blends rise into the collar above the arm and stop at the
// collar top, so the bending moment enters the tube around the post.
// Both arms slide into the cradle from the rear (from the posts, toward +Y).
// Each arm has a longitudinal slot down its length, 1 mm wider than the M8
// crest on each side, so the cradle can slide forward or back before it is
// locked. Its own thumbscrew comes up from below, through a clearance slot in
// the cradle bottom, through that arm slot, and into an M8×1.25 tap hole in
// the receiver roof, on the arm centerline. Fully seated, the head face clamps
// the bottom plate and the arm.
// The closed Galaxy Z Fold 4 sits in landscape. The cover screen is flush on
// the vertical back plate. The primary rear cameras face forward, out of the
// cradle opening, toward the road. The portrait-top end (rear cluster) is -X.
// The portrait-bottom end (USB-C) is +X. A short hook at +X stops the phone
// sliding forward. That hook does not cross the camera end. No published
// figure gives the camera-bump height or the cluster's edge offsets, so the
// model does not cut a bump pocket or a sized lens window.
//
// Use frame (assembly):
//   X  across the headrest posts
//   Y  forward, out from the headrest face (Y = 0 is the pad)
//   Z  up, along the posts
//
// Print frames are applied by the orient modules.

/* [Headrest posts] */
post_spacing_min = 120;  // mm, narrow end of the usual 120–170 mm center range
post_spacing_max = 170;  // mm, wide end of that range
post_spacing     = 150;  // mm, a common center distance (also 130 and 160)
// Measured post diameter. Presets: 10, 12, 12.7, 13.8, 14. Any value in 10..14.
post_od = 14; // [10:0.1:14]
// Diametral clearance. Bore = post_od + post_clearance.
post_clearance = 0.5; // [0.2:0.05:0.5]
// One collar. Outside diameter stays put; only the bore follows the post.
block_od = 25.5;  // mm, outer diameter of that collar
block_t  = 33;    // mm, axial thickness of the collar along the post

/* [Arms and lock] */
arm_length   = 200;  // mm, post axis to arm tip, measured forward
slot_gap     = 14;   // mm, cradle-bottom clearance slot length along the arm
slot_radius  = 185;  // mm, post axis to the preview screw, along the arm

/* [Phone cradle] */
// Closed Galaxy Z Fold 4, landscape. Long side is the closed height.
// Short side is the closed width, vertical in the cradle.
// Thickness is the hinge (glass to glass), the thickest published body.
// Sources are cited in dimensions.md. No case.
phone_width_min     = 67.1;
phone_width_max     = 67.1;
phone_length_min    = 155.1;
phone_length_max    = 155.1;
phone_thickness_max = 15.8;

/* [Export] */
// assembly, block, tray, coupon, screw
part = "assembly";

/* [Hidden] */
plate_t = 6.0;
arm_width = 56;     // wide rails beside the tight slot, so the arm does not flex
arm_t = 12.0;       // vertical thickness; the arm lies flat
arm_z0 = 0;
rail_half_y = 9.0;  // receiver roof fore-aft half-depth
bottom_t = 6.0;     // cradle bottom, the head's bearing plate
cheek = 6.0;        // side ties from the bottom plate up to the roof, outside the arms
arm_gap_z = 0.20;   // slide clearance each side; seating the head takes this up
// USB-C end hook. It stands proud of the 15.8 mm hinge face and stops
// forward slip. It stays on the portrait-bottom end only.
usb_hook_len = 12;
usb_hook_gap = 0.35;
usb_hook_t = 2.4;
front_reach = phone_thickness_max + usb_hook_gap + usb_hook_t;
// Roof is just taller than the forward overhang so the wedge prints at 45 degrees.
rail_h = front_reach + 0.45;
// M8×1.25. Crest is the major diameter. The roof hole is the tap drill.
screw_pitch = 1.25;
screw_major = 8.0;
// 1 mm of air on each side of the crest. The screw stays on the arm centerline.
slot_span = screw_major + 2.0;
screw_crest_r = 4.0;
// Valley is held 0.5 mm inside the tap-drill wall. A valley on that wall
// leaves open edges where the helix meets the hole.
screw_root_r = 2.90;
hole_tap = 6.8;     // M8×1.25 tap drill in the roof
hole_clear = 9.0;   // bottom slot, major diameter plus clearance
screw_head_d = 22;  // flat bearing face around the clearance slot
screw_head_h = 6.0;
screw_wing_l = 32;
screw_wing_t = 4.2;
screw_wing_h = 3.6;
screw_gap = 0.12;   // mesh gap; the seated face is this far off the bottom plate
screw_engage = 12;  // thread inside the roof when the head is seated (1.5 × major)
pinch_d = 4.2;
fn_bore = 64;
$fn = 32;

/* ----------------------- derived ----------------------- */

post_od_min = 10;
post_od_max = 14;
bore_id = post_od + post_clearance;
bore_d = bore_id;
block_r = block_od / 2;
block_wall = (block_od - bore_id) / 2;
half = post_spacing / 2;
// Rear of the collar is tangent to the pad.
bore_cy = block_r;
block_depth = block_od;
// The arm embeds in the front half of the same collar, then runs forward.
arm_y0 = bore_cy;
arm_y1 = bore_cy + arm_length;
y_slot = bore_cy + slot_radius;
z_arm = arm_z0 + arm_t / 2;
arm_rise = 0; // the arm cube is horizontal; this is the acceptance flag
slot_side_gap = (slot_span - screw_major) / 2;
slot_y0 = block_od + 8;              // longitudinal slot starts clear of the collar
// Short blends into the collar above the arm. They stop at the collar top
// and taper onto the arm within about 10 mm, ahead of the bore and behind the slot.
gusset_h = block_t - arm_t;
gusset_y0 = bore_cy + bore_id / 2 + 1.2;
gusset_y1 = min(slot_y0 - 3, gusset_y0 + 10);
slot_y1 = arm_y1 - 8;                 // and stops short of the tip
slot_len = slot_y1 - slot_y0;

side_wall = 3.4;
// Closed Fold 4 only. Clearance is per end on the long side, at the top, and in front of the hinge.
side_clear = 0.40;
top_clear = 0.40;
thick_clear = 0.40;
fold_d_min = 14.2; // published thin edge. The hinge, phone_thickness_max, sizes the pocket.
pocket_x = phone_length_max + 2 * side_clear;
pocket_z = phone_width_max + top_clear;
pocket_y = phone_thickness_max + thick_clear;
front_lip_t = 0; // no full-width fence across the camera face
front_lip_h = 0;
outer_x = pocket_x + 2 * side_wall;

rail_z0 = arm_z0 + arm_t + arm_gap_z;
rail_z1 = rail_z0 + rail_h;
rail_y0 = y_slot - rail_half_y;
rail_y1 = y_slot + rail_half_y;
// Vertical phone plate is the forward skin of the receiver roof.
y_plate0 = rail_y1 - plate_t;
y_plate1 = rail_y1;
z_pocket0 = rail_z1;
z_pocket1 = z_pocket0 + pocket_z;
z_plate0 = rail_z0;
z_plate1 = z_pocket1;

// One tap hole per arm, on that arm's centerline. The slot is too tight to
// reach a neighboring hole, so post_spacing is set before export.
hole_nx = 2;
hole_nz = 1;
// Bottom plate under the arm. Cheeks outside the arm sweep tie it to the roof.
z_bot1 = arm_z0 - arm_gap_z;
z_bot0 = z_bot1 - bottom_t;
bot_slot_w = hole_clear;
rail_span = max(outer_x, post_spacing_max + arm_width + 2 * cheek + 4);
z_bear = z_bot0 - screw_gap;
// Seated shank: through the open stack and `screw_engage` into the roof.
screw_shank_l = (rail_z0 + screw_engage) - z_bear;
clamp_stack = bottom_t + arm_t;          // bottom plate + arm, gaps closed
clamp_takeup = 2 * arm_gap_z;            // slide clearance the seating load closes
asm_lift = -(z_bear - screw_head_h);

standout_y = y_plate1 + front_reach;
asm_top_z = z_pocket1 + asm_lift;
y_solid = slot_y0 + 20;

/* ----------------------- checks ----------------------- */

assert(post_spacing_max > post_spacing_min + 8, "post spacing range collapsed");
assert(post_spacing + 0.01 >= post_spacing_min && post_spacing - 0.01 <= post_spacing_max,
       "post_spacing preview is outside min..max");
assert(phone_width_max + 0.01 >= phone_width_min, "phone width range");
assert(phone_length_max + 0.01 >= phone_length_min, "phone length range");
assert(phone_length_min + 0.01 >= phone_width_max, "landscape long side must exceed the short side");
assert(phone_thickness_max >= 6 && phone_thickness_max <= 18, "phone thickness out of bracket range");
assert(abs(phone_length_max - 155.1) < 0.05, "length is not the Fold 4 closed height");
assert(abs(phone_width_max - 67.1) < 0.05, "short side is not the Fold 4 closed width");
assert(abs(phone_thickness_max - 15.8) < 0.05, "thickness is not the Fold 4 hinge");
assert(abs(phone_length_min - phone_length_max) < 0.05, "length range is not one closed Fold 4");
assert(abs(phone_width_min - phone_width_max) < 0.05, "width range is not one closed Fold 4");
assert(abs(fold_d_min - 14.2) < 0.05, "thin edge is not the published 14.2 mm");
assert(fold_d_min + 0.5 < phone_thickness_max, "thin edge is not thinner than the hinge");
assert(usb_hook_len >= 8 && usb_hook_len <= 16, "USB-end hook is not a short stop");
assert(phone_length_max - usb_hook_len >= 130, "USB-end hook reaches the camera end");
assert(front_reach > phone_thickness_max + 1.5, "hook does not stand proud of the hinge face");
assert(pocket_y + 0.01 >= phone_thickness_max, "pocket is shallower than the hinge");
assert(pocket_y + 0.05 < front_reach, "pocket depth swallows the USB hook");
assert(post_od + 0.001 >= post_od_min && post_od - 0.001 <= post_od_max, "post_od is outside 10..14 mm");
assert(post_clearance + 0.001 >= 0.2 && post_clearance - 0.001 <= 0.5, "clearance is outside 0.2..0.5 mm");
assert(abs(bore_id - (post_od + post_clearance)) < 0.01, "bore is not post_od plus clearance");
assert(abs(block_od - 25.5) < 0.01, "post block is not 25.5 mm outside");
assert(abs(block_t - 33) < 0.01, "post collar is not 33 mm thick");
assert(block_wall + 0.01 >= 5, "collar wall is under 5 mm");
assert(bore_cy - bore_d / 2 + 0.01 >= block_wall - 0.05, "bore breaks the rear wall");
assert(arm_length + 0.01 >= 190 && arm_length <= 260, "arm length is outside the 200 mm class");
assert(slot_y0 > block_depth + 4, "longitudinal slot cuts the post block");
assert(slot_y1 + 4 < arm_y1, "longitudinal slot runs off the arm tip");
assert(y_slot > slot_y0 + 30, "not enough rearward depth adjustment along the arm");
assert(y_slot + 4 < slot_y1, "preview screw is past the end of the arm slot");
assert((arm_width - slot_span) / 2 + 0.01 >= 10, "rails beside the longitudinal slot are too thin");
assert(arm_t + 0.01 >= 12, "arm is too thin to stay stiff");
assert(slot_side_gap <= 1.001, "arm slot gap is over 1 mm on a side");
assert(slot_side_gap + 0.001 >= 0.8, "arm slot does not clear the crest");
assert(slot_gap + 0.01 >= hole_clear, "bottom slot is shorter than the screw clearance");
assert(slot_gap + 2 < rail_y1 - rail_y0, "bottom slot does not fit on the cradle bottom");
assert(post_spacing_min > screw_head_d + bot_slot_w, "the two screw heads collide at minimum spacing");
assert(screw_engage + 0.01 >= screw_major * 1.25, "thread engagement is under 1.25 diameters");
assert(rail_h >= screw_engage + 3, "screw tip breaks out of the roof when seated");
assert(hole_tap + 0.01 < screw_major, "roof hole is not a thread; it clears the major diameter");
assert(hole_tap / 2 > screw_root_r + 0.35, "thread valley grazes the tap hole");
assert(bot_slot_w + 0.01 >= screw_major + 0.6, "bottom slot does not clear the major diameter");
assert(screw_head_d >= bot_slot_w + 8, "head bearing annulus is too narrow");
assert(slot_span + 0.01 >= screw_major + 1.6, "arm slot binds on the crest");
assert(y_slot > rail_y0 + 1 && y_slot < rail_y1 - 1, "slot is not under the receiver roof");
assert(rail_z0 >= arm_z0 + arm_t, "receiver roof cuts the arm");
assert(z_bot1 <= arm_z0 - 0.15, "cradle bottom cuts the arm");
assert(z_pocket0 >= rail_z1 - 0.01, "phone pocket overlaps the screw rail");
assert(plate_t + 1 < pocket_z, "back plate is not a vertical plate");
assert(arm_rise == 0, "arm is not horizontal");
assert(abs(arm_y1 - arm_y0) > 100, "arm does not reach forward");
assert(rail_h + 0.01 >= front_reach, "forward overhang is steeper than 45 degrees");
assert(rail_span / 2 + 0.01 >= post_spacing_max / 2 + arm_width / 2, "roof does not cover the wide-spacing arm");
assert(block_wall + 0.01 >= 5, "collar wall is thinner than 5 mm");
assert(gusset_y0 > bore_cy + bore_id / 2, "gusset covers the post bore");
assert(gusset_y1 + 1 < slot_y0, "gusset runs into the longitudinal slot");
assert(abs((arm_t + gusset_h) - block_t) < 0.01, "root blend rises past the collar");
assert(gusset_y1 - gusset_y0 >= 6 && gusset_y1 - gusset_y0 <= 12, "root blend is not a short run");
assert(gusset_h + 0.01 >= 12, "collar does not stand far enough above the arm");

echo(str("CHECK post_spacing_min=", post_spacing_min));
echo(str("CHECK post_spacing_max=", post_spacing_max));
echo(str("CHECK post_spacing=", post_spacing));
echo(str("CHECK post_od=", post_od));
echo(str("CHECK post_od_min=", post_od_min));
echo(str("CHECK post_od_max=", post_od_max));
echo(str("CHECK post_diameter=", post_od));
echo(str("CHECK post_clearance=", post_clearance));
echo(str("CHECK bore_d=", bore_d));
echo(str("CHECK bore_id=", bore_id));
echo(str("CHECK bore_cy=", bore_cy));
echo(str("CHECK block_depth=", block_depth));
echo(str("CHECK block_od=", block_od));
echo(str("CHECK block_t=", block_t));
echo(str("CHECK block_wall=", block_wall));
echo(str("CHECK arm_length=", arm_length));
echo(str("CHECK arm_width=", arm_width));
echo(str("CHECK arm_thick=", arm_t));
echo(str("CHECK arm_rise=", arm_rise));
echo(str("CHECK slot_span=", slot_span));
echo(str("CHECK slot_side_gap=", slot_side_gap));
echo(str("CHECK gusset_h=", gusset_h));
echo(str("CHECK gusset_y0=", gusset_y0));
echo(str("CHECK gusset_y1=", gusset_y1));
echo(str("CHECK slot_gap=", slot_gap));
echo(str("CHECK slot_radius=", slot_radius));
echo(str("CHECK slot_y0=", slot_y0));
echo(str("CHECK slot_y1=", slot_y1));
echo(str("CHECK slot_len=", slot_len));
echo(str("CHECK y_slot=", y_slot));
echo(str("CHECK z_bot0=", z_bot0));
echo(str("CHECK bottom_t=", bottom_t));
echo(str("CHECK screw_shank_l=", screw_shank_l));
echo(str("CHECK screw_head_d=", screw_head_d));
echo(str("CHECK screw_major=", screw_major));
echo(str("CHECK screw_pitch=", screw_pitch));
echo(str("CHECK screw_engage=", screw_engage));
echo(str("CHECK screw_crest_r=", screw_crest_r));
echo(str("CHECK screw_root_r=", screw_root_r));
echo(str("CHECK hole_tap=", hole_tap));
echo(str("CHECK hole_clear=", hole_clear));
echo(str("CHECK clamp_stack=", clamp_stack));
echo(str("CHECK clamp_takeup=", clamp_takeup));
echo(str("CHECK z_arm=", z_arm));
echo(str("CHECK y_solid=", y_solid));
echo(str("CHECK hole_nx=", hole_nx));
echo(str("CHECK hole_nz=", hole_nz));
echo(str("CHECK m5_tap=", hole_tap));
echo(str("CHECK plate_t=", plate_t));
echo(str("CHECK rail_z0=", rail_z0));
echo(str("CHECK rail_z1=", rail_z1));
echo(str("CHECK rail_h=", rail_h));
echo(str("CHECK rail_span=", rail_span));
echo(str("CHECK rail_y0=", rail_y0));
echo(str("CHECK block_wall_echo=", block_wall));
echo(str("CHECK y_plate0=", y_plate0));
echo(str("CHECK y_plate1=", y_plate1));
echo(str("CHECK half=", half));
echo(str("CHECK pocket_x=", pocket_x));
echo(str("CHECK pocket_y=", pocket_y));
echo(str("CHECK pocket_z=", pocket_z));
echo(str("CHECK z_pocket0=", z_pocket0));
echo(str("CHECK z_pocket1=", z_pocket1));
echo(str("CHECK outer_x=", outer_x));
echo(str("CHECK phone_width_min=", phone_width_min));
echo(str("CHECK phone_width_max=", phone_width_max));
echo(str("CHECK phone_length_min=", phone_length_min));
echo(str("CHECK phone_length_max=", phone_length_max));
echo(str("CHECK phone_thickness_max=", phone_thickness_max));
echo(str("CHECK fold_d_min=", fold_d_min));
echo(str("CHECK side_clear=", side_clear));
echo(str("CHECK top_clear=", top_clear));
echo(str("CHECK thick_clear=", thick_clear));
echo(str("CHECK front_reach=", front_reach));
echo(str("CHECK usb_hook_len=", usb_hook_len));
echo(str("CHECK usb_hook_gap=", usb_hook_gap));
echo(str("CHECK usb_hook_t=", usb_hook_t));
echo(str("CHECK standout_y=", standout_y));
echo(str("CHECK asm_top_z=", asm_top_z));
echo(str("CHECK cradle_count=", 1));
echo(str("CHECK screw_count=", 2));
echo(str("CHECK screw_x_left=", -half));
echo(str("CHECK screw_x_right=", half));
echo(str("CHECK front_lip_h=", front_lip_h));
echo(str("CHECK front_lip_t=", front_lip_t));

/* ----------------------- parts ----------------------- */

module tap_hole(length) {
    cylinder(h = length, d = hole_tap, $fn = 36);
}

module post_block_use(side) {
    post_x = side * half;
    difference() {
        union() {
            // One collar: bore follows the post, 25.5 mm outside, 33 mm thick.
            translate([post_x, bore_cy, 0])
                cylinder(h = block_t, d = block_od, $fn = fn_bore);
            // Horizontal arm, joined into the front half of that same collar.
            translate([post_x - arm_width / 2, arm_y0, arm_z0])
                cube([arm_width, arm_y1 - arm_y0, arm_t]);
            arm_gussets(post_x);
        }
        translate([post_x, bore_cy, -1])
            cylinder(h = block_t + 2, d = bore_id, $fn = fn_bore);
        translate([post_x, bore_cy, -0.01])
            cylinder(h = 1.2, d1 = bore_id + 1.6, d2 = bore_id, $fn = fn_bore);
        // Longitudinal slot down the arm. The cradle slides along it to set depth.
        translate([post_x - slot_span / 2, slot_y0, arm_z0 - 1])
            cube([slot_span, slot_len, arm_t + 2]);
        // Radial pinch through the collar wall, behind the arm, from the outboard side.
        translate([post_x, bore_cy - 4, block_t / 2])
            rotate([0, 90 * side, 0])
                translate([0, 0, bore_id / 2 - 1.5])
                    cylinder(h = block_wall + 4, d = pinch_d, $fn = 24);
    }
}

module arm_gussets(post_x) {
    // The arm is the lower 12 mm of the collar. These blends stay inside the
    // collar height and run only a short way forward. The center web lands on
    // the front wall of the tube, ahead of the bore. The side blends carry the
    // outer arm fibers into that same tube. Nothing stands above the collar.
    hull() {
        translate([post_x - 7.5, gusset_y0, 0])
            cube([15, 1.4, block_t]);
        translate([post_x - 6.0, gusset_y1, 0])
            cube([12, 1.2, arm_t]);
    }
    for (sx = [-1, 1]) {
        hull() {
            translate([
                post_x + sx * 9.2 - (sx < 0 ? 2.8 : 0),
                bore_cy - 3.75,
                0
            ])
                cube([2.8, 3.2, block_t]);
            translate([
                post_x + sx * (arm_width / 2 - 2.4) - (sx < 0 ? 2.4 : 0),
                arm_y0,
                0
            ])
                cube([2.4, 7.0, arm_t]);
        }
    }
}

module roof_holes() {
    for (sx = [-1, 1])
        translate([sx * half, y_slot, rail_z0 - 0.4])
            tap_hole(rail_h + 0.6);
}

module bottom_slots() {
    // One clearance slot under each arm's tap hole.
    for (sx = [-1, 1])
        translate([sx * half - bot_slot_w / 2, y_slot - slot_gap / 2, z_bot0 - 0.6])
            cube([bot_slot_w, slot_gap, bottom_t + 1.2]);
}

module tray_use() {
    difference() {
        union() {
            // Receiver roof. Open toward the posts (low Y). Arms slide in from the rear.
            translate([-rail_span / 2, rail_y0, rail_z0])
                cube([rail_span, rail_y1 - rail_y0, rail_h]);
            // Bottom plate. Thumbscrews come up through the slots in this plate.
            translate([-rail_span / 2, rail_y0, z_bot0])
                cube([rail_span, rail_y1 - rail_y0, bottom_t]);
            // Cheeks outside the arm sweep tie the bottom plate to the roof.
            for (sx = [-1, 1])
                translate([sx * rail_span / 2 - (sx > 0 ? cheek : 0), rail_y0, z_bot0])
                    cube([cheek, rail_y1 - rail_y0, rail_z0 - z_bot0 + 0.2]);
            // Ribs behind the upper plate so that plate reaches the rear print plane.
            for (x = [-outer_x / 2 + 1.4 : 16 : outer_x / 2 - 4])
                translate([x, rail_y0, rail_z1 - 0.2])
                    cube([2.8, y_plate0 - rail_y0, z_pocket1 - rail_z1 + 0.2]);
            // Vertical back plate. The phone sits flush on the forward face (y_plate1).
            translate([-outer_x / 2, y_plate0, rail_z0])
                cube([outer_x, plate_t, z_plate1 - rail_z0]);
            // End walls. They flank the phone. They do not wrap onto the camera face.
            // The wedge down to the roof prints at 45 degrees.
            for (sx = [-1, 1]) {
                x0 = sx * outer_x / 2 - (sx > 0 ? side_wall : 0);
                hull() {
                    translate([x0, y_plate1, rail_z0])
                        cube([side_wall, 0.02, z_pocket1 - rail_z0]);
                    translate([x0, y_plate1 + front_reach - 0.02, z_pocket0])
                        cube([side_wall, 0.02, pocket_z]);
                }
            }
            // Floor under the full body. The top of this wedge is the pocket floor.
            // It does not rise in front of the lenses.
            hull() {
                translate([-pocket_x / 2, y_plate1, rail_z0])
                    cube([pocket_x, 0.02, rail_h]);
                translate([-pocket_x / 2, y_plate1 + front_reach - 0.02, z_pocket0])
                    cube([pocket_x, 0.02, 0.02]);
            }
            // USB-C end only (+X, portrait bottom). Stops forward slip.
            // The camera end (-X, portrait top) stays open in front of the glass.
            translate([
                pocket_x / 2 - usb_hook_len,
                y_plate1 + phone_thickness_max + usb_hook_gap,
                z_pocket0
            ])
                cube([usb_hook_len, usb_hook_t, pocket_z]);
        }
        roof_holes();
        bottom_slots();
        strap_slots();
    }
}

module strap_slots() {
    slot_z = z_pocket1 - 16;
    for (sx = [-1, 1])
        translate([sx * outer_x / 2 - side_wall - 0.8, y_plate1 + 1.2, slot_z])
            cube([side_wall + 1.6, max(4, pocket_y - 2.4), 3.2]);
}

module coupon_use() {
    // Same collar as the post block, without the arm. Bore gauge only.
    difference() {
        translate([0, bore_cy, 0])
            cylinder(h = block_t, d = block_od, $fn = fn_bore);
        translate([0, bore_cy, -1])
            cylinder(h = block_t + 2, d = bore_id, $fn = fn_bore);
        translate([0, bore_cy, -0.01])
            cylinder(h = 1.2, d1 = bore_id + 1.6, d2 = bore_id, $fn = fn_bore);
    }
}

module external_thread(length) {
    // Single-start external helix. The valley cylinder stays inside the tap drill.
    // The tooth overlaps that cylinder and crosses the hole wall, so the crest bites
    // the roof without a coplanar valley/hole surface.
    turns = length / screw_pitch;
    valley = screw_root_r;
    buried = valley - 0.25;
    union() {
        cylinder(h = length, r = valley, $fn = 48);
        linear_extrude(height = length, twist = 360 * turns, slices = ceil(turns * 8), convexity = 8)
            polygon([
                [buried * cos(-20), buried * sin(-20)],
                [screw_crest_r * cos(-8), screw_crest_r * sin(-8)],
                [screw_crest_r * cos(8), screw_crest_r * sin(8)],
                [buried * cos(20), buried * sin(20)]
            ]);
    }
}

module thumbscrew_solid() {
    // Bearing face at z = 0. Head and wings occupy z < 0.
    // Threaded shank goes up through the bottom slot, the arm slot, and into the roof.
    // Seated, the face clamps the bottom plate and the arm; `screw_engage` of thread
    // remains in the roof.
    translate([0, 0, -screw_head_h])
        cylinder(h = screw_head_h, d = screw_head_d, $fn = 48);
    translate([-screw_wing_l / 2, -screw_wing_t / 2, -screw_head_h])
        cube([screw_wing_l, screw_wing_t, screw_wing_h]);
    translate([0, 0, -0.45])
        external_thread(screw_shank_l + 0.45);
}

module thumbscrew(side) {
    translate([side * half, y_slot, z_bear])
        thumbscrew_solid();
}

module assembly_body() {
    post_block_use(-1);
    post_block_use(1);
    tray_use();
    thumbscrew(-1);
    thumbscrew(1);
}

module assembly_use() {
    translate([0, 0, asm_lift])
        assembly_body();
}

/* ----------------------- print orientation ----------------------- */

module orient_block(side) {
    post_x = side * half;
    // Arm already lies on Z = 0 with the bore vertical. Shift into +X.
    translate([-(post_x - arm_width / 2), 0, 0])
        post_block_use(side);
}

module orient_tray() {
    // Rear opening on the bed. Bottom plate and roof print as vertical walls.
    // The phone plate stands on ribs that reach that same rear plane.
    translate([rail_span / 2, z_pocket1, -rail_y0])
        rotate([90, 0, 0])
            tray_use();
}

module orient_screw() {
    translate([screw_wing_l / 2, screw_head_d / 2, screw_head_h])
        thumbscrew_solid();
}

module orient_coupon() {
    translate([block_r, 0, 0])
        coupon_use();
}

/* ----------------------- export switch ----------------------- */

if (part == "assembly")
    assembly_use();
else if (part == "block")
    orient_block(-1);
else if (part == "tray")
    orient_tray();
else if (part == "coupon")
    orient_coupon();
else if (part == "screw")
    orient_screw();
else if (part == "raw_screw")
    thumbscrew_solid();
else if (part == "raw_block")
    post_block_use(-1);
else if (part == "raw_block_right")
    post_block_use(1);
else if (part == "raw_tray")
    tray_use();
else if (part == "raw_coupon")
    coupon_use();
else if (part == "fitcheck")
    intersection() {
        tray_use();
        union() {
            post_block_use(-1);
            post_block_use(1);
        }
    }
else if (part == "fitcheck_arms")
    intersection() {
        post_block_use(-1);
        post_block_use(1);
    }
else
    assert(false, str("unknown part: ", part));
