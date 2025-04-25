using Xunit;
using GraphQLSourceGen.Models;
using GraphQLSourceGen.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace GraphQLSourceGen.Tests
{
    public class InlineFragmentTests
    {
        [Fact]
        public void TestInlineFragmentParsing()
        {
            // Arrange
            string fragmentWithInlineFragment = @"
                fragment UserWithDetails on User {
                    id
                    name
                    ... on Admin {
                        permissions
                        role
                    }
                    ... on Customer {
                        tier
                        joinDate
                    }
                }
            ";

            // Act
            var fragments = GraphQLParser.ParseContent(fragmentWithInlineFragment);
            var fragment = fragments.First();

            // Assert
            Assert.Equal("UserWithDetails", fragment.Name);
            Assert.Equal("User", fragment.OnType);
            
            // Check that we have the base fields
            Assert.Equal(4, fragment.Fields.Count); // id, name, and two inline fragments
            
            // Find the inline fragments
            var adminFragment = fragment.Fields.FirstOrDefault(f => f.InlineFragmentType == "Admin");
            var customerFragment = fragment.Fields.FirstOrDefault(f => f.InlineFragmentType == "Customer");
            
            // Verify the Admin inline fragment
            Assert.NotNull(adminFragment);
            Assert.Equal("Admin", adminFragment.InlineFragmentType);
            Assert.Equal(2, adminFragment.SelectionSet.Count);
            Assert.Contains(adminFragment.SelectionSet, f => f.Name == "permissions");
            Assert.Contains(adminFragment.SelectionSet, f => f.Name == "role");
            
            // Verify the Customer inline fragment
            Assert.NotNull(customerFragment);
            Assert.Equal("Customer", customerFragment.InlineFragmentType);
            Assert.Equal(2, customerFragment.SelectionSet.Count);
            Assert.Contains(customerFragment.SelectionSet, f => f.Name == "tier");
            Assert.Contains(customerFragment.SelectionSet, f => f.Name == "joinDate");
        }

        [Fact]
        public void TestInlineFragmentWithSchema()
        {
            // Arrange
            string schemaContent = @"
                type Query {
                    node(id: ID!): Node
                }

                interface Node {
                    id: ID!
                }

                interface User implements Node {
                    id: ID!
                    name: String!
                    email: String
                }

                type Admin implements User & Node {
                    id: ID!
                    name: String!
                    email: String
                    permissions: [String!]!
                    role: String!
                }

                type Customer implements User & Node {
                    id: ID!
                    name: String!
                    email: String
                    tier: String!
                    joinDate: String!
                }
            ";

            string fragmentWithInlineFragment = @"
                fragment UserWithDetails on User {
                    id
                    name
                    ... on Admin {
                        permissions
                        role
                    }
                    ... on Customer {
                        tier
                        joinDate
                    }
                }
            ";

            // Act
            var schema = GraphQLSchemaParser.ParseSchema(schemaContent);
            var fragments = GraphQLParser.ParseContent(fragmentWithInlineFragment);
            var fragment = fragments.First();

            // Enhance the fragment with schema information
            var generator = new GraphQLFragmentGenerator();
            var enhanceMethod = typeof(GraphQLFragmentGenerator).GetMethod("EnhanceFragmentsWithSchema", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            enhanceMethod.Invoke(generator, new object[] { fragments, schema });

            // Assert
            Assert.Equal("UserWithDetails", fragment.Name);
            Assert.Equal("User", fragment.OnType);
            
            // Find the inline fragments
            var adminFragment = fragment.Fields.FirstOrDefault(f => f.InlineFragmentType == "Admin");
            var customerFragment = fragment.Fields.FirstOrDefault(f => f.InlineFragmentType == "Customer");
            
            // Verify the Admin inline fragment fields have types
            Assert.NotNull(adminFragment);
            var permissionsField = adminFragment.SelectionSet.FirstOrDefault(f => f.Name == "permissions");
            var roleField = adminFragment.SelectionSet.FirstOrDefault(f => f.Name == "role");
            
            Assert.NotNull(permissionsField);
            Assert.NotNull(permissionsField.Type);
            Assert.True(permissionsField.Type.IsList);
            Assert.False(permissionsField.Type.IsNullable);
            
            Assert.NotNull(roleField);
            Assert.NotNull(roleField.Type);
            Assert.False(roleField.Type.IsNullable);
            Assert.Equal("String", roleField.Type.Name);
            
            // Verify the Customer inline fragment fields have types
            Assert.NotNull(customerFragment);
            var tierField = customerFragment.SelectionSet.FirstOrDefault(f => f.Name == "tier");
            var joinDateField = customerFragment.SelectionSet.FirstOrDefault(f => f.Name == "joinDate");
            
            Assert.NotNull(tierField);
            Assert.NotNull(tierField.Type);
            Assert.False(tierField.Type.IsNullable);
            Assert.Equal("String", tierField.Type.Name);
            
            Assert.NotNull(joinDateField);
            Assert.NotNull(joinDateField.Type);
            Assert.False(joinDateField.Type.IsNullable);
            Assert.Equal("String", joinDateField.Type.Name);
        }

        [Fact]
        public void TestNestedInlineFragments()
        {
            // Arrange
            string fragmentWithNestedInlineFragments = @"
                fragment SearchResult on SearchResult {
                    ... on User {
                        id
                        name
                        ... on Admin {
                            permissions
                        }
                        ... on Customer {
                            tier
                        }
                    }
                    ... on Post {
                        id
                        title
                        ... on Article {
                            body
                        }
                        ... on Video {
                            url
                        }
                    }
                }
            ";

            // Act
            var fragments = GraphQLParser.ParseContent(fragmentWithNestedInlineFragments);
            var fragment = fragments.First();

            // Assert
            Assert.Equal("SearchResult", fragment.Name);
            Assert.Equal("SearchResult", fragment.OnType);
            
            // Check that we have the top-level inline fragments
            Assert.Equal(2, fragment.Fields.Count);
            
            // Find the top-level inline fragments
            var userFragment = fragment.Fields.FirstOrDefault(f => f.InlineFragmentType == "User");
            var postFragment = fragment.Fields.FirstOrDefault(f => f.InlineFragmentType == "Post");
            
            // Verify the User inline fragment
            Assert.NotNull(userFragment);
            Assert.Equal(4, userFragment.SelectionSet.Count); // id, name, and two nested inline fragments
            
            // Find the nested inline fragments in User
            var adminFragment = userFragment.SelectionSet.FirstOrDefault(f => f.InlineFragmentType == "Admin");
            var customerFragment = userFragment.SelectionSet.FirstOrDefault(f => f.InlineFragmentType == "Customer");
            
            // Verify the Admin nested inline fragment
            Assert.NotNull(adminFragment);
            Assert.Equal(1, adminFragment.SelectionSet.Count);
            Assert.Contains(adminFragment.SelectionSet, f => f.Name == "permissions");
            
            // Verify the Customer nested inline fragment
            Assert.NotNull(customerFragment);
            Assert.Equal(1, customerFragment.SelectionSet.Count);
            Assert.Contains(customerFragment.SelectionSet, f => f.Name == "tier");
            
            // Verify the Post inline fragment
            Assert.NotNull(postFragment);
            Assert.Equal(4, postFragment.SelectionSet.Count); // id, title, and two nested inline fragments
            
            // Find the nested inline fragments in Post
            var articleFragment = postFragment.SelectionSet.FirstOrDefault(f => f.InlineFragmentType == "Article");
            var videoFragment = postFragment.SelectionSet.FirstOrDefault(f => f.InlineFragmentType == "Video");
            
            // Verify the Article nested inline fragment
            Assert.NotNull(articleFragment);
            Assert.Equal(1, articleFragment.SelectionSet.Count);
            Assert.Contains(articleFragment.SelectionSet, f => f.Name == "body");
            
            // Verify the Video nested inline fragment
            Assert.NotNull(videoFragment);
            Assert.Equal(1, videoFragment.SelectionSet.Count);
            Assert.Contains(videoFragment.SelectionSet, f => f.Name == "url");
        }
    }
}