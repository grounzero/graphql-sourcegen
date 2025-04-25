using GraphQL.Generated;
using System;
using System.Collections.Generic;

namespace GraphQLSourceGen.BasicExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GraphQL Source Generator - Basic Example");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine("This example demonstrates basic fragment generation without schema awareness.");
            Console.WriteLine();

            try
            {
                // Create a UserBasicFragment instance
                var user = new UserBasicFragment
                {
                    Id = "user-123",
                    Name = "John Doe",
                    Email = "john.doe@example.com",
                    IsActive = "true"
                };

                Console.WriteLine("User Basic Fragment:");
                Console.WriteLine($"ID: {user.Id}");
                Console.WriteLine($"Name: {user.Name}");
                Console.WriteLine($"Email: {user.Email}");
                Console.WriteLine($"Active: {user.IsActive}");
                Console.WriteLine();

                // Create a PostBasicFragment instance
                var post = new PostBasicFragment
                {
                    Id = "post-456",
                    Title = "Introduction to GraphQL Source Generation",
                    Content = "This is a sample post about GraphQL source generation in C#...",
                    PublishedAt = DateTime.Now.AddDays(-5).ToString("o"),
                    ViewCount = "1250",
                    Rating = "4.8",
                    IsPublished = "true"
                };

                Console.WriteLine("Post Basic Fragment:");
                Console.WriteLine($"ID: {post.Id}");
                Console.WriteLine($"Title: {post.Title}");
                Console.WriteLine($"Content: {post.Content?.Substring(0, 20)}...");
                Console.WriteLine($"Published At: {post.PublishedAt}");
                Console.WriteLine($"View Count: {post.ViewCount}");
                Console.WriteLine($"Rating: {post.Rating}");
                Console.WriteLine($"Is Published: {post.IsPublished}");
                Console.WriteLine();

                // Demonstrate nullable properties
                Console.WriteLine("Demonstrating Nullable Properties:");
                var incompletePost = new PostBasicFragment
                {
                    Id = "post-789",
                    Title = "Draft Post"
                    // Other properties are intentionally left null
                };

                Console.WriteLine($"Incomplete Post ID: {incompletePost.Id}");
                Console.WriteLine($"Incomplete Post Title: {incompletePost.Title}");
                Console.WriteLine($"Incomplete Post Content: {incompletePost.Content ?? "No content yet"}");
                Console.WriteLine($"Incomplete Post Published At: {incompletePost.PublishedAt ?? "Not published"}");
                Console.WriteLine($"Incomplete Post View Count: {incompletePost.ViewCount ?? "No views"}");
                Console.WriteLine($"Incomplete Post Rating: {incompletePost.Rating ?? "No rating"}");
                Console.WriteLine($"Incomplete Post Is Published: {incompletePost.IsPublished ?? "Publication status unknown"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}