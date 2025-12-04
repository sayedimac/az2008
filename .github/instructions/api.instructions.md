---
applyTo: "src/api"
---

- This is a c# (dotnet 8.0) Function App that runs in Isolated Mode
- It connects to Azure Services via connection strings in environment variables  (like an Azure Storage Account)
- One Function retrieves  a list of blobs from a container (parameter)
- Another Function reads data from an Azure Storage Table with the partition and row keys (parameters)
