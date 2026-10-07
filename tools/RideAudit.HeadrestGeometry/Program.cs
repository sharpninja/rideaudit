// SPDX-License-Identifier: GPL-2.0
// Copyright (C) 2026 RideAudit contributors
// RideAudit headrest phone mount - geometry acceptance checks (C# port of verify-geometry.py).
// The OpenSCAD source is authoritative. This tool fails if cradle/post/arm/thread/flush checks fail.

using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace RideAudit.HeadrestGeometry;

internal static class Program
{
    private static string MountRoot = "";
    private static string Scad = "";
    private static string Exports = "";
    private static string Report = "";

    private static readonly Dictionary<string, string> PrintParts = new(StringComparer.Ordinal)
    {
        ["assembly"] = "headrest-phone-mount.stl",
        ["block"] = "post-block.stl",
        ["tray"] = "phone-cradle.stl",
        ["coupon"] = "fit-coupon.stl",
        ["screw"] = "thumbscrew.stl",
    };

    private static readonly string[] RawParts = ["raw_block", "raw_block_right", "raw_tray", "raw_coupon", "raw_screw"];

    private sealed class Fail : Exception
    {
        public Fail(string message) : base(message) { }
    }

    public static int Main(string[] args)
    {
        var notes = new List<string>();
        Dictionary<string, double> checks;
        Dictionary<string, MeshStats> stats;
        try
        {
            MountRoot = LocateMountRoot(args);
            Scad = Path.Combine(MountRoot, "headrest-phone-mount.scad");
            Exports = Path.Combine(MountRoot, "exports");
            Report = Path.Combine(MountRoot, "verification", "geometry-report.md");

            if (!File.Exists(Scad))
                throw new Fail("missing " + Scad);
            var tmp = Directory.CreateTempSubdirectory("rideaudit-mount-");
            try
            {
                var lines = OpenScad("coupon", Path.Combine(tmp.FullName, "echo.stl"));
                checks = ParseChecks(lines);
                notes.Add("OpenSCAD assertions accepted the default parameters.");

                foreach (var (part, name) in PrintParts)
                    OpenScad(part, Path.Combine(Exports, name));
                foreach (var part in RawParts)
                    OpenScad(part, Path.Combine(tmp.FullName, part + ".stl"));
                foreach (var part in new[] { "fitcheck", "fitcheck_arms" })
                    OpenScad(part, Path.Combine(tmp.FullName, part + ".stl"));
                notes.Add("The two arms do not intersect each other.");
                notes.Add("The arms pass under the receiver roof and do not intersect the cradle.");

                var meshes = PrintParts.Values.ToDictionary(n => n, n => LoadStl(Path.Combine(Exports, n)));
                var raw = RawParts.ToDictionary(p => p, p => LoadStl(Path.Combine(tmp.FullName, p + ".stl")));
                stats = meshes.ToDictionary(kv => kv.Key, kv => MeshStatsOf(kv.Value));
                var rawStats = raw.ToDictionary(kv => kv.Key, kv => MeshStatsOf(kv.Value));

                foreach (var (name, st) in stats)
                {
                    Expect(st.Tris > 50, name + " is unexpectedly small");
                    Expect(st.OpenEdges == 0, name + " is not closed (" + st.OpenEdges + " open edges)");
                    Expect(st.Volume > 10, name + " has no volume");
                    Expect(st.Min.Z > -0.05, name + " extends below the bed (" + st.Min.Z.ToString("0.000", CultureInfo.InvariantCulture) + ")");
                    if (name != "headrest-phone-mount.stl")
                    {
                        Expect(st.Min.Z < 0.05, name + " does not sit on the bed (min z " + st.Min.Z.ToString("0.000", CultureInfo.InvariantCulture) + ")");
                        Expect(st.Min.X > -0.2 && st.Min.Y > -0.2, name + " is not in the positive XY octant");
                    }
                }

                var c = checks;
                var tray = stats["phone-cradle.stl"];
                var block = stats["post-block.stl"];
                var blockSpan = Math.Max(Math.Max(block.Max.X - block.Min.X, block.Max.Y - block.Min.Y), block.Max.Z - block.Min.Z);
                Expect(Math.Abs((tray.Max.X - tray.Min.X) - c["rail_span"]) < 0.3, "cradle width is not the receiver span");
                Expect(blockSpan > c["arm_length"] - 1, "post block print is shorter than the arm");
                Expect(c["bore_d"] > c["post_diameter"], "bore does not clear the post");
                Expect(c["arm_length"] > c["slot_radius"], "preview screw is past the arm tip");
                Expect(c["slot_len"] > 100, "arm slot is not a longitudinal slot");
                Expect(c["y_slot"] - c["slot_y0"] > 40, "cradle cannot slide back along the arm");
                Expect(c["slot_y1"] > c["y_slot"] + 4, "arm slot does not contain the preview screw");
                Expect(c["arm_rise"] == 0, "arms are not horizontal");
                Expect(c["gusset_h"] + 0.01 >= 12, "collar does not stand far enough above the arm");
                Expect(Math.Abs((c["arm_thick"] + c["gusset_h"]) - c["block_t"]) < 0.05, "root blend rises past the collar");
                Expect(6 <= c["gusset_y1"] - c["gusset_y0"] && c["gusset_y1"] - c["gusset_y0"] <= 12, "root blend is not a short run");
                Expect(c["gusset_y1"] + 1 < c["slot_y0"], "gusset runs into the arm slot");
                Expect(c["gusset_y0"] > c["bore_cy"] + c["bore_id"] / 2, "gusset covers the post bore");
                Expect(c["screw_count"] == 2, "model does not have one thumbscrew per arm");
                Expect(c["hole_nz"] == 1, "threaded holes are not a single row");
                Expect(c["pocket_x"] >= c["phone_length_max"], "pocket shorter than the longest phone");
                Expect(c["pocket_y"] >= c["phone_thickness_max"], "pocket shallower than the thickest phone");
                Expect(c["pocket_z"] >= c["phone_width_max"], "pocket shorter than the widest short side");
                Expect(c["camera_clearance"] >= 12, "camera window too small");
                Expect(c["cradle_count"] == 1, "model is not the single shared cradle");
                Expect(c["slot_side_gap"] <= 1.001, "arm slot gap is over 1 mm on a side");
                Expect(c["slot_side_gap"] + 0.001 >= 0.8, "arm slot does not clear the crest");
                Expect(Math.Abs(c["slot_span"] - (c["screw_major"] + 2.0)) < 0.05, "arm slot is not major diameter plus 2 mm");
                Expect(Math.Abs(c["screw_x_left"] + c["half"]) < 0.05, "left screw is off the arm centerline");
                Expect(Math.Abs(c["screw_x_right"] - c["half"]) < 0.05, "right screw is off the arm centerline");
                Expect(Math.Abs(c["bore_id"] - (c["post_od"] + c["post_clearance"])) < 0.05, "bore is not post_od plus clearance");
                Expect(c["post_od"] + 0.001 >= c["post_od_min"] && c["post_od"] - 0.001 <= c["post_od_max"], "post_od is outside 10..14 mm");
                Expect(c["post_clearance"] + 0.001 >= 0.2 && c["post_clearance"] - 0.001 <= 0.5, "clearance is outside 0.2..0.5 mm");
                Expect(Math.Abs(c["post_od"] - 14.0) < 0.05, "default post is not the 14 mm maximum");
                Expect(Math.Abs(c["bore_id"] - 14.5) < 0.05, "default bore is not 14.5 mm");
                Expect(Math.Abs(c["block_od"] - 25.5) < 0.05, "post block is not 25.5 mm outside");
                Expect(Math.Abs(c["block_t"] - 33.0) < 0.05, "post block is not 33 mm thick");
                Expect(c["block_wall"] + 0.01 >= 5, "collar wall is under 5 mm");
                Expect(c["arm_thick"] + 0.01 >= 12, "arm is too thin");
                Expect((c["arm_width"] - c["slot_span"]) / 2 >= 10, "arm rails are too narrow");
                Expect(c["screw_major"] + 0.01 >= 8, "thumbscrew is not the thicker M8 shank");
                Expect(c["screw_engage"] + 0.01 >= c["screw_major"] * 1.25, "thread engagement is short");
                Expect(c["hole_tap"] + 0.05 < c["screw_major"], "roof hole clears the major diameter");
                Expect(c["hole_clear"] + 0.01 >= c["screw_major"] + 0.6, "bottom slot does not clear the thread");
                Expect(Math.Abs(c["clamp_stack"] - (c["bottom_t"] + c["arm_thick"])) < 0.05, "clamp stack is not bottom plate plus arm");
                foreach (var spacing in new[] { c["post_spacing_min"], c["post_spacing_max"], c["post_spacing"] })
                {
                    var halfS = spacing / 2;
                    Expect(c["rail_span"] / 2 + 0.01 >= halfS + c["arm_width"] / 2,
                        "receiver roof does not cover an arm at " + spacing.ToString("0", CultureInfo.InvariantCulture) + " mm spacing");
                }

                notes.Add("Set post_spacing to the measured centers (" + F0(c["post_spacing_min"]) + "-" + F0(c["post_spacing_max"]) + " mm) and re-export. Each arm then has one tap hole on its centerline.");
                notes.Add("The arm slot is " + F1(c["slot_span"]) + " mm wide, " + F2(c["slot_side_gap"]) + " mm clear of the M" + F0(c["screw_major"]) + " crest on each side.");
                notes.Add("Each post block is one collar, bore " + F1(c["bore_id"]) + " mm, outside " + F1(c["block_od"]) + " mm, " + F0(c["block_t"]) + " mm thick.");
                notes.Add("The arm shares the collar bottom. Short blends rise " + F0(c["gusset_h"]) + " mm into the collar and stop at its top, tapering onto the arm over " + F0(c["gusset_y1"] - c["gusset_y0"]) + " mm before the slot.");
                notes.Add("Arms lie in a horizontal plane and enter the receiver from the rear.");
                notes.Add("Each arm has a longitudinal slot. The cradle slides along it to set depth, then each thumbscrew locks.");
                notes.Add("Each thumbscrew comes up through a bottom clearance slot, through that arm slot, and into the roof tap hole.");
                notes.Add("Fully seated, the " + F0(c["screw_head_d"]) + " mm head face clamps " + F0(c["clamp_stack"]) + " mm (bottom plate " + F0(c["bottom_t"]) + " + arm " + F0(c["arm_thick"]) + "); " + F2(c["clamp_takeup"]) + " mm of slide clearance is the take-up, and " + F0(c["screw_engage"]) + " mm of M" + F0(c["screw_major"]) + "x" + F2(c["screw_pitch"]) + " thread stays in the roof.");

                foreach (var part in new[] { "raw_block", "raw_block_right", "raw_coupon" })
                {
                    var st = rawStats[part];
                    Expect(st.Min.Y > -0.05, part + " breaks the flush plane (min y " + st.Min.Y.ToString("0.000", CultureInfo.InvariantCulture) + ")");
                    Expect(st.Min.Y < 0.05, part + " does not bear on Y=0 (min y " + st.Min.Y.ToString("0.000", CultureInfo.InvariantCulture) + ")");
                }
                var trayRaw = rawStats["raw_tray"];
                Expect(trayRaw.Min.Y > c["rail_y0"] - 0.5, "cradle reaches back to the headrest pad");
                notes.Add("Post-block heels bear on Y=0. The cradle sits at the forward end of the arms.");

                var trayM = raw["raw_tray"];
                var blockM = raw["raw_block"];
                var rightM = raw["raw_block_right"];
                var couponM = raw["raw_coupon"];
                var screwM = raw["raw_screw"];
                var probes = new List<string>();
                var plateY = (c["y_plate0"] + c["y_plate1"]) / 2;
                var phoneY = c["y_plate1"] + 1.0;
                var phoneZ = c["z_pocket0"] + c["phone_width_max"] / 2;
                var holeZ = c["rail_z0"] + 2.0;
                var beside = c["slot_span"] / 2 + 1.5;
                probes.Add(Classify(trayM, (0.0, phoneY, phoneZ), false, "max-phone center is in the pocket"));
                probes.Add(Classify(trayM, (c["phone_length_max"] / 2 - 0.6, phoneY, c["z_pocket0"] + c["phone_width_max"] - 0.6), false, "max-phone upper corner is not blocked"));
                probes.Add(Classify(trayM, (0.0, plateY, c["z_pocket0"] + c["pocket_z"] / 2), true, "back plate is present behind the phone"));
                probes.Add(Classify(trayM, (-c["win_x"], plateY, c["win_z"]), false, "camera window is open"));
                probes.Add(Classify(trayM, (c["win_x"], plateY, c["win_z"]), false, "opposite camera window is open"));
                probes.Add(Classify(trayM, (0.0, plateY, c["win_z"]), true, "back plate remains between the camera windows"));
                var lipY = c["y_plate1"] + c["front_lip_t"] / 2;
                var lipZ = c["z_pocket0"] + c["front_lip_h"] / 2;
                probes.Add(Classify(trayM, (0.0, lipY, lipZ), true, "front retention lip is present"));
                probes.Add(Classify(trayM, (0.0, lipY, c["z_pocket0"] + c["front_lip_h"] + 8), false, "front of the cradle stays open above the lip"));
                var lx = c["screw_x_left"];
                var rx = c["screw_x_right"];
                probes.Add(Classify(trayM, (lx, c["y_slot"], holeZ), false, "left thumbscrew hole is open in the roof"));
                probes.Add(Classify(trayM, (rx, c["y_slot"], holeZ), false, "right thumbscrew hole is open in the roof"));
                probes.Add(Classify(trayM, (0.0, c["y_slot"], holeZ), true, "roof stays solid between the two arm holes"));
                probes.Add(Classify(trayM, (-c["half"], c["y_slot"] - 4.0, c["rail_z0"] + c["rail_h"] / 2), true, "receiver roof is solid above the arm"));
                probes.Add(Classify(trayM, (-c["half"], c["rail_y0"] + 2.0, c["z_arm"]), false, "receiver is open at the rear on the arm centerline"));
                var botZ = c["z_bot0"] + c["bottom_t"] / 2;
                probes.Add(Classify(trayM, (lx, c["y_slot"], botZ), false, "left bottom slot is open under the threaded hole"));
                probes.Add(Classify(trayM, (rx, c["y_slot"], botZ), false, "right bottom slot is open under the threaded hole"));
                probes.Add(Classify(trayM, (0.0, c["y_slot"], botZ), true, "cradle bottom stays solid between the screw slots"));
                probes.Add(Classify(blockM, (-c["half"], c["bore_cy"], c["block_t"] / 2), false, "post bore passes through the collar"));
                probes.Add(Classify(blockM, (-c["half"], 1.2, c["block_t"] / 2), true, "flush heel is solid behind the bore"));
                probes.Add(Classify(blockM, (-c["half"] + c["block_od"] / 2 - 0.6, 0.6, c["block_t"] / 2), false, "post block is round in plan, not a square corner"));
                probes.Add(Classify(blockM, (-c["half"], c["bore_cy"] + c["bore_d"] / 2 + 2.0, c["z_arm"]), true, "arm root is solid in front of the bore"));
                probes.Add(Classify(blockM, (-c["half"], c["slot_y0"] + 8.0, c["z_arm"]), false, "longitudinal slot is open near the arm root"));
                probes.Add(Classify(blockM, (-c["half"], c["y_slot"], c["z_arm"]), false, "left arm slot is open at the preview screw"));
                probes.Add(Classify(blockM, (-c["half"], c["slot_y1"] - 6.0, c["z_arm"]), false, "longitudinal slot is open near the arm tip"));
                probes.Add(Classify(blockM, (-c["half"] + beside, c["slot_y0"] + 8.0, c["z_arm"]), true, "left arm rail is solid beside the slot near the root"));
                probes.Add(Classify(blockM, (-c["half"] + beside, c["y_slot"], c["z_arm"]), true, "left arm rail is solid beside the slot, under the roof"));
                probes.Add(Classify(blockM, (-c["half"] + beside, c["slot_y1"] - 6.0, c["z_arm"]), true, "left arm rail stays at the same height near the tip"));
                probes.Add(Classify(blockM, (-c["half"], c["gusset_y0"] + 0.6, c["arm_thick"] + 6.0), true, "root blend is solid in the collar above the arm"));
                probes.Add(Classify(blockM, (-c["half"], c["gusset_y0"] + 0.6, c["block_t"] + 1.5), false, "root blend does not stand above the collar"));
                probes.Add(Classify(blockM, (-c["half"], c["gusset_y1"] + 2.5, c["arm_thick"] + 3.0), false, "root blend has ended before the slot"));
                probes.Add(Classify(blockM, (-c["half"] - 18.0, c["bore_cy"] - 1.2, 4.0), true, "side blend fills the step beside the collar"));
                probes.Add(Classify(blockM, (-c["half"] + beside, c["bore_cy"] + 80.0, c["z_arm"] + c["arm_thick"] / 2 + 2.0), false, "arm stays flat ahead of the root blends"));
                probes.Add(Classify(rightM, (c["half"], c["y_slot"], c["z_arm"]), false, "right arm slot is open on its own screw, not the left screw"));
                probes.Add(Classify(rightM, (c["half"] - beside, c["y_slot"], c["z_arm"]), true, "right arm is solid beside its slot"));
                probes.Add(Classify(couponM, (0.0, c["bore_cy"], c["block_t"] / 2), false, "fit coupon bore is open"));
                probes.Add(Classify(couponM, (c["block_od"] / 2 - 1.2, c["bore_cy"], c["block_t"] / 2), true, "fit coupon wall surrounds the bore"));
                probes.Add(Classify(screwM, (0.0, 0.0, -2.0), true, "thumbscrew head is solid"));
                probes.Add(Classify(screwM, (c["screw_head_d"] / 2 + 2.0, 0.0, -4.0), true, "thumbscrew wing is solid"));
                probes.Add(Classify(screwM, (0.0, 0.0, 6.0), true, "thumbscrew shank core is solid"));

                var bodyHits = 0;
                var tipHits = 0;
                var crestSamples = 0;
                var bodyR = c["screw_crest_r"] - 0.45;
                var tipR = c["screw_crest_r"] - 0.12;
                foreach (var z in new[] { 4.0, 8.0, 14.0 })
                {
                    for (var deg = 0; deg < 360; deg += 10)
                    {
                        crestSamples++;
                        var rad = deg * Math.PI / 180.0;
                        if (Inside(screwM, (bodyR * Math.Cos(rad), bodyR * Math.Sin(rad), z))) bodyHits++;
                        if (Inside(screwM, (tipR * Math.Cos(rad), tipR * Math.Sin(rad), z))) tipHits++;
                    }
                }
                Expect(bodyHits >= 4, "external thread tooth is missing from the shank");
                Expect(tipHits >= 1, "external thread does not reach the major diameter");
                probes.Add("solid  external thread on " + bodyHits + " of " + crestSamples + " tooth samples, major-diameter crest on " + tipHits);

                var narrowX = c["phone_length_min"] / 2 - 0.5;
                var narrowZ = c["z_pocket0"] + Math.Min(c["phone_width_min"], c["pocket_z"]) - 0.8;
                probes.Add(Classify(trayM, (narrowX, phoneY, narrowZ), false, "minimum-length phone corner fits in the pocket"));
                notes.AddRange(probes);
            }
            finally
            {
                try { tmp.Delete(true); } catch { /* ignore */ }
            }
        }
        catch (Fail ex)
        {
            Console.Error.WriteLine("FAIL: " + ex.Message);
            return 1;
        }

        WriteReport(checks!, stats!, notes);
        Console.WriteLine(
            "PASS  horizontal arms " + F0(checks!["arm_length"]) + " mm  " +
            "pocket " + F1(checks["pocket_x"]) + "x" + F1(checks["pocket_z"]) + "x" + F1(checks["pocket_y"]) + " mm  " +
            "reach " + F1(checks["standout_y"]) + " mm");
        Console.WriteLine("report " + Report);
        return 0;
    }

    private static string LocateMountRoot(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
            if (args[i] is "--root" or "-r")
                return Path.GetFullPath(args[i + 1]);

        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "artifacts", "hardware", "headrest-phone-mount");
            if (File.Exists(Path.Combine(candidate, "headrest-phone-mount.scad")))
                return candidate;
            var here = Path.Combine(dir.FullName, "headrest-phone-mount.scad");
            if (File.Exists(here))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new Fail("Could not locate artifacts/hardware/headrest-phone-mount (pass --root).");
    }

    private static List<string> OpenScad(string part, string dest)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        var psi = new ProcessStartInfo
        {
            FileName = "openscad",
            ArgumentList = { "-o", dest, "--export-format", "binstl", "-D", "part=\"" + part + "\"", Scad },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        using var proc = Process.Start(psi) ?? throw new Fail("failed to start openscad");
        // Drain stdout and stderr concurrently to avoid pipe deadlock.
        var stdoutTask = proc.StandardOutput.ReadToEndAsync();
        var stderrTask = proc.StandardError.ReadToEndAsync();
        proc.WaitForExit();
        var text = stdoutTask.GetAwaiter().GetResult() + stderrTask.GetAwaiter().GetResult();
        if (part.StartsWith("fitcheck", StringComparison.Ordinal))
        {
            if (!text.Contains("Current top level object is empty.", StringComparison.Ordinal))
                throw new Fail(part + " found an interference or did not run:\n" + Tail(text, 800));
            return text.Split('\n').ToList();
        }
        if (proc.ExitCode != 0)
            throw new Fail("OpenSCAD failed for " + part + " (" + proc.ExitCode + "):\n" + Tail(text, 1200));
        return text.Split('\n').ToList();
    }

    private static Dictionary<string, double> ParseChecks(IEnumerable<string> lines)
    {
        var checks = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var line in lines)
        {
            if (!line.Contains("ECHO: \"CHECK ", StringComparison.Ordinal))
                continue;
            var payload = line.Split("CHECK ", 2)[1].TrimEnd('\r', '"');
            var parts = payload.Split('=', 2);
            checks[parts[0]] = double.Parse(parts[1], CultureInfo.InvariantCulture);
        }
        if (!checks.ContainsKey("arm_length"))
            throw new Fail("OpenSCAD did not emit CHECK lines");
        return checks;
    }

    private readonly record struct Vec3(double X, double Y, double Z);
    private readonly record struct Tri(Vec3 A, Vec3 B, Vec3 C);
    private sealed record MeshStats(int Tris, double Volume, int OpenEdges, Vec3 Min, Vec3 Max);

    private static List<Tri> LoadStl(string path)
    {
        var data = File.ReadAllBytes(path);
        if (data.Length >= 5 && Encoding.ASCII.GetString(data, 0, 5).ToLowerInvariant() == "solid"
            && Encoding.ASCII.GetString(data, 0, Math.Min(200, data.Length)).Contains("facet", StringComparison.Ordinal))
        {
            var verts = new List<Vec3>();
            foreach (var line in File.ReadAllLines(path))
            {
                if (!line.Contains("vertex", StringComparison.Ordinal)) continue;
                var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                verts.Add(new Vec3(
                    double.Parse(parts[^3], CultureInfo.InvariantCulture),
                    double.Parse(parts[^2], CultureInfo.InvariantCulture),
                    double.Parse(parts[^1], CultureInfo.InvariantCulture)));
            }
            var tris = new List<Tri>();
            for (var i = 0; i + 2 < verts.Count; i += 3)
                tris.Add(new Tri(verts[i], verts[i + 1], verts[i + 2]));
            return tris;
        }

        var count = BitConverter.ToInt32(data, 80);
        var list = new List<Tri>(count);
        var off = 84;
        for (var i = 0; i < count; i++)
        {
            // skip normal (3 floats), then 9 floats for vertices, then 2 byte attr
            float F() { var v = BitConverter.ToSingle(data, off); off += 4; return v; }
            F(); F(); F(); // normal
            var a = new Vec3(F(), F(), F());
            var b = new Vec3(F(), F(), F());
            var c = new Vec3(F(), F(), F());
            off += 2;
            list.Add(new Tri(a, b, c));
        }
        return list;
    }

    private static MeshStats MeshStatsOf(List<Tri> tris)
    {
        var edges = new Dictionary<(string, string), int>();
        var xs = new List<double>();
        var ys = new List<double>();
        var zs = new List<double>();
        double volume = 0;
        string Key(Vec3 p) =>
            (Math.Round(p.X, 4) + 0.0).ToString(CultureInfo.InvariantCulture) + "," +
            (Math.Round(p.Y, 4) + 0.0).ToString(CultureInfo.InvariantCulture) + "," +
            (Math.Round(p.Z, 4) + 0.0).ToString(CultureInfo.InvariantCulture);

        foreach (var tri in tris)
        {
            var a = tri.A; var b = tri.B; var c = tri.C;
            volume += a.X * (b.Y * c.Z - b.Z * c.Y) - a.Y * (b.X * c.Z - b.Z * c.X) + a.Z * (b.X * c.Y - b.Y * c.X);
            foreach (var p in new[] { a, b, c })
            {
                xs.Add(p.X); ys.Add(p.Y); zs.Add(p.Z);
            }
            void Edge(Vec3 u, Vec3 v)
            {
                var ku = Key(u); var kv = Key(v);
                var k = string.CompareOrdinal(ku, kv) <= 0 ? (ku, kv) : (kv, ku);
                edges[k] = edges.TryGetValue(k, out var n) ? n + 1 : 1;
            }
            Edge(a, b); Edge(b, c); Edge(c, a);
        }
        return new MeshStats(
            tris.Count,
            Math.Abs(volume) / 6.0,
            edges.Values.Count(n => n != 2),
            new Vec3(xs.Min(), ys.Min(), zs.Min()),
            new Vec3(xs.Max(), ys.Max(), zs.Max()));
    }

    private static bool RayHits(Vec3 origin, Vec3 direction, Tri tri)
    {
        const double eps = 1e-8;
        var ox = origin.X; var oy = origin.Y; var oz = origin.Z;
        var dx = direction.X; var dy = direction.Y; var dz = direction.Z;
        var ax = tri.A.X; var ay = tri.A.Y; var az = tri.A.Z;
        var bx = tri.B.X; var by = tri.B.Y; var bz = tri.B.Z;
        var cx = tri.C.X; var cy = tri.C.Y; var cz = tri.C.Z;
        var abx = bx - ax; var aby = by - ay; var abz = bz - az;
        var acx = cx - ax; var acy = cy - ay; var acz = cz - az;
        var px = dy * acz - dz * acy;
        var py = dz * acx - dx * acz;
        var pz = dx * acy - dy * acx;
        var det = abx * px + aby * py + abz * pz;
        if (Math.Abs(det) < eps) return false;
        var inv = 1.0 / det;
        var sx = ox - ax; var sy = oy - ay; var sz = oz - az;
        var u = (sx * px + sy * py + sz * pz) * inv;
        if (u < -eps || u > 1 + eps) return false;
        var qx = sy * abz - sz * aby;
        var qy = sz * abx - sx * abz;
        var qz = sx * aby - sy * abx;
        var v = (dx * qx + dy * qy + dz * qz) * inv;
        if (v < -eps || u + v > 1 + eps) return false;
        var t = (acx * qx + acy * qy + acz * qz) * inv;
        return t > eps;
    }

    private static bool OddHit(List<Tri> tris, Vec3 origin, Vec3 direction)
    {
        var hits = 0;
        foreach (var tri in tris)
            if (RayHits(origin, direction, tri)) hits++;
        return hits % 2 == 1;
    }

    private static bool Inside(List<Tri> tris, (double X, double Y, double Z) point)
    {
        var directions = new (double, double, double)[]
        {
            (0.31, 0.17, 1.0), (-0.22, 0.28, 1.0), (0.19, -0.27, 1.0), (1.0, 0.13, 0.21), (0.11, 1.0, 0.18),
        };
        var votes = 0;
        foreach (var d in directions)
        {
            var origin = new Vec3(point.X + d.Item1 * 0.01, point.Y + d.Item2 * 0.01, point.Z + d.Item3 * 0.01);
            if (OddHit(tris, origin, new Vec3(d.Item1, d.Item2, d.Item3))) votes++;
        }
        return votes >= 3;
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition) throw new Fail(message);
    }

    private static string Classify(List<Tri> tris, (double X, double Y, double Z) point, bool shouldBeSolid, string label)
    {
        var solid = Inside(tris, point);
        if (solid != shouldBeSolid)
        {
            var kind = solid ? "solid" : "air";
            var want = shouldBeSolid ? "solid" : "air";
            throw new Fail(label + ": point (" +
                Math.Round(point.X, 2).ToString(CultureInfo.InvariantCulture) + ", " +
                Math.Round(point.Y, 2).ToString(CultureInfo.InvariantCulture) + ", " +
                Math.Round(point.Z, 2).ToString(CultureInfo.InvariantCulture) + ") is " + kind + ", expected " + want);
        }
        return (shouldBeSolid ? "solid" : "air") + "  " + label;
    }

    private static void WriteReport(Dictionary<string, double> checks, Dictionary<string, MeshStats> stats, List<string> notes)
    {
        var c = checks;
        var lines = new List<string>
        {
            "# Geometry measurement log - ART-RIDE-MOUNT-001",
            "",
            "Generated by `dotnet run --project tools/RideAudit.HeadrestGeometry` from `headrest-phone-mount.scad`.",
            "This is a CAD measurement, not an on-vehicle print. Physical fit stays on the operator checklist.",
            "",
            "License: GPL-2.0",
            "",
            "## Envelope",
            "",
            "| Check | Value |",
            "| --- | --- |",
            "| Post spacing | " + F0(c["post_spacing_min"]) + " - " + F0(c["post_spacing_max"]) + " mm center-to-center; preview " + F0(c["post_spacing"]) + " mm |",
            "| Post block | one collar, bore " + F1(c["bore_id"]) + " mm, outside " + F1(c["block_od"]) + " mm, " + F0(c["block_t"]) + " mm thick, wall " + F1(c["block_wall"]) + " mm |",
            "| Default post | " + F1(c["post_od"]) + " mm OD, clearance " + F1(c["post_clearance"]) + " mm, bore " + F1(c["bore_id"]) + " mm |",
            "| Arm length | " + F0(c["arm_length"]) + " mm from the post axis, horizontal (rise " + F0(c["arm_rise"]) + ") |",
            "| Arm section | " + F0(c["arm_width"]) + " x " + F0(c["arm_thick"]) + " mm |",
            "| Arm slot, along the arm | " + F0(c["slot_len"]) + " mm long, " + F1(c["slot_span"]) + " mm wide (" + F2(c["slot_side_gap"]) + " mm each side of the crest), from " + F0(c["slot_y0"]) + " to " + F0(c["slot_y1"]) + " mm forward of the pad |",
            "| Depth adjustment behind the preview screw | " + F0(c["y_slot"] - c["slot_y0"]) + " mm |",
            "| Cradle bottom slots | " + F0(c["slot_gap"]) + " mm along the arm, " + F1(c["hole_clear"]) + " mm wide, one under each arm |",
            "| Thread | M" + F0(c["screw_major"]) + "x" + F2(c["screw_pitch"]) + " external, crest " + F1(c["screw_major"]) + " mm, engagement " + F0(c["screw_engage"]) + " mm when seated |",
            "| Roof holes | " + F0(c["hole_nx"]) + ", one on each arm centerline, tap drill " + F1(c["hole_tap"]) + " mm |",
            "| Clamp stack | bottom plate " + F0(c["bottom_t"]) + " mm + arm " + F0(c["arm_thick"]) + " mm = " + F0(c["clamp_stack"]) + " mm; slide take-up " + F2(c["clamp_takeup"]) + " mm |",
            "| Thumbscrews | " + F0(c["screw_count"]) + " modeled, head " + F0(c["screw_head_d"]) + " mm, shank " + F1(c["screw_shank_l"]) + " mm under the face |",
            "| Phone pocket (L x short side x thickness) | " + F1(c["pocket_x"]) + " x " + F1(c["pocket_z"]) + " x " + F1(c["pocket_y"]) + " mm |",
            "| Phone envelope | length " + F0(c["phone_length_min"]) + "-" + F0(c["phone_length_max"]) + " mm, short side " + F0(c["phone_width_min"]) + "-" + F0(c["phone_width_max"]) + " mm, thickness <= " + F0(c["phone_thickness_max"]) + " mm |",
            "| Camera window | " + F0(c["camera_clearance"]) + " mm square, both upper corners, through the back plate |",
            "| Phone front from the headrest face | " + F1(c["standout_y"]) + " mm |",
            "| Cradle top above the block bottom | " + F1(c["z_pocket1"]) + " mm |",
            "| Cradles | " + F0(c["cradle_count"]) + " shared landscape holder |",
            "",
            "## Print meshes",
            "",
            "| STL | Triangles | Volume mm3 | Size XxYxZ mm |",
            "| --- | --- | --- | --- |",
        };
        foreach (var (name, st) in stats)
        {
            var sx = st.Max.X - st.Min.X;
            var sy = st.Max.Y - st.Min.Y;
            var sz = st.Max.Z - st.Min.Z;
            lines.Add("| `" + name + "` | " + st.Tris + " | " + st.Volume.ToString("0", CultureInfo.InvariantCulture) + " | " +
                      sx.ToString("0.0", CultureInfo.InvariantCulture) + " x " +
                      sy.ToString("0.0", CultureInfo.InvariantCulture) + " x " +
                      sz.ToString("0.0", CultureInfo.InvariantCulture) + " |");
        }
        lines.Add("");
        lines.Add("## Probe results");
        lines.Add("");
        foreach (var note in notes)
            lines.Add("- " + note);
        lines.Add("");
        Directory.CreateDirectory(Path.GetDirectoryName(Report)!);
        File.WriteAllText(Report, string.Join("\n", lines), new UTF8Encoding(false));
    }

    private static string F0(double v) => v.ToString("0", CultureInfo.InvariantCulture);
    private static string F1(double v) => v.ToString("0.0", CultureInfo.InvariantCulture);
    private static string F2(double v) => v.ToString("0.00", CultureInfo.InvariantCulture);
    private static string Tail(string text, int n) => text.Length <= n ? text : text[^n..];
}
