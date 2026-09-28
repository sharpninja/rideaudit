// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using System.Linq;
using Avalonia.Controls;
using RideAudit.Viewer;

namespace RideAudit.Shared.Ui.Views;

public partial class ReviewShellView : UserControl
{
    public ReviewShellView()
    {
        InitializeComponent();
    }

    public void ShowOutcome(ReviewOutcome outcome)
    {
        DecryptButton.IsEnabled = outcome.DecryptAllowed;
        PlaybackButton.IsEnabled = outcome.DisplayAllowed;
        ChecksList.ItemsSource = outcome.Report.Checks.Select(check => check.RecordId + " " + check.Name + " " + check.Status).ToList();
        if (!outcome.DecryptAllowed)
        {
            ScreenId.Text = outcome.Phase == ReviewPhase.EscrowRequired ? "WF-R-05" : "WF-R-04";
            FailClosedText.IsVisible = true;
            FailClosedText.Text = outcome.BlockReason ?? "Verification failed closed.";
            TimelineText.Text = "Timeline locked until verification and escrow release.";
            return;
        }

        ScreenId.Text = "WF-R-06";
        FailClosedText.IsVisible = false;
        var timeline = outcome.Timelines.FirstOrDefault();
        QuorumText.Text = "Court release accepted. Working copy expires.";
        TimelineText.Text = timeline is null
            ? "Timeline empty"
            : "Sync " + timeline.SyncClockOffset
              + " composite=" + timeline.Composite.Present
              + " spider=" + timeline.Spider.Present
              + " gps=" + (timeline.Gps.Present ? "present" : timeline.Gps.AbsenceReason)
              + " obd2=" + (timeline.Obd2.Present ? "present" : timeline.Obd2.AbsenceReason);
        GapText.Text = "Lyft-native signals are Unverified when absent. No private Lyft API is used.";
    }
}
