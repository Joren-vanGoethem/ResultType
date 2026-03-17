# JV.Utils.Result

## Overview

This package provides a robust Result type implementation for handling success and failure states in your application.
It includes type-safe validation messages using `ValidationKeyDefinition` to ensure that users provide the correct
number and types of parameters when creating validation messages.

Results are binary - they are either successful (contain no validation messages) or unsuccessful (contain one or more
validation messages).

## Table of Contents

- [Result Types](Result-Types.md)
  - [Core Concepts](Result-Types.md#core-concepts)
  - [Binary Result State](Result-Types.md#binary-result-state)
  - [Type Safety](Result-Types.md#type-safety)
  - [Result Types](Result-Types.md#result-type)
  - [Result Creation Patterns](Result-Types.md#result-creation-patterns)
  - [Extensions Methods](Result-Types.md#extension-methods)
  - [Usage Examples](Result-Types.md#practical-usage-examples)
  - [Best Practices](Result-Types.md#best-practices)
- [Validation System](Validation-System.md)
  - [Core Components](Validation-System.md#core-components)
  - [Basic Usage](Validation-System.md#basic-usage)
  - [Validation Rules](Validation-System.md#validation-rules)
  - [Translation Key Definition](Validation-System.md#translation-key-definitions)
  - [Validation Message Types](Validation-System.md#validation-message-types)
  - [Advanced Pipeline Patterns](Validation-System.md#advanced-pipeline-patterns)
  - [Integration Examples](Validation-System.md#integration-examples)
- [Memoization](Memoization.md)
  - [What is Memoization](Memoization.md#what-is-memoization)
  - [Why use Memoization](Memoization.md#why-use-memoization)
  - [When to use Memoization](Memoization.md#when-to-use-memoization)
  - [When NOT to use Memoization](Memoization.md#when-not-to-use-memoization)
  - [Basic Usage](Memoization.md#basic-usage)
  - [Advanced Features](Memoization.md#advanced-features)
  - [Performance Considerations](Memoization.md#performance-considerations)
- [Features Guide](Features-Guide.md) - All features explained with real-world examples and when-to-use guidance
  - [Result Creation](Features-Guide.md#result-creation)
  - [Accessing Values Safely](Features-Guide.md#accessing-values-safely)
  - [Functional Combinators](Features-Guide.md#functional-combinators) (Map, Bind, Match, Ensure, Do)
  - [Merging Results](Features-Guide.md#merging-results)
  - [Collection Operations](Features-Guide.md#collection-operations) (TraverseAll, TraversePartialWithErrors)
  - [Validation Messages](Features-Guide.md#validation-messages) (Keys, Parameters, FieldName, CreateLenient, RawParameters)
  - [Validation Pipeline](Features-Guide.md#validation-pipeline) (Sync, Async, ShortCircuit)
  - [Exception Bridging](Features-Guide.md#exception-bridging)
  - [Memoization](Features-Guide.md#memoization) (Sync, Async, Expiring, Result-specific)
  - [Async Pipelines](Features-Guide.md#async-pipelines)
