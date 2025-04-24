# GraphQL Fragment Source Generator

A Roslyn-based C# Source Generator that scans GraphQL files for fragment definitions and produces matching C# types.

## Features

- Automatically generates C# types from GraphQL fragment definitions
- **Schema-aware type inference** for accurate field types
- Support for complex schema features (interfaces, unions, custom scalars)
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
  <!-- Basic configuration -->
  <GraphQLSourceGenNamespace>MyCompany.GraphQL.Generated</GraphQLSourceGenNamespace>
  <GraphQLSourceGenUseRecords>true</GraphQLSourceGenUseRecords>
  <GraphQLSourceGenUseInitProperties>true</GraphQLSourceGenUseInitProperties>
  <GraphQLSourceGenGenerateDocComments>true</GraphQLSourceGenGenerateDocComments>
  
  <!-- Schema-aware configuration -->
  <GraphQLSourceGenUseSchemaForTypeInference>true</GraphQLSourceGenUseSchemaForTypeInference>
  <GraphQLSourceGenValidateNonNullableFields>true</GraphQLSourceGenValidateNonNullableFields>
  <GraphQLSourceGenIncludeFieldDescriptions>true</GraphQLSourceGenIncludeFieldDescriptions>
  
  <!-- Schema files (semicolon-separated list) -->
  <GraphQLSourceGenSchemaFiles>schema.graphql;schema-extensions.graphql</GraphQLSourceGenSchemaFiles>
  
  <!-- Custom scalar mappings (semicolon-separated list of name:type pairs) -->
  <GraphQLSourceGenCustomScalarMappings>DateTime:System.DateTime;Upload:System.IO.Stream</GraphQLSourceGenCustomScalarMappings>
</PropertyGroup>
```

Schema-Aware Configuration Options
| Option | Default | Description |
|--------|---------|-------------|
| GraphQLSourceGenUseSchemaForTypeInference | true | Enable schema-based type inference for more accurate types |
| GraphQLSourceGenValidateNonNullableFields | true | Generate validation for non-nullable fields |
| GraphQLSourceGenIncludeFieldDescriptions | true | Include field descriptions from schema in generated code |
| GraphQLSourceGenSchemaFiles | "" | Semicolon-separated list of schema files to use for type inference |
| GraphQLSourceGenCustomScalarMappings | "DateTime:System.DateTime;Date:System.DateOnly;Time:System.TimeOnly" | Custom scalar type mappings |


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

## Schema-Aware Type Generation

The generator supports schema-aware type generation, which provides more accurate type information for your GraphQL fragments.

### Benefits of Schema-Aware Generation

1. **Accurate Type Inference**: The generator uses the schema to determine the exact type of each field, including nullability.
2. **Support for Complex Types**: Properly handles interfaces, unions, custom scalars, and nested types.
3. **Type Validation**: Validates that fields referenced in fragments actually exist in the schema.
4. **Better Documentation**: Includes field descriptions from the schema in the generated code.

### Using Schema-Aware Generation

1. Add your GraphQL schema files to your project
2. Mark them as `AdditionalFiles` in your project file
3. Configure the schema files in your project file:

```xml
<PropertyGroup>
  <GraphQLSourceGenSchemaFiles>schema.graphql;schema-extensions.graphql</GraphQLSourceGenSchemaFiles>
</PropertyGroup>
```
### Example: Schema-Aware Fragment Generation

#### 1. Define your GraphQL schema (schema.graphql)

```graphql
type User {
  id: ID!
  name: String!
  email: String
  isActive: Boolean
  profile: UserProfile
  posts: [Post!]
}

type UserProfile {
  bio: String
  avatarUrl: String
  joinDate: DateTime
}

type Post {
  id: ID!
  title: String!
  content: String
  author: User!
}

scalar DateTime
```
#### 2. Define your GraphQL fragment

```graphql
fragment UserWithPosts on User {
  id
  name
  email
  profile {
    bio
    avatarUrl
    joinDate
  }
  posts {
    id
    title
    content
  }
}
```
#### 3. Generated C# type with accurate type information

```cs
public record UserWithPostsFragment
{
    public string Id { get; init; } // Non-nullable because ID! in schema
    public string Name { get; init; } // Non-nullable because String! in schema
    public string? Email { get; init; } // Nullable because String in schema
    public ProfileModel? Profile { get; init; } // Nullable because UserProfile in schema
    public List<PostModel> Posts { get; init; } // Non-nullable list because [Post!] in schema
    
    public record ProfileModel
    {
        public string? Bio { get; init; }
        public string? AvatarUrl { get; init; }
        public DateTime? JoinDate { get; init; } // Mapped from DateTime scalar
    }
    
    public record PostModel
    {
        public string Id { get; init; }
        public string Title { get; init; }
        public string? Content { get; init; }
    }
}
```
Common Patterns
Custom Scalar Mappings
Map GraphQL scalar types to specific C# types:
```xml
<PropertyGroup>
  <GraphQLSourceGenCustomScalarMappings>
    DateTime:System.DateTime;
    Date:System.DateOnly;
    Time:System.TimeOnly;
    Upload:System.IO.Stream;
    JSON:Newtonsoft.Json.Linq.JObject
  </GraphQLSourceGenCustomScalarMappings>
</PropertyGroup>
```
### Handling Interface Types
For fragments on interface types, include the __typename field to enable proper type resolution:

```graphql
fragment NodeFragment on Node {
  __typename
  id
  ... on User {
    name
    email
  }
  ... on Post {
    title
    content
  }
}
```

### Working with Union Types
For fragments on union types, include the __typename field and use inline fragments:

```graphql
fragment SearchResultFragment on SearchResult {
  __typename
  ... on User {
    id
    name
  }
  ... on Post {
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

#### Error GQLSG005: Schema file not found

The specified schema file could not be found. Make sure:
- The file exists in your project
- The file is marked as an `AdditionalFile`
- The path in `GraphQLSourceGenSchemaFiles` is correct (relative to your project)

#### Error GQLSG006: Invalid schema definition

The schema file contains syntax errors. Common issues include:
- Missing closing braces or parentheses
- Invalid type definitions
- Incorrect directive syntax

#### Warning GQLSG007: Type not found in schema

A fragment references a type that doesn't exist in the schema. Make sure:
- The type name in the fragment matches the type name in the schema
- The schema file includes all types used in your fragments
- Type names are case-sensitive

#### Warning GQLSG008: Field not found in type

A fragment references a field that doesn't exist on the specified type. Make sure:
- The field name in the fragment matches the field name in the schema
- The field exists on the type specified in the fragment
- Field names are case-sensitive

### Schema Conflicts

#### Conflicting Type Definitions

If you have multiple schema files with conflicting type definitions:

1. **Merge the schemas**: Combine the schema files into a single file
2. **Use schema extensions**: Use the `extend type` syntax in GraphQL to extend existing types
3. **Prioritise schemas**: List the most important schema file first in `GraphQLSourceGenSchemaFiles`

```graphql
# Base schema
type User {
  id: ID!
  name: String!
}

# Extension schema
extend type User {
  email: String
  profile: UserProfile
}
```
#### Circular References
If your schema contains circular references (e.g., User references Post, which references User):

- The generator handles circular references automatically
- For deep nesting, consider using fragment spreads instead of inline selections
- Use the @skip or @include directives to conditionally include fields

#### Custom Scalar Type Conflicts

If you have conflicts with custom scalar mappings:

- Ensure consistent scalar definitions across schema files
- Provide explicit mappings for all custom scalars
- Use domain-specific C# types for clarity


## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
