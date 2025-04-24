using System;
using System.Collections.Generic;

namespace GraphQLSourceGen.Samples
{
    // Define the classes that would normally be generated
    public class UserBasicFragment
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
    }

    public class PostWithStatsFragment
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public int? ViewCount { get; set; }
        public double? Rating { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public List<string>? Tags { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GraphQL Source Generator Samples");
            Console.WriteLine("================================");
            
            // Create a UserBasicFragment instance
            var user = new UserBasicFragment
            {
                Id = "user-123",
                Name = "John Doe",
                Email = "john.doe@example.com",
                IsActive = true
            };
            
            Console.WriteLine("\nUser Basic Fragment:");
            Console.WriteLine($"ID: {user.Id}");
            Console.WriteLine($"Name: {user.Name}");
            Console.WriteLine($"Email: {user.Email}");
            Console.WriteLine($"Active: {user.IsActive}");
            
            // Create a PostWithStatsFragment instance
            var post = new PostWithStatsFragment
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
            
            Console.WriteLine("\nPost With Stats Fragment:");
            Console.WriteLine($"ID: {post.Id}");
            Console.WriteLine($"Title: {post.Title}");
            Console.WriteLine($"Views: {post.ViewCount}");
            Console.WriteLine($"Rating: {post.Rating}");
            Console.WriteLine($"Published: {post.IsPublished}");
            Console.WriteLine($"Published At: {post.PublishedAt}");
            Console.WriteLine($"Tags: {string.Join(", ", post.Tags ?? new List<string>())}");
            Console.WriteLine($"Categories: {string.Join(", ", post.Categories)}");
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}