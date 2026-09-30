// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Xunit;

namespace RideAudit.Client.Android.AiUnit.Tests;

public sealed class DeviceSerialChoiceTests
{
    [Fact]
    public void Empty_device_list_fails_closed()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => AdbDeviceSession.ChooseLine([], null));
        Assert.Contains("No adb device", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Default_choice_stays_on_the_Fold_when_both_phones_are_attached()
    {
        var rows = new[]
        {
            "adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp device product:avatrn_g model:motorola_edge_2024 device:avatrn",
            "RFCW7078MVZ device product:q4qsqw model:SM_F936U device:q4q",
        };

        var chosen = AdbDeviceSession.ChooseLine(rows, null);
        Assert.StartsWith("RFCW7078MVZ ", chosen, StringComparison.Ordinal);
    }

    [Fact]
    public void Requested_Edge_serial_does_not_fall_back_to_the_Fold()
    {
        var rows = new[]
        {
            "RFCW7078MVZ device product:q4qsqw model:SM_F936U device:q4q",
            "adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp device product:avatrn_g model:motorola_edge_2024 device:avatrn",
        };

        var chosen = AdbDeviceSession.ChooseLine(rows, "adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp");
        Assert.StartsWith("adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp ", chosen, StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_requested_serial_fails_closed()
    {
        var rows = new[] { "RFCW7078MVZ device product:q4qsqw model:SM_F936U device:q4q" };
        var ex = Assert.Throws<InvalidOperationException>(() =>
            AdbDeviceSession.ChooseLine(rows, "adb-ZD222QH58Q-DiWqEr._adb-tls-connect._tcp"));
        Assert.Contains("not an attached device", ex.Message, StringComparison.Ordinal);
    }
}
