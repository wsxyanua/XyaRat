using System;
using System.IO;
using System.Windows.Forms;
using System.Text;
using Server.Helper;

namespace Server.Forms
{
    /// <summary>
    /// Decryption viewer for encrypted recovery files
    /// Allows viewing encrypted .enc files with master key authentication
    /// </summary>
    public partial class FormDecryptViewer : Form
    {
        private string selectedFilePath;
        private TextBox txtContent;
        private Button btnBrowse;
        private Button btnDecrypt;
        private Button btnSaveDecrypted;
        private Label lblStatus;
        
        public FormDecryptViewer()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Encrypted File Viewer - XyaRat";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Browse button
            btnBrowse = new Button();
            btnBrowse.Text = "Browse .enc File";
            btnBrowse.Location = new System.Drawing.Point(20, 20);
            btnBrowse.Size = new System.Drawing.Size(150, 30);
            btnBrowse.Click += BtnBrowse_Click;
            
            // Decrypt button
            btnDecrypt = new Button();
            btnDecrypt.Text = "Decrypt & View";
            btnDecrypt.Location = new System.Drawing.Point(180, 20);
            btnDecrypt.Size = new System.Drawing.Size(150, 30);
            btnDecrypt.Click += BtnDecrypt_Click;
            btnDecrypt.Enabled = false;
            
            // Save decrypted button
            btnSaveDecrypted = new Button();
            btnSaveDecrypted.Text = "Save as .txt";
            btnSaveDecrypted.Location = new System.Drawing.Point(340, 20);
            btnSaveDecrypted.Size = new System.Drawing.Size(150, 30);
            btnSaveDecrypted.Click += BtnSaveDecrypted_Click;
            btnSaveDecrypted.Enabled = false;
            
            // Status label
            lblStatus = new Label();
            lblStatus.Location = new System.Drawing.Point(20, 60);
            lblStatus.Size = new System.Drawing.Size(750, 20);
            lblStatus.Text = "Select an encrypted file (.enc) to view";
            
            // Content textbox
            txtContent = new TextBox();
            txtContent.Location = new System.Drawing.Point(20, 90);
            txtContent.Size = new System.Drawing.Size(750, 450);
            txtContent.Multiline = true;
            txtContent.ScrollBars = ScrollBars.Both;
            txtContent.WordWrap = false;
            txtContent.ReadOnly = true;
            txtContent.Font = new System.Drawing.Font("Consolas", 9);
            
            // Add controls
            this.Controls.Add(btnBrowse);
            this.Controls.Add(btnDecrypt);
            this.Controls.Add(btnSaveDecrypted);
            this.Controls.Add(lblStatus);
            this.Controls.Add(txtContent);
        }
        
        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Title = "Select Encrypted File";
                    dialog.Filter = "Encrypted Files (*.enc)|*.enc|All Files (*.*)|*.*";
                    dialog.InitialDirectory = Path.Combine(Application.StartupPath, "ClientsFolder");
                    
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        selectedFilePath = dialog.FileName;
                        lblStatus.Text = $"Selected: {Path.GetFileName(selectedFilePath)}";
                        btnDecrypt.Enabled = true;
                        txtContent.Clear();
                        btnSaveDecrypted.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error browsing file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnDecrypt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                MessageBox.Show("Please select a valid file first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                lblStatus.Text = "Decrypting...";
                Application.DoEvents();
                
                string decrypted = EncryptionAtRest.DecryptFromFile(selectedFilePath);
                
                if (decrypted != null)
                {
                    txtContent.Text = decrypted;
                    lblStatus.Text = $"✓ Decrypted successfully: {Path.GetFileName(selectedFilePath)} ({decrypted.Length} chars)";
                    btnSaveDecrypted.Enabled = true;
                    
                    Logger.Log($"[DecryptViewer] Successfully decrypted: {selectedFilePath}", Logger.LogLevel.Info);
                }
                else
                {
                    txtContent.Text = "❌ DECRYPTION FAILED\n\n" +
                                     "Possible reasons:\n" +
                                     "1. File is corrupted\n" +
                                     "2. Wrong master key (encrypted on different machine)\n" +
                                     "3. File was tampered with (authentication failed)\n" +
                                     "4. Not a valid encrypted file";
                    lblStatus.Text = "✗ Decryption failed - Invalid file or wrong key";
                    btnSaveDecrypted.Enabled = false;
                    
                    Logger.Log($"[DecryptViewer] Decryption failed: {selectedFilePath}", Logger.LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                txtContent.Text = $"Error during decryption:\n{ex.Message}\n\n{ex.StackTrace}";
                lblStatus.Text = "✗ Error during decryption";
                btnSaveDecrypted.Enabled = false;
                
                Logger.Log($"[DecryptViewer] Exception: {ex.Message}", Logger.LogLevel.Error);
                MessageBox.Show($"Decryption error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnSaveDecrypted_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtContent.Text) || txtContent.Text.StartsWith("❌"))
            {
                MessageBox.Show("No decrypted content to save.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Title = "Save Decrypted Content";
                    dialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                    dialog.FileName = Path.GetFileNameWithoutExtension(selectedFilePath) + "_decrypted.txt";
                    
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(dialog.FileName, txtContent.Text, Encoding.UTF8);
                        lblStatus.Text = $"✓ Saved decrypted content to: {Path.GetFileName(dialog.FileName)}";
                        
                        Logger.Log($"[DecryptViewer] Saved decrypted file: {dialog.FileName}", Logger.LogLevel.Info);
                        MessageBox.Show("Decrypted content saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log($"[DecryptViewer] Save error: {ex.Message}", Logger.LogLevel.Error);
            }
        }
    }
}
