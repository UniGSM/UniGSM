using System;
using Avalonia.Controls;

namespace GsmManager.Events;

public class RequestCloseReplaceMainWindowEventArgs : EventArgs
{
    public Window NewMainWindow { get; set;  }
}