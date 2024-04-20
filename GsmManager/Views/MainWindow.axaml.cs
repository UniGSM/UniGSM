using System;
using System.Net.Mime;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using GsmManager.ViewModels;

namespace GsmManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindowOnClosed(object? sender, EventArgs e)
    {
        Environment.Exit(0);
    }
}