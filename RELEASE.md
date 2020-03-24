# Release Process

The release process consists of these steps:
1. Create a GitHub release with release notes. The tag name must be a semantic version, prefixed with "v" - e.g. `v0.1.0` or `v0.1.0-rc1`
1. Wait for the AppVeyor build to finish the *tag* build: https://ci.appveyor.com/project/jaegertracing/jaeger-client-csharp
1. As a signed-in AppVeyor user, click "Deploy" on the build details page and select "NuGet (JaegerTracing)".
This will upload the packages to NuGet.org

The authentication is handled by [generating an API Key at NuGet](https://www.nuget.org/account/ApiKeys) and saving it into [AppVeyor settings](https://ci.appveyor.com/account/jaegertracing/environment/37513/settings).
