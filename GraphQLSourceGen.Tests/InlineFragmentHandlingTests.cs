using Xunit;
using GraphQLSourceGen.Models;
using GraphQLSourceGen.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace GraphQLSourceGen.Tests
{
    public class InlineFragmentHandlingTests
    {
        [Fact]
        public void TestInlineFragmentParsing()
        {
            // Define a GraphQL fragment with inline fragments
            string fragmentContent = @"
                fragment SearchResult on SearchResult {
                  ... on User {
                    id
                    name
                    email
                  }
                  ... on Article {
                    id
                    title
                    body
                  }
                }
            ";

            // Parse the fragment
            var fragments = GraphQLParser.ParseContent(fragmentContent);
            
            // Verify that the fragment was parsed correctly
            Assert.Equal(1, fragments.Count);
            Assert.Equal("SearchResult", fragments[0].Name);
            Assert.Equal("SearchResult", fragments[0].OnType);
            
            // Verify that the inline fragments were parsed correctly
            var inlineFragments = fragments[0].Fields.Where(f => f.InlineFragmentType != null).ToList();
            Assert.Equal(2, inlineFragments.Count);
            
            // Verify the User inline fragment
            var userFragment = inlineFragments.FirstOrDefault(f => f.InlineFragmentType == "User");
            Assert.NotNull(userFragment);
            Assert.Equal(3, userFragment.SelectionSet.Count);
            Assert.Contains(userFragment.SelectionSet, f => f.Name == "id");
            Assert.Contains(userFragment.SelectionSet, f => f.Name == "name");
            Assert.Contains(userFragment.SelectionSet, f => f.Name == "email");
            
            // Verify the Article inline fragment
            var articleFragment = inlineFragments.FirstOrDefault(f => f.InlineFragmentType == "Article");
            Assert.NotNull(articleFragment);
            Assert.Equal(3, articleFragment.SelectionSet.Count);
            Assert.Contains(articleFragment.SelectionSet, f => f.Name == "id");
            Assert.Contains(articleFragment.SelectionSet, f => f.Name == "title");
            Assert.Contains(articleFragment.SelectionSet, f => f.Name == "body");
        }
    }
}