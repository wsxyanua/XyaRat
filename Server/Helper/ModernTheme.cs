using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Server.Helper
{
    public static class ModernTheme
    {
        // Ultra Modern 2025 Theme Colors - Cyberpunk/Neon Style
        public static class Colors
        {
            // Primary Colors - Electric Blue & Purple Gradient
            public static readonly Color Primary = ColorTranslator.FromHtml("#7C3AED");        // Vivid Purple
            public static readonly Color PrimaryDark = ColorTranslator.FromHtml("#5B21B6");
            public static readonly Color PrimaryLight = ColorTranslator.FromHtml("#A78BFA");
            
            // Accent Colors - Neon Cyan
            public static readonly Color Accent = ColorTranslator.FromHtml("#06B6D4");          // Cyan
            public static readonly Color AccentLight = ColorTranslator.FromHtml("#22D3EE");
            public static readonly Color AccentDark = ColorTranslator.FromHtml("#0891B2");
            
            // Secondary Accent - Hot Pink
            public static readonly Color Pink = ColorTranslator.FromHtml("#EC4899");
            public static readonly Color PinkLight = ColorTranslator.FromHtml("#F472B6");
            
            // Success/Warning/Error
            public static readonly Color Success = ColorTranslator.FromHtml("#10B981");         // Emerald
            public static readonly Color Warning = ColorTranslator.FromHtml("#F59E0B");         // Amber
            public static readonly Color Error = ColorTranslator.FromHtml("#EF4444");           // Red
            
            // Modern Dark Theme - Deeper blacks with subtle gradients
            public static readonly Color DarkBackground = ColorTranslator.FromHtml("#0A0A0A");  // Almost Black
            public static readonly Color DarkSurface = ColorTranslator.FromHtml("#121212");     // Dark Gray
            public static readonly Color DarkPanel = ColorTranslator.FromHtml("#1A1A1A");       // Darker Gray
            public static readonly Color DarkBorder = ColorTranslator.FromHtml("#2A2A2A");      // Border
            public static readonly Color DarkElevated = ColorTranslator.FromHtml("#1F1F1F");    // Elevated surface
            
            // Text Colors - Higher contrast
            public static readonly Color TextPrimary = ColorTranslator.FromHtml("#F5F5F5");
            public static readonly Color TextSecondary = ColorTranslator.FromHtml("#D1D5DB");
            public static readonly Color TextDisabled = ColorTranslator.FromHtml("#6B7280");
            public static readonly Color TextNeon = ColorTranslator.FromHtml("#22D3EE");        // Neon text
            
            // ListView Colors - Modern with gradient feel
            public static readonly Color ListViewItem = ColorTranslator.FromHtml("#1A1A1A");
            public static readonly Color ListViewItemAlt = ColorTranslator.FromHtml("#151515");
            public static readonly Color ListViewItemHover = ColorTranslator.FromHtml("#252525");
            public static readonly Color ListViewItemSelected = ColorTranslator.FromHtml("#7C3AED");  // Purple selection
            public static readonly Color ListViewItemSelectedGlow = ColorTranslator.FromHtml("#A78BFA"); // Glow effect
            
            // Status Colors - Vibrant
            public static readonly Color Online = ColorTranslator.FromHtml("#10B981");
            public static readonly Color Idle = ColorTranslator.FromHtml("#F59E0B");
            public static readonly Color Offline = ColorTranslator.FromHtml("#6B7280");
            
            // Neon Glow Colors
            public static readonly Color NeonBlue = ColorTranslator.FromHtml("#3B82F6");
            public static readonly Color NeonPurple = ColorTranslator.FromHtml("#8B5CF6");
            public static readonly Color NeonPink = ColorTranslator.FromHtml("#EC4899");
            public static readonly Color NeonCyan = ColorTranslator.FromHtml("#06B6D4");
        }
        
        // Apply modern theme to Form
        public static void ApplyTheme(Form form)
        {
            form.BackColor = Colors.DarkBackground;
            form.ForeColor = Colors.TextPrimary;
            
            // Add gradient background paint event
            form.Paint += Form_Paint;
            
            // Apply theme to all controls recursively
            ApplyThemeToControls(form.Controls);
        }
        
        private static void Form_Paint(object sender, PaintEventArgs e)
        {
            Form form = sender as Form;
            if (form == null) return;
            
            Graphics g = e.Graphics;
            
            // Subtle gradient background
            using (System.Drawing.Drawing2D.LinearGradientBrush brush = 
                new System.Drawing.Drawing2D.LinearGradientBrush(
                    form.ClientRectangle,
                    Colors.DarkBackground,
                    Colors.DarkSurface,
                    135f)) // Diagonal gradient
            {
                g.FillRectangle(brush, form.ClientRectangle);
            }
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
            listView.OwnerDraw = false;
            listView.FullRowSelect = true;
            listView.GridLines = false;
            listView.View = View.Details;
            
            // Modern font with better readability
            listView.Font = new Font("Inter", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            if (listView.Font.Name != "Inter")
            {
                listView.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            }
            
            // Add subtle border
            listView.Padding = new Padding(1);
        }
        
        public static void ApplyMenuStripTheme(MenuStrip menuStrip)
        {
            menuStrip.BackColor = Colors.DarkPanel;
            menuStrip.ForeColor = Colors.TextPrimary;
            menuStrip.Renderer = new ModernMenuStripRenderer();
            menuStrip.Font = new Font("Segoe UI Semibold", 9F);
            menuStrip.Padding = new Padding(6, 4, 0, 4);
        }
        
        public static void ApplyStatusStripTheme(StatusStrip statusStrip)
        {
            statusStrip.BackColor = Colors.DarkPanel;
            statusStrip.ForeColor = Colors.TextSecondary;
            statusStrip.Renderer = new ModernToolStripRenderer();
            statusStrip.Font = new Font("Segoe UI", 9F);
            statusStrip.SizingGrip = false;
        }
        
        public static void ApplyTabControlTheme(TabControl tabControl)
        {
            tabControl.BackColor = Colors.DarkSurface;
            tabControl.ForeColor = Colors.TextPrimary;
            tabControl.Font = new Font("Segoe UI Semibold", 9.5F);
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.DrawItem += TabControl_DrawItem;
            
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                tabPage.BackColor = Colors.DarkSurface;
                tabPage.ForeColor = Colors.TextPrimary;
                tabPage.BorderStyle = BorderStyle.None;
            }
        }
        
        // Custom tab drawing with gradient and glow
        private static void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            TabPage tabPage = tabControl.TabPages[e.Index];
            Rectangle tabRect = tabControl.GetTabRect(e.Index);
            
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            
            // Background
            using (SolidBrush bgBrush = new SolidBrush(isSelected ? Colors.DarkElevated : Colors.DarkPanel))
            {
                g.FillRectangle(bgBrush, tabRect);
            }
            
            // Bottom accent line for selected tab
            if (isSelected)
            {
                using (Pen accentPen = new Pen(Colors.Primary, 3))
                {
                    g.DrawLine(accentPen, tabRect.Left, tabRect.Bottom - 2, tabRect.Right, tabRect.Bottom - 2);
                }
            }
            
            // Text with glow effect for selected
            using (Font font = new Font("Segoe UI Semibold", 9F))
            {
                Color textColor = isSelected ? Colors.TextPrimary : Colors.TextSecondary;
                using (SolidBrush textBrush = new SolidBrush(textColor))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(tabPage.Text, font, textBrush, tabRect, sf);
                }
            }
        }
        
        public static void ApplyTextBoxTheme(TextBox textBox)
        {
            textBox.BackColor = Colors.DarkElevated;
            textBox.ForeColor = Colors.TextPrimary;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Font = new Font("Consolas", 9.5F);
        }
        
        public static void ApplyButtonTheme(Button button)
        {
            button.BackColor = Colors.Primary;
            button.ForeColor = Colors.TextPrimary;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.BorderColor = Colors.PrimaryLight;
            button.FlatAppearance.MouseOverBackColor = Colors.PrimaryLight;
            button.FlatAppearance.MouseDownBackColor = Colors.PrimaryDark;
            button.Font = new Font("Segoe UI Semibold", 9.5F);
            button.Cursor = Cursors.Hand;
            button.Padding = new Padding(12, 6, 12, 6);
            
            // Add rounded corners effect through paint event
            button.Paint += Button_Paint;
        }
        
        private static void Button_Paint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Draw subtle glow when mouse over
            if (btn.ClientRectangle.Contains(btn.PointToClient(Cursor.Position)))
            {
                using (Pen glowPen = new Pen(Colors.PrimaryLight, 2))
                {
                    Rectangle glowRect = new Rectangle(1, 1, btn.Width - 3, btn.Height - 3);
                    e.Graphics.DrawRectangle(glowPen, glowRect);
                }
            }
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
                
                // Gradient fill for selected item
                using (System.Drawing.Drawing2D.LinearGradientBrush brush = 
                    new System.Drawing.Drawing2D.LinearGradientBrush(
                        rc,
                        ModernTheme.Colors.Primary,
                        ModernTheme.Colors.PrimaryLight,
                        System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, rc);
                }
                
                // Subtle glow border
                using (Pen glowPen = new Pen(ModernTheme.Colors.AccentLight, 1))
                {
                    e.Graphics.DrawRectangle(glowPen, 0, 0, rc.Width - 1, rc.Height - 1);
                }
            }
        }
        
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Selected ? ModernTheme.Colors.TextPrimary : ModernTheme.Colors.TextSecondary;
            e.TextFont = new Font("Segoe UI", 9F, e.Item.Selected ? FontStyle.Bold : FontStyle.Regular);
            base.OnRenderItemText(e);
        }
        
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            // Custom separator with gradient
            if (e.Vertical)
            {
                base.OnRenderSeparator(e);
            }
            else
            {
                int y = e.Item.Height / 2;
                using (Pen pen = new Pen(ModernTheme.Colors.DarkBorder, 1))
                {
                    e.Graphics.DrawLine(pen, 30, y, e.Item.Width - 5, y);
                }
            }
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
