# ITAMS Project Structure

## Root Directory
- `appsettings.json` - Application configuration
- `Program.cs` - Application entry point
- `ITAMS.csproj` - Project file
- `README.md` - Project documentation

## Backend Structure

### `/Controllers`
API controllers for handling HTTP requests

### `/Services`
Business logic and service layer

### `/Middleware`
Custom middleware components (authentication, activity tracking, etc.)

### `/Data`
Database context and repositories

### `/Domain`
- `/Entities` - Database entities/models
- `/Interfaces` - Service interfaces

### `/Models`
DTOs (Data Transfer Objects) for API requests/responses

### `/Utilities`
Helper classes and utility functions

### `/Migrations`
Database migration SQL scripts

## Frontend Structure

### `/itams-frontend`
Angular application
- `/src/app` - Application components and services

## Scripts & Testing

### `/scripts/testing`
Test scripts and verification tools:
- `test-*.ps1` - Test scripts
- `check-*.ps1` - Verification scripts
- `verify-*.ps1` - Validation scripts
- `*.txt` - Test results and logs

### `/scripts/sql-queries`
SQL query files for database operations

### `/scripts/migrations`
Migration runner scripts

### `/scripts/database-setup`
Database initialization and setup scripts

## Documentation

### `/docs`
Project documentation:
- `/guides` - Implementation guides
- `/setup` - Setup instructions
- `*.md` - Technical documentation

## Logs

### `/logs`
Application log files (gitignored)
