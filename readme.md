# Reqnroll.Retry

Reqnroll generator plugins that automatically add retries to your BDD test methods. Useful for flaky tests that occasionally fail due to timing issues, network hiccups, or other transient problems.

## Packages

| Package                                                                       | Test Framework | Requirements                                              |
|:------------------------------------------------------------------------------|:---------------|:----------------------------------------------------------|
| [Retry.Reqnroll.MSTest](https://www.nuget.org/packages/Retry.Reqnroll.MSTest) | MSTest         | MSTest 3.8 or later                                       |
| [Retry.Reqnroll.NUnit](https://www.nuget.org/packages/Retry.Reqnroll.NUnit)   | NUnit          | NUnit 4.5 or later                                        |
| [Retry.Reqnroll.xUnit](https://www.nuget.org/packages/Retry.Reqnroll.xUnit)   | xUnit          | xUnit v2 (Reqnroll.xUnit) or xUnit v3 (Reqnroll.xunit.v3) |
| [Retry.Reqnroll.TUnit](https://www.nuget.org/packages/Retry.Reqnroll.TUnit)   | TUnit          |                                                           |

All packages require Reqnroll 3, and have no dependencies of their own.

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

Setting it to `0` turns retries off. Anything other than a whole number of `0` or more fails the build.

## What Is Retried

A failed test is retried until it passes or no retries are left, whether it failed an assertion or threw an exception. Ignored and skipped tests are never retried.

With xUnit, tests with pending or undefined steps, or with binding errors, are not retried either, because they would fail the same way on every attempt. Neither are tests in an xUnit v3 test run that is being cancelled.

## How It Works

These are Reqnroll generator plugins. When Reqnroll generates the code-behind files for your feature files, the plugin intercepts the generation process and adds the appropriate retry attribute to each test method. xUnit has no retry attribute, so for xUnit the plugin wraps each test method in a retry loop instead, without any additional dependencies.
