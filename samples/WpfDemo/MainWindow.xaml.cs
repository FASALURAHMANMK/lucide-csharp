using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Lucide.NET;

namespace WpfDemo;

public partial class MainWindow : Window
{
    public IReadOnlyList<LucideIconKind> Icons { get; } = Enum.GetValues(typeof(LucideIconKind))
        .Cast<LucideIconKind>()
        .OrderBy(kind => kind.ToString(), StringComparer.Ordinal)
        .ToArray();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }
}
