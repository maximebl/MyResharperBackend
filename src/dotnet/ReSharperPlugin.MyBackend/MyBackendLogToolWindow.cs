#if RIDER
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReSharperPlugin.MyBackend;

public static class LogWindow
{
    // VS Code-inspired dark palette
    private static readonly SolidColorBrush BgBrush        = Brush(0x1E, 0x1E, 0x1E);
    private static readonly SolidColorBrush EntryBgBrush   = Brush(0x2D, 0x2D, 0x2D);
    private static readonly SolidColorBrush FgBrush        = Brush(0xD4, 0xD4, 0xD4);
    private static readonly SolidColorBrush HeaderBrush    = Brush(0x9C, 0xDC, 0xFE); // light blue
    private static readonly SolidColorBrush SectionBrush   = Brush(0x56, 0x9C, 0xD6); // keyword blue
    private static readonly SolidColorBrush DividerBrush   = Brush(0x3C, 0x3C, 0x3C);
    private static readonly FontFamily MonoFont = new FontFamily("Consolas, Courier New");

    private static Window _window;
    private static StackPanel _panel;
    private static ScrollViewer _scroll;

    public static void AddEntry(string message)
    {
        Dispatch(() =>
        {
            EnsureWindow();

            var lines = message.Trim().Split('\n');
            var header = lines[0].Trim();

            var body = new TextBox
            {
                Text = message.Trim(),
                IsReadOnly = true,
                FontFamily = MonoFont,
                FontSize = 12,
                Foreground = FgBrush,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                TextWrapping = TextWrapping.NoWrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(24, 2, 4, 4),
            };

            var expander = new Expander
            {
                Header = new TextBlock
                {
                    Text = header,
                    Foreground = HeaderBrush,
                    FontFamily = MonoFont,
                    FontSize = 12,
                },
                Foreground = FgBrush,
                Background = EntryBgBrush,
                Content = body,
                IsExpanded = false,
                Margin = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(0),
            };

            _panel.Children.Add(expander);
            _scroll.ScrollToBottom();
        });
    }

    public static void AddSection(string title)
    {
        Dispatch(() =>
        {
            EnsureWindow();

            _panel.Children.Add(new Border
            {
                Background = DividerBrush,
                Margin = new Thickness(0, 8, 0, 0),
                Padding = new Thickness(8, 4, 8, 4),
                Child = new TextBlock
                {
                    Text = title.ToUpper(),
                    Foreground = SectionBrush,
                    FontFamily = MonoFont,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                },
            });
        });
    }

    private static void EnsureWindow()
    {
        if (_window != null) return;

        _panel = new StackPanel { Background = BgBrush };
        _scroll = new ScrollViewer
        {
            Content = _panel,
            Background = BgBrush,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
        _window = new Window
        {
            Title = "MyBackend Log",
            Width = 900,
            Height = 600,
            Background = BgBrush,
            Content = _scroll,
        };
        _window.Closed += (_, __) =>
        {
            _window = null;
            _panel = null;
            _scroll = null;
        };
        _window.Show();
    }

    private static void Dispatch(Action action)
    {
        var app = Application.Current;
        if (app == null) return;
        app.Dispatcher.BeginInvoke(action);
    }

    private static SolidColorBrush Brush(byte r, byte g, byte b) =>
        new SolidColorBrush(Color.FromRgb(r, g, b));
}
#endif
