# GraphQLSourceGen.Samples

This project demonstrates how to use the GraphQL Source Generator to generate strongly-typed C# classes from GraphQL fragments.

## Where to Find the Generated Types

The GraphQL Source Generator creates C# classes from your GraphQL fragments. These generated files have the following characteristics:

1. **File Location**: Generated files have a `.g.cs` extension and are placed in the output directory. In this project, you can find examples of generated files in:
   - `../GraphQLSourceGen/Samples/UserBasicFragment.g.cs`
   - `../GraphQLSourceGen/Samples/UserDetailsFragment.g.cs`
   - `../GraphQLSourceGen/Samples/UserWithDeprecatedFragment.g.cs`
   - `../GraphQLSourceGen/Samples/UserWithPostsFragment.g.cs`
   - `../GraphQLSourceGen/Samples/PostWithStatsFragment.g.cs`

2. **Namespace**: The generated classes are placed in the namespace specified in your project file. In this project, the namespace is configured as `GraphQL.Generated` with the fragment name appended (e.g., `GraphQL.Generated.UserBasic`).

3. **Class Names**: Each GraphQL fragment generates a class with the fragment name plus "Fragment" suffix (e.g., `UserBasicFragment`).

## How to Configure the Generator

The GraphQL Source Generator is configured in your project file (`GraphQLSourceGen.Samples.csproj`):

```xml
<ItemGroup>
  <ProjectReference Include="..\GraphQLSourceGen\GraphQLSourceGen.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="true" />
</ItemGroup>

<ItemGroup>
  <AdditionalFiles Include="**\*.graphql" />
</ItemGroup>
  
<!-- Custom configuration -->
<PropertyGroup>
  <GraphQLSourceGenNamespace>GraphQL.Generated</GraphQLSourceGenNamespace>
  <GraphQLSourceGenUseRecords>true</GraphQLSourceGenUseRecords>
</PropertyGroup>
```

Key configuration points:
- Add the GraphQLSourceGen project as an Analyzer reference
- Include your `.graphql` files as AdditionalFiles
- Configure custom options like namespace and whether to use C# records

## How to Use the Generated Types

Once the types are generated, you can use them in your code like any other C# class:

```csharp
// Create a UserBasicFragment instance
var user = new GraphQL.Generated.UserBasic.UserBasicFragment
{
    Id = "user-123",
    Name = "John Doe",
    Email = "john.doe@example.com",
    IsActive = true
};

// Create a PostWithStatsFragment instance
var post = new GraphQL.Generated.PostWithStats.PostWithStatsFragment
{
    Id = "post-123",
    Title = "GraphQL and C# Source Generators",
    ViewCount = 1250,
    Rating = 4.8,
    IsPublished = true,
    PublishedAt = DateTime.Now.AddDays(-14),
    Tags = new List<string> { "GraphQL", "C#", "Source Generators" },
    Categories = new List<string> { "Programming", "Web Development" }
};
```

## Build Process

When you build your project, the GraphQL Source Generator will:

1. Find all `.graphql` files included as AdditionalFiles
2. Parse the GraphQL fragments in these files
3. Generate corresponding C# classes
4. Include these generated files in the compilation

The generated files are created during the build process and are not physically added to your project files, but they are compiled into your assembly.
