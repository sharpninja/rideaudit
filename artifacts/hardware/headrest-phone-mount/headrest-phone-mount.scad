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
// Source of truth for the dual-cradle headrest bracket. STL files under
// exports/ are generated from this file; do not hand-edit them.
//
// Use frame (assembly):
//   X  across the headrest (post spacing)
//   Y+ road / forward face
//   Z  up, along the headrest posts
//
// Print frames are applied by orient_* modules. Part STLs are already
// oriented for FDM (flat on the bed, no required supports).

/* [Headrest posts] */
post_spacing_min = 110;   // mm, center-to-center minimum
post_spacing_max = 170;   // mm, center-to-center maximum
post_diameter    = 14;    // mm, nominal post OD
post_spacing     = 140;   // mm, clamp centers in the assembly preview

/* [Phone cradles] */
phone_width_min     = 70;   // mm, landscape short side, narrowest phone
phone_width_max     = 85;   // mm, landscape short side, widest phone
phone_length_min    = 140;  // mm, landscape long side, shortest phone
phone_length_max    = 172;  // mm, landscape long side, longest phone
phone_thickness_max = 12;   // mm, including a slim case
camera_clearance    = 18;   // mm, square lens window on each upper corner
dual_cradle         = true;
// 0 = opposed (forward + cabin). 1 = dual-forward (both phones face +Y).
cradle_layout       = 0;

/* [Clamp / structure] */
clamp_depth    = 35;  // mm, jaw engagement along the post
beam_thickness = 8;   // mm, rail height
jaw_wall       = 4;   // mm
cable_notch_w  = 10;  // mm, cable-slot width
cable_notch_d  = 6;   // mm, documented tie depth; groove is capped for strength
cradle_lip     = 3;   // mm, front retention lip thickness
clearance      = 0.4; // mm, diametral print clearance on the post

/* [Export] */
// assembly, assembly_forward, assembly_single,
// beam, extension, clamp, tray, clip, clip_cabin, pin, coupon
part = "assembly";

/* [Hidden] */
eps = 0.04;
slide_clear = 0.32;
channel_wall = 3.2;
lip = 1.65;
lip_h = 2.3;
roof_t = 6.2;
throat_ratio = 0.76;
pad_t = 4.2;
pad_w = 46;
foot_len = 62;
link_half = 18;
bridge_overlap = 0.35;
pin_r = 2.05;
pin_gap = 0.55;
tongue_len = 5.2;
tongue_y = 7.0;
tongue_z = 3.1;
tongue_fit = 0.30;
end_past_tongue = 1.15;
stub_end_pad = 3.0;
m3_clear = 3.4;
m3_nut = 6.55;
m3_nut_t = 2.7;
fn_bore = 64;

$fn = 48;

/* ----------------------- derived geometry ----------------------- */

clamp_body_x = post_diameter + 2 * jaw_wall;
clamp_body_y = post_diameter + 2 * jaw_wall;
carriage_len = max(16, clamp_body_x - 6);

beam_bar_y = 16;
beam_bar_z = beam_thickness;
beam_gap = 2.8;
beam_y0 = clamp_body_y / 2 + beam_gap;
beam_y1 = beam_y0 + beam_bar_y;
rail_yc = (beam_y0 + beam_y1) / 2;

stub_y0 = -beam_y1;
stub_y1 = -beam_y0;
stub_yc = (stub_y0 + stub_y1) / 2;

bore_d = post_diameter + clearance;
throat_w = post_diameter * throat_ratio;
lead_w = min(clamp_body_x - 2.2, throat_w + 3.4);

// Rail sits high in the clamp so the channel roof is the top face.
// That face is the FDM bed after the clamp/clip are flipped.
roof_top = clamp_depth;
void_z1 = roof_top - roof_t;
beam_z1 = void_z1 - slide_clear;
beam_z0 = beam_z1 - beam_bar_z;
rail_zc = (beam_z0 + beam_z1) / 2;

travel_outer = post_spacing_max / 2 + carriage_len / 2;
pin_x = travel_outer + pin_gap + pin_r;
tongue_x0 = pin_x + pin_r + 0.7;
beam_half = tongue_x0 + tongue_len + end_past_tongue;
beam_length = 2 * beam_half;

stub_pin_x = foot_len / 2 + pin_gap + pin_r;
stub_half = stub_pin_x + stub_end_pad;

side_wall = 3.4;
back_t = 5.0;
bottom_t = 4.0;
front_lip_t = cradle_lip;
front_lip_h = cradle_lip + 5;
pocket_x = phone_length_max + 1.6;
pocket_z = phone_width_max + 0.8;
pocket_y = phone_thickness_max + 0.5;
outer_x = pocket_x + 2 * side_wall;
outer_z = bottom_t + pocket_z;

void_y0 = beam_y0 - slide_clear;
void_y1 = beam_y1 + slide_clear;
foot_y0 = void_y0 - channel_wall;
foot_y1 = void_y1 + channel_wall;
void_z0 = beam_z0 - slide_clear;
foot_z0 = void_z0 - lip_h;

plate_y0 = foot_y1 + pad_t - 0.12;
pocket_y0 = plate_y0 + back_t;
pocket_y1 = pocket_y0 + pocket_y;
front_y1 = pocket_y1 + front_lip_t;
cradle_z0 = 0;
pocket_z0 = cradle_z0 + bottom_t;
pocket_z1 = pocket_z0 + pocket_z;

forward_projection = front_y1;
// Cabin cradle is the Y mirror of the forward cradle, on the mirrored stub rail.
cabin_projection = forward_projection;

layout = (part == "assembly_forward") ? 1
       : (part == "assembly_single") ? -1
       : cradle_layout;
is_dual = (part == "assembly_single") ? false : dual_cradle;
show_opposed = is_dual && layout == 0;
show_forward = is_dual && layout == 1;

cradle_gap = 8;
df_center = outer_x / 2 + cradle_gap / 2;
ext_pin_x = df_center + foot_len / 2 + pin_gap + pin_r;
ext_end = ext_pin_x + stub_end_pad;

cable_slot_w = min(6.5, max(3.5, cable_notch_w));
cable_slot_x = foot_len / 2 + cable_slot_w / 2 + 4;
cable_groove_d = min(2.2, max(1.2, cable_notch_d * 0.35));

screw_x = 12;
screw_z = rail_zc;

lip_inner = lip - slide_clear;

/* ----------------------- acceptance asserts ----------------------- */

assert(post_spacing_max > post_spacing_min + 8, "post spacing range collapsed");
assert(post_spacing + 0.01 >= post_spacing_min && post_spacing - 0.01 <= post_spacing_max,
       "post_spacing preview is outside min..max");
assert(phone_width_max + 0.01 >= phone_width_min, "phone width range");
assert(phone_length_max + 0.01 >= phone_length_min, "phone length range");
assert(phone_length_min + 0.01 >= phone_width_max, "landscape long side must exceed the short side");
assert(phone_thickness_max >= 6 && phone_thickness_max <= 18, "phone thickness out of bracket range");
assert(camera_clearance >= 12, "camera_clearance too small to uncover a lens");
assert(2 * camera_clearance + 10 <= pocket_x, "camera windows do not fit across the back plate");
assert(camera_clearance + 8 <= pocket_z, "camera window does not fit the short side");
assert(pocket_z1 - camera_clearance >= screw_z + m3_nut / 2 + 2,
       "camera window cuts the cradle screws");
assert(throat_w < post_diameter - 1.5, "throat will not snap onto the post");
assert(bore_d > post_diameter, "bore does not clear the post");
assert(bore_d + 1.5 < clamp_body_y, "jaw wall swallowed by the bore");
assert(lip_inner > 0.8, "rail lips do not stay engaged after slide clearance");
assert(beam_z0 > 4, "rail is too low in the clamp to leave a grip below it");
assert(roof_top + 0.01 >= void_z1 + roof_t, "roof is not the top of the clamp");
assert(post_spacing_min / 2 - carriage_len / 2 >= foot_len / 2 + 1.5,
       "clamps collide with the cradle clip at post_spacing_min");
assert(link_half + post_diameter / 2 + 3 < post_spacing_min / 2,
       "center link hits a headrest post at post_spacing_min");
assert(pin_x - pin_r >= travel_outer + 0.5, "stop pin blocks maximum clamp travel");
assert(tongue_x0 >= pin_x + pin_r + 0.6, "extension tongue cuts the stop-pin hole");
assert(beam_half > tongue_x0 + tongue_len, "beam does not contain the tongue pocket");
assert(cable_slot_x - cable_slot_w / 2 >= foot_len / 2 + 1.5, "cable slot hits the cradle clip");
assert(cable_slot_x + cable_slot_w / 2 + 1.2 <= post_spacing_min / 2 - carriage_len / 2,
       "cable slot hits a clamp at post_spacing_min");
assert(stub_half + 1 < post_spacing_min / 2 - post_diameter / 2,
       "cabin stub reaches a post at post_spacing_min");
assert(pocket_x + 0.01 >= phone_length_max, "pocket shorter than the longest phone");
assert(pocket_y + 0.01 >= phone_thickness_max, "pocket shallower than phone_thickness_max");
assert(pocket_z + 0.01 >= phone_width_max, "pocket shorter than the widest short side");
assert(front_lip_h + 1 < pocket_z - camera_clearance, "front lip rises into the camera window");
assert(ext_end > df_center + foot_len / 2 + 1, "extension does not support the outer cradle");
assert(jaw_wall >= 3 && clamp_depth >= 28, "clamp section is too light");

echo(str("CHECK post_spacing_min=", post_spacing_min));
echo(str("CHECK post_spacing_max=", post_spacing_max));
echo(str("CHECK post_spacing_preview=", post_spacing));
echo(str("CHECK post_diameter=", post_diameter));
echo(str("CHECK bore_d=", bore_d));
echo(str("CHECK throat_w=", throat_w));
echo(str("CHECK beam_length=", beam_length));
echo(str("CHECK beam_half=", beam_half));
echo(str("CHECK carriage_len=", carriage_len));
echo(str("CHECK foot_len=", foot_len));
echo(str("CHECK pocket_x=", pocket_x));
echo(str("CHECK pocket_y=", pocket_y));
echo(str("CHECK pocket_z=", pocket_z));
echo(str("CHECK camera_clearance=", camera_clearance));
echo(str("CHECK phone_width_min=", phone_width_min));
echo(str("CHECK phone_width_max=", phone_width_max));
echo(str("CHECK phone_length_min=", phone_length_min));
echo(str("CHECK phone_length_max=", phone_length_max));
echo(str("CHECK phone_thickness_max=", phone_thickness_max));
echo(str("CHECK forward_projection=", forward_projection));
echo(str("CHECK cabin_projection=", cabin_projection));
echo(str("CHECK cradle_top_z=", pocket_z1));
echo(str("CHECK beam_z0=", beam_z0));
echo(str("CHECK beam_z1=", beam_z1));
echo(str("CHECK roof_top=", roof_top));
echo(str("CHECK link_half=", link_half));
echo(str("CHECK pin_x=", pin_x));
echo(str("CHECK stub_half=", stub_half));
echo(str("CHECK ext_end=", ext_end));
echo(str("CHECK df_center=", df_center));
echo(str("CHECK outer_x=", outer_x));
echo(str("CHECK outer_z=", outer_z));
echo(str("CHECK lip_inner=", lip_inner));
echo(str("CHECK cable_slot_w=", cable_slot_w));
echo(str("CHECK cable_slot_x=", cable_slot_x));
echo(str("CHECK screw_z=", screw_z));
echo(str("CHECK screw_x=", screw_x));
echo(str("CHECK beam_y0=", beam_y0));
echo(str("CHECK beam_y1=", beam_y1));
echo(str("CHECK rail_yc=", rail_yc));
echo(str("CHECK foot_y0=", foot_y0));
echo(str("CHECK foot_y1=", foot_y1));
echo(str("CHECK plate_y0=", plate_y0));
echo(str("CHECK pocket_y0=", pocket_y0));
echo(str("CHECK pocket_y1=", pocket_y1));
echo(str("CHECK pocket_z0=", pocket_z0));
echo(str("CHECK pocket_z1=", pocket_z1));
echo(str("CHECK front_lip_h=", front_lip_h));
echo(str("CHECK clamp_body_x=", clamp_body_x));
echo(str("CHECK clamp_body_y=", clamp_body_y));
echo(str("CHECK pad_t=", pad_t));
echo(str("CHECK dual=", is_dual ? 1 : 0));
echo(str("CHECK layout=", layout));
echo(str("CHECK cradle_count=", is_dual ? 2 : 1));

/* ----------------------- primitives ----------------------- */

module m3_hole(h) {
    cylinder(h = h, d = m3_clear, $fn = 28);
}

module pin_shaft_hole(depth) {
    cylinder(h = depth, d = pin_r * 2, $fn = 36);
}

module rail(x0, x1, y0) {
    translate([x0, y0, beam_z0])
        cube([x1 - x0, beam_bar_y, beam_bar_z]);
}

module slide_channel(len, open_inner = false) {
    // open_inner drops the gap-side wall so a cradle clip can slide onto the
    // rail from the end without hitting the center link. Floor ties keep the
    // inner lip attached. Clamps leave the wall in place; it joins the C-body.
    x0 = -len / 2;
    difference() {
        translate([x0, foot_y0, foot_z0])
            cube([len, foot_y1 - foot_y0, roof_top - foot_z0]);
        translate([x0 - 1, void_y0, void_z0])
            cube([len + 2, void_y1 - void_y0, void_z1 - void_z0 + 0.02]);
        translate([x0 - 1, void_y0 + lip, foot_z0 - 1])
            cube([len + 2, (void_y1 - void_y0) - 2 * lip, (void_z0 - foot_z0) + 1]);
        if (open_inner)
            translate([x0 - 1, foot_y0 - 1, void_z0 - 0.02])
                cube([len + 2, (void_y0 + 0.8) - (foot_y0 - 1), roof_top]);
    }
    if (open_inner) {
        tie_y0 = void_y0 + lip;
        tie_y = (void_y1 - void_y0) - 2 * lip;
        tie_z = void_z0 - foot_z0;
        for (tx = [-1, 1])
            translate([tx * (len / 2 - 8) - 2.4, tie_y0, foot_z0])
                cube([4.8, tie_y, tie_z]);
    }
}

module rail_lock_cuts() {
    // Clearance hole through the roof. Square nut drops in from the flat top
    // and sits on the shoulder; the screw tip bears on the rail.
    translate([0, rail_yc, beam_z1 - 0.5])
        m3_hole(roof_top - beam_z1 + 1);
    translate([-m3_nut / 2, rail_yc - m3_nut / 2, roof_top - m3_nut_t])
        cube([m3_nut, m3_nut, m3_nut_t + 0.08]);
}

module c_clamp() {
    difference() {
        translate([-clamp_body_x / 2, -clamp_body_y / 2, 0])
            cube([clamp_body_x, clamp_body_y, clamp_depth]);
        translate([0, 0, -1])
            cylinder(h = clamp_depth + 2, d = bore_d, $fn = fn_bore);
        translate([-throat_w / 2, -clamp_body_y / 2 - 1, -1])
            cube([throat_w, clamp_body_y / 2 + bore_d / 4, clamp_depth + 2]);
        translate([-lead_w / 2, -clamp_body_y / 2 - 1, -1])
            cube([lead_w, 4.8, clamp_depth + 2]);
        translate([0, 0, -0.05])
            cylinder(h = 1.3, d1 = bore_d + 2.6, d2 = bore_d, $fn = fn_bore);
        translate([0, 0, clamp_depth - 1.25])
            cylinder(h = 1.35, d1 = bore_d, d2 = bore_d + 2.6, $fn = fn_bore);
        for (sx = [-1, 1])
            translate([sx * (throat_w / 2), -bore_d * 0.15, -1])
                cylinder(h = clamp_depth + 2, d = 2.2, $fn = 20);
    }
}

module clip_pad() {
    pad_z0 = screw_z - 8;
    pad_z1 = roof_top;
    translate([-pad_w / 2, foot_y1 - 0.12, pad_z0])
        cube([pad_w, pad_t + 0.12, pad_z1 - pad_z0]);
}

module clip_pad_cuts() {
    for (sx = [-1, 1]) {
        translate([sx * screw_x, foot_y1 - 0.4, screw_z])
            rotate([-90, 0, 0])
                m3_hole(pad_t + 1.0);
        // Nut pocket opens on the tray face (+Y). The tray closes it.
        translate([sx * screw_x - m3_nut / 2,
                   foot_y1 + pad_t - m3_nut_t,
                   screw_z - m3_nut / 2])
            cube([m3_nut, m3_nut_t + 0.25, m3_nut]);
    }
}

/* ----------------------- parts in the use frame ----------------------- */

module beam_use() {
    difference() {
        union() {
            rail(-beam_half, beam_half, beam_y0);
            rail(-stub_half, stub_half, stub_y0);
            translate([-link_half, stub_y1 - bridge_overlap, beam_z0])
                cube([2 * link_half,
                      (beam_y0 + bridge_overlap) - (stub_y1 - bridge_overlap),
                      beam_bar_z]);
        }
        for (sx = [-1, 1]) {
            translate([sx * pin_x, rail_yc, beam_z0 - 1])
                pin_shaft_hole(beam_bar_z + 2);
            translate([sx * stub_pin_x, stub_yc, beam_z0 - 1])
                pin_shaft_hole(beam_bar_z + 2);
            extension_pocket(sx);
            cable_slot(sx * cable_slot_x);
        }
        alignment_notch();
    }
}

module extension_pocket(sign) {
    depth = tongue_len + 0.45;
    y0 = rail_yc - (tongue_y + tongue_fit) / 2;
    z0 = beam_z0 + (beam_bar_z - (tongue_z + tongue_fit)) / 2;
    x0 = sign > 0 ? beam_half - depth : -beam_half - 0.2;
    translate([x0, y0, z0])
        cube([depth + 0.2, tongue_y + tongue_fit, tongue_z + tongue_fit]);
}

module cable_slot(x) {
    translate([x - cable_slot_w / 2, rail_yc - cable_slot_w / 2, beam_z0 - 1])
        cube([cable_slot_w, cable_slot_w, beam_bar_z + 2]);
    translate([x - cable_slot_w / 2, beam_y1 - cable_groove_d, beam_z0 + 1.4])
        cube([cable_slot_w, cable_groove_d + 0.2, beam_bar_z - 2.8]);
}

module alignment_notch() {
    translate([-1.1, rail_yc - 4, beam_z1 - 0.7])
        cube([2.2, 8, 1.2]);
}

module extension_use() {
    difference() {
        union() {
            translate([beam_half + 0.15, beam_y0, beam_z0])
                cube([ext_end - beam_half - 0.15, beam_bar_y, beam_bar_z]);
            translate([beam_half - tongue_len, rail_yc - tongue_y / 2,
                       beam_z0 + (beam_bar_z - tongue_z) / 2])
                cube([tongue_len - 0.05, tongue_y, tongue_z]);
        }
        translate([ext_pin_x, rail_yc, beam_z0 - 1])
            pin_shaft_hole(beam_bar_z + 2);
    }
}

module clamp_use() {
    difference() {
        union() {
            c_clamp();
            slide_channel(carriage_len, false);
        }
        rail_lock_cuts();
    }
}

module clip_use() {
    difference() {
        union() {
            slide_channel(foot_len, true);
            clip_pad();
        }
        rail_lock_cuts();
        clip_pad_cuts();
    }
}

module tray_use() {
    difference() {
        union() {
            translate([-outer_x / 2, plate_y0, cradle_z0])
                cube([outer_x, back_t, outer_z]);
            translate([-outer_x / 2, plate_y0, cradle_z0])
                cube([outer_x, front_y1 - plate_y0, bottom_t]);
            for (sx = [-1, 1])
                translate([sx * outer_x / 2 - (sx > 0 ? side_wall : 0), plate_y0, cradle_z0])
                    cube([side_wall, front_y1 - plate_y0, outer_z]);
            translate([-pocket_x / 2, pocket_y1, cradle_z0])
                cube([pocket_x, front_lip_t, bottom_t + front_lip_h]);
        }
        camera_windows();
        strap_slots();
        tray_screw_cuts();
    }
}

module camera_windows() {
    for (sx = [-1, 1]) {
        x0 = sx > 0
            ? pocket_x / 2 - camera_clearance
            : -pocket_x / 2;
        translate([x0, plate_y0 - 1, pocket_z1 - camera_clearance])
            cube([camera_clearance, back_t + 2, camera_clearance + 1]);
    }
}

module strap_slots() {
    slot_z = pocket_z1 - 14;
    slot_y0 = pocket_y0 + 1.2;
    slot_d = max(4, pocket_y - 2.4);
    for (sx = [-1, 1])
        translate([sx * outer_x / 2 - side_wall - 0.6, slot_y0, slot_z])
            cube([side_wall + 1.2, slot_d, 3.4]);
}

module tray_screw_cuts() {
    for (sx = [-1, 1]) {
        translate([sx * screw_x, plate_y0 - 0.4, screw_z])
            rotate([-90, 0, 0])
                m3_hole(back_t + 0.8);
        translate([sx * screw_x, plate_y0 + back_t - 2.05, screw_z])
            rotate([-90, 0, 0])
                cylinder(h = 2.2, d1 = m3_clear, d2 = 6.8, $fn = 28);
    }
}

module pin_use() {
    cylinder(h = beam_bar_z + 1.6, d = pin_r * 2 - 0.28, $fn = 36);
    translate([0, 0, beam_bar_z + 1.35])
        cylinder(h = 2.1, d = 9.0, $fn = 36);
}

module coupon_use() {
    h = 18;
    difference() {
        translate([-clamp_body_x / 2, -clamp_body_y / 2, 0])
            cube([clamp_body_x, clamp_body_y, h]);
        translate([0, 0, -1])
            cylinder(h = h + 2, d = bore_d, $fn = fn_bore);
        translate([-throat_w / 2, -clamp_body_y / 2 - 1, -1])
            cube([throat_w, clamp_body_y / 2 + bore_d / 4, h + 2]);
        translate([-lead_w / 2, -clamp_body_y / 2 - 1, -1])
            cube([lead_w, 4.8, h + 2]);
        translate([0, 0, -0.05])
            cylinder(h = 1.3, d1 = bore_d + 2.6, d2 = bore_d, $fn = fn_bore);
    }
}

module cradle_pair() {
    tray_use();
    clip_use();
}

module assembly_use() {
    beam_use();
    translate([ post_spacing / 2, 0, 0]) clamp_use();
    translate([-post_spacing / 2, 0, 0]) clamp_use();
    if (!is_dual) {
        cradle_pair(true);
    } else if (show_opposed) {
        cradle_pair(true);
        mirror([0, 1, 0]) cradle_pair(true);
    } else if (show_forward) {
        translate([ df_center, 0, 0]) cradle_pair(false);
        translate([-df_center, 0, 0]) cradle_pair(false);
        extension_use();
        mirror([1, 0, 0]) extension_use();
    }
    for (sx = [-1, 1]) {
        translate([sx * pin_x, rail_yc, beam_z0 - 0.2]) pin_use();
        translate([sx * stub_pin_x, stub_yc, beam_z0 - 0.2]) pin_use();
        if (show_forward)
            translate([sx * ext_pin_x, rail_yc, beam_z0 - 0.2]) pin_use();
    }
}

/* ----------------------- print orientation ----------------------- */

module orient_beam() {
    translate([beam_half, -stub_y0, -beam_z0])
        beam_use();
}

module orient_extension() {
    translate([-beam_half + tongue_len, -beam_y0, -beam_z0])
        extension_use();
}

module orient_clamp() {
    // Roof on the bed, trench open upward, part in the +X/+Y octant.
    translate([clamp_body_x / 2, foot_y1, roof_top])
        rotate([180, 0, 0])
            clamp_use();
}

module orient_tray() {
    // Back plate on the bed, pocket open upward, phone-bottom toward +Y.
    // rotate([90,0,0]) maps use Z to -Y; the mirror puts the bottom lip at Y=0.
    translate([outer_x / 2, 0, -plate_y0])
        mirror([0, 1, 0])
            rotate([90, 0, 0])
                tray_use();
}

module orient_clip() {
    translate([foot_len / 2, foot_y1 + pad_t, roof_top])
        rotate([180, 0, 0])
            clip_use();
}

module orient_clip_cabin() {
    // Mirror of the forward clip so the open side faces the center link.
    translate([foot_len / 2, -(foot_y0), roof_top])
        rotate([180, 0, 0])
            mirror([0, 1, 0])
                clip_use();
}

module orient_pin() {
    translate([4.6, 4.6, 0])
        pin_use();
}

module orient_coupon() {
    translate([clamp_body_x / 2, clamp_body_y / 2, 0])
        coupon_use();
}

/* ----------------------- export switch ----------------------- */

if (part == "assembly" || part == "assembly_forward" || part == "assembly_single")
    assembly_use();
else if (part == "beam")
    orient_beam();
else if (part == "extension")
    orient_extension();
else if (part == "clamp")
    orient_clamp();
else if (part == "tray")
    orient_tray();
else if (part == "clip")
    orient_clip();
else if (part == "clip_cabin")
    orient_clip_cabin();
else if (part == "pin")
    orient_pin();
else if (part == "coupon")
    orient_coupon();
else if (part == "raw_beam")
    beam_use();
else if (part == "raw_clamp")
    clamp_use();
else if (part == "raw_tray")
    tray_use();
else if (part == "raw_clip")
    clip_use();
else if (part == "raw_coupon")
    coupon_use();
else if (part == "fitcheck")
    intersection() {
        beam_use();
        union() {
            translate([ post_spacing / 2, 0, 0]) clamp_use();
            translate([-post_spacing / 2, 0, 0]) clamp_use();
            clip_use();
            mirror([0, 1, 0]) clip_use();
        }
    }
else if (part == "fitcheck_ext")
    intersection() {
        union() {
            beam_use();
            extension_use();
            mirror([1, 0, 0]) extension_use();
        }
        union() {
            translate([ df_center, 0, 0]) clip_use();
            translate([-df_center, 0, 0]) clip_use();
        }
    }
else if (part == "fitcheck_tongue")
    intersection() {
        beam_use();
        extension_use();
    }
else
    assert(false, str("unknown part: ", part));
