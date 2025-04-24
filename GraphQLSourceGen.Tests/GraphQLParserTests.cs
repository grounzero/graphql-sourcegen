using GraphQLSourceGen.Models;
using GraphQLSourceGen.Parsing;

namespace GraphQLSourceGen.Tests
{
    public class GraphQLParserTests
    {
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
            if (fragments.Count != 1)
                throw new Exception($"Expected 1 fragment, got {fragments.Count}");

            var fragment = fragments[0];
            if (fragment.Name != "UserBasic")
                throw new Exception($"Expected fragment name 'UserBasic', got '{fragment.Name}'");

            if (fragment.OnType != "User")
                throw new Exception($"Expected fragment type 'User', got '{fragment.OnType}'");

            if (fragment.Fields.Count != 4)
                throw new Exception($"Expected 4 fields, got {fragment.Fields.Count}");

            if (fragment.Fields[0].Name != "id")
                throw new Exception($"Expected field name 'id', got '{fragment.Fields[0].Name}'");

            if (fragment.Fields[1].Name != "name")
                throw new Exception($"Expected field name 'name', got '{fragment.Fields[1].Name}'");

            if (fragment.Fields[2].Name != "email")
                throw new Exception($"Expected field name 'email', got '{fragment.Fields[2].Name}'");

            if (fragment.Fields[3].Name != "isActive")
                throw new Exception($"Expected field name 'isActive', got '{fragment.Fields[3].Name}'");
        }

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
            if (fragments.Count != 1)
                throw new Exception($"Expected 1 fragment, got {fragments.Count}");

            var fragment = fragments[0];
            if (fragment.Name != "UserDetails")
                throw new Exception($"Expected fragment name 'UserDetails', got '{fragment.Name}'");

            if (fragment.Fields.Count != 2)
                throw new Exception($"Expected 2 fields, got {fragment.Fields.Count}");

            var profileField = fragment.Fields[1];
            if (profileField.Name != "profile")
                throw new Exception($"Expected field name 'profile', got '{profileField.Name}'");

            if (profileField.SelectionSet.Count != 2)
                throw new Exception($"Expected 2 nested fields, got {profileField.SelectionSet.Count}");

            if (profileField.SelectionSet[0].Name != "bio")
                throw new Exception($"Expected nested field name 'bio', got '{profileField.SelectionSet[0].Name}'");

            if (profileField.SelectionSet[1].Name != "avatarUrl")
                throw new Exception($"Expected nested field name 'avatarUrl', got '{profileField.SelectionSet[1].Name}'");
        }

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
            if (fragments.Count != 1)
                throw new Exception($"Expected 1 fragment, got {fragments.Count}");

            var fragment = fragments[0];
            if (fragment.Fields.Count != 3)
                throw new Exception($"Expected 3 fields, got {fragment.Fields.Count}");

            if (fragment.Fields[0].IsDeprecated)
                throw new Exception("Expected id field to not be deprecated");

            if (!fragment.Fields[1].IsDeprecated)
                throw new Exception("Expected username field to be deprecated");

            if (fragment.Fields[1].DeprecationReason != "Use email instead")
                throw new Exception($"Expected deprecation reason 'Use email instead', got '{fragment.Fields[1].DeprecationReason}'");

            if (!fragment.Fields[2].IsDeprecated)
                throw new Exception("Expected oldField to be deprecated");

            if (fragment.Fields[2].DeprecationReason != null)
                throw new Exception($"Expected null deprecation reason, got '{fragment.Fields[2].DeprecationReason}'");
        }

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
            if (fragments.Count != 1)
                throw new Exception($"Expected 1 fragment, got {fragments.Count}");

            var fragment = fragments[0];
            if (fragment.Fields.Count != 2)
                throw new Exception($"Expected 2 fields, got {fragment.Fields.Count}");

            // Check for fragment spreads
            if (!fragment.Fields.Any(f => f.FragmentSpreads.Contains("UserBasic")))
                throw new Exception("Expected fragment to contain UserBasic spread");

            // Check for posts field
            var postsField = fragment.Fields.FirstOrDefault(f => f.Name == "posts");
            if (postsField == null)
                throw new Exception("Expected posts field to exist");

            if (postsField.SelectionSet.Count != 2)
                throw new Exception($"Expected 2 nested fields in posts, got {postsField.SelectionSet.Count}");
        }

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
            if (fragments.Count != 1)
                throw new Exception($"Expected 1 fragment, got {fragments.Count}");

            var fragment = fragments[0];
            if (fragment.Fields.Count != 8)
                throw new Exception($"Expected 8 fields, got {fragment.Fields.Count}");

            // Check non-nullable fields
            if (fragment.Fields[0].Type.IsNullable) // id: ID!
                throw new Exception("Expected id field to be non-nullable");

            if (fragment.Fields[1].Type.IsNullable) // title: String!
                throw new Exception("Expected title field to be non-nullable");

            if (!fragment.Fields[2].Type.IsNullable) // viewCount: Int
                throw new Exception("Expected viewCount field to be nullable");

            if (!fragment.Fields[3].Type.IsNullable) // rating: Float
                throw new Exception("Expected rating field to be nullable");

            if (fragment.Fields[4].Type.IsNullable) // isPublished: Boolean!
                throw new Exception("Expected isPublished field to be non-nullable");

            if (!fragment.Fields[5].Type.IsNullable) // publishedAt: DateTime
                throw new Exception("Expected publishedAt field to be nullable");

            // Check list fields
            if (!fragment.Fields[6].Type.IsList) // tags: [String]
                throw new Exception("Expected tags field to be a list");

            if (!fragment.Fields[6].Type.IsNullable) // tags: [String] (nullable list)
                throw new Exception("Expected tags field to be a nullable list");

            if (!fragment.Fields[7].Type.IsList) // categories: [String!]!
                throw new Exception("Expected categories field to be a list");

            if (fragment.Fields[7].Type.IsNullable) // categories: [String!]! (non-nullable list)
                throw new Exception("Expected categories field to be a non-nullable list");

            if (fragment.Fields[7].Type.OfType?.IsNullable == true) // [String!] (non-nullable items)
                throw new Exception("Expected categories list items to be non-nullable");
        }

        public void MapToCSharpType_ScalarTypes_ReturnsCorrectMapping()
        {
            // String
            var stringType = new GraphQLType { Name = "String", IsNullable = true };
            string result = GraphQLParser.MapToCSharpType(stringType);
            if (result != "string?")
                throw new Exception($"Expected 'string?', got '{result}'");

            // Non-nullable String
            var nonNullableStringType = new GraphQLType { Name = "String", IsNullable = false };
            result = GraphQLParser.MapToCSharpType(nonNullableStringType);
            if (result != "string")
                throw new Exception($"Expected 'string', got '{result}'");

            // Int
            var intType = new GraphQLType { Name = "Int", IsNullable = true };
            result = GraphQLParser.MapToCSharpType(intType);
            if (result != "int?")
                throw new Exception($"Expected 'int?', got '{result}'");

            // Boolean
            var boolType = new GraphQLType { Name = "Boolean", IsNullable = false };
            result = GraphQLParser.MapToCSharpType(boolType);
            if (result != "bool")
                throw new Exception($"Expected 'bool', got '{result}'");

            // ID (defaults to string)
            var idType = new GraphQLType { Name = "ID", IsNullable = true };
            result = GraphQLParser.MapToCSharpType(idType);
            if (result != "string?")
                throw new Exception($"Expected 'string?', got '{result}'");

            // List of strings
            var listType = new GraphQLType
            {
                IsList = true,
                IsNullable = true,
                OfType = new GraphQLType { Name = "String", IsNullable = true }
            };
            result = GraphQLParser.MapToCSharpType(listType);
            if (result != "List<string?>?")
                throw new Exception($"Expected 'List<string?>?', got '{result}'");

            // Non-nullable list of non-nullable strings
            var nonNullableListType = new GraphQLType
            {
                IsList = true,
                IsNullable = false,
                OfType = new GraphQLType { Name = "String", IsNullable = false }
            };
            result = GraphQLParser.MapToCSharpType(nonNullableListType);
            if (result != "List<string>")
                throw new Exception($"Expected 'List<string>', got '{result}'");
        }
    }
}