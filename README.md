# GraphQL Fragment Source Generator

A Roslyn-based C# Source Generator that scans GraphQL files for fragment definitions and produces matching C# record types.

## Features

- Automatically generates C# record types from GraphQL fragment definitions
- Preserves GraphQL scalar-to-C# type mappings
- Supports nullable reference types to reflect GraphQL nullability
- Handles nested selections with nested record types
- Supports `@deprecated` directive with `[Obsolete]` attributes
- Handles fragment spreads through composition

## Installation

Add a reference to the source generator project in your .NET 6+ SDK-style project:

```xml
<ItemGroup>
  <ProjectReference Include="..\GraphQLSourceGen\GraphQLSourceGen.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

## Usage

1. Add `.graphql` files to your project with fragment definitions
2. Mark these files as `AdditionalFiles` in your project file:

```xml
<ItemGroup>
  <AdditionalFiles Include="**\*.graphql" />
</ItemGroup>
```

3. The source generator will automatically process these files and generate C# record types for each fragment

## Example

### GraphQL Fragment

```graphql
fragment UserBasic on User {
  id
  name
  email
  isActive
}
```

### Generated C# Record

```csharp
using System;
using System.Collections.Generic;

namespace GraphQL.Generated
{
    /// <summary>
    /// Generated from GraphQL fragment 'UserBasic' on type 'User'
    /// </summary>
    public record UserBasicFragment
    {
        /// <summary>
        /// id
        /// </summary>
        public string? Id { get; init; }

        /// <summary>
        /// name
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// email
        /// </summary>
        public string? Email { get; init; }

        /// <summary>
        /// isActive
        /// </summary>
        public bool? IsActive { get; init; }
    }
}
```
