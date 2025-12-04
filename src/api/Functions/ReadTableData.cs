using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Api.Functions;

public class ReadTableData
{
    private readonly ILogger<ReadTableData> _logger;

    public ReadTableData(ILogger<ReadTableData> logger)
    {
        _logger = logger;
    }

    [Function("ReadTableData")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "table/{tableName}/{partitionKey}/{rowKey}")] HttpRequest req,
        string tableName,
        string partitionKey,
        string rowKey)
    {
        _logger.LogInformation("ReadTableData function processing request for table: {TableName}, partition: {PartitionKey}, row: {RowKey}", 
            tableName, partitionKey, rowKey);

        try
        {
            var connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("StorageConnectionString environment variable is not set");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            var tableServiceClient = new TableServiceClient(connectionString);
            var tableClient = tableServiceClient.GetTableClient(tableName);

            var response = await tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
            
            if (response?.Value == null)
            {
                _logger.LogWarning("Entity not found in table {TableName} with partition {PartitionKey} and row {RowKey}", 
                    tableName, partitionKey, rowKey);
                return new NotFoundObjectResult($"Entity not found");
            }

            var entity = response.Value;
            var result = new Dictionary<string, object?>
            {
                { "PartitionKey", entity.PartitionKey },
                { "RowKey", entity.RowKey },
                { "Timestamp", entity.Timestamp }
            };

            foreach (var key in entity.Keys)
            {
                if (key != "PartitionKey" && key != "RowKey" && key != "Timestamp" && key != "odata.etag")
                {
                    result[key] = entity[key];
                }
            }

            _logger.LogInformation("Successfully retrieved entity from table {TableName}", tableName);
            return new OkObjectResult(result);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Entity or table not found: {Message}", ex.Message);
            return new NotFoundObjectResult("Entity or table not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading from table {TableName}", tableName);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
