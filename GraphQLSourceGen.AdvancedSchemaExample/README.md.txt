# GraphQLSourceGen.AdvancedSchemaExample

This project demonstrates advanced schema-aware fragment generation with GraphQL Source Generator.

## Overview

The AdvancedSchemaExample project shows how to:

1. Use a complex GraphQL schema with interfaces, unions, and custom scalars
2. Generate C# types with accurate type inference
3. Handle complex GraphQL types and relationships
4. Work with custom scalar mappings

## Project Structure

- **fragments/**: Contains GraphQL fragment definitions
  - `ComplexFragment.graphql`: Demonstrates complex fragments with multiple interfaces and nested types
  - `InterfaceFragment.graphql`: Demonstrates interface handling
  - `UnionFragment.graphql`: Demonstrates union type handling
- **schema/**: Contains GraphQL schema definitions
  - `advanced-schema.graphql`: A comprehensive schema with interfaces, unions, and custom scalars
- **Program.cs**: Demonstrates how to use the generated types

## Configuration

The project is configured with advanced schema-aware options in the `.csproj` file:

```xml
<PropertyGroup>
  <GraphQLSourceGenNamespace>GraphQL.Generated</GraphQLSourceGenNamespace>
  <GraphQLSourceGenUseSchemaForTypeInference>true</GraphQLSourceGenUseSchemaForTypeInference>
  <GraphQLSourceGenValidateNonNullableFields>true</GraphQLSourceGenValidateNonNullableFields>
  <GraphQLSourceGenIncludeFieldDescriptions>true</GraphQLSourceGenIncludeFieldDescriptions>
  <GraphQLSourceGenSchemaFiles>$(MSBuildProjectDirectory)/schema/advanced-schema.graphql</GraphQLSourceGenSchemaFiles>
  <GraphQLSourceGenCustomScalarMappings>DateTime:System.DateTime;JSON:System.Text.Json.JsonElement</GraphQLSourceGenCustomScalarMappings>
</PropertyGroup>
```

These options configure:

- **Schema-Aware Type Inference**: Uses the schema to determine the exact type of each field
- **Non-Nullable Field Validation**: Validates that non-nullable fields are provided
- **Field Descriptions**: Includes field descriptions from the schema in the generated code
- **Schema Files**: The GraphQL schema file to use for type inference
- **Custom Scalar Mappings**: Maps GraphQL scalar types to C# types

## Known Issues

This project currently has some issues that need to be fixed:

1. **Inline Fragment Spreads**: The source generator has issues with inline fragment spreads (`... on Type`). This affects fragments that use interfaces and unions.

2. **Type Resolution**: There are issues with resolving nested types in complex fragments.

## Features Demonstrated

1. **Interfaces**: The project demonstrates how to work with GraphQL interfaces like `Node`, `Content`, `Commentable`, and `Likeable`.

2. **Unions**: The project demonstrates how to work with GraphQL unions like `SearchResult`.

3. **Custom Scalars**: The project demonstrates how to map custom scalar types like `DateTime` and `JSON` to C# types.

4. **Complex Relationships**: The project demonstrates how to work with complex relationships between types, including nested objects and lists.