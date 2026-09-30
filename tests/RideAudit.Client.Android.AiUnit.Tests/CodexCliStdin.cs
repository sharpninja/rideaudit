// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Runtime.InteropServices;

namespace RideAudit.Client.Android.AiUnit.Tests;

public static class CodexCliStdin
{
    private static int _detached;

    public static string ShimDirectory => Path.Combine(AppContext.BaseDirectory, "codex-shim");

    public static void PrepareForExec()
    {
        var shim = Path.Combine(ShimDirectory, "codex.exe");
        if (!File.Exists(shim))
        {
            throw new InvalidOperationException(
                "codex shim is missing at " + shim + ". The aiUnit test build copies tests/RideAudit.CodexShim into the output codex-shim directory.");
        }

        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var prefix = ShimDirectory + Path.PathSeparator;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            Environment.SetEnvironmentVariable("PATH", prefix + path);
        }

        if (!OperatingSystem.IsWindows() || Volatile.Read(ref _detached) == 1)
        {
            return;
        }

        var nul = CreateFileW("NUL", 0x80000000, 1, IntPtr.Zero, 3, 0, IntPtr.Zero);
        if (nul == IntPtr.Zero || nul == new IntPtr(-1))
        {
            throw new InvalidOperationException(
                "Could not open NUL for codex exec standard input. An open stdin pipe makes codex exec wait for EOF.");
        }

        if (!SetStdHandle(-10, nul))
        {
            throw new InvalidOperationException(
                "Could not point standard input at NUL before codex exec. An open stdin pipe makes codex exec wait for EOF.");
        }

        Volatile.Write(ref _detached, 1);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetStdHandle(int nStdHandle, IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateFileW(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);
}
