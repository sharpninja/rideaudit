// RideAudit dual-phone headrest mount
// License: GPL-2.0
// Parametric OpenSCAD model for rideshare audit phone placement.

/* [Headrest posts] */
post_spacing_min = 110;      // mm, center-to-center minimum
post_spacing_max = 170;      // mm, center-to-center maximum
post_diameter    = 14;       // mm, nominal post OD

/* [Phone cradles] */
phone_width_min     = 70;    // mm
phone_width_max     = 85;    // mm
phone_thickness_max = 12;    // mm
camera_clearance    = 18;    // mm, lens cutout / standoff
dual_cradle         = true;  // false = single center cradle

/* [Clamp / structure] */
clamp_depth    = 35;         // mm along post
beam_thickness = 8;          // mm
jaw_wall       = 4;          // mm
cable_notch_w  = 10;         // mm
cable_notch_d  = 6;          // mm
cradle_lip     = 3;          // mm retention lip
clearance      = 0.4;        // mm print clearance on posts

$fn = 64;

post_span = (post_spacing_min + post_spacing_max) / 2;
beam_length = post_spacing_max + post_diameter + 2 * jaw_wall + 20;
cradle_w = phone_width_max + 2 * cradle_lip;
cradle_t = phone_thickness_max + cradle_lip + 2;

module post_clamp(at_x) {
    translate([at_x, 0, 0])
    difference() {
        // outer block
        translate([-post_diameter/2 - jaw_wall, -clamp_depth/2, -beam_thickness])
            cube([post_diameter + 2*jaw_wall, clamp_depth, beam_thickness + post_diameter + jaw_wall], center = false);
        // post bore (vertical)
        translate([0, 0, post_diameter/2])
            rotate([90, 0, 0])
                cylinder(h = clamp_depth + 2, d = post_diameter + clearance, center = true);
        // front slot for tool-less flex onto post
        translate([-1.5, -clamp_depth/2 - 1, 0])
            cube([3, clamp_depth + 2, post_diameter + jaw_wall + 2]);
    }
}

module sliding_slot() {
    // elongated clearance so clamps can slide between min/max spacing
    hull() {
        translate([-post_spacing_max/2, 0, 0])
            cylinder(h = beam_thickness + 2, d = post_diameter + 2);
        translate([ post_spacing_max/2, 0, 0])
            cylinder(h = beam_thickness + 2, d = post_diameter + 2);
    }
}

module cross_beam() {
    difference() {
        translate([-beam_length/2, -clamp_depth/2, -beam_thickness])
            cube([beam_length, clamp_depth, beam_thickness]);
        // cable routing notch at bottom rear edge
        translate([-cable_notch_w/2, clamp_depth/2 - cable_notch_d, -beam_thickness - 0.1])
            cube([cable_notch_w, cable_notch_d + 0.1, beam_thickness + 0.2]);
    }
}

module phone_cradle(at_x) {
    translate([at_x, -clamp_depth/2 - cradle_t, -beam_thickness]) {
        difference() {
            cube([cradle_w, cradle_t + cradle_lip, phone_width_max + cradle_lip]);
            // phone pocket
            translate([cradle_lip, cradle_lip, cradle_lip])
                cube([phone_width_max, phone_thickness_max + 0.2, phone_width_max]);
            // camera clearance window (front)
            translate([cradle_w/2 - camera_clearance/2, -0.1, cradle_lip + 8])
                cube([camera_clearance, cradle_lip + 1, camera_clearance]);
            // allow width range: relief slots for narrower phones (foam pads recommended)
            translate([cradle_lip + (phone_width_max - phone_width_min)/2, cradle_lip, cradle_lip])
                cube([phone_width_min, 0.01, 0.01]); // marker only; pocket uses max
        }
    }
}

module assembly() {
    cross_beam();
    post_clamp(-post_span/2);
    post_clamp( post_span/2);
    if (dual_cradle) {
        phone_cradle(-cradle_w - 4);
        phone_cradle(4);
    } else {
        phone_cradle(-cradle_w/2);
    }
}

assembly();
