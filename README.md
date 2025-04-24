# GraphQL Fragment Source Generator

A Roslyn-based C# Source Generator that scans GraphQL files for fragment definitions and produces matching C# types.

## Features

- Automatically generates C# types from GraphQL fragment definitions
- Preserves GraphQL scalar-to-C# type mappings
- Supports nullable reference types to reflect GraphQL nullability
- Handles nested selections with nested types
- Supports `@deprecated` directive with `[Obsolete]` attributes
- Handles fragment spreads through composition
- Configurable output (namespace, records vs classes, etc.)
- Comprehensive error reporting and diagnostics

## Installation

### NuGet Package (Recommended)

Install the package from NuGet:

```bash
dotnet add package GraphQLSourceGen
```

Or add it directly to your project file:

```xml
<ItemGroup>
  <PackageReference Include="GraphQLSourceGen" Version="1.0.0" />
</ItemGroup>
```

### Project Reference (For Development)

If you're developing or customising the generator, add a reference to the source generator project:

```xml
<ItemGroup>
  <ProjectReference Include="..\GraphQLSourceGen\GraphQLSourceGen.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="true" />
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

3. The source generator will automatically process these files and generate C# types for each fragment

## Configuration

You can configure the generator using MSBuild properties in your project file:

```xml
<PropertyGroup>
  <!-- Customise the namespace for generated types -->
  <GraphQLSourceGenNamespace>MyCompany.GraphQL.Generated</GraphQLSourceGenNamespace>
  
  <!-- Use classes instead of records -->
  <GraphQLSourceGenUseRecords>false</GraphQLSourceGenUseRecords>
  
  <!-- Use regular properties instead of init-only properties -->
  <GraphQLSourceGenUseInitProperties>false</GraphQLSourceGenUseInitProperties>
  
  <!-- Disable XML documentation comments -->
  <GraphQLSourceGenGenerateDocComments>false</GraphQLSourceGenGenerateDocComments>
</PropertyGroup>

## XML Documentation Comments

By default, the generator adds XML documentation comments to the generated C# types. These comments provide:

1. **Class-level documentation**: Shows which GraphQL fragment and type the class was generated from
   ```csharp
   /// <summary>
   /// Generated from GraphQL fragment 'UserBasic' on type 'User'
   /// </summary>
   ```

2. **Property-level documentation**: Includes the original field name from the GraphQL schema
   ```csharp
   /// <summary>
   /// id
   /// </summary>
   public string? Id { get; init; }
   ```

These comments are automatically extracted from your GraphQL schema files during the build process. If your GraphQL schema includes descriptions for types and fields, those descriptions will also be included in the XML comments.

Benefits of XML documentation comments:
- Maintains the connection between your C# code and the GraphQL schema
- Provides IntelliSense documentation when using the generated types
- Can be used to generate API documentation

You can disable the generation of these comments using the `GraphQLSourceGenGenerateDocComments` property as shown above.

## Examples

### Basic Fragment

```graphql
fragment UserBasic on User {
  id
  name
  email
  isActive
}
```

Generated C# type:

```csharp
using System;
using System.Collections.Generic;

#nullable enable

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

### Fragment with Nested Objects

```graphql
fragment UserWithProfile on User {
  id
  name
  profile {
    bio
    avatarUrl
    joinDate
  }
}
```

### Fragment with Non-Nullable Fields

```graphql
fragment RequiredUserInfo on User {
  id!
  name!
  email
}
```

### Fragment with Deprecated Fields

```graphql
fragment UserWithDeprecated on User {
  id
  name
  username @deprecated(reason: "Use email instead")
  oldField @deprecated
}
```

### Fragment that References Another Fragment

```graphql
fragment UserWithPosts on User {
  ...UserBasic
  posts {
    id
    title
  }
}
```

## Troubleshooting

### Common Issues

#### No Code is Generated

Make sure:
- Your `.graphql` files are marked as `AdditionalFiles` in your project file
- Your GraphQL files contain valid fragment definitions
- You've added the package reference correctly

#### Error GQLSG001: Invalid GraphQL syntax

This indicates a syntax error in your GraphQL file. Check the error message for details on the specific issue.

#### Error GQLSG002: No GraphQL fragments found

The GraphQL file doesn't contain any fragment definitions. Make sure you're using the `fragment Name on Type { ... }` syntax.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
