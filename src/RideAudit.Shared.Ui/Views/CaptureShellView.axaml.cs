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
        ContinueButton.Click += OnContinue;
        DiscoverButton.Click += OnDiscover;
        PeerButton.Click += OnPeer;
        ConfirmPairButton.Click += OnConfirmPair;
        StartButton.Click += OnStart;
        StopButton.Click += OnStop;
        SubmitButton.Click += OnSubmit;
        ReturnButton.Click += OnReturn;
        Show("WF-01");
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
        Show("WF-08");
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
        DriverButton.Classes.Add("selected");
        PassengerButton.Classes.Remove("selected");
        LocalRoleLine.Text = "Role: DRIVER (coordinator)";
        PeerRoleLine.Text = "Role: PASSENGER (compositor)";
        AdvertiseStatus.Text = "Advertising as: RideAudit-Driver";
        if (ScreenId.Text != "WF-01")
        {
            Show("WF-01");
        }
    }

    private void OnPassenger(object? sender, RoutedEventArgs e)
    {
        _role = "passenger";
        PassengerButton.Classes.Add("selected");
        DriverButton.Classes.Remove("selected");
        LocalRoleLine.Text = "Role: PASSENGER (compositor)";
        PeerRoleLine.Text = "Role: DRIVER (coordinator)";
        AdvertiseStatus.Text = "Scanning as: RideAudit-Passenger";
        SpiderGraph.Text = "Spider graph armed for telematics overlay";
        if (ScreenId.Text != "WF-01")
        {
            Show("WF-01");
        }
    }

    private void OnContinue(object? sender, RoutedEventArgs e) => OnDiscover(sender, e);

    private void OnDiscover(object? sender, RoutedEventArgs e)
    {
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

        Show("WF-02");
        PairingStatus.Text = "RideAudit Bluetooth discovery on " + _bus.TransportKind + ". No Lyft private API.";
    }

    private void OnPeer(object? sender, RoutedEventArgs e)
    {
        if (_role is null)
        {
            ShowFailClosed("Confirm a role before Bluetooth pairing.");
            return;
        }

        Show("WF-03");
    }

    private void OnConfirmPair(object? sender, RoutedEventArgs e)
    {
        if (ConfirmCheck.IsChecked != true)
        {
            ShowFailClosed("PAIR_ROLE_MISMATCH: Confirm the role checkbox before pairing.");
            return;
        }

        if (_role == "passenger")
        {
            Show("WF-05");
            return;
        }

        Show("WF-04");
        PairingStatus.Text = "Role confirmed: driver coordinator";
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

            Show("WF-04");
            ClockText.Text = "Driver session clock master";
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            SealStatus.Text = "Camera and Play gate armed. Seal waits for stop.";
            return;
        }

        Show("WF-04");
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
            Show("WF-06");
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

        Show("WF-06");
        StopButton.IsEnabled = false;
        SealStatus.Text = "Seal-at-collect in progress";
        SubmitStatus.Text = "Driver coordinates sealed submission";
    }

    private void OnSubmit(object? sender, RoutedEventArgs e)
    {
        Show("WF-07");
        if (string.IsNullOrWhiteSpace(SubmitStatus.Text) || SubmitStatus.Text == "No submission")
        {
            SubmitStatus.Text = "Last result: not admitted. Submission id: --";
        }
    }

    private void OnReturn(object? sender, RoutedEventArgs e) => Show("WF-01");

    private void Show(string screenId)
    {
        ScreenId.Text = screenId;
        RolePage.IsVisible = screenId == "WF-01";
        DiscoverPage.IsVisible = screenId == "WF-02";
        PairPage.IsVisible = screenId == "WF-03";
        DriverPage.IsVisible = screenId == "WF-04";
        PassengerPage.IsVisible = screenId == "WF-05";
        SealPage.IsVisible = screenId == "WF-06";
        SubmitPage.IsVisible = screenId == "WF-07";
        FailPage.IsVisible = screenId == "WF-08";
    }
}
