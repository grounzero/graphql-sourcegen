using GraphQL.Generated;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphQLSourceGen.IntegrationExample
{
    class Program
    {
        // This is a mock API endpoint - in a real application, this would be a real GraphQL API
        private const string API_ENDPOINT = "https://example.com/graphql";

        static async Task Main(string[] args)
        {
            Console.WriteLine("GraphQL Source Generator - Integration Example");
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.WriteLine("This example demonstrates how to integrate generated types with a GraphQL client.");
            Console.WriteLine("Note: This example uses mock data and doesn't actually connect to a GraphQL API.");
            Console.WriteLine();

            try
            {
                // Demonstrate using the BookStoreClient with generated types
                await DemonstrateClientIntegration();
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

        static async Task DemonstrateClientIntegration()
        {
            Console.WriteLine("Creating a mock BookStoreClient...");
            Console.WriteLine($"(In a real application, this would connect to {API_ENDPOINT})");
            Console.WriteLine();

            // Create a mock client
            var mockClient = new MockBookStoreClient();

            // Demonstrate querying books
            Console.WriteLine("1. Querying Books");
            Console.WriteLine("-----------------");
            
            Console.WriteLine("Getting all books (using BookBasicFragment):");
            var books = await mockClient.GetAllBooksAsync();
            DisplayBooks(books);
            
            Console.WriteLine("Getting book by ID (using BookCompleteFragment):");
            var book = await mockClient.GetBookByIdAsync("book-1");
            DisplayBookDetails(book);
            
            Console.WriteLine("Getting book with reviews (using BookWithReviewsFragment):");
            var bookWithReviews = await mockClient.GetBookWithReviewsAsync("book-1");
            DisplayBookReviews(bookWithReviews);
            
            Console.WriteLine();

            // Demonstrate querying authors
            Console.WriteLine("2. Querying Authors");
            Console.WriteLine("-------------------");
            
            Console.WriteLine("Getting all authors (using AuthorBasicFragment):");
            var authors = await mockClient.GetAllAuthorsAsync();
            DisplayAuthors(authors);
            
            Console.WriteLine("Getting author by ID (using AuthorCompleteFragment):");
            var author = await mockClient.GetAuthorByIdAsync("author-1");
            DisplayAuthorDetails(author);
            
            Console.WriteLine("Getting author with books (using AuthorWithBooksFragment):");
            var authorWithBooks = await mockClient.GetAuthorWithBooksAsync("author-1");
            DisplayAuthorBooks(authorWithBooks);
            
            Console.WriteLine();

            // Demonstrate mutations
            Console.WriteLine("3. Performing Mutations");
            Console.WriteLine("----------------------");
            
            Console.WriteLine("Adding a new book (using BookDetailedFragment):");
            var newBook = await mockClient.AddBookAsync(new AddBookInput
            {
                Title = "GraphQL in Action",
                Isbn = "978-1617295683",
                PageCount = 400,
                Status = "PUBLISHED",
                AuthorIds = new List<string> { "author-1" },
                Categories = new List<string> { "Programming", "GraphQL" },
                Price = 49.99f,
                Currency = "USD"
            });
            DisplayBookSummary(newBook);
            
            Console.WriteLine("Updating a book (using BookDetailedFragment):");
            var updatedBook = await mockClient.UpdateBookAsync("book-1", new UpdateBookInput
            {
                Title = "Updated Book Title",
                Status = "PUBLISHED"
            });
            DisplayBookSummary(updatedBook);
            
            Console.WriteLine("Deleting a book:");
            var deleteResult = await mockClient.DeleteBookAsync("book-2");
            Console.WriteLine($"Book deleted: {deleteResult}");
            
            Console.WriteLine();
            
            Console.WriteLine("Adding a new author (using AuthorDetailedFragment):");
            var newAuthor = await mockClient.AddAuthorAsync(new AddAuthorInput
            {
                FirstName = "Jane",
                LastName = "Smith",
                Biography = "Jane Smith is a software developer and author.",
                Website = "https://janesmith.dev"
            });
            DisplayAuthorSummary(newAuthor);
            
            Console.WriteLine("Updating an author (using AuthorDetailedFragment):");
            var updatedAuthor = await mockClient.UpdateAuthorAsync("author-1", new UpdateAuthorInput
            {
                Biography = "Updated author biography."
            });
            DisplayAuthorSummary(updatedAuthor);
            
            Console.WriteLine("Deleting an author:");
            var authorDeleteResult = await mockClient.DeleteAuthorAsync("author-2");
            Console.WriteLine($"Author deleted: {authorDeleteResult}");
            
            Console.WriteLine();

            // Demonstrate integration benefits
            Console.WriteLine("4. Benefits of Integration");
            Console.WriteLine("-------------------------");
            Console.WriteLine("1. Type Safety: Generated types ensure compile-time type checking");
            Console.WriteLine("2. IntelliSense: Full IDE support for exploring the API");
            Console.WriteLine("3. Consistency: Generated types match the GraphQL schema");
            Console.WriteLine("4. Maintainability: Schema changes are reflected in generated types");
            Console.WriteLine("5. Productivity: Reduced boilerplate code and fewer runtime errors");
            Console.WriteLine();
            
            Console.WriteLine("5. Integration Patterns");
            Console.WriteLine("----------------------");
            Console.WriteLine("1. Client Wrapper: Encapsulate GraphQL operations in a strongly-typed client");
            Console.WriteLine("2. Repository Pattern: Use generated types in your repository implementations");
            Console.WriteLine("3. Service Layer: Build services around GraphQL operations");
            Console.WriteLine("4. CQRS: Use queries and mutations as commands and queries");
            Console.WriteLine("5. State Management: Use generated types in your application state");
        }

        #region Display Methods

        static void DisplayBooks(List<BookBasicFragment> books)
        {
            Console.WriteLine($"Found {books.Count} books:");
            foreach (var book in books)
            {
                Console.WriteLine($"- {book.Title} (ID: {book.Id})");
                Console.WriteLine($"  ISBN: {book.Isbn}");
                Console.WriteLine($"  Status: {book.Status}");
                if (book.Price.HasValue)
                {
                    Console.WriteLine($"  Price: {book.Price} {book.Currency}");
                }
                Console.WriteLine();
            }
        }

        static void DisplayBookDetails(BookCompleteFragment book)
        {
            Console.WriteLine($"Book: {book.Title} (ID: {book.Id})");
            Console.WriteLine($"ISBN: {book.Isbn}");
            Console.WriteLine($"Page Count: {book.PageCount}");
            Console.WriteLine($"Published: {book.PublishedDate}");
            Console.WriteLine($"Status: {book.Status}");
            
            if (book.Categories != null && book.Categories.Count > 0)
            {
                Console.WriteLine($"Categories: {string.Join(", ", book.Categories)}");
            }
            
            if (book.Price.HasValue)
            {
                Console.WriteLine($"Price: {book.Price} {book.Currency}");
            }
            
            Console.WriteLine($"Rating: {book.Rating}");
            
            if (book.Authors != null && book.Authors.Count > 0)
            {
                Console.WriteLine("Authors:");
                foreach (var author in book.Authors)
                {
                    Console.WriteLine($"- {author.FullName} (ID: {author.Id})");
                }
            }
            
            if (book.ShortDescription != null)
            {
                Console.WriteLine($"Description: {book.ShortDescription}");
            }
            
            Console.WriteLine();
        }

        static void DisplayBookReviews(BookWithReviewsFragment book)
        {
            Console.WriteLine($"Book: {book.Title} (ID: {book.Id})");
            Console.WriteLine($"Rating: {book.Rating}");
            
            if (book.Reviews != null && book.Reviews.Count > 0)
            {
                Console.WriteLine($"Reviews ({book.Reviews.Count}):");
                foreach (var review in book.Reviews)
                {
                    Console.WriteLine($"- {review.Rating}/5 by {review.User} on {review.CreatedAt}");
                    if (review.Comment != null)
                    {
                        Console.WriteLine($"  \"{review.Comment}\"");
                    }
                }
            }
            else
            {
                Console.WriteLine("No reviews yet.");
            }
            
            Console.WriteLine();
        }

        static void DisplayBookSummary(BookDetailedFragment book)
        {
            Console.WriteLine($"Book: {book.Title} (ID: {book.Id})");
            Console.WriteLine($"ISBN: {book.Isbn}");
            Console.WriteLine($"Status: {book.Status}");
            
            if (book.Authors != null && book.Authors.Count > 0)
            {
                Console.WriteLine($"Authors: {string.Join(", ", book.Authors.ConvertAll(a => a.FullName))}");
            }
            
            Console.WriteLine();
        }

        static void DisplayAuthors(List<AuthorBasicFragment> authors)
        {
            Console.WriteLine($"Found {authors.Count} authors:");
            foreach (var author in authors)
            {
                Console.WriteLine($"- {author.FullName} (ID: {author.Id})");
                Console.WriteLine($"  Name: {author.FirstName} {author.LastName}");
                if (author.AvatarUrl != null)
                {
                    Console.WriteLine($"  Avatar: {author.AvatarUrl}");
                }
                Console.WriteLine();
            }
        }

        static void DisplayAuthorDetails(AuthorCompleteFragment author)
        {
            Console.WriteLine($"Author: {author.FullName} (ID: {author.Id})");
            Console.WriteLine($"Name: {author.FirstName} {author.LastName}");
            
            if (author.Biography != null)
            {
                Console.WriteLine($"Biography: {author.Biography}");
            }
            
            if (author.Website != null)
            {
                Console.WriteLine($"Website: {author.Website}");
            }
            
            if (author.Twitter != null)
            {
                Console.WriteLine($"Twitter: {author.Twitter}");
            }
            
            if (author.Github != null)
            {
                Console.WriteLine($"GitHub: {author.Github}");
            }
            
            if (author.Books != null && author.Books.Count > 0)
            {
                Console.WriteLine($"Books ({author.Books.Count}):");
                foreach (var book in author.Books)
                {
                    Console.WriteLine($"- {book.Title} (ID: {book.Id})");
                    Console.WriteLine($"  Status: {book.Status}");
                    if (book.Price.HasValue)
                    {
                        Console.WriteLine($"  Price: {book.Price} {book.Currency}");
                    }
                }
            }
            
            Console.WriteLine();
        }

        static void DisplayAuthorBooks(AuthorWithBooksFragment author)
        {
            Console.WriteLine($"Author: {author.FullName} (ID: {author.Id})");
            
            if (author.Books != null && author.Books.Count > 0)
            {
                Console.WriteLine($"Books ({author.Books.Count}):");
                foreach (var book in author.Books)
                {
                    Console.WriteLine($"- {book.Title} (ID: {book.Id})");
                    Console.WriteLine($"  Status: {book.Status}");
                }
            }
            else
            {
                Console.WriteLine("No books yet.");
            }
            
            Console.WriteLine();
        }

        static void DisplayAuthorSummary(AuthorDetailedFragment author)
        {
            Console.WriteLine($"Author: {author.FullName} (ID: {author.Id})");
            Console.WriteLine($"Name: {author.FirstName} {author.LastName}");
            
            if (author.Biography != null)
            {
                Console.WriteLine($"Biography: {author.Biography}");
            }
            
            Console.WriteLine();
        }

        #endregion
    }

    #region Mock Client

    /// <summary>
    /// A mock implementation of the BookStoreClient for demonstration purposes
    /// </summary>
    public class MockBookStoreClient
    {
        // Mock data
        private readonly List<BookBasicFragment> _books;
        private readonly List<AuthorBasicFragment> _authors;

        public MockBookStoreClient()
        {
            // Initialize mock authors
            _authors = new List<AuthorBasicFragment>
            {
                new AuthorBasicFragment
                {
                    Id = "author-1",
                    FirstName = "John",
                    LastName = "Doe",
                    FullName = "John Doe",
                    AvatarUrl = "https://example.com/avatars/johndoe.jpg"
                },
                new AuthorBasicFragment
                {
                    Id = "author-2",
                    FirstName = "Alice",
                    LastName = "Johnson",
                    FullName = "Alice Johnson",
                    AvatarUrl = "https://example.com/avatars/alicejohnson.jpg"
                }
            };

            // Initialize mock books
            _books = new List<BookBasicFragment>
            {
                new BookBasicFragment
                {
                    Id = "book-1",
                    Title = "GraphQL in Practice",
                    Isbn = "978-1234567890",
                    Status = "PUBLISHED",
                    ThumbnailUrl = "https://example.com/books/graphql-in-practice.jpg",
                    Price = 39.99f,
                    Currency = "USD"
                },
                new BookBasicFragment
                {
                    Id = "book-2",
                    Title = "Advanced C# Programming",
                    Isbn = "978-0987654321",
                    Status = "PUBLISHED",
                    ThumbnailUrl = "https://example.com/books/advanced-csharp.jpg",
                    Price = 49.99f,
                    Currency = "USD"
                },
                new BookBasicFragment
                {
                    Id = "book-3",
                    Title = "Web API Design",
                    Isbn = "978-5678901234",
                    Status = "DRAFT",
                    ThumbnailUrl = "https://example.com/books/web-api-design.jpg",
                    Price = 29.99f,
                    Currency = "USD"
                }
            };
        }

        #region Book Queries

        public Task<List<BookBasicFragment>> GetAllBooksAsync()
        {
            // Return mock books
            return Task.FromResult(_books);
        }

        public Task<BookCompleteFragment> GetBookByIdAsync(string id)
        {
            // Find the book by ID
            var book = _books.Find(b => b.Id == id);
            
            if (book == null)
            {
                throw new Exception($"Book with ID {id} not found");
            }

            // Create a complete book
            var completeBook = new BookCompleteFragment
            {
                Id = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                PageCount = 300,
                PublishedDate = "2023-01-15",
                ThumbnailUrl = book.ThumbnailUrl,
                ShortDescription = "A comprehensive guide to GraphQL development",
                LongDescription = "This book provides a deep dive into GraphQL development, covering schema design, resolvers, and client integration.",
                Status = book.Status,
                Categories = new List<string> { "Programming", "GraphQL", "Web Development" },
                Price = book.Price,
                Currency = book.Currency,
                Rating = 4.5f,
                Authors = new List<BookCompleteFragment.AuthorsModel>
                {
                    new BookCompleteFragment.AuthorsModel
                    {
                        Id = "author-1",
                        FirstName = "John",
                        LastName = "Doe",
                        FullName = "John Doe",
                        AvatarUrl = "https://example.com/avatars/johndoe.jpg"
                    }
                },
                Reviews = new List<BookCompleteFragment.ReviewsModel>
                {
                    new BookCompleteFragment.ReviewsModel
                    {
                        Id = "review-1",
                        Rating = 5,
                        Comment = "Excellent book! Very informative and well-written.",
                        User = "user123",
                        CreatedAt = "2023-02-10"
                    },
                    new BookCompleteFragment.ReviewsModel
                    {
                        Id = "review-2",
                        Rating = 4,
                        Comment = "Good book, but could use more examples.",
                        User = "user456",
                        CreatedAt = "2023-02-15"
                    }
                }
            };

            return Task.FromResult(completeBook);
        }

        public Task<BookWithReviewsFragment> GetBookWithReviewsAsync(string id)
        {
            // Find the book by ID
            var book = _books.Find(b => b.Id == id);
            
            if (book == null)
            {
                throw new Exception($"Book with ID {id} not found");
            }

            // Create a book with reviews
            var bookWithReviews = new BookWithReviewsFragment
            {
                Id = book.Id,
                Title = book.Title,
                Rating = 4.5f,
                Reviews = new List<BookWithReviewsFragment.ReviewsModel>
                {
                    new BookWithReviewsFragment.ReviewsModel
                    {
                        Id = "review-1",
                        Rating = 5,
                        Comment = "Excellent book! Very informative and well-written.",
                        User = "user123",
                        CreatedAt = "2023-02-10"
                    },
                    new BookWithReviewsFragment.ReviewsModel
                    {
                        Id = "review-2",
                        Rating = 4,
                        Comment = "Good book, but could use more examples.",
                        User = "user456",
                        CreatedAt = "2023-02-15"
                    }
                }
            };

            return Task.FromResult(bookWithReviews);
        }

        #endregion

        #region Author Queries

        public Task<List<AuthorBasicFragment>> GetAllAuthorsAsync()
        {
            // Return mock authors
            return Task.FromResult(_authors);
        }

        public Task<AuthorCompleteFragment> GetAuthorByIdAsync(string id)
        {
            // Find the author by ID
            var author = _authors.Find(a => a.Id == id);
            
            if (author == null)
            {
                throw new Exception($"Author with ID {id} not found");
            }

            // Create a complete author
            var completeAuthor = new AuthorCompleteFragment
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName,
                FullName = author.FullName,
                Biography = "John Doe is a software developer and author with over 10 years of experience.",
                AvatarUrl = author.AvatarUrl,
                Website = "https://johndoe.dev",
                Twitter = "https://twitter.com/johndoe",
                Github = "https://github.com/johndoe",
                Books = new List<AuthorCompleteFragment.BooksModel>
                {
                    new AuthorCompleteFragment.BooksModel
                    {
                        Id = "book-1",
                        Title = "GraphQL in Practice",
                        Isbn = "978-1234567890",
                        PageCount = 300,
                        PublishedDate = "2023-01-15",
                        ThumbnailUrl = "https://example.com/books/graphql-in-practice.jpg",
                        ShortDescription = "A comprehensive guide to GraphQL development",
                        Status = "PUBLISHED",
                        Categories = new List<string> { "Programming", "GraphQL", "Web Development" },
                        Price = 39.99f,
                        Currency = "USD",
                        Rating = 4.5f
                    },
                    new AuthorCompleteFragment.BooksModel
                    {
                        Id = "book-3",
                        Title = "Web API Design",
                        Isbn = "978-5678901234",
                        PageCount = 250,
                        PublishedDate = null,
                        ThumbnailUrl = "https://example.com/books/web-api-design.jpg",
                        ShortDescription = "A guide to designing effective web APIs",
                        Status = "DRAFT",
                        Categories = new List<string> { "Programming", "API", "Web Development" },
                        Price = 29.99f,
                        Currency = "USD",
                        Rating = null
                    }
                }
            };

            return Task.FromResult(completeAuthor);
        }

        public Task<AuthorWithBooksFragment> GetAuthorWithBooksAsync(string id)
        {
            // Find the author by ID
            var author = _authors.Find(a => a.Id == id);
            
            if (author == null)
            {
                throw new Exception($"Author with ID {id} not found");
            }

            // Create an author with books
            var authorWithBooks = new AuthorWithBooksFragment
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName,
                FullName = author.FullName,
                AvatarUrl = author.AvatarUrl,
                Books = new List<AuthorWithBooksFragment.BooksModel>
                {
                    new AuthorWithBooksFragment.BooksModel
                    {
                        Id = "book-1",
                        Title = "GraphQL in Practice",
                        ThumbnailUrl = "https://example.com/books/graphql-in-practice.jpg",
                        Status = "PUBLISHED"
                    },
                    new AuthorWithBooksFragment.BooksModel
                    {
                        Id = "book-3",
                        Title = "Web API Design",
                        ThumbnailUrl = "https://example.com/books/web-api-design.jpg",
                        Status = "DRAFT"
                    }
                }
            };

            return Task.FromResult(authorWithBooks);
        }

        #endregion

        #region Book Mutations

        public Task<BookDetailedFragment> AddBookAsync(AddBookInput input)
        {
            // Create a new book
            var newBook = new BookDetailedFragment
            {
                Id = $"book-{_books.Count + 1}",
                Title = input.Title,
                Isbn = input.Isbn,
                PageCount = input.PageCount,
                PublishedDate = input.PublishedDate,
                ThumbnailUrl = input.ThumbnailUrl,
                ShortDescription = input.ShortDescription,
                LongDescription = input.LongDescription,
                Status = input.Status,
                Categories = input.Categories,
                Price = input.Price,
                Currency = input.Currency,
                Rating = null,
                Authors = new List<BookDetailedFragment.AuthorsModel>()
            };

            // Add authors
            foreach (var authorId in input.AuthorIds)
            {
                var author = _authors.Find(a => a.Id == authorId);
                if (author != null)
                {
                    newBook.Authors.Add(new BookDetailedFragment.AuthorsModel
                    {
                        Id = author.Id,
                        FullName = author.FullName
                    });
                }
            }

            // Add the book to the list
            _books.Add(new BookBasicFragment
            {
                Id = newBook.Id,
                Title = newBook.Title,
                Isbn = newBook.Isbn,
                Status = newBook.Status,
                ThumbnailUrl = newBook.ThumbnailUrl,
                Price = newBook.Price,
                Currency = newBook.Currency
            });

            return Task.FromResult(newBook);
        }

        public Task<BookDetailedFragment> UpdateBookAsync(string id, UpdateBookInput input)
        {
            // Find the book by ID
            var bookIndex = _books.FindIndex(b => b.Id == id);
            
            if (bookIndex == -1)
            {
                throw new Exception($"Book with ID {id} not found");
            }

            // Update the book
            var book = _books[bookIndex];
            
            if (input.Title != null)
            {
                book.Title = input.Title;
            }
            
            if (input.Isbn != null)
            {
                book.Isbn = input.Isbn;
            }
            
            if (input.Status != null)
            {
                book.Status = input.Status;
            }
            
            if (input.ThumbnailUrl != null)
            {
                book.ThumbnailUrl = input.ThumbnailUrl;
            }
            
            if (input.Price.HasValue)
            {
                book.Price = input.Price;
            }
            
            if (input.Currency != null)
            {
                book.Currency = input.Currency;
            }

            // Update the book in the list
            _books[bookIndex] = book;

            // Create a detailed book
            var updatedBook = new BookDetailedFragment
            {
                Id = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                PageCount = input.PageCount ?? 300,
                PublishedDate = input.PublishedDate ?? "2023-01-15",
                ThumbnailUrl = book.ThumbnailUrl,
                ShortDescription = input.ShortDescription ?? "A comprehensive guide to GraphQL development",
                LongDescription = input.LongDescription ?? "This book provides a deep dive into GraphQL development, covering schema design, resolvers, and client integration.",
                Status = book.Status,
                Categories = input.Categories ?? new List<string> { "Programming", "GraphQL", "Web Development" },
                Price = book.Price,
                Currency = book.Currency,
                Rating = 4.5f,
                Authors = new List<BookDetailedFragment.AuthorsModel>
                {
                    new BookDetailedFragment.AuthorsModel
                    {
                        Id = "author-1",
                        FullName = "John Doe"
                    }
                }
            };

            return Task.FromResult(updatedBook);
        }

        public Task<bool> DeleteBookAsync(string id)
        {
            // Find the book by ID
            var bookIndex = _books.FindIndex(b => b.Id == id);
            
            if (bookIndex == -1)
            {
                throw new Exception($"Book with ID {id} not found");
            }

            // Remove the book from the list
            _books.RemoveAt(bookIndex);

            return Task.FromResult(true);
        }

        #endregion

        #region Author Mutations

        public Task<AuthorDetailedFragment> AddAuthorAsync(AddAuthorInput input)
        {
            // Create a new author
            var newAuthor = new AuthorDetailedFragment
            {
                Id = $"author-{_authors.Count + 1}",
                FirstName = input.FirstName,
                LastName = input.LastName,
                FullName = $"{input.FirstName} {input.LastName}",
                Biography = input.Biography,
                AvatarUrl = input.AvatarUrl,
                Website = input.Website,
                Twitter = input.Twitter,
                Github = input.Github
            };

            // Add the author to the list
            _authors.Add(new AuthorBasicFragment
            {
                Id = newAuthor.Id,
                FirstName = newAuthor.FirstName,
                LastName = newAuthor.LastName,
                FullName = newAuthor.FullName,
                AvatarUrl = newAuthor.AvatarUrl
            });

            return Task.FromResult(newAuthor);
        }

        public Task<AuthorDetailedFragment> UpdateAuthorAsync(string id, UpdateAuthorInput input)
        {
            // Find the author by ID
            var authorIndex = _authors.FindIndex(a => a.Id == id);
            
            if (authorIndex == -1)
            {
                throw new Exception($"Author with ID {id} not found");
            }

            // Update the author
            var author = _authors[authorIndex];
            
            if (input.FirstName != null)
            {
                author.FirstName = input.FirstName;
                author.FullName = $"{input.FirstName} {author.LastName}";
            }
            
            if (input.LastName != null)
            {
                author.LastName = input.LastName;
                author.FullName = $"{author.FirstName} {input.LastName}";
            }
            
            if (input.AvatarUrl != null)
            {
                author.AvatarUrl = input.AvatarUrl;
            }

            // Update the author in the list
            _authors[authorIndex] = author;

            // Create a detailed author
            var updatedAuthor = new AuthorDetailedFragment
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName,
                FullName = author.FullName,
                Biography = input.Biography ?? "Updated author biography.",
                AvatarUrl = author.AvatarUrl,
                Website = input.Website ?? "https://example.com",
                Twitter = input.Twitter ?? "https://twitter.com/example",
                Github = input.Github ?? "https://github.com/example"
            };

            return Task.FromResult(updatedAuthor);
        }

        public Task<bool> DeleteAuthorAsync(string id)
        {
            // Find the author by ID
            var authorIndex = _authors.FindIndex(a => a.Id == id);
            
            if (authorIndex == -1)
            {
                throw new Exception($"Author with ID {id} not found");
            }

            // Remove the author from the list
            _authors.RemoveAt(authorIndex);

            return Task.FromResult(true);
        }

        #endregion
    }

    #endregion
}