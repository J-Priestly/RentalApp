!https://img.shields.io/badge/.NET-9.0-512BD4
!https://img.shields.io/badge/.NET%20MAUI-cross--platform-512BD4
!https://img.shields.io/badge/PostgreSQL-16-336791
!https://img.shields.io/badge/tests-96%20passing-brightgreen
!https://img.shields.io/badge/Docker-compose-2496ED
!https://img.shields.io/badge/license-MIT-green
# RentalApp – Rental Marketplace

A .NET MAUI mobile application allowing members to list, find, and rent items from each other. 

## Features

- User authentication via JWT API
- Browse and search all available items
- Create and manage item listings
- Location based nearby item discovery using PostGIS spatial queries
- Full rental workflow (request, approve, reject, return, complete)
- Reviews and ratings
- State Pattern for rental management
- Offline fallback to local SQLite database



## Setup Instructions

Clone the repo and cd into it
Start the database: docker compose up -d
Copy the settings template: cp RentalApp.Database/appsettings.json.template RentalApp.Database/appsettings.json
Build: dotnet build RentalApp.Database/RentalApp.Database.csproj

## Running Tests
dotnet test RentalApp.Test/RentalApp.Test.csproj

## API
The app connects to the SET09102 API: https://set09102-api.b-davison.workers.dev/

### Prerequisites

- Docker Desktop
- .NET 9 SDK
- Android Emulator or physical device
- ADB (Android Debug Bridge)

### 1. Clone the repository

```bash
git clone https://github.com/J-Priestly/RentalApp.git
cd RentalApp
