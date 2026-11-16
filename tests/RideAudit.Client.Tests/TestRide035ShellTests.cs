// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using RideAudit.Bt;
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
    [Trait("AC", "AC-TEST-035-001")]
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
    [Trait("AC", "AC-UC-027-001")]
    public void Both_hosts_reference_shared_ui()
    {
        var root = Repo.Root();
        Assert.Contains("RideAudit.Shared.Ui.csproj", File.ReadAllText(Path.Combine(root, "src/RideAudit.Client.Android/RideAudit.Client.Android.csproj")));
        Assert.Contains("RideAudit.Shared.Ui.csproj", File.ReadAllText(Path.Combine(root, "src/RideAudit.Client.Desktop/RideAudit.Client.Desktop.csproj")));
        Assert.Contains("GPL-2.0-or-later", UiLicense.Notice);
    }

    [Fact]
    public void Clients_bind_to_authoritative_protos()
    {
        var contracts = typeof(InterimInProcessAdmissionClient).Assembly;
        Assert.Null(contracts.GetType("RideAudit.V1.SealedAdmission+SealedAdmissionClient"));
        var protosName = contracts.GetReferencedAssemblies().Single(name => name.Name == "RideAudit.Protos");
        var protos = System.Reflection.Assembly.Load(protosName);
        var generated = protos.GetType("RideAudit.Protos.Admission.V1.Admission+AdmissionClient");
        Assert.NotNull(generated);
        Assert.Equal("src/RideAudit.Protos", ContractProvenance.Source);
        Assert.Equal("0.4.0", ContractProvenance.ContractVersion);
        Assert.Equal("src/RideAudit.Protos", ContractProvenance.SwapTarget);
        Assert.Equal("non-authoritative", ContractProvenance.OpenApiAuthority);
        var root = Repo.Root();
        Assert.True(File.Exists(Path.Combine(root, "src/RideAudit.Protos/Protos/rideaudit/admission/v1/admission.proto")));
        Assert.False(File.Exists(Path.Combine(root, "src/RideAudit.Client.Contracts/interim/rideaudit/v1/custody.proto")));
        var csproj = File.ReadAllText(Path.Combine(root, "src/RideAudit.Client.Contracts/RideAudit.Client.Contracts.csproj"));
        Assert.Contains("RideAudit.Protos.csproj", csproj);
        Assert.DoesNotContain("custody.proto", csproj);
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
        // FR-RIDE-074: copyright moved from top LicenseNotice to About view (TEST-RIDE-055).
        var mainForAbout = new MainView();
        mainForAbout.OpenAbout();
        var aboutBody = Assert.IsType<AboutView>(mainForAbout.FindControl<ContentControl>("Body")!.Content);
        Assert.Contains("GPL-2.0-or-later", aboutBody.FindControl<TextBlock>("CopyrightNotice")!.Text);
        Assert.Null(mainForAbout.FindControl<TextBlock>("LicenseNotice"));
        view.SelectPassenger();
        view.StartSession();
        Assert.Equal("WF-08", view.FindControl<TextBlock>("ScreenId")!.Text);
        Assert.Contains("driver", view.FindControl<TextBlock>("FailClosedText")!.Text, StringComparison.OrdinalIgnoreCase);
        view.SelectDriver();
        view.Discover();
        Assert.Equal("WF-08", view.FindControl<TextBlock>("ScreenId")!.Text);
        Assert.Contains("BT_DISABLED", view.FindControl<TextBlock>("FailClosedText")!.Text, StringComparison.Ordinal);
        Assert.Contains("No Lyft private API", view.FindControl<TextBlock>("FailClosedText")!.Text);

        var radio = new CaptureShellView(new InMemoryDiscoveryBus());
        var radioWindow = new Window { Width = 400, Height = 800, Content = radio };
        radioWindow.Show();
        radio.SelectDriver();
        radio.Discover();
        Assert.Contains("in-memory", radio.FindControl<TextBlock>("PairingStatus")!.Text, StringComparison.Ordinal);
        Assert.Contains("No Lyft private API", radio.FindControl<TextBlock>("PairingStatus")!.Text);
        radio.StartSession();
        Assert.Contains("clock master", radio.FindControl<TextBlock>("ClockText")!.Text, StringComparison.OrdinalIgnoreCase);
        radio.StopSession();
        Assert.Equal("WF-06", radio.FindControl<TextBlock>("ScreenId")!.Text);
    }

    [AvaloniaFact]
    [Trait("AC", "AC-TEST-035-002")]
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
        }, "reviewer", Fixtures.Release(fixture.Clock));
        var view = new ReviewShellView();
        var window = new Window { Width = 800, Height = 600, Content = view };
        window.Show();
        view.ShowOutcome(outcome);
        Assert.False(view.FindControl<Button>("DecryptButton")!.IsEnabled);
        Assert.False(view.FindControl<Button>("PlaybackButton")!.IsEnabled);
        Assert.Equal("WF-R-04", view.FindControl<TextBlock>("ScreenId")!.Text);
    }
}
