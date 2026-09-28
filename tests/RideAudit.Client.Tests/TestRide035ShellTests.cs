// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using RideAudit.Client.Contracts;
using RideAudit.Client.Tests.Support;
using RideAudit.Shared.Ui;
using RideAudit.Shared.Ui.Views;
using RideAudit.Viewer;
using Xunit;

namespace RideAudit.Client.Tests;

[Trait("Partition", "TEST-RIDE-035")]
public class TestRide035ShellTests
{
    [Fact]
    [Trait("FR", "FR-RIDE-056")]
    [Trait("AC", "AC-RIDE-056-002")]
    public void Capture_projects_do_not_reference_a_second_ui_framework()
    {
        var root = Repo.Root();
        foreach (var relative in new[]
        {
            "src/RideAudit.Shared.Ui/RideAudit.Shared.Ui.csproj",
            "src/RideAudit.Client.Android/RideAudit.Client.Android.csproj",
            "src/RideAudit.Client.Desktop/RideAudit.Client.Desktop.csproj",
        })
        {
            var text = File.ReadAllText(Path.Combine(root, relative));
            Assert.DoesNotContain("Microsoft.Maui", text);
            Assert.DoesNotContain("Xamarin.Forms", text);
            Assert.DoesNotContain("Uno.WinUI", text);
        }

        var packages = File.ReadAllText(Path.Combine(root, "Directory.Packages.props"));
        Assert.Contains("Avalonia\" Version=\"12.1.3\"", packages);
        Assert.Contains("Avalonia.Android\" Version=\"12.1.3\"", packages);
        Assert.Contains("Avalonia.Desktop\" Version=\"12.1.3\"", packages);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-056")]
    [Trait("AC", "AC-RIDE-056-001")]
    public void Android_host_is_avalonia_12_for_both_roles()
    {
        var root = Repo.Root();
        var csproj = File.ReadAllText(Path.Combine(root, "src/RideAudit.Client.Android/RideAudit.Client.Android.csproj"));
        Assert.Contains("net10.0-android", csproj);
        Assert.Contains("org.rideaudit.app", csproj);
        Assert.Contains("Avalonia.Android", csproj);
        var activity = File.ReadAllText(Path.Combine(root, "src/RideAudit.Client.Android/MainActivity.cs"));
        Assert.Contains("AvaloniaMainActivity", activity);
        Assert.Contains("ShellMode.Capture", File.ReadAllText(Path.Combine(root, "src/RideAudit.Client.Android/Application.cs")));
    }

    [Fact]
    [Trait("FR", "FR-RIDE-057")]
    [Trait("AC", "AC-RIDE-057-001")]
    public void Desktop_host_builds_with_avalonia_desktop()
    {
        var program = File.ReadAllText(Path.Combine(Repo.Root(), "src/RideAudit.Client.Desktop/Program.cs"));
        Assert.Contains("UsePlatformDetect", program);
        Assert.Contains("ShellMode.Review", program);
        Assert.Contains("Avalonia UI 12", UiLicense.Framework);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-057")]
    [Trait("AC", "AC-RIDE-057-002")]
    public void Shared_review_shell_keeps_decrypt_disabled_until_allowed()
    {
        var view = new ReviewShellView();
        Assert.False(view.FindControl<Button>("DecryptButton")!.IsEnabled);
        Assert.False(view.FindControl<Button>("PlaybackButton")!.IsEnabled);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-058")]
    [Trait("AC", "AC-RIDE-058-001")]
    public void Shared_ui_notice_is_gpl()
    {
        Assert.Contains("GPL-2.0-or-later", UiLicense.Notice);
        Assert.Contains("not relicensed MIT or Apache-2.0", UiLicense.Notice);
        var notice = File.ReadAllText(Path.Combine(Repo.Root(), "NOTICE"));
        Assert.Contains("GPL-2.0-or-later", notice);
    }

    [Fact]
    [Trait("FR", "FR-RIDE-058")]
    [Trait("AC", "AC-RIDE-058-002")]
    public void Both_hosts_reference_shared_ui()
    {
        var root = Repo.Root();
        Assert.Contains("RideAudit.Shared.Ui.csproj", File.ReadAllText(Path.Combine(root, "src/RideAudit.Client.Android/RideAudit.Client.Android.csproj")));
        Assert.Contains("RideAudit.Shared.Ui.csproj", File.ReadAllText(Path.Combine(root, "src/RideAudit.Client.Desktop/RideAudit.Client.Desktop.csproj")));
    }

    [Fact]
    public void Interim_grpc_client_stub_is_generated_and_isolated()
    {
        var generated = typeof(InterimInProcessAdmissionClient).Assembly.GetType("RideAudit.V1.SealedAdmission+SealedAdmissionClient");
        Assert.NotNull(generated);
        Assert.Equal("interim-companion", ContractProvenance.Source);
        Assert.Equal("src/RideAudit.Protos", ContractProvenance.SwapTarget);
        Assert.Equal("non-authoritative", ContractProvenance.OpenApiAuthority);
        Assert.False(Directory.Exists(Path.Combine(Repo.Root(), "src", "RideAudit.Protos")));
    }

    [Fact]
    public void Historical_kotlin_is_archived()
    {
        var root = Repo.Root();
        Assert.True(File.Exists(Path.Combine(root, "artifacts/android/legacy-kotlin/app/src/main/java/org/rideaudit/app/MainActivity.kt")));
        Assert.False(Directory.Exists(Path.Combine(root, "artifacts/android/app")));
    }

    [Fact]
    public void Client_source_does_not_call_lyft_private_apis()
    {
        var src = Path.Combine(Repo.Root(), "src");
        var hits = Directory.GetFiles(src, "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cs") || path.EndsWith(".axaml") || path.EndsWith(".proto"))
            .Where(path => File.ReadAllText(path).Contains("api.lyft.com", StringComparison.OrdinalIgnoreCase))
            .ToList();
        Assert.Empty(hits);
    }

    [AvaloniaFact]
    public void Capture_shell_roles_fail_closed_for_passenger_start()
    {
        var view = new CaptureShellView();
        var window = new Window { Width = 400, Height = 800, Content = view };
        window.Show();
        Assert.Equal("WF-01", view.FindControl<TextBlock>("ScreenId")!.Text);
        Assert.Contains("GPL-2.0-or-later", new MainView().FindControl<TextBlock>("LicenseNotice")!.Text);
        view.SelectPassenger();
        view.StartSession();
        Assert.Equal("WF-08", view.FindControl<TextBlock>("ScreenId")!.Text);
        Assert.Contains("driver", view.FindControl<TextBlock>("FailClosedText")!.Text, StringComparison.OrdinalIgnoreCase);
        view.SelectDriver();
        view.Discover();
        Assert.Contains("No Lyft private API", view.FindControl<TextBlock>("PairingStatus")!.Text);
        view.StartSession();
        Assert.Contains("clock master", view.FindControl<TextBlock>("ClockText")!.Text, StringComparison.OrdinalIgnoreCase);
        view.StopSession();
        Assert.Equal("WF-06", view.FindControl<TextBlock>("ScreenId")!.Text);
    }

    [AvaloniaFact]
    public void Review_shell_fail_closed_blocks_decrypt_and_playback()
    {
        var fixture = Fixtures.CaptureHappy();
        var outcome = Fixtures.ViewerFor(fixture).Review(new RideBundle
        {
            BundleId = fixture.Bundle.BundleId,
            CaseId = fixture.Bundle.CaseId,
            VehicleId = fixture.Bundle.VehicleId,
            Records =
            [
                new ReviewRecord
                {
                    Sealed = fixture.Bundle.Records[0].Sealed,
                    Ots = fixture.Bundle.Records[0].Ots,
                    Admitted = false,
                },
            ],
        }, "counsel", Fixtures.Release(fixture.Clock));
        var view = new ReviewShellView();
        var window = new Window { Width = 800, Height = 600, Content = view };
        window.Show();
        view.ShowOutcome(outcome);
        Assert.False(view.FindControl<Button>("DecryptButton")!.IsEnabled);
        Assert.False(view.FindControl<Button>("PlaybackButton")!.IsEnabled);
        Assert.Equal("WF-R-04", view.FindControl<TextBlock>("ScreenId")!.Text);
    }
}
