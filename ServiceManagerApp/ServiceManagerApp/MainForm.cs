using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text.Json;

namespace ServiceManagerApp
{
    public partial class MainForm : Form
    {
        private readonly ServiceConfiguration _config;
        private readonly string _configFilePath;
        private NotifyIcon? _notifyIcon;
        private System.Windows.Forms.Timer? _logUpdateTimer;

        public MainForm()
        {
            InitializeComponent();
            _configFilePath = Path.Combine(Application.StartupPath, "service-config.json");
            _config = LoadConfiguration();
            LoadConfigurationToUI();
            SetupSystemTray();
            SetupLogUpdateTimer();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "XML Processing Service Manager";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(600, 400);
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);

            // Configuration Panel
            var configPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(740, 200),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };

            var configLabel = new Label
            {
                Text = "Service Configuration",
                Location = new Point(10, 10),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Black
            };

            // Watch Folder
            var watchFolderLabel = new Label
            {
                Text = "Watch Folder:",
                Location = new Point(10, 45),
                Size = new Size(100, 20),
                ForeColor = Color.Black
            };

            txtWatchFolder = new TextBox
            {
                Location = new Point(120, 43),
                Size = new Size(400, 23),
                BackColor = Color.White
            };

            var btnBrowseWatch = new Button
            {
                Text = "Browse",
                Location = new Point(530, 42),
                Size = new Size(75, 25),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            btnBrowseWatch.Click += BtnBrowseWatch_Click;

            // Processed Folder
            var processedFolderLabel = new Label
            {
                Text = "Processed Folder:",
                Location = new Point(10, 75),
                Size = new Size(100, 20),
                ForeColor = Color.Black
            };

            txtProcessedFolder = new TextBox
            {
                Location = new Point(120, 73),
                Size = new Size(400, 23),
                BackColor = Color.White
            };

            var btnBrowseProcessed = new Button
            {
                Text = "Browse",
                Location = new Point(530, 72),
                Size = new Size(75, 25),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            btnBrowseProcessed.Click += BtnBrowseProcessed_Click;

            // Error Folder
            var errorFolderLabel = new Label
            {
                Text = "Error Folder:",
                Location = new Point(10, 105),
                Size = new Size(100, 20),
                ForeColor = Color.Black
            };

            txtErrorFolder = new TextBox
            {
                Location = new Point(120, 103),
                Size = new Size(400, 23),
                BackColor = Color.White
            };

            var btnBrowseError = new Button
            {
                Text = "Browse",
                Location = new Point(530, 102),
                Size = new Size(75, 25),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            btnBrowseError.Click += BtnBrowseError_Click;

            // Database Connection
            var dbConnectionLabel = new Label
            {
                Text = "DB Connection:",
                Location = new Point(10, 135),
                Size = new Size(100, 20),
                ForeColor = Color.Black
            };

            txtConnectionString = new TextBox
            {
                Location = new Point(120, 133),
                Size = new Size(400, 23),
                BackColor = Color.White,
                UseSystemPasswordChar = true
            };

            // Database Type
            var dbTypeLabel = new Label
            {
                Text = "DB Type:",
                Location = new Point(530, 135),
                Size = new Size(60, 20),
                ForeColor = Color.Black
            };

            cmbDatabaseType = new ComboBox
            {
                Location = new Point(595, 133),
                Size = new Size(100, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            cmbDatabaseType.Items.AddRange(new[] { "MSSQL", "HANA" });

            // Save Configuration Button
            var btnSaveConfig = new Button
            {
                Text = "Save Configuration",
                Location = new Point(10, 165),
                Size = new Size(150, 25),
                BackColor = Color.DarkGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSaveConfig.Click += BtnSaveConfig_Click;

            // Add controls to config panel
            configPanel.Controls.AddRange(new Control[] {
                configLabel, watchFolderLabel, txtWatchFolder, btnBrowseWatch,
                processedFolderLabel, txtProcessedFolder, btnBrowseProcessed,
                errorFolderLabel, txtErrorFolder, btnBrowseError,
                dbConnectionLabel, txtConnectionString, dbTypeLabel, cmbDatabaseType,
                btnSaveConfig
            });

            // Service Control Panel
            var servicePanel = new Panel
            {
                Location = new Point(20, 240),
                Size = new Size(740, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };

            var serviceLabel = new Label
            {
                Text = "Service Control",
                Location = new Point(10, 10),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Black
            };

            btnInstall = new Button
            {
                Text = "Install Service",
                Location = new Point(10, 40),
                Size = new Size(120, 30),
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnInstall.Click += BtnInstall_Click;

            btnUninstall = new Button
            {
                Text = "Uninstall Service",
                Location = new Point(140, 40),
                Size = new Size(120, 30),
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUninstall.Click += BtnUninstall_Click;

            btnStart = new Button
            {
                Text = "Start Service",
                Location = new Point(270, 40),
                Size = new Size(120, 30),
                BackColor = Color.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnStart.Click += BtnStart_Click;

            btnStop = new Button
            {
                Text = "Stop Service",
                Location = new Point(400, 40),
                Size = new Size(120, 30),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnStop.Click += BtnStop_Click;

            lblServiceStatus = new Label
            {
                Text = "Service Status: Unknown",
                Location = new Point(530, 45),
                Size = new Size(200, 20),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            servicePanel.Controls.AddRange(new Control[] {
                serviceLabel, btnInstall, btnUninstall, btnStart, btnStop, lblServiceStatus
            });

            // Log Panel
            var logLabel = new Label
            {
                Text = "Service Logs",
                Location = new Point(20, 330),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Black
            };

            txtLogs = new TextBox
            {
                Location = new Point(20, 360),
                Size = new Size(740, 200),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 8F)
            };

            // Add all controls to form
            this.Controls.AddRange(new Control[] {
                configPanel, servicePanel, logLabel, txtLogs
            });

            this.ResumeLayout(false);
        }

        // Control declarations
        private TextBox txtWatchFolder = null!;
        private TextBox txtProcessedFolder = null!;
        private TextBox txtErrorFolder = null!;
        private TextBox txtConnectionString = null!;
        private ComboBox cmbDatabaseType = null!;
        private Button btnInstall = null!;
        private Button btnUninstall = null!;
        private Button btnStart = null!;
        private Button btnStop = null!;
        private Label lblServiceStatus = null!;
        private TextBox txtLogs = null!;

        private ServiceConfiguration LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var json = File.ReadAllText(_configFilePath);
                    return JsonSerializer.Deserialize<ServiceConfiguration>(json) ?? new ServiceConfiguration();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return new ServiceConfiguration
            {
                WatchFolder = @"C:\XMLFiles\Input",
                ProcessedFolder = @"C:\XMLFiles\Processed",
                ErrorFolder = @"C:\XMLFiles\Error",
                DatabaseType = "MSSQL",
                ConnectionString = "",
                LogFilePath = @"C:\XMLFiles\Logs\service.log"
            };
        }

        private void LoadConfigurationToUI()
        {
            txtWatchFolder.Text = _config.WatchFolder;
            txtProcessedFolder.Text = _config.ProcessedFolder;
            txtErrorFolder.Text = _config.ErrorFolder;
            txtConnectionString.Text = _config.ConnectionString;
            cmbDatabaseType.SelectedItem = _config.DatabaseType;
        }

        private void SetupSystemTray()
        {
            _notifyIcon = new NotifyIcon
            {
                Text = "XML Processing Service Manager",
                Visible = true
            };

            // Create a simple icon programmatically
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.FillRectangle(Brushes.Blue, 0, 0, 16, 16);
                g.DrawString("X", new Font("Arial", 8), Brushes.White, 2, 2);
            }
            _notifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());

            _notifyIcon.DoubleClick += (s, e) => {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, (s, e) => {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            });
            contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());
            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void SetupLogUpdateTimer()
        {
            _logUpdateTimer = new System.Windows.Forms.Timer
            {
                Interval = 5000 // Update every 5 seconds
            };
            _logUpdateTimer.Tick += UpdateLogs;
            _logUpdateTimer.Start();
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(value);
            if (!value)
            {
                this.ShowInTaskbar = false;
            }
        }

        private void BtnBrowseWatch_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtWatchFolder.Text = dialog.SelectedPath;
            }
        }

        private void BtnBrowseProcessed_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtProcessedFolder.Text = dialog.SelectedPath;
            }
        }

        private void BtnBrowseError_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtErrorFolder.Text = dialog.SelectedPath;
            }
        }

        private void BtnSaveConfig_Click(object? sender, EventArgs e)
        {
            try
            {
                _config.WatchFolder = txtWatchFolder.Text;
                _config.ProcessedFolder = txtProcessedFolder.Text;
                _config.ErrorFolder = txtErrorFolder.Text;
                _config.ConnectionString = txtConnectionString.Text;
                _config.DatabaseType = cmbDatabaseType.SelectedItem?.ToString() ?? "MSSQL";

                var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configFilePath, json);

                // Also update the service's appsettings.json
                UpdateServiceConfiguration();

                MessageBox.Show("Configuration saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateServiceConfiguration()
        {
            try
            {
                var serviceConfigPath = Path.Combine(Application.StartupPath, "XmlProcessingService", "appsettings.json");
                if (File.Exists(serviceConfigPath))
                {
                    var serviceConfig = new
                    {
                        Logging = new
                        {
                            LogLevel = new
                            {
                                Default = "Information",
                                Microsoft_Hosting_Lifetime = "Information"
                            }
                        },
                        ServiceConfiguration = _config
                    };

                    var json = JsonSerializer.Serialize(serviceConfig, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(serviceConfigPath, json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Warning: Could not update service configuration: {ex.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnInstall_Click(object? sender, EventArgs e)
        {
            try
            {
                var serviceExePath = Path.Combine(Application.StartupPath, "XmlProcessingService", "XmlProcessingService.exe");
                var result = ExecuteCommand($"sc create \"XML Processing Service\" binPath=\"{serviceExePath}\" start=auto");
                
                if (result.Contains("SUCCESS"))
                {
                    MessageBox.Show("Service installed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateServiceStatus();
                }
                else
                {
                    MessageBox.Show($"Error installing service: {result}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error installing service: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUninstall_Click(object? sender, EventArgs e)
        {
            try
            {
                var result = ExecuteCommand("sc delete \"XML Processing Service\"");
                
                if (result.Contains("SUCCESS"))
                {
                    MessageBox.Show("Service uninstalled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateServiceStatus();
                }
                else
                {
                    MessageBox.Show($"Error uninstalling service: {result}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uninstalling service: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            try
            {
                var result = ExecuteCommand("sc start \"XML Processing Service\"");
                
                if (result.Contains("SUCCESS") || result.Contains("PENDING"))
                {
                    MessageBox.Show("Service start command sent!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateServiceStatus();
                }
                else
                {
                    MessageBox.Show($"Error starting service: {result}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting service: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            try
            {
                var result = ExecuteCommand("sc stop \"XML Processing Service\"");
                
                if (result.Contains("SUCCESS") || result.Contains("PENDING"))
                {
                    MessageBox.Show("Service stop command sent!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateServiceStatus();
                }
                else
                {
                    MessageBox.Show($"Error stopping service: {result}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping service: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ExecuteCommand(string command)
        {
            try
            {
                var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(processInfo);
                if (process != null)
                {
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    
                    return string.IsNullOrEmpty(error) ? output : error;
                }
            }
            catch (Exception ex)
            {
                return $"Error executing command: {ex.Message}";
            }
            
            return "Unknown error";
        }

        private void UpdateServiceStatus()
        {
            try
            {
                var result = ExecuteCommand("sc query \"XML Processing Service\"");
                
                if (result.Contains("RUNNING"))
                {
                    lblServiceStatus.Text = "Service Status: Running";
                    lblServiceStatus.ForeColor = Color.Green;
                }
                else if (result.Contains("STOPPED"))
                {
                    lblServiceStatus.Text = "Service Status: Stopped";
                    lblServiceStatus.ForeColor = Color.Red;
                }
                else if (result.Contains("does not exist"))
                {
                    lblServiceStatus.Text = "Service Status: Not Installed";
                    lblServiceStatus.ForeColor = Color.Gray;
                }
                else
                {
                    lblServiceStatus.Text = "Service Status: Unknown";
                    lblServiceStatus.ForeColor = Color.Orange;
                }
            }
            catch
            {
                lblServiceStatus.Text = "Service Status: Error";
                lblServiceStatus.ForeColor = Color.Red;
            }
        }

        private void UpdateLogs(object? sender, EventArgs e)
        {
            try
            {
                if (File.Exists(_config.LogFilePath))
                {
                    var lines = File.ReadAllLines(_config.LogFilePath);
                    var lastLines = lines.TakeLast(50).ToArray();
                    txtLogs.Text = string.Join(Environment.NewLine, lastLines);
                    
                    // Auto-scroll to bottom
                    txtLogs.SelectionStart = txtLogs.Text.Length;
                    txtLogs.ScrollToCaret();
                }
            }
            catch
            {
                // Ignore errors when reading log file
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                this.ShowInTaskbar = false;
                _notifyIcon?.ShowBalloonTip(2000, "XML Service Manager", "Application minimized to tray", ToolTipIcon.Info);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _notifyIcon?.Dispose();
                _logUpdateTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class ServiceConfiguration
    {
        public string WatchFolder { get; set; } = string.Empty;
        public string ProcessedFolder { get; set; } = string.Empty;
        public string ErrorFolder { get; set; } = string.Empty;
        public string DatabaseType { get; set; } = "MSSQL";
        public string ConnectionString { get; set; } = string.Empty;
        public string LogFilePath { get; set; } = string.Empty;
    }
}
