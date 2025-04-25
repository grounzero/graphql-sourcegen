# GraphQLSourceGen.BasicExample

This project demonstrates the basic usage of GraphQL Source Generator to generate C# types from GraphQL fragments without schema awareness.

## Overview

The BasicExample project shows how to:

1. Define simple GraphQL fragments
2. Configure basic generation options
3. Use the generated C# types in your code

## Project Structure

- **fragments/**: Contains GraphQL fragment definitions
  - `UserBasic.graphql`: A simple fragment for user data
  - `PostBasic.graphql`: A simple fragment for post data
- **Program.cs**: Demonstrates how to use the generated types

## Configuration

The project is configured with the following options in the `.csproj` file:

```xml
<PropertyGroup>
  <GraphQLSourceGenNamespace>GraphQL.Generated</GraphQLSourceGenNamespace>
  <GraphQLSourceGenUseRecords>true</GraphQLSourceGenUseRecords>
  <GraphQLSourceGenUseInitProperties>true</GraphQLSourceGenUseInitProperties>
  <GraphQLSourceGenGenerateDocComments>true</GraphQLSourceGenGenerateDocComments>
</PropertyGroup>
```

These options configure:

- **Namespace**: All generated types will be in the `GraphQL.Generated` namespace
- **Records**: Generated types will be C# records instead of classes
- **Init Properties**: Properties will use the `init` accessor instead of `set`
- **Doc Comments**: XML documentation comments will be generated for types and properties

## GraphQL Fragments

### UserBasic Fragment

```graphql
fragment UserBasic on User {
  id
  name
  email
  isActive
}
```

### PostBasic Fragment

```graphql
fragment PostBasic on Post {
  id
  title
  content
  publishedAt
  viewCount
  rating
  isPublished
}
```

## Generated C# Types

The source generator creates C# record types for each fragment:

- `UserBasicFragment`: Represents the UserBasic fragment
- `PostBasicFragment`: Represents the PostBasic fragment

Each property in the generated types corresponds to a field in the GraphQL fragment, with appropriate C# naming conventions (camelCase to PascalCase).

## Running the Example

To run the example:

```bash
dotnet run
```

The example demonstrates:
1. Creating instances of the generated types
2. Setting and accessing properties
3. Handling nullable properties

## Key Takeaways

- GraphQL Source Generator automatically creates C# types from GraphQL fragments
- Without schema awareness, all properties are generated as nullable strings
- The generator handles naming conventions and documentation
- The generated types can be used like any other C# type