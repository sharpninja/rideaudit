// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia.Controls;
using Avalonia.Interactivity;
using RideAudit.Bt;

namespace RideAudit.Shared.Ui.Views;

public partial class CaptureShellView : UserControl
{
    private readonly IDiscoveryBus _bus;
    private readonly RideAudit.Shared.Ui.CaptureRuntime? _runtime;
    private string? _role;

    public CaptureShellView()
        : this(new UnavailableDiscoveryBus(), productionEntry: false)
    {
    }

    public CaptureShellView(IDiscoveryBus bus)
        : this(bus, productionEntry: false)
    {
    }

    public CaptureShellView(RideAudit.Shared.Ui.CaptureRuntime runtime)
        : this(runtime.Graph.Discovery, productionEntry: false)
    {
        _runtime = runtime;
    }

    public CaptureShellView(IDiscoveryBus bus, bool productionEntry)
    {
        _bus = bus;
        InitializeComponent();
        DriverButton.Click += OnDriver;
        PassengerButton.Click += OnPassenger;
        DiscoverButton.Click += OnDiscover;
        StartButton.Click += OnStart;
        StopButton.Click += OnStop;
        if (productionEntry && !_bus.RadioAvailable)
        {
            ShowFailClosed(
                "BT_DISABLED: Production capture composition has no radio. This is not a silent pairing success.");
        }
    }

    public static CaptureShellView CreateUncomposedRefuse()
    {
        var view = new CaptureShellView(new UnavailableDiscoveryBus(), productionEntry: true);
        view.ShowFailClosed(
            "PRODUCTION_UNAVAILABLE: Capture composition was not installed. The APK will not default to a silent UnavailableDiscoveryBus success.");
        return view;
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

    public void ShowUnavailableBanner(string message)
    {
        FailClosedText.IsVisible = true;
        FailClosedText.Text = message;
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

        if (!_bus.RadioAvailable)
        {
            ShowFailClosed("BT_DISABLED: No Bluetooth radio is available on this host. No Lyft private API.");
            return;
        }

        PairingStatus.Text = "RideAudit Bluetooth discovery on " + _bus.TransportKind + ". No Lyft private API.";
    }

    private void OnStart(object? sender, RoutedEventArgs e)
    {
        if (_role != "driver")
        {
            ShowFailClosed("Only the driver phone may start the session.");
            return;
        }

        if (_runtime is not null)
        {
            var result = _runtime.Start(_role);
            if (!result.Ok)
            {
                ShowFailClosed(result.Display);
                return;
            }

            ScreenId.Text = "WF-04";
            ClockText.Text = "Driver session clock master";
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            SealStatus.Text = "Camera and Play gate armed. Seal waits for stop.";
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

        if (_runtime is not null)
        {
            var result = _runtime.StopAndSubmit();
            ScreenId.Text = "WF-06";
            StopButton.IsEnabled = false;
            if (!result.Ok)
            {
                ShowFailClosed(result.Display);
                return;
            }

            SealStatus.Text = "Seal-at-collect completed";
            SubmitStatus.Text = result.Message ?? "Driver coordinated sealed submission";
            return;
        }

        ScreenId.Text = "WF-06";
        StopButton.IsEnabled = false;
        SealStatus.Text = "Seal-at-collect in progress";
        SubmitStatus.Text = "Driver coordinates sealed submission";
    }
}
