# Changelog

All notable changes to the UCD.Rosetta.Client project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-11-06

### Added
- Initial release of UCD.Rosetta.Client
- Full support for Rosetta API v1.0.10
- OAuth 2.0 client credentials authentication with automatic token management
- Auto-generated client from OpenAPI specification using NSwag
- Strongly-typed models and API methods
- Dependency injection support with `AddRosettaClient` and `AddRosettaClientWithFactory` extensions
- IHttpClientFactory integration for proper HttpClient lifecycle management
- System.Text.Json serialization for modern, high-performance JSON handling
- Comprehensive XML documentation for IntelliSense support
- Example console application demonstrating usage
- Support for all Rosetta API endpoints:
  - Identity and people management
  - Account operations
  - Employee and student data
  - Groups, organizations, and roles
  - Reference data (colleges, majors)
  - Campaign contacts export
- Automatic token caching and refresh
- Cancellation token support for all async operations
- Configurable timeout and API version
- Thread-safe OAuth token acquisition

### Developer Experience
- Complete README with examples and best practices
- Publishing guide for NuGet.org
- Update script for refreshing OpenAPI specification
- MIT License
- .NET 8.0 target framework
- Nullable reference types enabled
- XML documentation file generation

## [Unreleased]

### Planned
- GraphQL API support (pending IAM team rollout)
- Response caching options
- Retry policies with Polly integration
- Enhanced logging and diagnostics
- Rate limiting support
- Batch operation helpers

---

## Version History

- **1.0.0** - Initial stable release supporting Rosetta API v1.0.10
