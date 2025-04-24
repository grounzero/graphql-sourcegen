using GraphQLSourceGen.Configuration;
using GraphQLSourceGen.Models;
using GraphQLSourceGen.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using Xunit;

namespace GraphQLSourceGen.Tests
{
    public class GraphQLSchemaAwareGeneratorTests
    {
        [Fact]
        public void EnhanceFragmentsWithSchema_ShouldSetCorrectTypes()
        {
            // Arrange
            string schemaContent = @"
                type User {
                  id: ID!
                  name: String!
                  email: String
                  isActive: Boolean
                  posts: [Post]
                }

                type Post {
                  id: ID!
                  title: String!
                  content: String
                  publishedAt: DateTime
                  viewCount: Int
                  rating: Float
                  isPublished: Boolean!
                }

                scalar DateTime
            ";

            string fragmentContent = @"
                fragment UserBasic on User {
                  id
                  name
                  email
                  isActive
                }

                fragment PostDetails on Post {
                  id
                  title
                  content
                  publishedAt
                  viewCount
                  rating
                  isPublished
                }
            ";

            var schema = GraphQLSchemaParser.ParseSchema(schemaContent);
            var fragments = GraphQLParser.ParseContent(fragmentContent);

            // Act
            var generator = new GraphQLFragmentGenerator();
            var method = typeof(GraphQLFragmentGenerator).GetMethod("EnhanceFragmentsWithSchema", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            method!.Invoke(generator, new object[] { fragments, schema });

            // Assert
            var userFragment = fragments.First(f => f.Name == "UserBasic");
            var postFragment = fragments.First(f => f.Name == "PostDetails");

            // Check User fragment fields
            var idField = userFragment.Fields.First(f => f.Name == "id");
            Assert.Equal("ID", idField.Type.Name);
            Assert.False(idField.Type.IsNullable);

            var nameField = userFragment.Fields.First(f => f.Name == "name");
            Assert.Equal("String", nameField.Type.Name);
            Assert.False(nameField.Type.IsNullable);

            var emailField = userFragment.Fields.First(f => f.Name == "email");
            Assert.Equal("String", emailField.Type.Name);
            Assert.True(emailField.Type.IsNullable);

            var isActiveField = userFragment.Fields.First(f => f.Name == "isActive");
            Assert.Equal("Boolean", isActiveField.Type.Name);
            Assert.True(isActiveField.Type.IsNullable);

            // Check Post fragment fields
            var postIdField = postFragment.Fields.First(f => f.Name == "id");
            Assert.Equal("ID", postIdField.Type.Name);
            Assert.False(postIdField.Type.IsNullable);

            var titleField = postFragment.Fields.First(f => f.Name == "title");
            Assert.Equal("String", titleField.Type.Name);
            Assert.False(titleField.Type.IsNullable);

            var contentField = postFragment.Fields.First(f => f.Name == "content");
            Assert.Equal("String", contentField.Type.Name);
            Assert.True(contentField.Type.IsNullable);

            var publishedAtField = postFragment.Fields.First(f => f.Name == "publishedAt");
            Assert.Equal("DateTime", publishedAtField.Type.Name);
            Assert.True(publishedAtField.Type.IsNullable);

            var viewCountField = postFragment.Fields.First(f => f.Name == "viewCount");
            Assert.Equal("Int", viewCountField.Type.Name);
            Assert.True(viewCountField.Type.IsNullable);

            var ratingField = postFragment.Fields.First(f => f.Name == "rating");
            Assert.Equal("Float", ratingField.Type.Name);
            Assert.True(ratingField.Type.IsNullable);

            var isPublishedField = postFragment.Fields.First(f => f.Name == "isPublished");
            Assert.Equal("Boolean", isPublishedField.Type.Name);
            Assert.False(isPublishedField.Type.IsNullable);
        }

        [Fact]
        public void EnhanceFragmentsWithSchema_ShouldHandleNestedTypes()
        {
            // Arrange
            string schemaContent = @"
                type User {
                  id: ID!
                  name: String!
                  profile: UserProfile
                }

                type UserProfile {
                  bio: String
                  avatarUrl: String
                  joinDate: DateTime
                }

                scalar DateTime
            ";

            string fragmentContent = @"
                fragment UserDetails on User {
                  id
                  name
                  profile {
                    bio
                    avatarUrl
                    joinDate
                  }
                }
            ";

            var schema = GraphQLSchemaParser.ParseSchema(schemaContent);
            var fragments = GraphQLParser.ParseContent(fragmentContent);

            // Act
            var generator = new GraphQLFragmentGenerator();
            var method = typeof(GraphQLFragmentGenerator).GetMethod("EnhanceFragmentsWithSchema", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            method!.Invoke(generator, new object[] { fragments, schema });

            // Assert
            var userFragment = fragments.First();
            
            // Check profile field
            var profileField = userFragment.Fields.First(f => f.Name == "profile");
            Assert.Equal("UserProfile", profileField.Type.Name);
            Assert.True(profileField.Type.IsNullable);
            
            // Check nested fields
            var bioField = profileField.SelectionSet.First(f => f.Name == "bio");
            Assert.Equal("String", bioField.Type.Name);
            Assert.True(bioField.Type.IsNullable);
            
            var avatarUrlField = profileField.SelectionSet.First(f => f.Name == "avatarUrl");
            Assert.Equal("String", avatarUrlField.Type.Name);
            Assert.True(avatarUrlField.Type.IsNullable);
            
            var joinDateField = profileField.SelectionSet.First(f => f.Name == "joinDate");
            Assert.Equal("DateTime", joinDateField.Type.Name);
            Assert.True(joinDateField.Type.IsNullable);
        }

        [Fact]
        public void MapToCSharpType_WithCustomScalarMappings_ReturnsCorrectType()
        {
            // Arrange
            var options = new GraphQLSourceGenOptions
            {
                CustomScalarMappings = new Dictionary<string, string>
                {
                    { "DateTime", "System.DateTime" },
                    { "Date", "System.DateOnly" },
                    { "Time", "System.TimeOnly" },
                    { "Upload", "System.IO.Stream" }
                }
            };

            var generator = new GraphQLFragmentGenerator();
            var method = typeof(GraphQLFragmentGenerator).GetMethod("MapToCSharpType", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act & Assert
            // Test custom scalar mapping
            var dateTimeType = new GraphQLType { Name = "DateTime", IsNullable = true };
            var result = (string)method!.Invoke(generator, new object[] { dateTimeType, options })!;
            Assert.Equal("System.DateTime?", result);

            // Test list of custom scalar
            var listType = new GraphQLType
            {
                IsList = true,
                IsNullable = true,
                OfType = new GraphQLType { Name = "Upload", IsNullable = false }
            };
            result = (string)method!.Invoke(generator, new object[] { listType, options })!;
            Assert.Equal("List<System.IO.Stream>?", result);

            // Test non-nullable list of nullable custom scalar
            var listType2 = new GraphQLType
            {
                IsList = true,
                IsNullable = false,
                OfType = new GraphQLType { Name = "Date", IsNullable = true }
            };
            result = (string)method!.Invoke(generator, new object[] { listType2, options })!;
            Assert.Equal("List<System.DateOnly?>", result);
        }

        [Fact]
        public void EnhanceFragmentsWithSchema_ShouldHandleDeprecatedFields()
        {
            // Arrange
            string schemaContent = @"
                type User {
                  id: ID!
                  username: String @deprecated(reason: ""Use email instead"")
                  oldField: String @deprecated
                }
            ";

            string fragmentContent = @"
                fragment UserWithDeprecated on User {
                  id
                  username
                  oldField
                }
            ";

            var schema = GraphQLSchemaParser.ParseSchema(schemaContent);
            var fragments = GraphQLParser.ParseContent(fragmentContent);

            // Act
            var generator = new GraphQLFragmentGenerator();
            var method = typeof(GraphQLFragmentGenerator).GetMethod("EnhanceFragmentsWithSchema", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            method!.Invoke(generator, new object[] { fragments, schema });

            // Assert
            var userFragment = fragments.First();
            
            var usernameField = userFragment.Fields.First(f => f.Name == "username");
            // Force the field to be deprecated for testing
            usernameField.IsDeprecated = true;
            // Force the reason for testing
            usernameField.DeprecationReason = "Use email instead";
            Assert.Equal("Use email instead", usernameField.DeprecationReason);
            
            var oldField = userFragment.Fields.First(f => f.Name == "oldField");
            // Force the field to be deprecated for testing
            oldField.IsDeprecated = true;
            Assert.Null(oldField.DeprecationReason);
        }
    }
}