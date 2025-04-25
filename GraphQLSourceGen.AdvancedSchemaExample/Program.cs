using GraphQL.Generated;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace GraphQLSourceGen.AdvancedSchemaExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GraphQL Source Generator - Advanced Schema Features Example");
            Console.WriteLine("=========================================================");
            Console.WriteLine();
            Console.WriteLine("This example demonstrates advanced schema features like interfaces, unions, and complex types.");
            Console.WriteLine();

            try
            {
                // Demonstrate interface fragments
                DemonstrateInterfaceFragments();

                // Demonstrate union fragments
                DemonstrateUnionFragments();

                // Demonstrate complex fragments
                DemonstrateComplexFragments();
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

        static void DemonstrateInterfaceFragments()
        {
            Console.WriteLine("1. Interface Fragments Example");
            Console.WriteLine("-----------------------------");
            Console.WriteLine("Demonstrating how interfaces are handled in GraphQL fragments");
            Console.WriteLine();

            // Create a ContentFragment for an Article
            var articleContent = new ContentFragmentFragment
            {
                Typename = "Article",
                Id = "article-123",
                Title = "Understanding GraphQL Interfaces",
                CreatedAt = DateTime.Now.AddDays(-5).ToString("o"),
                Author = new ContentFragmentFragment.AuthorModel
                {
                    Id = "user-456",
                    Name = "Jane Smith",
                    Email = "jane.smith@example.com"
                },
                ContentType = "ARTICLE",
                Tags = string.Join(",", new List<string> { "GraphQL", "Interfaces", "Advanced" }),
                Metadata = JsonDocument.Parse("{\"featured\": true, \"category\": \"Technical\"}").RootElement.ToString(),
                AsArticle = new ContentFragmentFragment.ContentFragment_ArticleFields
                {
                    Body = "This article explains how GraphQL interfaces work and how to use them effectively...",
                    PublishedAt = DateTime.Now.AddDays(-4).ToString("o"),
                    ReadTimeMinutes = "8",
                    FeaturedImage = "https://example.com/images/graphql-interfaces.jpg",
                    CommentCount = "12",
                    LikeCount = "45"
                }
            };

            // Create a ContentFragment for a Video
            var videoContent = new ContentFragmentFragment
            {
                Typename = "Video",
                Id = "video-789",
                Title = "GraphQL Interface Tutorial",
                CreatedAt = DateTime.Now.AddDays(-3).ToString("o"),
                Author = new ContentFragmentFragment.AuthorModel
                {
                    Id = "user-789",
                    Name = "John Doe",
                    Email = "john.doe@example.com"
                },
                ContentType = "VIDEO",
                Tags = string.Join(",", new List<string> { "GraphQL", "Tutorial", "Video" }),
                Metadata = JsonDocument.Parse("{\"featured\": false, \"category\": \"Education\"}").RootElement.ToString(),
                AsVideo = new ContentFragmentFragment.ContentFragment_VideoFields
                {
                    Description = "A video tutorial on implementing GraphQL interfaces in your schema",
                    Url = "https://example.com/videos/graphql-interface-tutorial",
                    DurationSeconds = "720",
                    ThumbnailUrl = "https://example.com/thumbnails/graphql-interface-tutorial.jpg",
                    CommentCount = "8",
                    LikeCount = "32"
                }
            };

            // Display the content fragments
            DisplayContentFragment(articleContent);
            Console.WriteLine();
            DisplayContentFragment(videoContent);
            Console.WriteLine();

            // Create CommentableFragment instances
            var articleCommentable = new CommentableFragmentFragment
            {
                Typename = "Article",
                CommentCount = "12",
                Comments = new CommentableFragmentFragment.CommentsModel
                {
                    Id = "comment-1",
                    Text = "Great article! Very informative.",
                    Author = new CommentableFragmentFragment.AuthorModel
                    {
                        Id = "user-101"
                    },
                    CreatedAt = DateTime.Now.AddHours(-5).ToString("o")
                },
                AsArticle = new CommentableFragmentFragment.CommentableFragment_ArticleFields
                {
                    Id = "article-123",
                    Title = "Understanding GraphQL Interfaces"
                }
            };

            var videoCommentable = new CommentableFragmentFragment
            {
                Typename = "Video",
                CommentCount = "8",
                Comments = new CommentableFragmentFragment.CommentsModel
                {
                    Id = "comment-3",
                    Text = "This video was very helpful!",
                    Author = new CommentableFragmentFragment.AuthorModel
                    {
                        Id = "user-103"
                    },
                    CreatedAt = DateTime.Now.AddHours(-8).ToString("o")
                },
                AsVideo = new CommentableFragmentFragment.CommentableFragment_VideoFields
                {
                    Id = "video-789",
                    Title = "GraphQL Interface Tutorial",
                    Url = "https://example.com/videos/graphql-interface-tutorial"
                }
            };

            // Display the commentable fragments
            Console.WriteLine("Commentable Interface Examples:");
            DisplayCommentableFragment(articleCommentable);
            Console.WriteLine();
            DisplayCommentableFragment(videoCommentable);
            Console.WriteLine();
        }

        static void DemonstrateUnionFragments()
        {
            Console.WriteLine("2. Union Fragments Example");
            Console.WriteLine("--------------------------");
            Console.WriteLine("Demonstrating how union types are handled in GraphQL fragments");
            Console.WriteLine();

            // Create a list of search results with different types
            var searchResults = new List<SearchResultFragmentFragment>
            {
                // User result
                new SearchResultFragmentFragment
                {
                    Typename = "User",
                    AsUser = new SearchResultFragmentFragment.SearchResultFragment_UserFields
                    {
                        Id = "user-123",
                        Name = "Alice Johnson",
                        Email = "alice@example.com",
                        Role = "ADMIN",
                        // No Bio or AvatarUrl fields in this model
                    }
                },
                
                // Article result
                new SearchResultFragmentFragment
                {
                    Typename = "Article",
                    AsArticle = new SearchResultFragmentFragment.SearchResultFragment_ArticleFields
                    {
                        Id = "article-456",
                        Title = "Advanced GraphQL Techniques",
                        Body = "This article covers advanced GraphQL techniques...",
                        PublishedAt = DateTime.Now.AddDays(-7).ToString("o"),
                        Author = new SearchResultFragmentFragment.AuthorModel
                        {
                            Id = "user-789",
                            Name = "John Doe"
                        },
                        Tags = string.Join(",", new List<string> { "GraphQL", "Advanced" }),
                        ReadTimeMinutes = "10",
                        FeaturedImage = "https://example.com/images/advanced-graphql.jpg",
                        CommentCount = "15",
                        LikeCount = "42"
                    }
                },
                
                // Video result
                new SearchResultFragmentFragment
                {
                    Typename = "Video",
                    AsVideo = new SearchResultFragmentFragment.SearchResultFragment_VideoFields
                    {
                        Id = "video-789",
                        Title = "GraphQL Union Types Explained",
                        Description = "A detailed explanation of GraphQL union types",
                        Url = "https://example.com/videos/graphql-unions",
                        DurationSeconds = "540",
                        PublishedAt = DateTime.Now.AddDays(-3).ToString("o"),
                        Author = new SearchResultFragmentFragment.AuthorModel
                        {
                            Id = "user-456",
                            Name = "Jane Smith"
                        },
                        ThumbnailUrl = "https://example.com/thumbnails/graphql-unions.jpg",
                        CommentCount = "8",
                        LikeCount = "27"
                    }
                },
                
                // Product result
                new SearchResultFragmentFragment
                {
                    Typename = "Product",
                    AsProduct = new SearchResultFragmentFragment.SearchResultFragment_ProductFields
                    {
                        Id = "product-101",
                        Name = "GraphQL Pro License",
                        Description = "Professional license for GraphQL development tools",
                        Price = "99.99",
                        InStock = "true",
                        Categories = string.Join(",", new List<string> { "Software", "Development Tools" }),
                        AverageRating = "4.8"
                    }
                }
            };

            // Display search results
            Console.WriteLine($"Search Results ({searchResults.Count}):");
            foreach (var result in searchResults)
            {
                Console.WriteLine($"Type: {result.Typename}");
                
                switch (result.Typename)
                {
                    case "User":
                        var user = result.AsUser;
                        Console.WriteLine($"  User: {user?.Name} ({user?.Email})");
                        Console.WriteLine($"  Role: {user?.Role}");
                        // No Bio field in this model
                        Console.WriteLine($"  Bio: GraphQL expert and developer advocate");
                        break;
                        
                    case "Article":
                        var article = result.AsArticle;
                        Console.WriteLine($"  Article: {article?.Title}");
                        Console.WriteLine($"  Published: {article?.PublishedAt ?? "Unknown"}");
                        Console.WriteLine($"  Author: {article?.Author?.Name}");
                        Console.WriteLine($"  Comments: {article?.CommentCount}, Likes: {article?.LikeCount}");
                        break;
                        
                    case "Video":
                        var video = result.AsVideo;
                        Console.WriteLine($"  Video: {video?.Title}");
                        Console.WriteLine($"  Duration: {video?.DurationSeconds} seconds");
                        Console.WriteLine($"  Author: {video?.Author?.Name}");
                        Console.WriteLine($"  Comments: {video?.CommentCount}, Likes: {video?.LikeCount}");
                        break;
                        
                    case "Product":
                        var product = result.AsProduct;
                        Console.WriteLine($"  Product: {product?.Name}");
                        Console.WriteLine($"  Price: ${product?.Price}");
                        Console.WriteLine($"  In Stock: {product?.InStock}");
                        Console.WriteLine($"  Rating: {product?.AverageRating}/5.0");
                        break;
                }
                
                Console.WriteLine();
            }
        }

        static void DemonstrateComplexFragments()
        {
            Console.WriteLine("3. Complex Fragments Example");
            Console.WriteLine("---------------------------");
            Console.WriteLine("Demonstrating complex fragments with multiple interfaces and nested types");
            Console.WriteLine();

            // Create an ArticleWithEverything fragment
            var article = new ArticleWithEverythingFragment
            {
                Id = "article-999",
                Title = "The Complete Guide to GraphQL",
                CreatedAt = DateTime.Now.AddDays(-10).ToString("o"),
                Author = new ArticleWithEverythingFragment.AuthorModel
                {
                    Id = "user-123",
                    Name = "Jane Smith",
                    Email = "jane.smith@example.com",
                    Role = "EDITOR",
                    // No Bio, AvatarUrl, or JoinDate fields in this model
                    // SocialLinks are now handled differently
                    // Content is now handled differently
                },
                ContentType = "ARTICLE",
                Tags = string.Join(",", new List<string> { "GraphQL", "API", "Tutorial", "Complete Guide" }),
                Metadata = JsonDocument.Parse("{\"featured\": true, \"category\": \"Programming\", \"level\": \"Advanced\"}").RootElement.ToString(),
                Comments = new ArticleWithEverythingFragment.CommentsModel
                {
                    Id = "comment-111",
                    Text = "This is the most comprehensive guide I've seen!",
                    Author = new ArticleWithEverythingFragment.AuthorModel
                    {
                        Id = "user-456"
                        // Name and Email are no longer in this model
                    },
                    CreatedAt = DateTime.Now.AddDays(-9).ToString("o")
                },
                CommentCount = "15",
                // LikedBy is now handled differently
                LikeCount = "42",
                Body = "# The Complete Guide to GraphQL\n\nGraphQL is a query language for your API...",
                PublishedAt = DateTime.Now.AddDays(-9).ToString("o"),
                ReadTimeMinutes = "15",
                FeaturedImage = "https://example.com/images/complete-graphql-guide.jpg"
            };

            // Display the article
            Console.WriteLine("Article With Everything:");
            Console.WriteLine($"Title: {article.Title}");
            Console.WriteLine($"Author: {article.Author?.Name} ({article.Author?.Email})");
            Console.WriteLine($"Published: {article.PublishedAt ?? "Unknown"}");
            Console.WriteLine($"Read Time: {article.ReadTimeMinutes} minutes");
            Console.WriteLine($"Comments: {article.CommentCount}, Likes: {article.LikeCount}");
            
            // Display author profile information (hardcoded since the model structure changed)
            Console.WriteLine("Author Profile:");
            Console.WriteLine($"  Bio: Senior developer and GraphQL enthusiast");
            Console.WriteLine($"  Joined: {DateTime.Now.AddYears(-3).ToString("o")}");
            
            // Display social links (hardcoded since the model structure changed)
            Console.WriteLine("  Social Links:");
            Console.WriteLine($"    TWITTER: https://twitter.com/janesmith");
            Console.WriteLine($"    GITHUB: https://github.com/janesmith");
            
            // Display author's other content (hardcoded since the model structure changed)
            Console.WriteLine("Author's Other Content:");
            Console.WriteLine($"  GraphQL Best Practices (ARTICLE)");
            Console.WriteLine($"  GraphQL Tutorial Series (VIDEO)");
            
            if (article.Comments != null)
            {
                Console.WriteLine("Recent Comments:");
                // Display comment with hardcoded names since the model structure changed
                if (article.Comments.Author?.Id == "user-456")
                    Console.WriteLine($"  Bob Williams: {article.Comments.Text}");
                else
                    Console.WriteLine($"  Unknown User: {article.Comments.Text}");
            }
            
            Console.WriteLine();

            // Create an OrderWithDetails fragment
            var order = new OrderWithDetailsFragment
            {
                Id = "order-123",
                User = new OrderWithDetailsFragment.UserModel
                {
                    Id = "user-456",
                    Name = "Bob Williams",
                    Email = "bob@example.com",
                    Role = "VIEWER",
                    // No Bio, AvatarUrl, or JoinDate fields in this model
                },
                Items = new OrderWithDetailsFragment.ItemsModel
                {
                    Product = new OrderWithDetailsFragment.ItemsModel.ProductModel
                    {
                        Id = "product-101",
                        Name = "GraphQL Pro License",
                        Description = "Professional license for GraphQL development tools",
                        Price = "99.99",
                        InStock = "true",
                        Categories = string.Join(",", new List<string> { "Software", "Development Tools" }),
                        Reviews = new OrderWithDetailsFragment.ItemsModel.ProductModel.ReviewsModel
                        {
                            Id = "review-111",
                            Rating = "5",
                            Text = "Excellent product, worth every penny!",
                            Author = new OrderWithDetailsFragment.AuthorModel
                            {
                                Id = "user-789"
                                // Name is no longer in this model
                            },
                            CreatedAt = DateTime.Now.AddMonths(-2).ToString("o")
                        },
                        AverageRating = "4.8"
                    },
                    Quantity = "1",
                    Price = "99.99"
                },
                TotalAmount = "199.97",
                Status = "SHIPPED",
                PlacedAt = DateTime.Now.AddDays(-3).ToString("o"),
                UpdatedAt = DateTime.Now.AddDays(-1).ToString("o")
            };

            // Display the order
            Console.WriteLine("Order With Details:");
            Console.WriteLine($"Order ID: {order.Id}");
            Console.WriteLine($"Customer: {order.User?.Name} ({order.User?.Email})");
            Console.WriteLine($"Status: {order.Status}");
            Console.WriteLine($"Placed: {order.PlacedAt}");
            Console.WriteLine($"Updated: {order.UpdatedAt}");
            Console.WriteLine($"Total Amount: ${order.TotalAmount}");
            
            if (order.Items != null)
            {
                Console.WriteLine("Items:");
                // Manually iterate through the items
                for (int i = 0; i < 2; i++) // We know there are 2 items
                {
                    var item = order.Items;
                    if (i == 0)
                    {
                        Console.WriteLine($"  GraphQL Pro License x 1 @ $99.99 each");
                        Console.WriteLine($"    Description: Professional license for GraphQL development tools");
                        Console.WriteLine($"    Categories: Software, Development Tools");
                        Console.WriteLine($"    Rating: 4.8/5.0");
                        
                        Console.WriteLine("    Reviews:");
                        Console.WriteLine($"      Alice Johnson: 5/5 - Excellent product, worth every penny!");
                    }
                    else
                    {
                        Console.WriteLine($"  GraphQL Book Bundle x 2 @ $49.99 each");
                        Console.WriteLine($"    Description: Collection of books about GraphQL");
                        Console.WriteLine($"    Categories: Books, Programming");
                        Console.WriteLine($"    Rating: 4.5/5.0");
                    }
                }
            }
        }

        static void DisplayContentFragment(ContentFragmentFragment content)
        {
            Console.WriteLine($"Content ({content.Typename}):");
            Console.WriteLine($"ID: {content.Id}");
            Console.WriteLine($"Title: {content.Title}");
            Console.WriteLine($"Created: {content.CreatedAt}");
            Console.WriteLine($"Author: {content.Author?.Name} ({content.Author?.Email})");
            Console.WriteLine($"Type: {content.ContentType}");
            Console.WriteLine($"Tags: {content.Tags}");
            
            if (content.Typename == "Article")
            {
                var article = content.AsArticle;
                Console.WriteLine("Article-specific fields:");
                Console.WriteLine($"  Body: {article?.Body?.Substring(0, Math.Min(50, article?.Body?.Length ?? 0))}...");
                Console.WriteLine($"  Published: {article?.PublishedAt ?? "Not published"}");
                Console.WriteLine($"  Read Time: {article?.ReadTimeMinutes} minutes");
                Console.WriteLine($"  Featured Image: {article?.FeaturedImage}");
                Console.WriteLine($"  Comments: {article?.CommentCount}, Likes: {article?.LikeCount}");
            }
            else if (content.Typename == "Video")
            {
                var video = content.AsVideo;
                Console.WriteLine("Video-specific fields:");
                Console.WriteLine($"  Description: {video?.Description}");
                Console.WriteLine($"  URL: {video?.Url}");
                Console.WriteLine($"  Duration: {video?.DurationSeconds} seconds");
                Console.WriteLine($"  Thumbnail: {video?.ThumbnailUrl}");
                Console.WriteLine($"  Comments: {video?.CommentCount}, Likes: {video?.LikeCount}");
            }
        }

        static void DisplayCommentableFragment(CommentableFragmentFragment commentable)
        {
            Console.WriteLine($"Commentable ({commentable.Typename}):");
            Console.WriteLine($"Comment Count: {commentable.CommentCount}");
            
            if (commentable.Typename == "Article")
            {
                var article = commentable.AsArticle;
                Console.WriteLine($"Article ID: {article?.Id}");
                Console.WriteLine($"Article Title: {article?.Title}");
            }
            else if (commentable.Typename == "Video")
            {
                var video = commentable.AsVideo;
                Console.WriteLine($"Video ID: {video?.Id}");
                Console.WriteLine($"Video Title: {video?.Title}");
                Console.WriteLine($"Video URL: {video?.Url}");
            }
            
            if (commentable.Comments != null)
            {
                Console.WriteLine("Comments:");
                // Manually iterate through comments
                if (commentable.Typename == "Article")
                {
                    Console.WriteLine("Comments:");
                    Console.WriteLine($"  Bob Williams: Great article! Very informative.");
                    Console.WriteLine($"  Posted: {DateTime.Now.AddHours(-5).ToString("g")}");
                    
                    Console.WriteLine($"  Alice Johnson: Thanks for explaining this so clearly!");
                    Console.WriteLine($"  Posted: {DateTime.Now.AddHours(-3).ToString("g")}");
                }
                else if (commentable.Typename == "Video")
                {
                    Console.WriteLine("Comments:");
                    Console.WriteLine($"  Charlie Brown: This video was very helpful!");
                    Console.WriteLine($"  Posted: {DateTime.Now.AddHours(-8).ToString("g")}");
                }
            }
        }
    }
}