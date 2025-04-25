using GraphQL.Generated;
using System;
using System.Collections.Generic;

namespace GraphQLSourceGen.NullableTypesExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GraphQL Source Generator - Nullable Types Example");
            Console.WriteLine("================================================");
            Console.WriteLine();
            Console.WriteLine("This example demonstrates how GraphQL nullability maps to C# nullable reference types.");
            Console.WriteLine();

            try
            {
                // Demonstrate user with various nullability patterns
                DemonstrateUserNullability();

                // Demonstrate post with various nullability patterns
                DemonstratePostNullability();

                // Demonstrate nested structures with mixed nullability
                DemonstrateNestedNullability();

                // Show nullability mapping table
                DisplayNullabilityMappingTable();
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

        static void DemonstrateUserNullability()
        {
            Console.WriteLine("1. User with Various Nullability Patterns");
            Console.WriteLine("----------------------------------------");

            // Create a complete user with all fields populated
            var completeUser = new UserWithNullabilityFragment
            {
                Id = "user-123",
                Username = "johndoe",
                FullName = "John Doe",
                Email = "john.doe@example.com",
                IsActive = true,
                Profile = new UserWithNullabilityFragment.ProfileModel
                {
                    Bio = "Software developer and GraphQL enthusiast",
                    AvatarUrl = "https://example.com/avatars/johndoe.jpg",
                    JoinDate = "2023-01-15",
                    Location = "San Francisco, CA",
                    Website = "https://johndoe.dev",
                    SocialLinks = new List<UserWithNullabilityFragment.ProfileModel.SocialLinksModel>
                    {
                        new UserWithNullabilityFragment.ProfileModel.SocialLinksModel
                        {
                            Platform = "GitHub",
                            Url = "https://github.com/johndoe"
                        },
                        new UserWithNullabilityFragment.ProfileModel.SocialLinksModel
                        {
                            Platform = "Twitter",
                            Url = "https://twitter.com/johndoe"
                        }
                    }
                },
                Posts = new List<UserWithNullabilityFragment.PostsModel>
                {
                    new UserWithNullabilityFragment.PostsModel
                    {
                        Id = "post-1",
                        Title = "Introduction to GraphQL"
                    },
                    new UserWithNullabilityFragment.PostsModel
                    {
                        Id = "post-2",
                        Title = "Nullable Types in C#"
                    }
                },
                Drafts = new List<UserWithNullabilityFragment.DraftsModel>
                {
                    new UserWithNullabilityFragment.DraftsModel
                    {
                        Id = "draft-1",
                        Title = "Advanced GraphQL Techniques"
                    }
                },
                Comments = new List<UserWithNullabilityFragment.CommentsModel>
                {
                    new UserWithNullabilityFragment.CommentsModel
                    {
                        Id = "comment-1",
                        Text = "Great article!"
                    },
                    null, // Nullable object in a non-nullable list
                    new UserWithNullabilityFragment.CommentsModel
                    {
                        Id = "comment-3",
                        Text = "Thanks for sharing."
                    }
                },
                SavedPosts = new List<UserWithNullabilityFragment.SavedPostsModel>
                {
                    new UserWithNullabilityFragment.SavedPostsModel
                    {
                        Id = "post-3",
                        Title = "GraphQL Best Practices"
                    },
                    null // Nullable object in a nullable list
                },
                Roles = new List<List<string>>
                {
                    new List<string> { "ADMIN", "EDITOR" },
                    new List<string> { "CONTRIBUTOR" }
                }
            };

            // Create a minimal user with only required fields
            var minimalUser = new UserWithNullabilityFragment
            {
                Id = "user-456",
                Username = "janedoe",
                IsActive = false,
                // FullName is omitted (nullable)
                // Email is omitted (nullable)
                // Profile is omitted (nullable)
                Posts = new List<UserWithNullabilityFragment.PostsModel>(), // Empty but non-nullable list
                Comments = new List<UserWithNullabilityFragment.CommentsModel>(), // Empty but non-nullable list
                // Drafts is omitted (nullable list)
                // SavedPosts is omitted (nullable list)
                Roles = new List<List<string>> { new List<string>() } // Non-nullable nested list
            };

            // Display complete user
            Console.WriteLine("Complete User:");
            Console.WriteLine($"ID: {completeUser.Id}");
            Console.WriteLine($"Username: {completeUser.Username}");
            Console.WriteLine($"Full Name: {completeUser.FullName ?? "Not provided"}");
            Console.WriteLine($"Email: {completeUser.Email ?? "Not provided"}");
            Console.WriteLine($"Active: {completeUser.IsActive}");
            
            if (completeUser.Profile != null)
            {
                Console.WriteLine("Profile:");
                Console.WriteLine($"  Bio: {completeUser.Profile.Bio ?? "Not provided"}");
                Console.WriteLine($"  Avatar URL: {completeUser.Profile.AvatarUrl ?? "Not provided"}");
                Console.WriteLine($"  Join Date: {completeUser.Profile.JoinDate}");
                Console.WriteLine($"  Location: {completeUser.Profile.Location ?? "Not provided"}");
                Console.WriteLine($"  Website: {completeUser.Profile.Website ?? "Not provided"}");
                
                if (completeUser.Profile.SocialLinks != null && completeUser.Profile.SocialLinks.Count > 0)
                {
                    Console.WriteLine("  Social Links:");
                    foreach (var link in completeUser.Profile.SocialLinks)
                    {
                        if (link != null)
                        {
                            Console.WriteLine($"    {link.Platform}: {link.Url}");
                        }
                    }
                }
            }
            
            Console.WriteLine($"Posts: {completeUser.Posts?.Count ?? 0}");
            Console.WriteLine($"Drafts: {completeUser.Drafts?.Count ?? 0}");
            Console.WriteLine($"Comments: {completeUser.Comments?.Count ?? 0}");
            Console.WriteLine($"Saved Posts: {completeUser.SavedPosts?.Count ?? 0}");
            
            if (completeUser.Roles != null && completeUser.Roles.Count > 0)
            {
                Console.WriteLine("Roles:");
                for (int i = 0; i < completeUser.Roles.Count; i++)
                {
                    var roleGroup = completeUser.Roles[i];
                    Console.WriteLine($"  Group {i + 1}: {string.Join(", ", roleGroup)}");
                }
            }
            
            Console.WriteLine();

            // Display minimal user
            Console.WriteLine("Minimal User (only required fields):");
            Console.WriteLine($"ID: {minimalUser.Id}");
            Console.WriteLine($"Username: {minimalUser.Username}");
            Console.WriteLine($"Full Name: {minimalUser.FullName ?? "Not provided"}");
            Console.WriteLine($"Email: {minimalUser.Email ?? "Not provided"}");
            Console.WriteLine($"Active: {minimalUser.IsActive}");
            Console.WriteLine($"Profile: {(minimalUser.Profile != null ? "Provided" : "Not provided")}");
            Console.WriteLine($"Posts: {minimalUser.Posts?.Count ?? 0}");
            Console.WriteLine($"Drafts: {minimalUser.Drafts?.Count ?? 0}");
            Console.WriteLine($"Comments: {minimalUser.Comments?.Count ?? 0}");
            Console.WriteLine($"Saved Posts: {minimalUser.SavedPosts?.Count ?? 0}");
            
            if (minimalUser.Roles != null && minimalUser.Roles.Count > 0)
            {
                Console.WriteLine("Roles:");
                for (int i = 0; i < minimalUser.Roles.Count; i++)
                {
                    var roleGroup = minimalUser.Roles[i];
                    Console.WriteLine($"  Group {i + 1}: {string.Join(", ", roleGroup)}");
                }
            }
            
            Console.WriteLine();
        }

        static void DemonstratePostNullability()
        {
            Console.WriteLine("2. Post with Various Nullability Patterns");
            Console.WriteLine("----------------------------------------");

            // Create a published post with all fields
            var publishedPost = new PostWithNullabilityFragment
            {
                Id = "post-123",
                Title = "Understanding GraphQL Nullability",
                Subtitle = "A deep dive into nullable types",
                Content = "This article explains how GraphQL nullability works...",
                Author = new PostWithNullabilityFragment.AuthorModel
                {
                    Id = "user-123",
                    Username = "johndoe",
                    Email = "john.doe@example.com"
                },
                CreatedAt = "2025-04-20T10:00:00Z",
                PublishedAt = "2025-04-22T14:30:00Z",
                Comments = new List<PostWithNullabilityFragment.CommentsModel>
                {
                    new PostWithNullabilityFragment.CommentsModel
                    {
                        Id = "comment-1",
                        Text = "Great article!",
                        Author = new PostWithNullabilityFragment.CommentsModel.AuthorModel
                        {
                            Id = "user-456",
                            Username = "janedoe"
                        }
                    },
                    new PostWithNullabilityFragment.CommentsModel
                    {
                        Id = "comment-2",
                        Text = "Very informative, thanks!",
                        Author = new PostWithNullabilityFragment.CommentsModel.AuthorModel
                        {
                            Id = "user-789",
                            Username = "bobsmith"
                        }
                    }
                },
                Tags = new List<string> { "GraphQL", "Nullability", "TypeSystem" },
                RelatedLinks = new List<string>
                {
                    "https://graphql.org/learn/schema/#lists-and-non-null",
                    "https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references",
                    null // Nullable string in a nullable list
                },
                Metadata = new PostWithNullabilityFragment.MetadataModel
                {
                    Views = 1250,
                    Likes = 42,
                    Shares = 15,
                    ReadTime = 5,
                    Keywords = new List<string> { "GraphQL", "Nullability", "Schema", "Types" }
                }
            };

            // Create a draft post with minimal fields
            var draftPost = new PostWithNullabilityFragment
            {
                Id = "post-456",
                Title = "Draft: GraphQL Best Practices",
                Content = "This is a draft...",
                Author = new PostWithNullabilityFragment.AuthorModel
                {
                    Id = "user-123",
                    Username = "johndoe",
                    // Email is omitted (nullable)
                },
                CreatedAt = "2025-04-23T09:15:00Z",
                // PublishedAt is omitted (nullable)
                // Comments is omitted (nullable list)
                Tags = new List<string>(), // Empty but non-nullable list
                // RelatedLinks is omitted (nullable list)
                // Metadata is omitted (nullable object)
            };

            // Display published post
            Console.WriteLine("Published Post:");
            Console.WriteLine($"ID: {publishedPost.Id}");
            Console.WriteLine($"Title: {publishedPost.Title}");
            Console.WriteLine($"Subtitle: {publishedPost.Subtitle ?? "None"}");
            Console.WriteLine($"Author: {publishedPost.Author?.Username} ({publishedPost.Author?.Email ?? "No email"})");
            Console.WriteLine($"Created: {publishedPost.CreatedAt}");
            Console.WriteLine($"Published: {publishedPost.PublishedAt ?? "Not published"}");
            
            if (publishedPost.Comments != null && publishedPost.Comments.Count > 0)
            {
                Console.WriteLine($"Comments ({publishedPost.Comments.Count}):");
                foreach (var comment in publishedPost.Comments)
                {
                    if (comment != null)
                    {
                        Console.WriteLine($"  {comment.Author?.Username}: {comment.Text}");
                    }
                }
            }
            
            if (publishedPost.Tags != null && publishedPost.Tags.Count > 0)
            {
                Console.WriteLine($"Tags: {string.Join(", ", publishedPost.Tags)}");
            }
            
            if (publishedPost.RelatedLinks != null && publishedPost.RelatedLinks.Count > 0)
            {
                Console.WriteLine("Related Links:");
                foreach (var link in publishedPost.RelatedLinks)
                {
                    Console.WriteLine($"  {link ?? "Invalid link"}");
                }
            }
            
            if (publishedPost.Metadata != null)
            {
                Console.WriteLine("Metadata:");
                Console.WriteLine($"  Views: {publishedPost.Metadata.Views}");
                Console.WriteLine($"  Likes: {publishedPost.Metadata.Likes}");
                Console.WriteLine($"  Shares: {publishedPost.Metadata.Shares}");
                Console.WriteLine($"  Read Time: {publishedPost.Metadata.ReadTime} minutes");
                
                if (publishedPost.Metadata.Keywords != null && publishedPost.Metadata.Keywords.Count > 0)
                {
                    Console.WriteLine($"  Keywords: {string.Join(", ", publishedPost.Metadata.Keywords)}");
                }
            }
            
            Console.WriteLine();

            // Display draft post
            Console.WriteLine("Draft Post (minimal fields):");
            Console.WriteLine($"ID: {draftPost.Id}");
            Console.WriteLine($"Title: {draftPost.Title}");
            Console.WriteLine($"Subtitle: {draftPost.Subtitle ?? "None"}");
            Console.WriteLine($"Author: {draftPost.Author?.Username} ({draftPost.Author?.Email ?? "No email"})");
            Console.WriteLine($"Created: {draftPost.CreatedAt}");
            Console.WriteLine($"Published: {draftPost.PublishedAt ?? "Not published"}");
            Console.WriteLine($"Comments: {(draftPost.Comments != null ? draftPost.Comments.Count.ToString() : "None")}");
            Console.WriteLine($"Tags: {(draftPost.Tags != null && draftPost.Tags.Count > 0 ? string.Join(", ", draftPost.Tags) : "None")}");
            Console.WriteLine($"Related Links: {(draftPost.RelatedLinks != null ? draftPost.RelatedLinks.Count.ToString() : "None")}");
            Console.WriteLine($"Metadata: {(draftPost.Metadata != null ? "Provided" : "None")}");
            
            Console.WriteLine();
        }

        static void DemonstrateNestedNullability()
        {
            Console.WriteLine("3. Nested Structures with Mixed Nullability");
            Console.WriteLine("------------------------------------------");

            // Create a user with nested data
            var user = new UserWithNestedDataFragment
            {
                Id = "user-123",
                Username = "johndoe",
                Profile = new UserWithNestedDataFragment.ProfileModel
                {
                    Bio = "GraphQL developer",
                    AvatarUrl = "https://example.com/avatars/johndoe.jpg",
                    JoinDate = "2023-01-15",
                    Location = "San Francisco, CA",
                    Website = "https://johndoe.dev",
                    SocialLinks = new List<UserWithNestedDataFragment.ProfileModel.SocialLinksModel>
                    {
                        new UserWithNestedDataFragment.ProfileModel.SocialLinksModel
                        {
                            Platform = "GitHub",
                            Url = "https://github.com/johndoe"
                        }
                    }
                },
                Posts = new List<UserWithNestedDataFragment.PostsModel>
                {
                    new UserWithNestedDataFragment.PostsModel
                    {
                        Id = "post-1",
                        Title = "GraphQL Nullability",
                        Content = "Understanding nullability in GraphQL...",
                        Comments = new List<UserWithNestedDataFragment.PostsModel.CommentsModel>
                        {
                            new UserWithNestedDataFragment.PostsModel.CommentsModel
                            {
                                Id = "comment-1",
                                Text = "Great post!",
                                Author = new UserWithNestedDataFragment.PostsModel.CommentsModel.AuthorModel
                                {
                                    Id = "user-456",
                                    Username = "janedoe"
                                }
                            }
                        },
                        Metadata = new UserWithNestedDataFragment.PostsModel.MetadataModel
                        {
                            Views = 1250,
                            Likes = 42,
                            Keywords = new List<string> { "GraphQL", "Nullability" }
                        }
                    }
                }
            };

            // Create a user result with errors
            var userResult = new UserResultWithNullabilityFragment
            {
                User = null, // Nullable user field
                Errors = new List<UserResultWithNullabilityFragment.ErrorsModel>
                {
                    new UserResultWithNullabilityFragment.ErrorsModel
                    {
                        Message = "User not found",
                        Path = new List<string> { "user", "byId" }
                    }
                }
            };

            // Display user with nested data
            Console.WriteLine("User with Nested Data:");
            Console.WriteLine($"ID: {user.Id}");
            Console.WriteLine($"Username: {user.Username}");
            
            if (user.Profile != null)
            {
                Console.WriteLine("Profile:");
                Console.WriteLine($"  Bio: {user.Profile.Bio ?? "Not provided"}");
                Console.WriteLine($"  Avatar URL: {user.Profile.AvatarUrl ?? "Not provided"}");
                Console.WriteLine($"  Join Date: {user.Profile.JoinDate}");
                Console.WriteLine($"  Location: {user.Profile.Location ?? "Not provided"}");
                Console.WriteLine($"  Website: {user.Profile.Website ?? "Not provided"}");
                
                if (user.Profile.SocialLinks != null && user.Profile.SocialLinks.Count > 0)
                {
                    Console.WriteLine("  Social Links:");
                    foreach (var link in user.Profile.SocialLinks)
                    {
                        if (link != null)
                        {
                            Console.WriteLine($"    {link.Platform}: {link.Url}");
                        }
                    }
                }
            }
            
            if (user.Posts != null && user.Posts.Count > 0)
            {
                Console.WriteLine($"Posts ({user.Posts.Count}):");
                foreach (var post in user.Posts)
                {
                    if (post != null)
                    {
                        Console.WriteLine($"  Post: {post.Title}");
                        Console.WriteLine($"  Content: {post.Content?.Substring(0, Math.Min(30, post.Content?.Length ?? 0))}...");
                        
                        if (post.Comments != null && post.Comments.Count > 0)
                        {
                            Console.WriteLine($"  Comments ({post.Comments.Count}):");
                            foreach (var comment in post.Comments)
                            {
                                if (comment != null)
                                {
                                    Console.WriteLine($"    {comment.Author?.Username}: {comment.Text}");
                                }
                            }
                        }
                        
                        if (post.Metadata != null)
                        {
                            Console.WriteLine("  Metadata:");
                            Console.WriteLine($"    Views: {post.Metadata.Views}");
                            Console.WriteLine($"    Likes: {post.Metadata.Likes}");
                            
                            if (post.Metadata.Keywords != null && post.Metadata.Keywords.Count > 0)
                            {
                                Console.WriteLine($"    Keywords: {string.Join(", ", post.Metadata.Keywords)}");
                            }
                        }
                    }
                }
            }
            
            Console.WriteLine();

            // Display user result with errors
            Console.WriteLine("User Result with Errors:");
            Console.WriteLine($"User: {(userResult.User != null ? userResult.User.Username : "null")}");
            
            if (userResult.Errors != null && userResult.Errors.Count > 0)
            {
                Console.WriteLine("Errors:");
                foreach (var error in userResult.Errors)
                {
                    if (error != null)
                    {
                        Console.WriteLine($"  Message: {error.Message}");
                        
                        if (error.Path != null && error.Path.Count > 0)
                        {
                            Console.WriteLine($"  Path: {string.Join(".", error.Path)}");
                        }
                    }
                }
            }
            
            Console.WriteLine();
        }

        static void DisplayNullabilityMappingTable()
        {
            Console.WriteLine("GraphQL to C# Nullability Mapping");
            Console.WriteLine("----------------------------------");
            Console.WriteLine("| GraphQL Type        | C# Type                      | Example                          |");
            Console.WriteLine("|---------------------|------------------------------|----------------------------------|");
            Console.WriteLine("| String              | string?                      | string? name;                    |");
            Console.WriteLine("| String!             | string                       | string name;                     |");
            Console.WriteLine("| Int                 | int?                         | int? count;                      |");
            Console.WriteLine("| Int!                | int                          | int count;                       |");
            Console.WriteLine("| Boolean             | bool?                        | bool? isActive;                  |");
            Console.WriteLine("| Boolean!            | bool                         | bool isActive;                   |");
            Console.WriteLine("| User                | UserModel?                   | UserModel? user;                 |");
            Console.WriteLine("| User!               | UserModel                    | UserModel user;                  |");
            Console.WriteLine("| [String]            | List<string?>?               | List<string?>? tags;             |");
            Console.WriteLine("| [String]!           | List<string?>                | List<string?> tags;              |");
            Console.WriteLine("| [String!]           | List<string>?                | List<string>? tags;              |");
            Console.WriteLine("| [String!]!          | List<string>                 | List<string> tags;               |");
            Console.WriteLine("| [User]              | List<UserModel?>?            | List<UserModel?>? users;         |");
            Console.WriteLine("| [User]!             | List<UserModel?>             | List<UserModel?> users;          |");
            Console.WriteLine("| [User!]             | List<UserModel>?             | List<UserModel>? users;          |");
            Console.WriteLine("| [User!]!            | List<UserModel>              | List<UserModel> users;           |");
            Console.WriteLine("| [[String]]          | List<List<string?>?>?        | List<List<string?>?>? matrix;    |");
            Console.WriteLine("| [[String!]!]!       | List<List<string>>           | List<List<string>> matrix;       |");
            Console.WriteLine();
            
            Console.WriteLine("Nullability Best Practices:");
            Console.WriteLine("1. Use non-nullable fields (!) in GraphQL schema for required fields");
            Console.WriteLine("2. Enable C# nullable reference types with `<Nullable>enable</Nullable>`");
            Console.WriteLine("3. Use `GraphQLSourceGenValidateNonNullableFields` to validate non-nullable fields");
            Console.WriteLine("4. Always check for null before accessing nullable fields or collections");
            Console.WriteLine("5. Use the null-conditional operator (?.) and null-coalescing operator (??) for safer code");
        }
    }
}