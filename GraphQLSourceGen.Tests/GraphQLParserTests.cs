using GraphQLSourceGen.Models;
using GraphQLSourceGen.Parsing;
using Xunit;

namespace GraphQLSourceGen.Tests
{
    public class GraphQLParserTests
    {
        [Fact]
        public void ParseContent_SimpleFragment_ReturnsCorrectModel()
        {
            // Arrange
            string graphql = @"
                fragment UserBasic on User {
                  id
                  name
                  email
                  isActive
                }
            ";

            // Act
            var fragments = GraphQLParser.ParseContent(graphql);

            // Assert
            Assert.Single(fragments);

            var fragment = fragments[0];
            Assert.Equal("UserBasic", fragment.Name);
            Assert.Equal("User", fragment.OnType);
            Assert.Equal(4, fragment.Fields.Count);
            Assert.Equal("id", fragment.Fields[0].Name);
            Assert.Equal("name", fragment.Fields[1].Name);
            Assert.Equal("email", fragment.Fields[2].Name);
            Assert.Equal("isActive", fragment.Fields[3].Name);
        }

        [Fact]
        public void ParseContent_FragmentWithNestedObjects_ReturnsCorrectModel()
        {
            // Arrange
            string graphql = @"
                fragment UserDetails on User {
                  id
                  profile {
                    bio
                    avatarUrl
                  }
                }
            ";

            // Act
            var fragments = GraphQLParser.ParseContent(graphql);

            // Assert
            Assert.Single(fragments);

            var fragment = fragments[0];
            Assert.Equal("UserDetails", fragment.Name);
            Assert.Equal(2, fragment.Fields.Count);

            var profileField = fragment.Fields[1];
            Assert.Equal("profile", profileField.Name);
            Assert.Equal(2, profileField.SelectionSet.Count);
            Assert.Equal("bio", profileField.SelectionSet[0].Name);
            Assert.Equal("avatarUrl", profileField.SelectionSet[1].Name);
        }

        [Fact]
        public void ParseContent_FragmentWithDeprecatedFields_ReturnsCorrectModel()
        {
            // Arrange
            string graphql = @"
                fragment UserWithDeprecated on User {
                  id
                  username @deprecated(reason: ""Use email instead"")
                  oldField @deprecated
                }
            ";

            // Act
            var fragments = GraphQLParser.ParseContent(graphql);

            // Assert
            Assert.Single(fragments);

            var fragment = fragments[0];
            Assert.Equal(3, fragment.Fields.Count);
            
            Assert.False(fragment.Fields[0].IsDeprecated);
            
            Assert.True(fragment.Fields[1].IsDeprecated);
            Assert.Equal("Use email instead", fragment.Fields[1].DeprecationReason);
            
            Assert.True(fragment.Fields[2].IsDeprecated);
            Assert.Null(fragment.Fields[2].DeprecationReason);
        }

        [Fact]
        public void ParseContent_FragmentWithFragmentSpreads_ReturnsCorrectModel()
        {
            // Arrange
            string graphql = @"
                fragment UserWithPosts on User {
                  ...UserBasic
                  posts {
                    id
                    title
                  }
                }
            ";

            // Act
            var fragments = GraphQLParser.ParseContent(graphql);

            // Assert
            Assert.Single(fragments);

            var fragment = fragments[0];
            Assert.Equal(2, fragment.Fields.Count);

            // Check for fragment spreads
            Assert.Contains(fragment.Fields, f => f.FragmentSpreads.Contains("UserBasic"));

            // Check for posts field
            var postsField = fragment.Fields.FirstOrDefault(f => f.Name == "posts");
            Assert.NotNull(postsField);
            Assert.Equal(2, postsField.SelectionSet.Count);
        }

        [Fact]
        public void ParseContent_FragmentWithScalarTypes_ReturnsCorrectModel()
        {
            // Arrange
            string graphql = @"
                fragment PostWithStats on Post {
                  id: ID!
                  title: String!
                  viewCount: Int
                  rating: Float
                  isPublished: Boolean!
                  publishedAt: DateTime
                  tags: [String]
                  categories: [String!]!
                }
            ";

            // Act
            var fragments = GraphQLParser.ParseContent(graphql);

            // Assert
            Assert.Single(fragments);

            var fragment = fragments[0];
            Assert.Equal(8, fragment.Fields.Count);

            // Check non-nullable fields
            Assert.False(fragment.Fields[0].Type.IsNullable); // id: ID!
            Assert.False(fragment.Fields[1].Type.IsNullable); // title: String!
            Assert.True(fragment.Fields[2].Type.IsNullable); // viewCount: Int
            Assert.True(fragment.Fields[3].Type.IsNullable); // rating: Float
            Assert.False(fragment.Fields[4].Type.IsNullable); // isPublished: Boolean!
            Assert.True(fragment.Fields[5].Type.IsNullable); // publishedAt: DateTime

            // Check list fields
            Assert.True(fragment.Fields[6].Type.IsList); // tags: [String]
            Assert.True(fragment.Fields[6].Type.IsNullable); // tags: [String] (nullable list)
            Assert.True(fragment.Fields[7].Type.IsList); // categories: [String!]!
            Assert.False(fragment.Fields[7].Type.IsNullable); // categories: [String!]! (non-nullable list)
            Assert.False(fragment.Fields[7].Type.OfType?.IsNullable); // [String!] (non-nullable items)
        }

        [Fact]
        public void MapToCSharpType_ScalarTypes_ReturnsCorrectMapping()
        {
            // String
            var stringType = new GraphQLType { Name = "String", IsNullable = true };
            string result = GraphQLParser.MapToCSharpType(stringType);
            Assert.Equal("string?", result);

            // Non-nullable String
            var nonNullableStringType = new GraphQLType { Name = "String", IsNullable = false };
            result = GraphQLParser.MapToCSharpType(nonNullableStringType);
            Assert.Equal("string", result);

            // Int
            var intType = new GraphQLType { Name = "Int", IsNullable = true };
            result = GraphQLParser.MapToCSharpType(intType);
            Assert.Equal("int?", result);

            // Boolean
            var boolType = new GraphQLType { Name = "Boolean", IsNullable = false };
            result = GraphQLParser.MapToCSharpType(boolType);
            Assert.Equal("bool", result);

            // ID (defaults to string)
            var idType = new GraphQLType { Name = "ID", IsNullable = true };
            result = GraphQLParser.MapToCSharpType(idType);
            Assert.Equal("string?", result);

            // List of strings
            var listType = new GraphQLType
            {
                IsList = true,
                IsNullable = true,
                OfType = new GraphQLType { Name = "String", IsNullable = true }
            };
            result = GraphQLParser.MapToCSharpType(listType);
            Assert.Equal("List<string?>?", result);

            // Non-nullable list of non-nullable strings
            var nonNullableListType = new GraphQLType
            {
                IsList = true,
                IsNullable = false,
                OfType = new GraphQLType { Name = "String", IsNullable = false }
            };
            result = GraphQLParser.MapToCSharpType(nonNullableListType);
            Assert.Equal("List<string>", result);
        }
    }
}