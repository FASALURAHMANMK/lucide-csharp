using System.Drawing;
using System.Windows.Forms;
using Lucide.NET;
using Lucide.NET.WinForms;

namespace WinFormsDemo;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        BuildUi();
    }

    private void BuildUi()
    {
        BackColor = Color.FromArgb(248, 250, 252);

        var header = new Label
        {
            Text = "Lucide.NET",
            Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            AutoSize = true,
            Location = new Point(24, 20)
        };

        var subhead = new Label
        {
            Text = "WinForms rendering with cached SVG rasterization",
            Font = new Font("Segoe UI", 10f, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 116, 139),
            AutoSize = true,
            Location = new Point(24, 52)
        };

        var buttonsPanel = new FlowLayoutPanel
        {
            Location = new Point(24, 90),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        buttonsPanel.Controls.Add(CreateIconButton("Home", LucideIconKind.Home));
        buttonsPanel.Controls.Add(CreateIconButton("Alert", LucideIconKind.AlertCircle));
        buttonsPanel.Controls.Add(CreateIconButton("Cart", LucideIconKind.ShoppingCart));

        var listView = new ListView
        {
            Location = new Point(24, 160),
            Size = new Size(840, 380),
            View = View.Details,
            FullRowSelect = true
        };

        listView.Columns.Add("Icon", 120);
        listView.Columns.Add("Description", 640);

        var imageList = new ImageList
        {
            ImageSize = new Size(24, 24),
            ColorDepth = ColorDepth.Depth32Bit
        };

        imageList.Images.Add("home", LucideWinForms.ToBitmap(LucideIconKind.Home, 24, Color.FromArgb(29, 78, 216)));
        imageList.Images.Add("alert", LucideWinForms.ToBitmap(LucideIconKind.AlertCircle, 24, Color.FromArgb(220, 38, 38)));
        imageList.Images.Add("cart", LucideWinForms.ToBitmap(LucideIconKind.ShoppingCart, 24, Color.FromArgb(5, 150, 105)));

        listView.SmallImageList = imageList;

        listView.Items.Add(new ListViewItem(new[] { "Home", "Primary navigation icon" }, "home"));
        listView.Items.Add(new ListViewItem(new[] { "Alert", "Status or warning indicator" }, "alert"));
        listView.Items.Add(new ListViewItem(new[] { "Cart", "Commerce action icon" }, "cart"));

        Controls.Add(header);
        Controls.Add(subhead);
        Controls.Add(buttonsPanel);
        Controls.Add(listView);
    }

    private static Button CreateIconButton(string text, LucideIconKind kind)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            ImageAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Padding = new Padding(8, 6, 12, 6),
            Margin = new Padding(0, 0, 12, 0)
        };

        button.Image = LucideWinForms.ToBitmap(kind, 20, Color.FromArgb(29, 78, 216));
        return button;
    }
}
