using System.Drawing;
using System.Windows.Forms;

namespace Tldr.Platform;

public sealed class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly Action _showWindow;
    private readonly Action _exitApp;

    public TrayIconManager(Action showWindow, Action exitApp)
    {
        _showWindow = showWindow;
        _exitApp = exitApp;

        var iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "icon.ico");

        _icon = new NotifyIcon
        {
            Icon = System.IO.File.Exists(iconPath) ? new Icon(iconPath) : SystemIcons.Application,
            Text = "TLDR",
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };

        _icon.DoubleClick += (_, _) => _showWindow();
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open", null, (_, _) => _showWindow());
        menu.Items.Add("-");
        menu.Items.Add("Exit", null, (_, _) => _exitApp());
        return menu;
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
