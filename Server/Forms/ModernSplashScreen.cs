using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Server.Helper;

namespace Server.Forms
{
    public partial class ModernSplashScreen : Form
    {
        private Timer fadeTimer;
        private int fadeStep = 0;
        private const int FADE_STEPS = 20;
        
        public ModernSplashScreen()
        {
            InitializeComponent();
            InitializeModernSplash();
        }
        
        private void InitializeModernSplash()
        {
            // Form settings
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(600, 400);
            this.BackColor = ModernTheme.Colors.DarkBackground;
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            
            // Enable double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                         ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.UserPaint, true);
            
            // Fade in effect
            fadeTimer = new Timer();
            fadeTimer.Interval = 30;
            fadeTimer.Tick += FadeTimer_Tick;
            fadeTimer.Start();
        }
        
        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            fadeStep++;
            this.Opacity = (double)fadeStep / FADE_STEPS;
            
            if (fadeStep >= FADE_STEPS)
            {
                fadeTimer.Stop();
                fadeTimer.Dispose();
            }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            
            // Draw gradient background
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle, 
                ModernTheme.Colors.DarkBackground, 
                ModernTheme.Colors.DarkSurface, 
                45f))
            {
                g.FillRectangle(brush, this.ClientRectangle);
            }
            
            // Draw accent border
            using (Pen pen = new Pen(ModernTheme.Colors.Primary, 3))
            {
                g.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
            
            // Draw logo/title
            string title = "XyaRat";
            using (Font titleFont = new Font("Segoe UI", 48, FontStyle.Bold))
            {
                SizeF titleSize = g.MeasureString(title, titleFont);
                float titleX = (this.Width - titleSize.Width) / 2;
                float titleY = 100;
                
                // Draw shadow
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                {
                    g.DrawString(title, titleFont, shadowBrush, titleX + 3, titleY + 3);
                }
                
                // Draw gradient text
                using (LinearGradientBrush textBrush = new LinearGradientBrush(
                    new Rectangle((int)titleX, (int)titleY, (int)titleSize.Width, (int)titleSize.Height),
                    ModernTheme.Colors.Primary,
                    ModernTheme.Colors.AccentLight,
                    LinearGradientMode.Horizontal))
                {
                    g.DrawString(title, titleFont, textBrush, titleX, titleY);
                }
            }
            
            // Draw version
            string version = "v1.0.8 Modern Edition";
            using (Font versionFont = new Font("Segoe UI", 12, FontStyle.Regular))
            using (SolidBrush versionBrush = new SolidBrush(ModernTheme.Colors.TextSecondary))
            {
                SizeF versionSize = g.MeasureString(version, versionFont);
                float versionX = (this.Width - versionSize.Width) / 2;
                g.DrawString(version, versionFont, versionBrush, versionX, 190);
            }
            
            // Draw loading text
            string loading = "Remote Administration Tool";
            using (Font loadingFont = new Font("Segoe UI", 10, FontStyle.Regular))
            using (SolidBrush loadingBrush = new SolidBrush(ModernTheme.Colors.TextSecondary))
            {
                SizeF loadingSize = g.MeasureString(loading, loadingFont);
                float loadingX = (this.Width - loadingSize.Width) / 2;
                g.DrawString(loading, loadingFont, loadingBrush, loadingX, 220);
            }
            
            // Draw progress bar
            int progressBarWidth = 400;
            int progressBarHeight = 4;
            int progressBarX = (this.Width - progressBarWidth) / 2;
            int progressBarY = 280;
            
            // Background
            using (SolidBrush bgBrush = new SolidBrush(ModernTheme.Colors.DarkPanel))
            {
                g.FillRectangle(bgBrush, progressBarX, progressBarY, progressBarWidth, progressBarHeight);
            }
            
            // Progress (animated)
            int progress = (int)((DateTime.Now.Millisecond / 1000.0) * progressBarWidth);
            using (LinearGradientBrush progressBrush = new LinearGradientBrush(
                new Rectangle(progressBarX, progressBarY, progress, progressBarHeight),
                ModernTheme.Colors.Primary,
                ModernTheme.Colors.AccentLight,
                LinearGradientMode.Horizontal))
            {
                g.FillRectangle(progressBrush, progressBarX, progressBarY, progress, progressBarHeight);
            }
            
            // Draw footer
            string footer = "© 2024-2025 XyaRat Project | Educational Purpose Only";
            using (Font footerFont = new Font("Segoe UI", 8, FontStyle.Regular))
            using (SolidBrush footerBrush = new SolidBrush(ModernTheme.Colors.TextDisabled))
            {
                SizeF footerSize = g.MeasureString(footer, footerFont);
                float footerX = (this.Width - footerSize.Width) / 2;
                g.DrawString(footer, footerFont, footerBrush, footerX, this.Height - 40);
            }
        }
        
        // Auto-close after delay
        public void StartAutoClose(int milliseconds = 2000)
        {
            Timer closeTimer = new Timer();
            closeTimer.Interval = milliseconds;
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                closeTimer.Dispose();
                
                // Fade out
                Timer fadeOutTimer = new Timer();
                int fadeOutStep = FADE_STEPS;
                fadeOutTimer.Interval = 30;
                fadeOutTimer.Tick += (s2, e2) =>
                {
                    fadeOutStep--;
                    this.Opacity = (double)fadeOutStep / FADE_STEPS;
                    
                    if (fadeOutStep <= 0)
                    {
                        fadeOutTimer.Stop();
                        fadeOutTimer.Dispose();
                        this.Close();
                    }
                };
                fadeOutTimer.Start();
            };
            closeTimer.Start();
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ModernSplashScreen
            // 
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Name = "ModernSplashScreen";
            this.ResumeLayout(false);
        }
    }
}
