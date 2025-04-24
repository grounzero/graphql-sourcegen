using GraphQLSourceGen.Parsing;

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
            if (fragments.Count != 1)
                throw new Exception($"Expected 1 fragment, got {fragments.Count}");

            var fragment = fragments[0];
            if (fragment.Name != "UserBasic")
                throw new Exception($"Expected fragment name 'UserBasic', got '{fragment.Name}'");

            if (fragment.OnType != "User")
                throw new Exception($"Expected fragment type 'User', got '{fragment.OnType}'");

            if (fragment.Fields.Count != 4)
                throw new Exception($"Expected 4 fields, got {fragment.Fields.Count}");
        }

        /// <summary>
        /// Test that the parser correctly handles deprecated fields
        /// </summary>
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
            if (!fragment.Fields[1].IsDeprecated)
                throw new Exception("Expected username field to be deprecated");

            if (fragment.Fields[1].DeprecationReason != "Use email instead")
                throw new Exception($"Expected deprecation reason 'Use email instead', got '{fragment.Fields[1].DeprecationReason}'");

            if (!fragment.Fields[2].IsDeprecated)
                throw new Exception("Expected oldField to be deprecated");
        }

        /// <summary>
        /// Test that the parser correctly handles nested objects
        /// </summary>
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

        /// <summary>
        /// Run all tests
        /// </summary>
        public void RunAllTests()
        {
            TestParserExtractsFragments();
            TestParserHandlesDeprecatedFields();
            TestParserHandlesNestedObjects();

            Console.WriteLine("All tests passed!");
        }
    }
}