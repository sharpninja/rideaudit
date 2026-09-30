// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Diagnostics;
using System.Text.RegularExpressions;

if (args.Length == 1 && args[0] == "--shim-probe")
{
    Console.WriteLine("rideaudit-codex-shim");
    return 0;
}

if (args.Length > 0 && args[0] == "--shim-dump")
{
    foreach (var arg in Rewrite(args.Skip(1).ToArray()))
    {
        Console.WriteLine(arg);
    }

    return 0;
}

var real = FindRealCodex();
if (real is null)
{
    Console.Error.WriteLine("codex shim could not find codex.exe on PATH.");
    return 127;
}

var psi = new ProcessStartInfo
{
    FileName = real,
    UseShellExecute = false,
    CreateNoWindow = true,
    RedirectStandardInput = true,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
};

foreach (var arg in Rewrite(args))
{
    psi.ArgumentList.Add(arg);
}

using var process = new Process { StartInfo = psi };
if (!process.Start())
{
    Console.Error.WriteLine("codex shim could not start " + real);
    return 127;
}

process.StandardInput.Close();
var stdout = process.StandardOutput.BaseStream.CopyToAsync(Console.OpenStandardOutput());
var stderr = process.StandardError.BaseStream.CopyToAsync(Console.OpenStandardError());
process.WaitForExit();
Task.WaitAll(stdout, stderr);
return process.ExitCode;

static string[] Rewrite(string[] incoming)
{
    if (incoming.Length == 0 || !string.Equals(incoming[0], "exec", StringComparison.Ordinal))
    {
        return incoming;
    }

    var list = new List<string>(incoming);
    var insertAt = 1;
    InsertOnce(list, ref insertAt, "--dangerously-bypass-hook-trust");
    InsertOnce(list, ref insertAt, "--dangerously-bypass-approvals-and-sandbox");
    if (!list.Contains("features.hooks=false", StringComparer.Ordinal))
    {
        list.Insert(insertAt, "-c");
        insertAt++;
        list.Insert(insertAt, "features.hooks=false");
        insertAt++;
    }

    foreach (var arg in incoming)
    {
        foreach (Match match in Regex.Matches(arg, @"Image attachment \([^)]*\): (?<path>.+)"))
        {
            var imagePath = match.Groups["path"].Value.Trim();
            if (imagePath.Length == 0)
            {
                continue;
            }

            list.Add("-i");
            list.Add(imagePath);
        }
    }

    return list.ToArray();
}

static void InsertOnce(List<string> list, ref int insertAt, string flag)
{
    if (list.Contains(flag, StringComparer.Ordinal))
    {
        return;
    }

    list.Insert(insertAt, flag);
    insertAt++;
}

static string? FindRealCodex()
{
    var selfDir = Path.GetDirectoryName(Environment.ProcessPath);
    var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
    foreach (var entry in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
    {
        string fullDir;
        try
        {
            fullDir = Path.GetFullPath(entry.Trim());
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            continue;
        }

        if (selfDir is not null && string.Equals(fullDir, Path.GetFullPath(selfDir), StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        var candidate = Path.Combine(fullDir, "codex.exe");
        if (File.Exists(candidate))
        {
            return candidate;
        }
    }

    return null;
}
