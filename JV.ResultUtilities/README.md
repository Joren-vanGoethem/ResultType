# JV.Utils.Result

## Overview

This package provides a robust Result type implementation for handling success and failure states in your application.
It includes type-safe validation messages using `TranslationKeyDefinition` to ensure that users provide the correct
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
  - [Best Practices](Validation-System.md#best-practices)
  - [Integration Examples](Validation-System.md#integration-examples)
- [Memoization](Memoization.md)
  - [What is Memoization](Memoization.md#what-is-memoization)
  - [Why use Memoization](Memoization.md#why-use-memoization)
  - [When to use Memoization](Memoization.md#when-to-use-memoization)
  - [When NOT to use Memoization](Memoization.md#when-not-to-use-memoization)
  - [Basic Usage](Memoization.md#basic-usage)
  - [Advanced Features](Memoization.md#advanced-features)
  - [Performance Considerations](Memoization.md#performance-considerations)
