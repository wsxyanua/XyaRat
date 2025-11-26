using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Server.Helper
{
    public static class ModernTheme
    {
        // Modern Dark Theme Colors
        public static class Colors
        {
            // Primary Colors
            public static readonly Color Primary = ColorTranslator.FromHtml("#2196F3");        // Blue
            public static readonly Color PrimaryDark = ColorTranslator.FromHtml("#1976D2");
            public static readonly Color PrimaryLight = ColorTranslator.FromHtml("#BBDEFB");
            
            // Accent Colors
            public static readonly Color Accent = ColorTranslator.FromHtml("#FF5722");          // Deep Orange
            public static readonly Color AccentLight = ColorTranslator.FromHtml("#FF7043");
            
            // Success/Warning/Error
            public static readonly Color Success = ColorTranslator.FromHtml("#4CAF50");         // Green
            public static readonly Color Warning = ColorTranslator.FromHtml("#FFC107");         // Amber
            public static readonly Color Error = ColorTranslator.FromHtml("#F44336");           // Red
            
            // Dark Theme
            public static readonly Color DarkBackground = ColorTranslator.FromHtml("#1E1E1E");  // VS Code Dark
            public static readonly Color DarkSurface = ColorTranslator.FromHtml("#252526");
            public static readonly Color DarkPanel = ColorTranslator.FromHtml("#2D2D30");
            public static readonly Color DarkBorder = ColorTranslator.FromHtml("#3E3E42");
            
            // Text Colors
            public static readonly Color TextPrimary = ColorTranslator.FromHtml("#FFFFFF");
            public static readonly Color TextSecondary = ColorTranslator.FromHtml("#CCCCCC");
            public static readonly Color TextDisabled = ColorTranslator.FromHtml("#6A6A6A");
            
            // ListView Colors
            public static readonly Color ListViewItem = ColorTranslator.FromHtml("#2D2D30");
            public static readonly Color ListViewItemAlt = ColorTranslator.FromHtml("#252526");
            public static readonly Color ListViewItemHover = ColorTranslator.FromHtml("#3E3E42");
            public static readonly Color ListViewItemSelected = ColorTranslator.FromHtml("#094771");
            
            // Status Colors
            public static readonly Color Online = ColorTranslator.FromHtml("#4CAF50");
            public static readonly Color Idle = ColorTranslator.FromHtml("#FFC107");
            public static readonly Color Offline = ColorTranslator.FromHtml("#9E9E9E");
        }
        
        // Apply modern theme to Form
        public static void ApplyTheme(Form form)
        {
            form.BackColor = Colors.DarkBackground;
            form.ForeColor = Colors.TextPrimary;
            
            // Apply theme to all controls recursively
            ApplyThemeToControls(form.Controls);
        }
        
        private static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                // ListView
                if (control is ListView listView)
                {
                    ApplyListViewTheme(listView);
                }
                // MenuStrip
                else if (control is MenuStrip menuStrip)
                {
                    ApplyMenuStripTheme(menuStrip);
                }
                // StatusStrip
                else if (control is StatusStrip statusStrip)
                {
                    ApplyStatusStripTheme(statusStrip);
                }
                // TabControl
                else if (control is TabControl tabControl)
                {
                    ApplyTabControlTheme(tabControl);
                }
                // TextBox
                else if (control is TextBox textBox)
                {
                    ApplyTextBoxTheme(textBox);
                }
                // Button
                else if (control is Button button)
                {
                    ApplyButtonTheme(button);
                }
                // Panel
                else if (control is Panel panel)
                {
                    panel.BackColor = Colors.DarkPanel;
                    panel.ForeColor = Colors.TextPrimary;
                }
                // SplitContainer
                else if (control is SplitContainer splitContainer)
                {
                    splitContainer.BackColor = Colors.DarkBorder;
                    ApplyThemeToControls(splitContainer.Panel1.Controls);
                    ApplyThemeToControls(splitContainer.Panel2.Controls);
                }
                // Generic controls
                else
                {
                    control.BackColor = Colors.DarkSurface;
                    control.ForeColor = Colors.TextPrimary;
                }
                
                // Recursive for container controls
                if (control.HasChildren)
                {
                    ApplyThemeToControls(control.Controls);
                }
            }
        }
        
        public static void ApplyListViewTheme(ListView listView)
        {
            listView.BackColor = Colors.DarkSurface;
            listView.ForeColor = Colors.TextPrimary;
            listView.BorderStyle = BorderStyle.None;
            listView.OwnerDraw = false; // Let Windows handle drawing for better performance
            listView.FullRowSelect = true;
            listView.GridLines = false;
            listView.View = View.Details;
            
            // Modern font
            listView.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            
            // Set header style
            if (listView.View == View.Details)
            {
                foreach (ColumnHeader column in listView.Columns)
                {
                    column.Width = column.Width; // Refresh
                }
            }
        }
        
        public static void ApplyMenuStripTheme(MenuStrip menuStrip)
        {
            menuStrip.BackColor = Colors.DarkPanel;
            menuStrip.ForeColor = Colors.TextPrimary;
            menuStrip.Renderer = new ModernMenuStripRenderer();
            menuStrip.Font = new Font("Segoe UI", 9F);
        }
        
        public static void ApplyStatusStripTheme(StatusStrip statusStrip)
        {
            statusStrip.BackColor = Colors.DarkPanel;
            statusStrip.ForeColor = Colors.TextSecondary;
            statusStrip.Renderer = new ModernToolStripRenderer();
            statusStrip.Font = new Font("Segoe UI", 9F);
        }
        
        public static void ApplyTabControlTheme(TabControl tabControl)
        {
            tabControl.BackColor = Colors.DarkSurface;
            tabControl.ForeColor = Colors.TextPrimary;
            tabControl.Font = new Font("Segoe UI", 9F);
            
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                tabPage.BackColor = Colors.DarkSurface;
                tabPage.ForeColor = Colors.TextPrimary;
            }
        }
        
        public static void ApplyTextBoxTheme(TextBox textBox)
        {
            textBox.BackColor = Colors.DarkPanel;
            textBox.ForeColor = Colors.TextPrimary;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Font = new Font("Consolas", 9F);
        }
        
        public static void ApplyButtonTheme(Button button)
        {
            button.BackColor = Colors.Primary;
            button.ForeColor = Colors.TextPrimary;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Colors.PrimaryDark;
            button.FlatAppearance.MouseDownBackColor = Colors.PrimaryLight;
            button.Font = new Font("Segoe UI", 9F);
            button.Cursor = Cursors.Hand;
        }
        
        // Context Menu Theme
        public static void ApplyContextMenuTheme(ContextMenuStrip contextMenu)
        {
            contextMenu.BackColor = Colors.DarkPanel;
            contextMenu.ForeColor = Colors.TextPrimary;
            contextMenu.Renderer = new ModernMenuStripRenderer();
            contextMenu.Font = new Font("Segoe UI", 9F);
        }
    }
    
    // Custom ToolStrip Renderer for Modern Look
    public class ModernToolStripRenderer : ToolStripProfessionalRenderer
    {
        public ModernToolStripRenderer() : base(new ModernColorTable()) { }
        
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            // No border
        }
    }
    
    public class ModernMenuStripRenderer : ToolStripProfessionalRenderer
    {
        public ModernMenuStripRenderer() : base(new ModernColorTable()) { }
        
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Selected)
            {
                base.OnRenderMenuItemBackground(e);
            }
            else
            {
                Rectangle rc = new Rectangle(Point.Empty, e.Item.Size);
                e.Graphics.FillRectangle(new SolidBrush(ModernTheme.Colors.Primary), rc);
            }
        }
        
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Selected ? ModernTheme.Colors.TextPrimary : ModernTheme.Colors.TextSecondary;
            base.OnRenderItemText(e);
        }
    }
    
    // Custom Color Table
    public class ModernColorTable : ProfessionalColorTable
    {
        public override Color MenuBorder => ModernTheme.Colors.DarkBorder;
        public override Color MenuItemBorder => ModernTheme.Colors.Primary;
        public override Color MenuItemSelected => ModernTheme.Colors.Primary;
        public override Color MenuItemSelectedGradientBegin => ModernTheme.Colors.Primary;
        public override Color MenuItemSelectedGradientEnd => ModernTheme.Colors.PrimaryDark;
        public override Color MenuItemPressedGradientBegin => ModernTheme.Colors.PrimaryDark;
        public override Color MenuItemPressedGradientEnd => ModernTheme.Colors.PrimaryDark;
        public override Color ToolStripDropDownBackground => ModernTheme.Colors.DarkPanel;
        public override Color ImageMarginGradientBegin => ModernTheme.Colors.DarkSurface;
        public override Color ImageMarginGradientMiddle => ModernTheme.Colors.DarkSurface;
        public override Color ImageMarginGradientEnd => ModernTheme.Colors.DarkSurface;
        public override Color ToolStripBorder => ModernTheme.Colors.DarkBorder;
        public override Color SeparatorDark => ModernTheme.Colors.DarkBorder;
        public override Color SeparatorLight => ModernTheme.Colors.DarkBorder;
    }
}
