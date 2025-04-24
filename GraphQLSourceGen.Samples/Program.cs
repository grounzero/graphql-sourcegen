using GraphQL.Generated;

namespace GraphQLSourceGen.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GraphQL Source Generator Samples");
            Console.WriteLine("================================");

            try
            {
                // Create a UserBasicFragment instance
                var user = new UserBasicFragment
                {
                    Id = "user-123",
                    Name = "John Doe",
                    Email = "john.doe@example.com",
                    // IsActive might be a different type in the generated code
                    // IsActive = true
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
                    // ViewCount might be a different type in the generated code
                    // ViewCount = 1250,
                    // Rating might be a different type in the generated code
                    // Rating = 4.8,
                    // IsPublished might be a different type in the generated code
                    // IsPublished = true,
                    // PublishedAt might be a different type in the generated code
                    // PublishedAt = DateTime.Now.AddDays(-14),
                    // Tags might be a different type in the generated code
                    // Tags = new List<string> { "GraphQL", "C#", "Source Generators" },
                    // Categories might be a different type in the generated code
                    // Categories = new List<string> { "Programming", "Web Development" }
                };
                
                Console.WriteLine("\nPost With Stats Fragment:");
                Console.WriteLine($"ID: {post.Id}");
                Console.WriteLine($"Title: {post.Title}");
                Console.WriteLine($"Views: {post.ViewCount}");
                Console.WriteLine($"Rating: {post.Rating}");
                Console.WriteLine($"Published: {post.IsPublished}");
                Console.WriteLine($"Published At: {post.PublishedAt}");
                
                // Handle potential null values
                var tags = post.Tags as IEnumerable<object>;
                Console.WriteLine($"Tags: {(tags != null ? string.Join(", ", tags) : "none")}");
                
                var categories = post.Categories as IEnumerable<object>;
                Console.WriteLine($"Categories: {(categories != null ? string.Join(", ", categories) : "none")}");
                
                // Try to access other generated fragments
                Console.WriteLine("\nOther Generated Fragments:");
                
                // UserDetails fragment
                try
                {
                    var userDetails = Activator.CreateInstance(Type.GetType("GraphQL.Generated.UserDetailsFragment, GraphQLSourceGen.Samples"));
                    Console.WriteLine("UserDetailsFragment was successfully created!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create UserDetailsFragment: {ex.Message}");
                }
                
                // UserWithPosts fragment
                try
                {
                    var userWithPosts = Activator.CreateInstance(Type.GetType("GraphQL.Generated.UserWithPostsFragment, GraphQLSourceGen.Samples"));
                    Console.WriteLine("UserWithPostsFragment was successfully created!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create UserWithPostsFragment: {ex.Message}");
                }
                
                // RequiredUserInfo fragment
                try
                {
                    var requiredUserInfo = Activator.CreateInstance(Type.GetType("GraphQL.Generated.RequiredUserInfoFragment, GraphQLSourceGen.Samples"));
                    Console.WriteLine("RequiredUserInfoFragment was successfully created!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create RequiredUserInfoFragment: {ex.Message}");
                }
                
                // UserWithDeprecated fragment
                try
                {
                    var userWithDeprecated = Activator.CreateInstance(Type.GetType("GraphQL.Generated.UserWithDeprecatedFragment, GraphQLSourceGen.Samples"));
                    Console.WriteLine("UserWithDeprecatedFragment was successfully created!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create UserWithDeprecatedFragment: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}