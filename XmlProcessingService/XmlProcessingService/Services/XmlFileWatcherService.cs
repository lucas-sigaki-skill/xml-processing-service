using XmlProcessingService.Models;

namespace XmlProcessingService.Services
{
    public class XmlFileWatcherService : BackgroundService
    {
        private readonly ILogger<XmlFileWatcherService> _logger;
        private readonly ServiceConfiguration _config;
        private readonly XmlProcessor _xmlProcessor;
        private FileSystemWatcher? _fileWatcher;

        public XmlFileWatcherService(
            ILogger<XmlFileWatcherService> logger,
            ServiceConfiguration config,
            XmlProcessor xmlProcessor)
        {
            _logger = logger;
            _config = config;
            _xmlProcessor = xmlProcessor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("XML File Watcher Service starting...");

            try
            {
                // Ensure directories exist
                EnsureDirectoriesExist();

                // Perform initial scan
                await PerformInitialScanAsync();

                // Setup file system watcher
                SetupFileSystemWatcher();

                _logger.LogInformation("XML File Watcher Service started successfully");

                // Keep the service running
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(5000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in XML File Watcher Service");
            }
        }

        private void EnsureDirectoriesExist()
        {
            try
            {
                if (!Directory.Exists(_config.WatchFolder))
                {
                    Directory.CreateDirectory(_config.WatchFolder);
                    _logger.LogInformation($"Created watch folder: {_config.WatchFolder}");
                }

                if (!Directory.Exists(_config.ProcessedFolder))
                {
                    Directory.CreateDirectory(_config.ProcessedFolder);
                    _logger.LogInformation($"Created processed folder: {_config.ProcessedFolder}");
                }

                if (!Directory.Exists(_config.ErrorFolder))
                {
                    Directory.CreateDirectory(_config.ErrorFolder);
                    _logger.LogInformation($"Created error folder: {_config.ErrorFolder}");
                }

                // Ensure log directory exists
                var logDir = Path.GetDirectoryName(_config.LogFilePath);
                if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                    _logger.LogInformation($"Created log directory: {logDir}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating directories");
            }
        }

        private async Task PerformInitialScanAsync()
        {
            try
            {
                _logger.LogInformation("Performing initial scan of watch folder...");

                var xmlFiles = Directory.GetFiles(_config.WatchFolder, "*.xml", SearchOption.TopDirectoryOnly);
                _logger.LogInformation($"Found {xmlFiles.Length} XML files in watch folder");

                foreach (var file in xmlFiles)
                {
                    await ProcessFileAsync(file);
                }

                _logger.LogInformation("Initial scan completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during initial scan");
            }
        }

        private void SetupFileSystemWatcher()
        {
            try
            {
                _fileWatcher = new FileSystemWatcher(_config.WatchFolder)
                {
                    Filter = "*.xml",
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName,
                    EnableRaisingEvents = true
                };

                _fileWatcher.Created += OnFileCreated;
                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.Error += OnWatcherError;

                _logger.LogInformation($"File system watcher setup for folder: {_config.WatchFolder}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up file system watcher");
            }
        }

        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation($"New file detected: {e.FullPath}");
            await ProcessFileAsync(e.FullPath);
        }

        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation($"File changed: {e.FullPath}");
            await ProcessFileAsync(e.FullPath);
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            _logger.LogError(e.GetException(), "File system watcher error");
        }

        private async Task ProcessFileAsync(string filePath)
        {
            try
            {
                // Wait a bit to ensure file is completely written
                await Task.Delay(1000);

                // Check if file still exists and is not locked
                if (!File.Exists(filePath) || IsFileLocked(filePath))
                {
                    _logger.LogWarning($"File is locked or doesn't exist: {filePath}");
                    return;
                }

                _logger.LogInformation($"Processing file: {filePath}");

                // Process the XML file
                var success = await _xmlProcessor.ProcessXmlFileAsync(filePath);

                // Move file to appropriate folder
                await MoveFileAsync(filePath, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing file: {filePath}");
                await MoveFileAsync(filePath, false);
            }
        }

        private async Task MoveFileAsync(string sourceFile, bool success)
        {
            try
            {
                var fileName = Path.GetFileName(sourceFile);
                var destinationFolder = success ? _config.ProcessedFolder : _config.ErrorFolder;
                var destinationFile = Path.Combine(destinationFolder, fileName);

                // Handle duplicate file names
                var counter = 1;
                while (File.Exists(destinationFile))
                {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var extension = Path.GetExtension(fileName);
                    var newFileName = $"{nameWithoutExt}_{counter}{extension}";
                    destinationFile = Path.Combine(destinationFolder, newFileName);
                    counter++;
                }

                File.Move(sourceFile, destinationFile);
                _logger.LogInformation($"File moved to {(success ? "processed" : "error")} folder: {destinationFile}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error moving file: {sourceFile}");
            }
        }

        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        public override void Dispose()
        {
            _fileWatcher?.Dispose();
            base.Dispose();
        }
    }
}
