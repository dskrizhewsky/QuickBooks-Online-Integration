
# QuickBooks Online Integration for Azure-Based Services

This project provides a production-grade integration with QuickBooks Online, designed for business workflows hosted on Microsoft Azure.

## Purpose

The integration automates accounting operations such as journal entries, caching, and batch processing using the QuickBooks Online API. It is intended to be embedded in enterprise-grade backend systems.

## Project Structure

- `qbolib/` — Core library:
  - `Authorization.cs` — OAuth2 authorization
  - `Core/` — API query building, exception handling
  - `Journal/` — Journal entry and batching logic
  - `Repository.cs`, `ObjectCache.cs` — Repository and caching logic
- `qbo_integration.sln` — Solution file for Visual Studio

## Features

- Supports Azure-hosted environments
- OAuth2-based authentication with QuickBooks Online
- Batch processing and validation of journal entries
- In-memory caching for performance
- Modular, extensible architecture

## Technologies Used

- C# .NET
- QuickBooks Online API
- Microsoft Azure

## Getting Started

```bash
git clone https://github.com/your-username/QuickBooks-Online-Integration.git
cd QuickBooks-Online-Integration
start qbo_integration.sln
```

Before running, configure your QuickBooks developer credentials and Azure environment settings.

## License

MIT
