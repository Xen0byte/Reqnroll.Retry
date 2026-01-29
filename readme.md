# Reqnroll.Retry

Reqnroll generator plugins that automatically add retry attributes to your BDD test methods. Useful for flaky tests that occasionally fail due to timing issues, network hiccups, or other transient problems.

## Packages

| Package                                                                       | Test Framework |
|:------------------------------------------------------------------------------|---------------:|
| [Retry.Reqnroll.MSTest](https://www.nuget.org/packages/Retry.Reqnroll.MSTest) | MSTest         |
| [Retry.Reqnroll.NUnit](https://www.nuget.org/packages/Retry.Reqnroll.NUnit)   | NUnit          |
| [Retry.Reqnroll.xUnit](https://www.nuget.org/packages/Retry.Reqnroll.xUnit)   | xUnit          |
| [Retry.Reqnroll.TUnit](https://www.nuget.org/packages/Retry.Reqnroll.TUnit)   | TUnit          |

## Installation

Install the package that matches your test framework:

```
dotnet add package Retry.Reqnroll.MSTest
dotnet add package Retry.Reqnroll.NUnit
dotnet add package Retry.Reqnroll.xUnit
dotnet add package Retry.Reqnroll.TUnit
```

## Configuration

By default, failed tests will retry once. You can change this by setting the `ReqnrollRetryCount` property in your test project:

```xml
<PropertyGroup>
    <ReqnrollRetryCount>2</ReqnrollRetryCount>
</PropertyGroup>
```

## How It Works

These are Reqnroll generator plugins. When Reqnroll generates the code-behind files for your feature files, the plugin intercepts the generation process and adds the appropriate retry attribute to each test method.
