// SPDX-License-Identifier: GPL-2.0-or-later
// Copyright (C) 2026 RideAudit contributors

using Avalonia.Controls;

namespace RideAudit.Shared.Ui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainView view)
        : this()
    {
        Content = view;
    }
}
