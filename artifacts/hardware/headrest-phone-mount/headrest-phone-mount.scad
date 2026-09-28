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
// Each post passes through its block. The block heel is flush on the headrest
// (Y = 0). Each block carries a ~200 mm arm in a horizontal plane (constant Z).
// Both arms slide into the cradle from the rear (from the posts, toward +Y).
// Each arm has a longitudinal slot down its length so the cradle can slide
// forward or back before it is locked. Its own thumbscrew comes up from below,
// through a slot in the cradle bottom, through that arm slot, and into a
// tapped hole in the receiver roof.
// The phone sits flush on the vertical back plate.
//
// Use frame (assembly):
//   X  across the headrest posts
//   Y  forward, out from the headrest face (Y = 0 is the pad)
//   Z  up, along the posts
//
// Print frames are applied by the orient modules.

/* [Headrest posts] */
post_spacing_min = 110;  // mm, narrowest centers whose arms still find a hole
post_spacing_max = 170;  // mm, widest centers whose arms still find a hole
post_diameter    = 14;   // mm, nominal post OD
post_spacing     = 140;  // mm, block centers in the assembly preview

/* [Arms and lock] */
arm_length   = 200;  // mm, post axis to arm tip, measured forward
slot_span    = 16;   // mm, longitudinal slot width across the arm
slot_gap     = 12;   // mm, cradle-bottom slot length along the arm, at each screw
slot_radius  = 185;  // mm, post axis to the preview screw, along the arm
hole_pitch   = 10;   // mm, threaded holes across the receiver roof
hole_x_max   = 90;   // mm, half-width of the hole row

/* [Phone cradle] */
phone_width_min     = 70;   // mm, landscape short side (vertical on the plate)
phone_width_max     = 85;
phone_length_min    = 140;  // mm, landscape long side
phone_length_max    = 172;
phone_thickness_max = 12;   // mm, including a slim case
camera_clearance    = 18;   // mm, square window, both upper corners of the back plate

/* [Structure] */
jaw_wall   = 4;    // mm, material outside the post bore
front_wall = 12;   // mm, block material in front of the bore, where the arm roots
clearance  = 0.4;  // mm, diametral clearance in the bore
cradle_lip = 3;    // mm, front lip thickness

/* [Export] */
// assembly, block, tray, coupon, screw
part = "assembly";

/* [Hidden] */
plate_t = 6.0;
arm_width = 28;     // horizontal width; leaves a rail on each side of the long slot
arm_t = 8.0;        // vertical thickness; the arm lies flat
arm_z0 = 0;
block_w = 36;
block_h = 44;
heel = 4.0;
rail_half_y = 9.0;  // receiver roof fore-aft half-depth
rail_h = 16.0;      // solid above the arm; also the 45° print wedge height
bottom_t = 4.0;     // cradle bottom under the arms
cheek = 6.0;        // side ties from the bottom plate up to the roof, outside the arms
arm_gap_z = 0.45;   // air between the arm and the roof, and between the arm and the bottom
m5_tap = 4.2;
m5_clear = 5.4;
screw_head_d = 16;
screw_head_h = 5.0;
screw_wing_l = 26;
screw_wing_t = 3.4;
screw_wing_h = 3.2;
screw_gap = 0.25;   // air between the thumbscrew head and the cradle bottom
screw_engage = 8;   // shank depth into the roof threads
pinch_d = 4.2;
fn_bore = 64;
$fn = 32;

/* ----------------------- derived ----------------------- */

bore_d = post_diameter + clearance;
half = post_spacing / 2;
bore_cy = heel + bore_d / 2;
block_depth = bore_cy + bore_d / 2 + front_wall;
r_arm0 = bore_d / 2 + 1.2;
arm_y0 = bore_cy + r_arm0 - bore_d / 2; // overlaps the front wall of the bore
arm_y1 = bore_cy + arm_length;
y_slot = bore_cy + slot_radius;
z_arm = arm_z0 + arm_t / 2;
arm_rise = 0; // the arm cube is horizontal; this is the acceptance flag
slot_y0 = block_depth + 16;          // longitudinal slot starts clear of the block
slot_y1 = arm_y1 - 8;                 // and stops short of the tip
slot_len = slot_y1 - slot_y0;

side_wall = 3.4;
pocket_x = phone_length_max + 1.6;
pocket_z = phone_width_max + 0.8;
pocket_y = phone_thickness_max + 0.5;
front_lip_t = cradle_lip;
front_lip_h = cradle_lip + 5;
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

hole_nx = floor(2 * hole_x_max / hole_pitch) + 1;
hole_nz = 1;
// Bottom plate under the arm. Cheeks outside the arm sweep tie it to the roof.
z_bot1 = arm_z0 - arm_gap_z;
z_bot0 = z_bot1 - bottom_t;
bot_slot_w = m5_clear + 1.0;
rail_span = max(outer_x, post_spacing_max + arm_width + 2 * cheek + 4);
z_bear = z_bot0 - screw_gap;
screw_shank_l = (rail_z0 + screw_engage) - z_bear;
asm_lift = -(z_bear - screw_head_h);

standout_y = y_plate1 + pocket_y + front_lip_t;
asm_top_z = z_pocket1 + asm_lift;
win_x = pocket_x / 2 - camera_clearance / 2;
win_z = z_pocket1 - camera_clearance / 2;
y_solid = slot_y0 + 20;

/* ----------------------- checks ----------------------- */

assert(post_spacing_max > post_spacing_min + 8, "post spacing range collapsed");
assert(post_spacing + 0.01 >= post_spacing_min && post_spacing - 0.01 <= post_spacing_max,
       "post_spacing preview is outside min..max");
assert(phone_width_max + 0.01 >= phone_width_min, "phone width range");
assert(phone_length_max + 0.01 >= phone_length_min, "phone length range");
assert(phone_length_min + 0.01 >= phone_width_max, "landscape long side must exceed the short side");
assert(phone_thickness_max >= 6 && phone_thickness_max <= 18, "phone thickness out of bracket range");
assert(camera_clearance >= 12, "camera_clearance too small to uncover a lens");
assert(2 * camera_clearance + 12 <= pocket_x, "camera windows do not fit across the back plate");
assert(camera_clearance + 6 <= pocket_z, "camera window does not fit the short side");
assert(bore_d > post_diameter, "bore does not clear the post");
assert(heel + 0.01 >= jaw_wall, "heel behind the bore is thinner than jaw_wall");
assert((block_w - bore_d) / 2 + 0.01 >= jaw_wall, "block side wall is thinner than jaw_wall");
assert(bore_cy - bore_d / 2 + 0.01 >= heel - 0.01, "bore breaks the flush heel");
assert(arm_length + 0.01 >= 190 && arm_length <= 260, "arm length is outside the 200 mm class");
assert(slot_y0 > block_depth + 4, "longitudinal slot cuts the post block");
assert(slot_y1 + 4 < arm_y1, "longitudinal slot runs off the arm tip");
assert(y_slot > slot_y0 + 30, "not enough rearward depth adjustment along the arm");
assert(y_slot + 4 < slot_y1, "preview screw is past the end of the arm slot");
assert(slot_span + 8 <= arm_width, "rails beside the longitudinal slot are too thin");
assert(slot_gap >= m5_clear + 1, "bottom slot will not pass an M5 shank");
assert(slot_gap + 2 < rail_y1 - rail_y0, "bottom slot does not fit on the cradle bottom");
assert(hole_x_max + 0.01 >= post_spacing_max / 2, "hole row does not cover post_spacing_max");
assert(hole_x_max + 0.01 >= post_spacing_min / 2, "hole row does not cover post_spacing_min");
assert(hole_pitch >= 8 && hole_pitch <= 14, "hole pitch is outside the discrete-lock range");
assert(slot_span / 2 + 0.01 >= hole_pitch / 2, "slot is narrower than the hole pitch");
assert(y_slot > rail_y0 + 1 && y_slot < rail_y1 - 1, "slot is not under the receiver roof");
assert(rail_z0 >= arm_z0 + arm_t, "receiver roof cuts the arm");
assert(z_bot1 <= arm_z0 - 0.3, "cradle bottom cuts the arm");
assert(z_pocket0 >= rail_z1 - 0.01, "phone pocket overlaps the screw rail");
assert(plate_t + 1 < pocket_z, "back plate is not a vertical plate");
assert(arm_rise == 0, "arm is not horizontal");
assert(abs(arm_y1 - arm_y0) > 100, "arm does not reach forward");
assert(rail_h + 0.01 >= pocket_y + front_lip_t, "pocket overhang is steeper than 45 degrees");
assert(rail_span / 2 + 0.01 >= post_spacing_max / 2 + arm_width / 2, "roof does not cover the wide-spacing arm");
assert(front_wall >= 8, "arm root in front of the bore is too short");

echo(str("CHECK post_spacing_min=", post_spacing_min));
echo(str("CHECK post_spacing_max=", post_spacing_max));
echo(str("CHECK post_spacing=", post_spacing));
echo(str("CHECK post_diameter=", post_diameter));
echo(str("CHECK bore_d=", bore_d));
echo(str("CHECK bore_cy=", bore_cy));
echo(str("CHECK block_depth=", block_depth));
echo(str("CHECK block_w=", block_w));
echo(str("CHECK block_h=", block_h));
echo(str("CHECK heel=", heel));
echo(str("CHECK arm_length=", arm_length));
echo(str("CHECK arm_width=", arm_width));
echo(str("CHECK arm_thick=", arm_t));
echo(str("CHECK arm_rise=", arm_rise));
echo(str("CHECK slot_span=", slot_span));
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
echo(str("CHECK z_arm=", z_arm));
echo(str("CHECK y_solid=", y_solid));
echo(str("CHECK hole_pitch=", hole_pitch));
echo(str("CHECK hole_nx=", hole_nx));
echo(str("CHECK hole_nz=", hole_nz));
echo(str("CHECK hole_x_max=", hole_x_max));
echo(str("CHECK m5_tap=", m5_tap));
echo(str("CHECK plate_t=", plate_t));
echo(str("CHECK rail_z0=", rail_z0));
echo(str("CHECK rail_z1=", rail_z1));
echo(str("CHECK rail_h=", rail_h));
echo(str("CHECK rail_span=", rail_span));
echo(str("CHECK rail_y0=", rail_y0));
echo(str("CHECK front_wall=", front_wall));
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
echo(str("CHECK camera_clearance=", camera_clearance));
echo(str("CHECK win_x=", win_x));
echo(str("CHECK win_z=", win_z));
echo(str("CHECK standout_y=", standout_y));
echo(str("CHECK asm_top_z=", asm_top_z));
echo(str("CHECK cradle_count=", 1));
echo(str("CHECK screw_count=", 2));
echo(str("CHECK front_lip_h=", front_lip_h));
echo(str("CHECK front_lip_t=", front_lip_t));

/* ----------------------- parts ----------------------- */

module m5_hole(length) {
    cylinder(h = length, d = m5_tap, $fn = 28);
}

module post_block_use(side) {
    post_x = side * half;
    difference() {
        union() {
            translate([post_x - block_w / 2, 0, 0])
                cube([block_w, block_depth, block_h]);
            // Horizontal arm. Length is +Y, width is X, thickness is Z.
            translate([post_x - arm_width / 2, arm_y0, arm_z0])
                cube([arm_width, arm_y1 - arm_y0, arm_t]);
            arm_gusset(post_x);
        }
        translate([post_x, bore_cy, -1])
            cylinder(h = block_h + 2, d = bore_d, $fn = fn_bore);
        translate([post_x, bore_cy, -0.01])
            cylinder(h = 1.3, d1 = bore_d + 2.2, d2 = bore_d, $fn = fn_bore);
        // Longitudinal slot down the arm. The cradle slides along it to set depth.
        translate([post_x - slot_span / 2, slot_y0, arm_z0 - 1])
            cube([slot_span, slot_len, arm_t + 2]);
        // Pinch screw from the top of the block down into the bore.
        translate([post_x, bore_cy, block_h - 10])
            cylinder(h = 12, d = pinch_d, $fn = 24);
    }
}

module arm_gusset(post_x) {
    // Fillet in front of the bore so the arm is not carried by the front wall alone.
    y0 = bore_cy + bore_d / 2 + 0.6;
    hull() {
        translate([post_x - arm_width / 2, y0, arm_z0])
            cube([arm_width, 16, arm_t]);
        translate([post_x - 12, y0, arm_z0])
            cube([24, 1.2, 22]);
    }
}

module hole_row() {
    for (ix = [0 : hole_nx - 1]) {
        hx = -hole_x_max + ix * hole_pitch;
        translate([hx, y_slot, rail_z0 - 0.4])
            m5_hole(rail_h + 0.6);
    }
}

module bottom_slots() {
    // One clearance slot under each threaded hole. The shank passes through,
    // then through the arm's longitudinal slot, then into the roof thread.
    for (ix = [0 : hole_nx - 1]) {
        hx = -hole_x_max + ix * hole_pitch;
        translate([hx - bot_slot_w / 2, y_slot - slot_gap / 2, z_bot0 - 0.6])
            cube([bot_slot_w, slot_gap, bottom_t + 1.2]);
    }
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
            // Phone side walls, with a 45° wedge down to the roof so the print needs no support.
            for (sx = [-1, 1]) {
                x0 = sx * outer_x / 2 - (sx > 0 ? side_wall : 0);
                hull() {
                    translate([x0, y_plate1, rail_z0])
                        cube([side_wall, 0.02, z_pocket1 - rail_z0]);
                    translate([x0, y_plate1 + pocket_y + front_lip_t - 0.02, z_pocket0])
                        cube([side_wall, 0.02, pocket_z]);
                }
            }
            // Lip at the bottom of the pocket, and the wedge under it.
            hull() {
                translate([-pocket_x / 2, y_plate1, rail_z0])
                    cube([pocket_x, 0.02, rail_h]);
                translate([-pocket_x / 2, y_plate1 + front_lip_t - 0.02, z_pocket0])
                    cube([pocket_x, 0.02, front_lip_h]);
            }
        }
        hole_row();
        bottom_slots();
        camera_windows();
        strap_slots();
    }
}

module camera_windows() {
    for (sx = [-1, 1]) {
        x0 = sx > 0 ? pocket_x / 2 - camera_clearance : -pocket_x / 2;
        translate([x0, rail_y0 - 1, z_pocket1 - camera_clearance])
            cube([camera_clearance, y_plate1 - rail_y0 + 2, camera_clearance + 1]);
    }
}

module strap_slots() {
    slot_z = z_pocket1 - 16;
    for (sx = [-1, 1])
        translate([sx * outer_x / 2 - side_wall - 0.8, y_plate1 + 1.2, slot_z])
            cube([side_wall + 1.6, max(4, pocket_y - 2.4), 3.2]);
}

module coupon_use() {
    h = 18;
    difference() {
        translate([-block_w / 2, 0, 0])
            cube([block_w, block_depth, h]);
        translate([0, bore_cy, -1])
            cylinder(h = h + 2, d = bore_d, $fn = fn_bore);
        translate([0, bore_cy, -0.01])
            cylinder(h = 1.2, d1 = bore_d + 2.2, d2 = bore_d, $fn = fn_bore);
    }
}

module thumbscrew_solid() {
    // Bearing face at z = 0. Head and wings occupy z < 0. Shank goes up through
    // the cradle bottom, the arm slot, and into the roof. No modeled helix.
    shank_d = m5_tap - 0.4;
    translate([0, 0, -screw_head_h])
        cylinder(h = screw_head_h, d = screw_head_d, $fn = 40);
    translate([-screw_wing_l / 2, -screw_wing_t / 2, -screw_head_h])
        cube([screw_wing_l, screw_wing_t, screw_wing_h]);
    translate([0, 0, -1.2])
        cylinder(h = screw_shank_l + 1.2, d = shank_d, $fn = 24);
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
    translate([-(post_x - block_w / 2), 0, 0])
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
    translate([block_w / 2, 0, 0])
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
