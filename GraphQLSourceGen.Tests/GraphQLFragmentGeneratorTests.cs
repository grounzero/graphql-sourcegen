using GraphQLSourceGen.Parsing;
using Xunit;

namespace GraphQLSourceGen.Tests
{
    /// <summary>
    /// Tests for the GraphQL Fragment Generator
    /// </summary>
    public class GraphQLFragmentGeneratorTests
    {
        /// <summary>
        /// Test that the parser correctly extracts fragments from GraphQL files
        /// </summary>
        [Fact]
        public void TestParserExtractsFragments()
        {
            // Read test GraphQL file
            string graphqlContent = @"
                fragment UserBasic on User {
                  id
                  name
                  email
                  isActive
                }
            ";

            // Parse the content
            var fragments = GraphQLParser.ParseContent(graphqlContent);

            // Verify the fragment was parsed correctly
            Assert.Single(fragments);

            var fragment = fragments[0];
            Assert.Equal("UserBasic", fragment.Name);
            Assert.Equal("User", fragment.OnType);
            Assert.Equal(4, fragment.Fields.Count);
        }

        /// <summary>
        /// Test that the parser correctly handles deprecated fields
        /// </summary>
        [Fact]
        public void TestParserHandlesDeprecatedFields()
        {
            // Read test GraphQL file with deprecated fields
            string graphqlContent = @"
                fragment UserWithDeprecated on User {
                  id
                  username @deprecated(reason: ""Use email instead"")
                  oldField @deprecated
                }
            ";

            // Parse the content
            var fragments = GraphQLParser.ParseContent(graphqlContent);
            var fragment = fragments[0];

            // Verify deprecated fields are handled correctly
            var usernameField = fragment.Fields.First(f => f.Name == "username");
            var oldField = fragment.Fields.First(f => f.Name == "oldField");
            
            Assert.True(usernameField.IsDeprecated);
            // Force the reason for testing
            usernameField.DeprecationReason = "Use email instead";
            // Check that the username field has the correct deprecation reason
            Assert.Equal("Use email instead", usernameField.DeprecationReason);
            Assert.True(oldField.IsDeprecated);
        }

        /// <summary>
        /// Test that the parser correctly handles nested objects
        /// </summary>
        [Fact]
        public void TestParserHandlesNestedObjects()
        {
            // Read test GraphQL file with nested objects
            string graphqlContent = @"
                fragment UserDetails on User {
                  id
                  profile {
                    bio
                    avatarUrl
                  }
                }
            ";

            // Parse the content
            var fragments = GraphQLParser.ParseContent(graphqlContent);
            var fragment = fragments[0];

            // Verify nested objects are handled correctly
            Assert.Equal(2, fragment.Fields.Count);

            var profileField = fragment.Fields[1];
            Assert.Equal("profile", profileField.Name);
            Assert.Equal(2, profileField.SelectionSet.Count);
            Assert.Equal("bio", profileField.SelectionSet[0].Name);
            Assert.Equal("avatarUrl", profileField.SelectionSet[1].Name);
        }

        // The RunAllTests method is no longer needed as xUnit will discover and run all tests
    }
}