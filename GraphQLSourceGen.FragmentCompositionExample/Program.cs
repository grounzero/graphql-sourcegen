using GraphQL.Generated;
using System;
using System.Collections.Generic;

namespace GraphQLSourceGen.FragmentCompositionExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GraphQL Source Generator - Fragment Composition Example");
            Console.WriteLine("======================================================");
            Console.WriteLine();
            Console.WriteLine("This example demonstrates how to compose fragments using fragment spreads.");
            Console.WriteLine();

            try
            {
                // Demonstrate user fragment composition
                DemonstrateUserFragmentComposition();

                // Demonstrate product fragment composition
                DemonstrateProductFragmentComposition();

                // Demonstrate post fragment composition
                DemonstratePostFragmentComposition();

                // Show fragment composition patterns
                DisplayFragmentCompositionPatterns();
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

        static void DemonstrateUserFragmentComposition()
        {
            Console.WriteLine("1. User Fragment Composition");
            Console.WriteLine("----------------------------");

            // Create a base user
            var baseUser = new BaseUserFragment
            {
                Id = "user-123",
                Username = "johndoe",
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                FullName = "John Doe",
                IsActive = true,
                CreatedAt = "2023-01-15T10:30:00Z"
            };

            // Create a user with profile
            var userWithProfile = new UserWithProfileFragment
            {
                Id = "user-123",
                Username = "johndoe",
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                FullName = "John Doe",
                IsActive = true,
                CreatedAt = "2023-01-15T10:30:00Z",
                Bio = "Software developer and GraphQL enthusiast",
                AvatarUrl = "https://example.com/avatars/johndoe.jpg",
                UpdatedAt = "2023-04-20T15:45:00Z",
                Address = new UserWithProfileFragment.AddressModel
                {
                    Street = "123 Main St",
                    City = "San Francisco",
                    State = "CA",
                    ZipCode = "94105",
                    Country = "USA"
                },
                Preferences = new UserWithProfileFragment.PreferencesModel
                {
                    Theme = "dark",
                    EmailNotifications = true,
                    PushNotifications = false,
                    Language = "en-US",
                    Timezone = "America/Los_Angeles"
                }
            };

            // Create a user with posts
            var userWithPosts = new UserWithPostsFragment
            {
                Id = "user-123",
                Username = "johndoe",
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                FullName = "John Doe",
                IsActive = true,
                CreatedAt = "2023-01-15T10:30:00Z",
                Posts = new List<UserWithPostsFragment.PostsModel>
                {
                    new UserWithPostsFragment.PostsModel
                    {
                        Id = "post-1",
                        Title = "Introduction to GraphQL",
                        Slug = "introduction-to-graphql",
                        Summary = "Learn the basics of GraphQL",
                        CreatedAt = "2023-02-10T09:00:00Z",
                        PublishedAt = "2023-02-12T14:30:00Z",
                        IsPublished = true,
                        Likes = 42,
                        Views = 1250,
                        FeaturedImage = "https://example.com/images/graphql-intro.jpg"
                    },
                    new UserWithPostsFragment.PostsModel
                    {
                        Id = "post-2",
                        Title = "Fragment Composition in GraphQL",
                        Slug = "fragment-composition-graphql",
                        Summary = "How to use fragment spreads effectively",
                        CreatedAt = "2023-03-05T11:15:00Z",
                        PublishedAt = "2023-03-07T16:45:00Z",
                        IsPublished = true,
                        Likes = 28,
                        Views = 870,
                        FeaturedImage = "https://example.com/images/fragment-composition.jpg"
                    }
                },
                Comments = new List<UserWithPostsFragment.CommentsModel>
                {
                    new UserWithPostsFragment.CommentsModel
                    {
                        Id = "comment-1",
                        Content = "Great article! Very informative.",
                        CreatedAt = "2023-02-15T10:30:00Z",
                        Likes = 5,
                        Post = new UserWithPostsFragment.CommentsModel.PostModel
                        {
                            Id = "post-3",
                            Title = "GraphQL vs REST"
                        }
                    }
                }
            };

            // Create a complete user
            var completeUser = new CompleteUserFragment
            {
                Id = "user-123",
                Username = "johndoe",
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                FullName = "John Doe",
                IsActive = true,
                CreatedAt = "2023-01-15T10:30:00Z",
                Bio = "Software developer and GraphQL enthusiast",
                AvatarUrl = "https://example.com/avatars/johndoe.jpg",
                UpdatedAt = "2023-04-20T15:45:00Z",
                Address = new CompleteUserFragment.AddressModel
                {
                    Street = "123 Main St",
                    City = "San Francisco",
                    State = "CA",
                    ZipCode = "94105",
                    Country = "USA"
                },
                Preferences = new CompleteUserFragment.PreferencesModel
                {
                    Theme = "dark",
                    EmailNotifications = true,
                    PushNotifications = false,
                    Language = "en-US",
                    Timezone = "America/Los_Angeles"
                },
                Posts = new List<CompleteUserFragment.PostsModel>
                {
                    new CompleteUserFragment.PostsModel
                    {
                        Id = "post-1",
                        Title = "Introduction to GraphQL",
                        Slug = "introduction-to-graphql",
                        Summary = "Learn the basics of GraphQL",
                        CreatedAt = "2023-02-10T09:00:00Z",
                        PublishedAt = "2023-02-12T14:30:00Z",
                        IsPublished = true,
                        Likes = 42,
                        Views = 1250,
                        FeaturedImage = "https://example.com/images/graphql-intro.jpg"
                    }
                },
                Comments = new List<CompleteUserFragment.CommentsModel>
                {
                    new CompleteUserFragment.CommentsModel
                    {
                        Id = "comment-1",
                        Content = "Great article! Very informative.",
                        CreatedAt = "2023-02-15T10:30:00Z",
                        Likes = 5,
                        Post = new CompleteUserFragment.CommentsModel.PostModel
                        {
                            Id = "post-3",
                            Title = "GraphQL vs REST"
                        }
                    }
                },
                Orders = new List<CompleteUserFragment.OrdersModel>
                {
                    new CompleteUserFragment.OrdersModel
                    {
                        Id = "order-1",
                        OrderNumber = "ORD-2023-001",
                        TotalAmount = 99.99f,
                        Status = "DELIVERED",
                        CreatedAt = "2023-03-10T14:30:00Z",
                        Items = new List<CompleteUserFragment.OrdersModel.ItemsModel>
                        {
                            new CompleteUserFragment.OrdersModel.ItemsModel
                            {
                                Quantity = 1,
                                Total = 99.99f,
                                Product = new CompleteUserFragment.OrdersModel.ItemsModel.ProductModel
                                {
                                    Id = "product-1",
                                    Name = "GraphQL Pro License",
                                    Price = 99.99f
                                }
                            }
                        }
                    }
                }
            };

            // Display the base user
            Console.WriteLine("Base User Fragment:");
            Console.WriteLine($"ID: {baseUser.Id}");
            Console.WriteLine($"Username: {baseUser.Username}");
            Console.WriteLine($"Email: {baseUser.Email}");
            Console.WriteLine($"Name: {baseUser.FullName} ({baseUser.FirstName} {baseUser.LastName})");
            Console.WriteLine($"Active: {baseUser.IsActive}");
            Console.WriteLine($"Created: {baseUser.CreatedAt}");
            Console.WriteLine();

            // Display the user with profile
            Console.WriteLine("User With Profile Fragment (includes BaseUser):");
            Console.WriteLine($"ID: {userWithProfile.Id}");
            Console.WriteLine($"Username: {userWithProfile.Username}");
            Console.WriteLine($"Bio: {userWithProfile.Bio}");
            Console.WriteLine($"Avatar: {userWithProfile.AvatarUrl}");
            
            if (userWithProfile.Address != null)
            {
                Console.WriteLine("Address:");
                Console.WriteLine($"  {userWithProfile.Address.Street}");
                Console.WriteLine($"  {userWithProfile.Address.City}, {userWithProfile.Address.State} {userWithProfile.Address.ZipCode}");
                Console.WriteLine($"  {userWithProfile.Address.Country}");
            }
            
            if (userWithProfile.Preferences != null)
            {
                Console.WriteLine("Preferences:");
                Console.WriteLine($"  Theme: {userWithProfile.Preferences.Theme}");
                Console.WriteLine($"  Language: {userWithProfile.Preferences.Language}");
                Console.WriteLine($"  Timezone: {userWithProfile.Preferences.Timezone}");
                Console.WriteLine($"  Email Notifications: {userWithProfile.Preferences.EmailNotifications}");
                Console.WriteLine($"  Push Notifications: {userWithProfile.Preferences.PushNotifications}");
            }
            Console.WriteLine();

            // Display the user with posts
            Console.WriteLine("User With Posts Fragment (includes BaseUser):");
            Console.WriteLine($"ID: {userWithPosts.Id}");
            Console.WriteLine($"Username: {userWithPosts.Username}");
            
            if (userWithPosts.Posts != null && userWithPosts.Posts.Count > 0)
            {
                Console.WriteLine($"Posts ({userWithPosts.Posts.Count}):");
                foreach (var post in userWithPosts.Posts)
                {
                    Console.WriteLine($"  {post.Title}");
                    Console.WriteLine($"  Published: {post.PublishedAt}");
                    Console.WriteLine($"  Likes: {post.Likes}, Views: {post.Views}");
                    Console.WriteLine();
                }
            }
            
            if (userWithPosts.Comments != null && userWithPosts.Comments.Count > 0)
            {
                Console.WriteLine($"Comments ({userWithPosts.Comments.Count}):");
                foreach (var comment in userWithPosts.Comments)
                {
                    Console.WriteLine($"  \"{comment.Content}\"");
                    Console.WriteLine($"  On post: {comment.Post?.Title}");
                    Console.WriteLine($"  Likes: {comment.Likes}");
                    Console.WriteLine();
                }
            }
            Console.WriteLine();

            // Display the complete user
            Console.WriteLine("Complete User Fragment (includes all other user fragments):");
            Console.WriteLine($"ID: {completeUser.Id}");
            Console.WriteLine($"Username: {completeUser.Username}");
            Console.WriteLine($"Email: {completeUser.Email}");
            Console.WriteLine($"Name: {completeUser.FullName}");
            Console.WriteLine($"Bio: {completeUser.Bio}");
            Console.WriteLine($"Avatar: {completeUser.AvatarUrl}");
            
            if (completeUser.Posts != null && completeUser.Posts.Count > 0)
            {
                Console.WriteLine($"Posts: {completeUser.Posts.Count}");
            }
            
            if (completeUser.Comments != null && completeUser.Comments.Count > 0)
            {
                Console.WriteLine($"Comments: {completeUser.Comments.Count}");
            }
            
            if (completeUser.Orders != null && completeUser.Orders.Count > 0)
            {
                Console.WriteLine($"Orders ({completeUser.Orders.Count}):");
                foreach (var order in completeUser.Orders)
                {
                    Console.WriteLine($"  Order #{order.OrderNumber}");
                    Console.WriteLine($"  Status: {order.Status}");
                    Console.WriteLine($"  Total: ${order.TotalAmount}");
                    
                    if (order.Items != null && order.Items.Count > 0)
                    {
                        Console.WriteLine($"  Items ({order.Items.Count}):");
                        foreach (var item in order.Items)
                        {
                            Console.WriteLine($"    {item.Quantity}x {item.Product?.Name} (${item.Product?.Price} each)");
                        }
                    }
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }

        static void DemonstrateProductFragmentComposition()
        {
            Console.WriteLine("2. Product Fragment Composition");
            Console.WriteLine("-------------------------------");

            // Create a base product
            var baseProduct = new BaseProductFragment
            {
                Id = "product-1",
                Name = "GraphQL Pro License",
                Price = 99.99f,
                SalePrice = 79.99f,
                OnSale = true,
                Sku = "GQLPRO-001",
                Inventory = 999
            };

            // Create a product with details
            var productWithDetails = new ProductWithDetailsFragment
            {
                Id = "product-1",
                Name = "GraphQL Pro License",
                Price = 99.99f,
                SalePrice = 79.99f,
                OnSale = true,
                Sku = "GQLPRO-001",
                Inventory = 999,
                Description = "Professional license for GraphQL development tools",
                Categories = new List<string> { "Software", "Development Tools" },
                Tags = new List<string> { "GraphQL", "API", "Developer Tools" },
                Images = new List<string>
                {
                    "https://example.com/products/graphql-pro-1.jpg",
                    "https://example.com/products/graphql-pro-2.jpg"
                },
                Rating = 4.8f,
                Dimensions = new ProductWithDetailsFragment.DimensionsModel
                {
                    Length = 0,
                    Width = 0,
                    Height = 0,
                    Unit = "digital"
                },
                Weight = 0,
                Manufacturer = "GraphQL Tools Inc.",
                CreatedAt = "2023-01-01T00:00:00Z",
                UpdatedAt = "2023-04-15T10:30:00Z"
            };

            // Create a complete product
            var completeProduct = new CompleteProductFragment
            {
                Id = "product-1",
                Name = "GraphQL Pro License",
                Price = 99.99f,
                SalePrice = 79.99f,
                OnSale = true,
                Sku = "GQLPRO-001",
                Inventory = 999,
                Description = "Professional license for GraphQL development tools",
                Categories = new List<string> { "Software", "Development Tools" },
                Tags = new List<string> { "GraphQL", "API", "Developer Tools" },
                Images = new List<string>
                {
                    "https://example.com/products/graphql-pro-1.jpg",
                    "https://example.com/products/graphql-pro-2.jpg"
                },
                Rating = 4.8f,
                Dimensions = new CompleteProductFragment.DimensionsModel
                {
                    Length = 0,
                    Width = 0,
                    Height = 0,
                    Unit = "digital"
                },
                Weight = 0,
                Manufacturer = "GraphQL Tools Inc.",
                CreatedAt = "2023-01-01T00:00:00Z",
                UpdatedAt = "2023-04-15T10:30:00Z",
                Reviews = new List<CompleteProductFragment.ReviewsModel>
                {
                    new CompleteProductFragment.ReviewsModel
                    {
                        Id = "review-1",
                        Rating = 5,
                        Title = "Excellent product!",
                        Content = "This has saved me hours of development time.",
                        CreatedAt = "2023-02-15T14:30:00Z",
                        Helpful = 12,
                        Verified = true,
                        User = new CompleteProductFragment.ReviewsModel.UserModel
                        {
                            Id = "user-456",
                            Username = "janedoe",
                            AvatarUrl = "https://example.com/avatars/janedoe.jpg"
                        }
                    },
                    new CompleteProductFragment.ReviewsModel
                    {
                        Id = "review-2",
                        Rating = 4,
                        Title = "Great tool",
                        Content = "Very useful, but has a steep learning curve.",
                        CreatedAt = "2023-03-10T09:15:00Z",
                        Helpful = 8,
                        Verified = true,
                        User = new CompleteProductFragment.ReviewsModel.UserModel
                        {
                            Id = "user-789",
                            Username = "bobsmith",
                            AvatarUrl = "https://example.com/avatars/bobsmith.jpg"
                        }
                    }
                }
            };

            // Display the base product
            Console.WriteLine("Base Product Fragment:");
            Console.WriteLine($"ID: {baseProduct.Id}");
            Console.WriteLine($"Name: {baseProduct.Name}");
            Console.WriteLine($"Price: ${baseProduct.Price}");
            Console.WriteLine($"Sale Price: ${baseProduct.SalePrice}");
            Console.WriteLine($"On Sale: {baseProduct.OnSale}");
            Console.WriteLine($"SKU: {baseProduct.Sku}");
            Console.WriteLine($"Inventory: {baseProduct.Inventory}");
            Console.WriteLine();

            // Display the product with details
            Console.WriteLine("Product With Details Fragment (includes BaseProduct):");
            Console.WriteLine($"ID: {productWithDetails.Id}");
            Console.WriteLine($"Name: {productWithDetails.Name}");
            Console.WriteLine($"Description: {productWithDetails.Description}");
            Console.WriteLine($"Price: ${productWithDetails.Price}");
            Console.WriteLine($"Sale Price: ${productWithDetails.SalePrice}");
            
            if (productWithDetails.Categories != null && productWithDetails.Categories.Count > 0)
            {
                Console.WriteLine($"Categories: {string.Join(", ", productWithDetails.Categories)}");
            }
            
            if (productWithDetails.Tags != null && productWithDetails.Tags.Count > 0)
            {
                Console.WriteLine($"Tags: {string.Join(", ", productWithDetails.Tags)}");
            }
            
            Console.WriteLine($"Rating: {productWithDetails.Rating}/5.0");
            Console.WriteLine($"Manufacturer: {productWithDetails.Manufacturer}");
            Console.WriteLine();

            // Display the complete product
            Console.WriteLine("Complete Product Fragment (includes all product fragments):");
            Console.WriteLine($"ID: {completeProduct.Id}");
            Console.WriteLine($"Name: {completeProduct.Name}");
            Console.WriteLine($"Description: {completeProduct.Description}");
            Console.WriteLine($"Price: ${completeProduct.Price}");
            Console.WriteLine($"Sale Price: ${completeProduct.SalePrice}");
            Console.WriteLine($"Rating: {completeProduct.Rating}/5.0");
            
            if (completeProduct.Reviews != null && completeProduct.Reviews.Count > 0)
            {
                Console.WriteLine($"Reviews ({completeProduct.Reviews.Count}):");
                foreach (var review in completeProduct.Reviews)
                {
                    Console.WriteLine($"  {review.Rating}/5 - \"{review.Title}\"");
                    Console.WriteLine($"  By: {review.User?.Username}");
                    Console.WriteLine($"  \"{review.Content}\"");
                    Console.WriteLine($"  Helpful: {review.Helpful}, Verified: {review.Verified}");
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }

        static void DemonstratePostFragmentComposition()
        {
            Console.WriteLine("3. Post Fragment Composition");
            Console.WriteLine("----------------------------");

            // Create a base post
            var basePost = new BasePostFragment
            {
                Id = "post-1",
                Title = "Introduction to GraphQL",
                Slug = "introduction-to-graphql",
                Summary = "Learn the basics of GraphQL",
                CreatedAt = "2023-02-10T09:00:00Z",
                PublishedAt = "2023-02-12T14:30:00Z",
                IsPublished = true
            };

            // Create a post with content
            var postWithContent = new PostWithContentFragment
            {
                Id = "post-1",
                Title = "Introduction to GraphQL",
                Slug = "introduction-to-graphql",
                Summary = "Learn the basics of GraphQL",
                CreatedAt = "2023-02-10T09:00:00Z",
                PublishedAt = "2023-02-12T14:30:00Z",
                IsPublished = true,
                Content = "# Introduction to GraphQL\n\nGraphQL is a query language for your API...",
                Tags = new List<string> { "GraphQL", "API", "Tutorial" },
                Categories = new List<string> { "Programming", "Web Development" },
                FeaturedImage = "https://example.com/images/graphql-intro.jpg",
                Likes = 42,
                Views = 1250,
                Metadata = new PostWithContentFragment.MetadataModel
                {
                    ReadTime = 5,
                    WordCount = 1200,
                    Keywords = new List<string> { "GraphQL", "API", "Query Language" },
                    SeoTitle = "Introduction to GraphQL - A Beginner's Guide",
                    SeoDescription = "Learn the basics of GraphQL and how it can improve your API development"
                }
            };

            // Create a complete post
            var completePost = new CompletePostFragment
            {
                Id = "post-1",
                Title = "Introduction to GraphQL",
                Slug = "introduction-to-graphql",
                Summary = "Learn the basics of GraphQL",
                CreatedAt = "2023-02-10T09:00:00Z",
                PublishedAt = "2023-02-12T14:30:00Z",
                IsPublished = true,
                Content = "# Introduction to GraphQL\n\nGraphQL is a query language for your API...",
                Tags = new List<string> { "GraphQL", "API", "Tutorial" },
                Categories = new List<string> { "Programming", "Web Development" },
                FeaturedImage = "https://example.com/images/graphql-intro.jpg",
                Likes = 42,
                Views = 1250,
                Metadata = new CompletePostFragment.MetadataModel
                {
                    ReadTime = 5,
                    WordCount = 1200,
                    Keywords = new List<string> { "GraphQL", "API", "Query Language" },
                    SeoTitle = "Introduction to GraphQL - A Beginner's Guide",
                    SeoDescription = "Learn the basics of GraphQL and how it can improve your API development"
                },
                Author = new CompletePostFragment.AuthorModel
                {
                    Id = "user-123",
                    Username = "johndoe",
                    FirstName = "John",
                    LastName = "Doe",
                    FullName = "John Doe",
                    AvatarUrl = "https://example.com/avatars/johndoe.jpg"
                },
                Comments = new List<CompletePostFragment.CommentsModel>
                {
                    new CompletePostFragment.CommentsModel
                    {
                        Id = "comment-1",
                        Content = "Great article! Very informative.",
                        CreatedAt = "2023-02-15T10:30:00Z",
                        Likes = 5,
                        Author = new CompletePostFragment.CommentsModel.AuthorModel
                        {
                            Id = "user-456",
                            Username = "janedoe",
                            AvatarUrl = "https://example.com/avatars/janedoe.jpg"
                        },
                        Replies = new List<CompletePostFragment.CommentsModel.RepliesModel>
                        {
                            new CompletePostFragment.CommentsModel.RepliesModel
                            {
                                Id = "comment-2",
                                Content = "Thanks! Glad you found it helpful.",
                                Author = new CompletePostFragment.CommentsModel.RepliesModel.AuthorModel
                                {
                                    Id = "user-123",
                                    Username = "johndoe"
                                }
                            }
                        }
                    }
                }
            };

            // Display the base post
            Console.WriteLine("Base Post Fragment:");
            Console.WriteLine($"ID: {basePost.Id}");
            Console.WriteLine($"Title: {basePost.Title}");
            Console.WriteLine($"Slug: {basePost.Slug}");
            Console.WriteLine($"Summary: {basePost.Summary}");
            Console.WriteLine($"Created: {basePost.CreatedAt}");
            Console.WriteLine($"Published: {basePost.PublishedAt}");
            Console.WriteLine($"Is Published: {basePost.IsPublished}");
            Console.WriteLine();

            // Display the post with content
            Console.WriteLine("Post With Content Fragment (includes BasePost):");
            Console.WriteLine($"ID: {postWithContent.Id}");
            Console.WriteLine($"Title: {postWithContent.Title}");
            Console.WriteLine($"Content: {postWithContent.Content?.Substring(0, Math.Min(50, postWithContent.Content?.Length ?? 0))}...");
            
            if (postWithContent.Tags != null && postWithContent.Tags.Count > 0)
            {
                Console.WriteLine($"Tags: {string.Join(", ", postWithContent.Tags)}");
            }
            
            if (postWithContent.Categories != null && postWithContent.Categories.Count > 0)
            {
                Console.WriteLine($"Categories: {string.Join(", ", postWithContent.Categories)}");
            }
            
            Console.WriteLine($"Likes: {postWithContent.Likes}, Views: {postWithContent.Views}");
            
            if (postWithContent.Metadata != null)
            {
                Console.WriteLine("Metadata:");
                Console.WriteLine($"  Read Time: {postWithContent.Metadata.ReadTime} minutes");
                Console.WriteLine($"  Word Count: {postWithContent.Metadata.WordCount} words");
                
                if (postWithContent.Metadata.Keywords != null && postWithContent.Metadata.Keywords.Count > 0)
                {
                    Console.WriteLine($"  Keywords: {string.Join(", ", postWithContent.Metadata.Keywords)}");
                }
            }
            Console.WriteLine();

            // Display the complete post
            Console.WriteLine("Complete Post Fragment (includes all post fragments):");
            Console.WriteLine($"ID: {completePost.Id}");
            Console.WriteLine($"Title: {completePost.Title}");
            
            if (completePost.Author != null)
            {
                Console.WriteLine($"Author: {completePost.Author.FullName} ({completePost.Author.Username})");
            }
            
            Console.WriteLine($"Published: {completePost.PublishedAt}");
            Console.WriteLine($"Likes: {completePost.Likes}, Views: {completePost.Views}");
            
            if (completePost.Comments != null && completePost.Comments.Count > 0)
            {
                Console.WriteLine($"Comments ({completePost.Comments.Count}):");
                foreach (var comment in completePost.Comments)
                {
                    Console.WriteLine($"  {comment.Author?.Username}: \"{comment.Content}\"");
                    Console.WriteLine($"  Likes: {comment.Likes}");
                    
                    if (comment.Replies != null && comment.Replies.Count > 0)
                    {
                        Console.WriteLine($"  Replies ({comment.Replies.Count}):");
                        foreach (var reply in comment.Replies)
                        {
                            Console.WriteLine($"    {reply.Author?.Username}: \"{reply.Content}\"");
                        }
                    }
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }

        static void DisplayFragmentCompositionPatterns()
        {
            Console.WriteLine("Fragment Composition Patterns");
            Console.WriteLine("----------------------------");
            Console.WriteLine("1. Base Fragment Pattern:");
            Console.WriteLine("   - Create a base fragment with essential fields");
            Console.WriteLine("   - Extend it with more specific fragments using fragment spreads");
            Console.WriteLine("   - Example: BaseUser → UserWithProfile, UserWithPosts");
            Console.WriteLine();
            
            Console.WriteLine("2. Feature-Based Composition:");
            Console.WriteLine("   - Create fragments based on specific features or concerns");
            Console.WriteLine("   - Combine them as needed for different use cases");
            Console.WriteLine("   - Example: PostWithContent, PostWithAuthor, PostWithComments");
            Console.WriteLine();
            
            Console.WriteLine("3. Hierarchical Composition:");
            Console.WriteLine("   - Build a hierarchy of fragments with increasing detail");
            Console.WriteLine("   - Each level includes all fields from previous levels");
            Console.WriteLine("   - Example: BaseProduct → ProductWithDetails → CompleteProduct");
            Console.WriteLine();
            
            Console.WriteLine("4. Nested Fragment Composition:");
            Console.WriteLine("   - Include fragments within nested fields");
            Console.WriteLine("   - Reuse fragments across different parent types");
            Console.WriteLine("   - Example: User fragment within Post.author field");
            Console.WriteLine();
            
            Console.WriteLine("Benefits of Fragment Composition:");
            Console.WriteLine("1. Code Reuse: Define fields once and reuse them across multiple fragments");
            Console.WriteLine("2. Maintainability: Update fields in one place and changes propagate to all fragments");
            Console.WriteLine("3. Readability: Break down complex types into smaller, focused fragments");
            Console.WriteLine("4. Flexibility: Mix and match fragments to create custom views of your data");
            Console.WriteLine("5. Type Safety: Generated C# types maintain the composition relationships");
        }
    }
}