# XML Processing Service Solution

This solution consists of two .NET Core 8 applications for processing NFCe and SAT CFe XML files:

1. **XmlProcessingService** - A Windows Service that monitors folders and processes XML files
2. **ServiceManagerApp** - A Windows Forms application for configuration and service management

## Features

### Windows Service (XmlProcessingService)
- **Folder Monitoring**: Watches a configured folder for new XML files using FileSystemWatcher
- **XML Processing**: Automatically detects and processes NFCe and SAT CFe XML files
- **Database Integration**: Inserts processed data into MSSQL Server or SAP HANA databases
- **File Management**: Moves processed files to success/error folders based on processing results
- **Logging**: Comprehensive logging with configurable log levels

### Windows Forms Application (ServiceManagerApp)
- **Configuration Management**: GUI for setting up folders, database connections, and service parameters
- **Service Control**: Install, uninstall, start, and stop the Windows Service
- **System Tray Integration**: Minimizes to system tray for background operation
- **Real-time Logging**: Displays service logs in real-time
- **Modern UI**: Clean, professional interface using standard Windows Forms controls

## XML File Support

### NFCe (Nota Fiscal de Consumidor Eletrônica)
- Root element: `<nfeProc>`
- Extracts: Document info, emitter details, items, totals, payment information
- Database table: `NFCe_Documents`

### SAT CFe (Sistema Autenticador e Transmissor - Cupom Fiscal Eletrônico)
- Root element: `<CFe>`
- Extracts: Document info, emitter details, items, totals, payment information
- Database table: `SAT_CFe_Documents`

## Database Schema

The service automatically creates the following tables if they don't exist:

### NFCe_Documents
- Id (Primary Key)
- ChaveNFe (Document Key)
- CNPJ (Company Tax ID)
- RazaoSocial (Company Name)
- DataEmissao (Issue Date)
- ValorTotal (Total Value)
- ArquivoOrigem (Source File)
- DataProcessamento (Processing Date)

### SAT_CFe_Documents
- Id (Primary Key)
- ChaveCFe (Document Key)
- CNPJ (Company Tax ID)
- RazaoSocial (Company Name)
- DataEmissao (Issue Date)
- ValorTotal (Total Value)
- ArquivoOrigem (Source File)
- DataProcessamento (Processing Date)

## Installation and Setup

### Prerequisites
- .NET 8.0 Runtime
- Windows Operating System
- MSSQL Server or SAP HANA database (optional)

### Installation Steps

1. **Build the Projects**:
   ```bash
   cd XmlProcessingService/XmlProcessingService
   dotnet publish -c Release -r win-x64 --self-contained
   
   cd ../../ServiceManagerApp/ServiceManagerApp
   dotnet publish -c Release -r win-x64 --self-contained
   ```

2. **Deploy Files**:
   - Copy the published service files to a permanent location (e.g., `C:\Services\XmlProcessingService\`)
   - Copy the published Windows Forms app to the desired location

3. **Configure the Service**:
   - Run the ServiceManagerApp.exe
   - Configure the following settings:
     - **Watch Folder**: Folder to monitor for XML files
     - **Processed Folder**: Destination for successfully processed files
     - **Error Folder**: Destination for files that failed processing
     - **Database Connection**: Connection string for your database
     - **Database Type**: Select MSSQL or HANA

4. **Install the Service**:
   - Click "Install Service" in the ServiceManagerApp
   - Start the service using "Start Service"

## Configuration

### Default Folder Structure
```
C:\XMLFiles\
├── Input\          (Watch folder)
├── Processed\      (Successfully processed files)
├── Error\          (Failed processing files)
└── Logs\           (Service log files)
```

### Database Connection Examples

**MSSQL Server**:
```
Server=localhost;Database=XMLProcessing;Integrated Security=true;
```

**SAP HANA**:
```
Server=localhost:30015;Database=XMLProcessing;UserID=username;Password=password;
```

## Usage

1. **Start the Service**: Use the ServiceManagerApp to start the Windows Service
2. **Add XML Files**: Place NFCe or SAT CFe XML files in the configured watch folder
3. **Monitor Processing**: Check the logs in ServiceManagerApp to see processing status
4. **Review Results**: Processed files are moved to appropriate folders, and data is inserted into the database

## Logging

The service logs all activities including:
- File detection and processing
- Database operations
- Error conditions
- Service lifecycle events

Logs are written to the configured log file and can be viewed in real-time through the ServiceManagerApp.

## Troubleshooting

### Common Issues

1. **Service Won't Start**:
   - Check that the service executable path is correct
   - Verify the service account has necessary permissions
   - Review the Windows Event Log for detailed error messages

2. **Files Not Processing**:
   - Ensure the watch folder exists and is accessible
   - Check file permissions
   - Verify XML file format is supported (NFCe or SAT CFe)

3. **Database Connection Issues**:
   - Verify connection string is correct
   - Check database server is running and accessible
   - Ensure database user has necessary permissions

4. **Files Stuck in Watch Folder**:
   - Check if files are locked by another process
   - Verify processed/error folders exist and are writable
   - Review service logs for specific error messages

## Development

### Project Structure
```
XmlProcessingService/
├── Models/
│   └── ServiceConfiguration.cs
├── Services/
│   ├── XmlFileWatcherService.cs
│   ├── XmlProcessor.cs
│   └── DbHelper.cs
├── Program.cs
└── appsettings.json

ServiceManagerApp/
├── MainForm.cs
├── Program.cs
└── ServiceManagerApp.csproj
```

### Key Components

- **XmlFileWatcherService**: Background service that monitors folders
- **XmlProcessor**: Handles XML parsing and data extraction
- **DbHelper**: Manages database operations
- **MainForm**: Windows Forms UI for service management

## License

This project is provided as-is for educational and development purposes.
