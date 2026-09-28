// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RideAudit.Shared.Ui.Views;

public partial class CaptureShellView : UserControl
{
    private string? _role;

    public CaptureShellView()
    {
        InitializeComponent();
        DriverButton.Click += OnDriver;
        PassengerButton.Click += OnPassenger;
        DiscoverButton.Click += OnDiscover;
        StartButton.Click += OnStart;
        StopButton.Click += OnStop;
    }

    public void SelectDriver() => OnDriver(null, new RoutedEventArgs());

    public void SelectPassenger() => OnPassenger(null, new RoutedEventArgs());

    public void Discover() => OnDiscover(null, new RoutedEventArgs());

    public void StartSession() => OnStart(null, new RoutedEventArgs());

    public void StopSession() => OnStop(null, new RoutedEventArgs());

    public void ShowFailClosed(string message)
    {
        ScreenId.Text = "WF-08";
        FailClosedText.IsVisible = true;
        FailClosedText.Text = message;
        StartButton.IsEnabled = false;
        StopButton.IsEnabled = false;
    }

    private void OnDriver(object? sender, RoutedEventArgs e)
    {
        _role = "driver";
        ScreenId.Text = "WF-04";
        PairingStatus.Text = "Role confirmed: driver coordinator";
    }

    private void OnPassenger(object? sender, RoutedEventArgs e)
    {
        _role = "passenger";
        ScreenId.Text = "WF-05";
        PairingStatus.Text = "Role confirmed: passenger compositor";
        SpiderGraph.Text = "Spider graph armed for telematics overlay";
    }

    private void OnDiscover(object? sender, RoutedEventArgs e)
    {
        ScreenId.Text = "WF-02";
        if (_role is null)
        {
            ShowFailClosed("Confirm a role before Bluetooth pairing.");
            return;
        }

        PairingStatus.Text = "RideAudit Bluetooth discovery. No Lyft private API.";
    }

    private void OnStart(object? sender, RoutedEventArgs e)
    {
        if (_role != "driver")
        {
            ShowFailClosed("Only the driver phone may start the session.");
            return;
        }

        ScreenId.Text = "WF-04";
        ClockText.Text = "Driver session clock master";
        StartButton.IsEnabled = false;
        StopButton.IsEnabled = true;
        SealStatus.Text = "Seal waits for Play Integrity and capture";
    }

    private void OnStop(object? sender, RoutedEventArgs e)
    {
        if (_role != "driver")
        {
            ShowFailClosed("Only the driver phone may stop the session.");
            return;
        }

        ScreenId.Text = "WF-06";
        StopButton.IsEnabled = false;
        SealStatus.Text = "Seal-at-collect in progress";
        SubmitStatus.Text = "Driver coordinates sealed submission";
    }
}
