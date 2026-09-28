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
// One shared landscape cradle. Two post blocks, one per headrest post.
// Each post passes through its block. The flat heel of each block and the
// flat back of the cradle sit on the same plane (Y = 0) against the headrest
// face. Each block carries a ~200 mm arm. Both arms lie in one receiver on
// the cradle. A slot across each arm's width lets one M5 thumbscrew pass
// through both slots into a tapped hole in the solid back. A grid of those
// holes is the discrete lock as the arms pivot on the posts.
//
// Use frame (assembly):
//   X  across the headrest posts
//   Y  out from the headrest face (Y = 0 is the flush bearing plane)
//   Z  up, along the posts
//
// Print frames are applied by the orient modules.

/* [Headrest posts] */
post_spacing_min = 110;  // mm, narrowest center-to-center the arms can still join
post_spacing_max = 170;  // mm, widest center-to-center the arms can still join
post_diameter    = 14;   // mm, nominal post OD
post_spacing     = 140;  // mm, block centers in the assembly preview

/* [Arms and lock] */
arm_length   = 200;  // mm, post axis to arm tip
slot_span    = 16;   // mm, slot length across the arm width
slot_gap     = 8;    // mm, slot opening along the arm (clears an M5 shank at an angle)
slot_radius  = 185;  // mm, post axis to the slot center
hole_pitch   = 10;   // mm, threaded-hole grid
hole_nx      = 5;    // odd, so a hole sits on the centerline
hole_nz      = 3;

/* [Phone cradle] */
phone_width_min     = 70;   // mm, landscape short side
phone_width_max     = 85;
phone_length_min    = 140;  // mm, landscape long side
phone_length_max    = 172;
phone_thickness_max = 12;   // mm, including a slim case
camera_clearance    = 18;   // mm, square window, both upper corners of the back plate

/* [Structure] */
jaw_wall   = 4;    // mm, material outside the post bore
clearance  = 0.4;  // mm, diametral clearance in the bore
cradle_lip = 3;    // mm, front lip thickness

/* [Export] */
// assembly, block, block_outer, tray, coupon
part = "assembly";

/* [Hidden] */
eps = 0.04;
plate_t = 6.0;
arm_thick = 5.0;
arm_gap = 0.35;
arm_width = 22;
block_w = 36;
block_h = 40;
heel = 4.0;
m5_tap = 4.2;     // M5×0.8 tap-drill; chase with a tap
m5_clear = 5.4;
pinch_d = 4.2;
fn_bore = 64;
$fn = 32;

/* ----------------------- derived ----------------------- */

bore_d = post_diameter + clearance;
half = post_spacing / 2;
z_meet = sqrt(slot_radius * slot_radius - half * half);
ang = atan2(half, z_meet);
z_post = block_h / 2;
z_slot = z_post + z_meet;
bore_cy = heel + bore_d / 2;
block_depth = bore_cy + bore_d / 2 + jaw_wall;
r_arm0 = bore_d / 2 + 2.6;

side_wall = 3.4;
back_is_plate = plate_t;
pocket_x = phone_length_max + 1.6;
pocket_z = phone_width_max + 0.8;
pocket_y = phone_thickness_max + 0.5;
front_lip_t = cradle_lip;
front_lip_h = cradle_lip + 5;
floor_t = 4.0;
outer_x = pocket_x + 2 * side_wall;

recv_w = 108;
recv_below = 44;
recv_above = 26;
z_recv0 = z_slot - recv_below;
z_recv1 = z_slot + recv_above;
z_pocket0 = z_recv1 + floor_t;
z_pocket1 = z_pocket0 + pocket_z;
z_plate0 = z_recv0;
z_plate1 = z_pocket1;

arm_y0 = plate_t + 0.35;
arm_y0_outer = arm_y0 + arm_thick + arm_gap;
arm_y1_outer = arm_y0_outer + arm_thick;
recv_y0 = plate_t;
recv_y1 = arm_y1_outer + 0.8;
standout_y = max(block_depth, plate_t + pocket_y + front_lip_t);
asm_top_z = z_pocket1;

ix0 = -(hole_nx - 1) / 2;
iz0 = -(hole_nz - 1) / 2;

solid_r = slot_radius - slot_gap / 2 - 3.5;
dirx = half / slot_radius;
dirz = z_meet / slot_radius;
solid_x = -half + solid_r * dirx;
solid_z = z_post + solid_r * dirz;
inner_slot_y = arm_y0 + arm_thick / 2;
outer_slot_y = arm_y0_outer + arm_thick / 2;
win_x = pocket_x / 2 - camera_clearance / 2;
win_z = z_pocket1 - camera_clearance / 2;

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
assert(heel + 0.01 >= 3, "heel behind the bore is too thin to bear on the headrest");
assert(bore_cy - bore_d / 2 + 0.01 >= heel - 0.01, "bore breaks the flush heel");
assert(arm_length + 0.01 >= 190 && arm_length <= 260, "arm length is outside the 200 mm class");
assert(slot_radius + slot_gap / 2 + 6 <= arm_length, "slot runs off the arm tip");
assert(slot_span + 4 <= arm_width, "slot does not leave a margin across the arm width");
assert(slot_gap >= m5_clear + 1.5, "slot will not pass an M5 shank when the arms cross at an angle");
assert(slot_radius > post_spacing_max / 2 + 8, "arms cannot meet between the posts at post_spacing_max");
assert(slot_radius > post_spacing_min / 2 + 8, "arms cannot meet between the posts at post_spacing_min");
assert(hole_nx % 2 == 1 && hole_nz % 2 == 1, "hole grid needs a center hole");
assert(hole_pitch >= 8 && hole_pitch <= 16, "hole pitch is outside the discrete-lock range");
assert((hole_nx - 1) * hole_pitch + m5_tap < recv_w - 8, "hole row is wider than the receiver");
assert(z_pocket0 > z_slot + recv_above - 0.01, "phone pocket overlaps the arm receiver");
assert(arm_y0 + 0.01 >= plate_t, "inner arm is buried in the back plate");
assert(arm_y0_outer >= arm_y0 + arm_thick + 0.2, "arm layers collide");
assert(arm_y1_outer <= recv_y1, "outer arm stands out of the receiver");
assert(r_arm0 > bore_d / 2 + 1, "arm root is cut by the post bore");

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
echo(str("CHECK arm_thick=", arm_thick));
echo(str("CHECK slot_span=", slot_span));
echo(str("CHECK slot_gap=", slot_gap));
echo(str("CHECK slot_radius=", slot_radius));
echo(str("CHECK hole_pitch=", hole_pitch));
echo(str("CHECK hole_nx=", hole_nx));
echo(str("CHECK hole_nz=", hole_nz));
echo(str("CHECK m5_tap=", m5_tap));
echo(str("CHECK plate_t=", plate_t));
echo(str("CHECK z_post=", z_post));
echo(str("CHECK z_slot=", z_slot));
echo(str("CHECK z_meet=", z_meet));
echo(str("CHECK ang=", ang));
echo(str("CHECK half=", half));
echo(str("CHECK pocket_x=", pocket_x));
echo(str("CHECK pocket_y=", pocket_y));
echo(str("CHECK pocket_z=", pocket_z));
echo(str("CHECK z_pocket0=", z_pocket0));
echo(str("CHECK z_pocket1=", z_pocket1));
echo(str("CHECK z_recv0=", z_recv0));
echo(str("CHECK outer_x=", outer_x));
echo(str("CHECK phone_width_min=", phone_width_min));
echo(str("CHECK phone_width_max=", phone_width_max));
echo(str("CHECK phone_length_min=", phone_length_min));
echo(str("CHECK phone_length_max=", phone_length_max));
echo(str("CHECK phone_thickness_max=", phone_thickness_max));
echo(str("CHECK camera_clearance=", camera_clearance));
echo(str("CHECK win_x=", win_x));
echo(str("CHECK win_z=", win_z));
echo(str("CHECK inner_slot_y=", inner_slot_y));
echo(str("CHECK outer_slot_y=", outer_slot_y));
echo(str("CHECK solid_x=", solid_x));
echo(str("CHECK solid_z=", solid_z));
echo(str("CHECK standout_y=", standout_y));
echo(str("CHECK asm_top_z=", asm_top_z));
echo(str("CHECK cradle_count=", 1));
echo(str("CHECK front_lip_h=", front_lip_h));
echo(str("CHECK front_lip_t=", front_lip_t));
echo(str("CHECK floor_t=", floor_t));

/* ----------------------- parts ----------------------- */

module m5_hole(length) {
    cylinder(h = length, d = m5_tap, $fn = 28);
}

module post_block_use(side, outer) {
    post_x = side * half;
    y0 = outer ? arm_y0_outer : arm_y0;
    difference() {
        union() {
            translate([post_x - block_w / 2, 0, 0])
                cube([block_w, block_depth, block_h]);
            translate([post_x, y0, z_post])
                rotate([0, -side * ang, 0])
                    translate([-arm_width / 2, 0, r_arm0])
                        cube([arm_width, arm_thick, arm_length - r_arm0]);
        }
        // Round bore plus a teardrop toward +Y so the horizontal print needs no support.
        // The inscribed circle stays at bore_d; the coupon is the round fit gauge.
        translate([post_x, 0, -1])
            hull() {
                translate([0, bore_cy, 0])
                    cylinder(h = block_h + 2, d = bore_d, $fn = fn_bore);
                translate([0, block_depth - 1.3, 0])
                    cylinder(h = block_h + 2, d = 0.4, $fn = 6);
            }
        translate([post_x, bore_cy, -0.01])
            cylinder(h = 1.35, d1 = bore_d + 2.4, d2 = bore_d, $fn = fn_bore);
        translate([post_x, bore_cy, block_h - 1.34])
            cylinder(h = 1.35, d1 = bore_d, d2 = bore_d + 2.4, $fn = fn_bore);
        // Slot across the arm width, through the thickness.
        translate([post_x, y0 - 0.8, z_post])
            rotate([0, -side * ang, 0])
                translate([-slot_span / 2, 0, slot_radius - slot_gap / 2])
                    cube([slot_span, arm_thick + 1.6, slot_gap]);
        // Pinch screw along +Y into the bore, so the block can be locked after it pivots.
        translate([post_x, bore_cy, z_post])
            rotate([-90, 0, 0])
                cylinder(h = block_depth - bore_cy + 1, d = pinch_d, $fn = 24);
    }
}

module hole_grid() {
    for (ix = [0 : hole_nx - 1], iz = [0 : hole_nz - 1]) {
        hx = (ix + ix0) * hole_pitch;
        hz = z_slot + (iz + iz0) * hole_pitch;
        translate([hx, plate_t + 0.4, hz])
            rotate([90, 0, 0])
                m5_hole(plate_t + 0.8);
    }
}

module tray_use() {
    difference() {
        union() {
            // Flush back. The entire rear face is the headrest bearing.
            translate([-outer_x / 2, 0, z_plate0])
                cube([outer_x, plate_t, z_plate1 - z_plate0]);
            // Floor between the receiver and the phone pocket.
            translate([-outer_x / 2, plate_t, z_recv1])
                cube([outer_x, pocket_y + front_lip_t, floor_t]);
            // Side walls.
            for (sx = [-1, 1])
                translate([sx * outer_x / 2 - (sx > 0 ? side_wall : 0), plate_t, z_pocket0])
                    cube([side_wall, pocket_y + front_lip_t, pocket_z]);
            // Front lip, bottom of the pocket, so the phone stays seated.
            translate([-pocket_x / 2, plate_t + pocket_y, z_pocket0])
                cube([pocket_x, front_lip_t, front_lip_h]);
        }
        // Receiver: open toward the posts (-Z) and toward +Y.
        translate([-recv_w / 2, recv_y0, z_recv0 - 1])
            cube([recv_w, recv_y1 - recv_y0, recv_above + recv_below + 1]);
        hole_grid();
        camera_windows();
        strap_slots();
    }
}

module camera_windows() {
    for (sx = [-1, 1]) {
        x0 = sx > 0 ? pocket_x / 2 - camera_clearance : -pocket_x / 2;
        translate([x0, -1, z_pocket1 - camera_clearance])
            cube([camera_clearance, plate_t + 2, camera_clearance + 1]);
    }
}

module strap_slots() {
    slot_z = z_pocket1 - 16;
    for (sx = [-1, 1])
        translate([sx * outer_x / 2 - side_wall - 0.8, plate_t + 1.4, slot_z])
            cube([side_wall + 1.6, max(4, pocket_y - 2.8), 3.2]);
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

module thumbscrew() {
    // Preview hardware only. Shank stays inside the flush face; head bears on the outer arm.
    y_head = arm_y1_outer + 0.35;
    translate([0, 0.6, z_slot])
        rotate([-90, 0, 0])
            cylinder(h = y_head - 0.6, d = m5_tap - 0.35, $fn = 24);
    translate([0, y_head, z_slot])
        rotate([-90, 0, 0])
            cylinder(h = 3.6, d = 14, $fn = 32);
}

module assembly_use() {
    post_block_use(-1, false);
    post_block_use(1, true);
    tray_use();
    thumbscrew();
}

/* ----------------------- print orientation ----------------------- */

module print_rib(side, outer) {
    // Sacrificial wall under the free length of the arm so the print STL needs
    // no slicer support. Snap it off before assembly. Not part of the use frame.
    post_x = side * half;
    y0 = outer ? arm_y0_outer : arm_y0;
    rib_start = block_h / 2 + 8;
    rib_end = slot_radius - slot_gap / 2 - 6;
    translate([post_x, 0, z_post])
        rotate([0, -side * ang, 0])
            translate([-0.55, 0, rib_start])
                cube([1.1, y0 + 0.15, rib_end - rib_start]);
}

module orient_block(side, outer) {
    post_x = side * half;
    // Heel (use Y = 0) on the bed. Arm runs along +X. A snapped-off rib holds the arm up.
    span = block_w / 2 * cos(ang) + block_h / 2 * sin(ang);
    translate([arm_length, span, 0])
        rotate([0, 0, -90])
            rotate([90, 0, 0])
                rotate([0, side * ang, 0])
                    translate([-post_x, 0, -z_post])
                        union() {
                            post_block_use(side, outer);
                            print_rib(side, outer);
                        }
}

module orient_tray() {
    tray_depth = plate_t + pocket_y + front_lip_t;
    translate([outer_x / 2, -z_plate0, tray_depth])
        rotate([-90, 0, 0])
            tray_use();
}

module orient_coupon() {
    translate([block_w / 2, 0, 0])
        coupon_use();
}

/* ----------------------- export switch ----------------------- */

if (part == "assembly")
    assembly_use();
else if (part == "block")
    orient_block(-1, false);
else if (part == "block_outer")
    orient_block(1, true);
else if (part == "tray")
    orient_tray();
else if (part == "coupon")
    orient_coupon();
else if (part == "raw_block")
    post_block_use(-1, false);
else if (part == "raw_block_outer")
    post_block_use(1, true);
else if (part == "raw_tray")
    tray_use();
else if (part == "raw_coupon")
    coupon_use();
else if (part == "fitcheck")
    intersection() {
        tray_use();
        union() {
            post_block_use(-1, false);
            post_block_use(1, true);
        }
    }
else if (part == "fitcheck_arms")
    intersection() {
        post_block_use(-1, false);
        post_block_use(1, true);
    }
else
    assert(false, str("unknown part: ", part));
