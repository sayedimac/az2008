# CopilotTest

A minimal Blazor WebAssembly app with .NET 8 Azure Functions backend for browsing Azure Storage resources.

## Project Structure

```
├── src/
│   ├── web/        # Blazor WebAssembly client app
│   └── api/        # Azure Functions (.NET isolated)
├── CopilotTest.sln # Solution file
└── .gitignore
```

## Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4+ (for local function development)
- An Azure Storage Account (for production use)

## Features

### API Functions

1. **ListBlobs** (`GET /api/blobs/{containerName}`)
   - Lists all blobs in a specified Azure Storage container
   - Returns blob name, size, content type, and last modified date

2. **ReadTableData** (`GET /api/table/{tableName}/{partitionKey}/{rowKey}`)
   - Reads a specific entity from an Azure Storage Table
   - Returns all properties of the entity

### Web Application

- **Blob Storage Browser**: Browse and list blobs in any container
- **Table Data Browser**: Query table entities by partition key and row key

## Local Development

### Build the solution

```bash
dotnet build
```

### Run the web app

```bash
dotnet run --project src/web
```

### Run the functions locally

```bash
cd src/api
func start
```

### Configuration

For local development, update `src/api/local.settings.json` with your storage connection string:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "StorageConnectionString": "YOUR_CONNECTION_STRING_HERE"
  }
}
```

## Deployment

This app is designed to be deployed to Azure Static Web Apps. See the GitHub Actions workflow for automated deployments.

### Environment Variables

Set the following in your Azure Function App settings:

- `StorageConnectionString`: Your Azure Storage Account connection string

## License

MIT
