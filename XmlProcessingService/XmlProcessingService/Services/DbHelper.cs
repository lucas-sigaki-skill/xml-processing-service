using Microsoft.Data.SqlClient;
using XmlProcessingService.Models;
using XmlProcessingService.Services;

namespace XmlProcessingService.Services
{
    public class DbHelper
    {
        private readonly ILogger<DbHelper> _logger;
        private readonly ServiceConfiguration _config;

        public DbHelper(ILogger<DbHelper> logger, ServiceConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<bool> InsertNFCeDocumentAsync(NFCeDocument document)
        {
            try
            {
                if (string.IsNullOrEmpty(_config.ConnectionString))
                {
                    _logger.LogWarning("Database connection string is not configured");
                    return false;
                }

                using var connection = new SqlConnection(_config.ConnectionString);
                await connection.OpenAsync();

                // Create table if not exists
                await CreateNFCeTableIfNotExistsAsync(connection);

                var insertQuery = @"
                    INSERT INTO NFCe_Documents 
                    (ChaveNFe, CNPJ, RazaoSocial, DataEmissao, ValorTotal, ArquivoOrigem, DataProcessamento)
                    VALUES 
                    (@ChaveNFe, @CNPJ, @RazaoSocial, @DataEmissao, @ValorTotal, @ArquivoOrigem, @DataProcessamento)";

                using var command = new SqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@ChaveNFe", document.ChaveNFe);
                command.Parameters.AddWithValue("@CNPJ", document.CNPJ);
                command.Parameters.AddWithValue("@RazaoSocial", document.RazaoSocial);
                command.Parameters.AddWithValue("@DataEmissao", document.DataEmissao);
                command.Parameters.AddWithValue("@ValorTotal", document.ValorTotal);
                command.Parameters.AddWithValue("@ArquivoOrigem", document.ArquivoOrigem);
                command.Parameters.AddWithValue("@DataProcessamento", document.DataProcessamento);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                _logger.LogInformation($"NFCe document inserted successfully. Rows affected: {rowsAffected}");
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting NFCe document into database");
                return false;
            }
        }

        public async Task<bool> InsertSATCFeDocumentAsync(SATCFeDocument document)
        {
            try
            {
                if (string.IsNullOrEmpty(_config.ConnectionString))
                {
                    _logger.LogWarning("Database connection string is not configured");
                    return false;
                }

                using var connection = new SqlConnection(_config.ConnectionString);
                await connection.OpenAsync();

                // Create table if not exists
                await CreateSATCFeTableIfNotExistsAsync(connection);

                var insertQuery = @"
                    INSERT INTO SAT_CFe_Documents 
                    (ChaveCFe, CNPJ, RazaoSocial, DataEmissao, ValorTotal, ArquivoOrigem, DataProcessamento)
                    VALUES 
                    (@ChaveCFe, @CNPJ, @RazaoSocial, @DataEmissao, @ValorTotal, @ArquivoOrigem, @DataProcessamento)";

                using var command = new SqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@ChaveCFe", document.ChaveCFe);
                command.Parameters.AddWithValue("@CNPJ", document.CNPJ);
                command.Parameters.AddWithValue("@RazaoSocial", document.RazaoSocial);
                command.Parameters.AddWithValue("@DataEmissao", document.DataEmissao);
                command.Parameters.AddWithValue("@ValorTotal", document.ValorTotal);
                command.Parameters.AddWithValue("@ArquivoOrigem", document.ArquivoOrigem);
                command.Parameters.AddWithValue("@DataProcessamento", document.DataProcessamento);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                _logger.LogInformation($"SAT CFe document inserted successfully. Rows affected: {rowsAffected}");
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting SAT CFe document into database");
                return false;
            }
        }

        private async Task CreateNFCeTableIfNotExistsAsync(SqlConnection connection)
        {
            var createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NFCe_Documents' AND xtype='U')
                CREATE TABLE NFCe_Documents (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    ChaveNFe NVARCHAR(100) NOT NULL,
                    CNPJ NVARCHAR(20) NOT NULL,
                    RazaoSocial NVARCHAR(255) NOT NULL,
                    DataEmissao DATETIME NOT NULL,
                    ValorTotal DECIMAL(18,2) NOT NULL,
                    ArquivoOrigem NVARCHAR(255) NOT NULL,
                    DataProcessamento DATETIME NOT NULL,
                    INDEX IX_NFCe_ChaveNFe (ChaveNFe),
                    INDEX IX_NFCe_CNPJ (CNPJ),
                    INDEX IX_NFCe_DataEmissao (DataEmissao)
                )";

            using var command = new SqlCommand(createTableQuery, connection);
            await command.ExecuteNonQueryAsync();
        }

        private async Task CreateSATCFeTableIfNotExistsAsync(SqlConnection connection)
        {
            var createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SAT_CFe_Documents' AND xtype='U')
                CREATE TABLE SAT_CFe_Documents (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    ChaveCFe NVARCHAR(100) NOT NULL,
                    CNPJ NVARCHAR(20) NOT NULL,
                    RazaoSocial NVARCHAR(255) NOT NULL,
                    DataEmissao DATETIME NOT NULL,
                    ValorTotal DECIMAL(18,2) NOT NULL,
                    ArquivoOrigem NVARCHAR(255) NOT NULL,
                    DataProcessamento DATETIME NOT NULL,
                    INDEX IX_SATCFe_ChaveCFe (ChaveCFe),
                    INDEX IX_SATCFe_CNPJ (CNPJ),
                    INDEX IX_SATCFe_DataEmissao (DataEmissao)
                )";

            using var command = new SqlCommand(createTableQuery, connection);
            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_config.ConnectionString))
                {
                    _logger.LogWarning("Database connection string is not configured");
                    return false;
                }

                using var connection = new SqlConnection(_config.ConnectionString);
                await connection.OpenAsync();
                _logger.LogInformation("Database connection test successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                return false;
            }
        }
    }
}
