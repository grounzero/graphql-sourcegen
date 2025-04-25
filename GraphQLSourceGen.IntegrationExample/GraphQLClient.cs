using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using GraphQL.Generated;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace GraphQLSourceGen.IntegrationExample
{
    /// <summary>
    /// A client for interacting with a GraphQL API using generated fragment types
    /// </summary>
    public class BookStoreClient : IDisposable
    {
        private readonly GraphQLHttpClient _client;
        private bool _disposed = false;

        /// <summary>
        /// Creates a new instance of the BookStoreClient
        /// </summary>
        /// <param name="endpoint">The GraphQL API endpoint URL</param>
        public BookStoreClient(string endpoint)
        {
            var options = new GraphQLHttpClientOptions
            {
                EndPoint = new Uri(endpoint)
            };

            _client = new GraphQLHttpClient(options, new SystemTextJsonSerializer());
        }

        #region Book Queries

        /// <summary>
        /// Gets all books with basic information
        /// </summary>
        public async Task<List<BookBasicFragment>> GetAllBooksAsync()
        {
            // Create a request using the generated fragment
            var request = new GraphQLHttpRequest
            {
                Query = @"
                    query GetAllBooks {
                        books {
                            id
                            title
                            isbn
                            status
                            thumbnailUrl
                            price
                            currency
                        }
                    }"
            };

            // Execute the request
            var response = await _client.SendQueryAsync<BooksResponse>(request);
            
            // Return the books
            return response.Data.Books;
        }

        /// <summary>
        /// Gets a book by ID with complete information
        /// </summary>
        public async Task<BookCompleteFragment> GetBookByIdAsync(string id)
        {
            // Create a request using the generated fragment
            var request = new GraphQLHttpRequest
            {
                Query = @"
                    query GetBookById($id: ID!) {
                        book(id: $id) {
                            id
                            title
                            isbn
                            pageCount
                            publishedDate
                            thumbnailUrl
                            shortDescription
                            longDescription
                            status
                            categories
                            price
                            currency
                            rating
                            authors {
                                id
                                firstName
                                lastName
                                fullName
                                avatarUrl
                            }
                            reviews {
                                id
                                rating
                                comment
                                user
                                createdAt
                            }
                        }
                    }",
                Variables = new { id }
            };

            // Execute the request
            var response = await _client.SendQueryAsync<BookResponse>(request);
            
            // Return the book
            return response.Data.Book;
        }

        /// <summary>
        /// Gets a book with its reviews
        /// </summary>
        public async Task<BookWithReviewsFragment> GetBookWithReviewsAsync(string id)
        {
            // Create a request using the generated fragment
            var request = new GraphQLHttpRequest
            {
                Query = @"
                    query GetBookWithReviews($id: ID!) {
                        book(id: $id) {
                            id
                            title
                            rating
                            reviews {
                                id
                                rating
                                comment
                                user
                                createdAt
                            }
                        }
                    }",
                Variables = new { id }
            };

            // Execute the request
            var response = await _client.SendQueryAsync<BookReviewsResponse>(request);
            
            // Return the book with reviews
            return response.Data.Book;
        }

        #endregion

        #region Author Queries

        /// <summary>
        /// Gets all authors with basic information
        /// </summary>
        public async Task<List<AuthorBasicFragment>> GetAllAuthorsAsync()
        {
            // Create a request using the generated fragment
            var request = new GraphQLHttpRequest
            {
                Query = @"
                    query GetAllAuthors {
                        authors {
                            id
                            firstName
                            lastName
                            fullName
                            avatarUrl
                        }
                    }"
            };

            // Execute the request
            var response = await _client.SendQueryAsync<AuthorsResponse>(request);
            
            // Return the authors
            return response.Data.Authors;
        }

        /// <summary>
        /// Gets an author by ID with complete information
        /// </summary>
        public async Task<AuthorCompleteFragment> GetAuthorByIdAsync(string id)
        {
            // Create a request using the generated fragment
            var request = new GraphQLHttpRequest
            {
                Query = @"
                    query GetAuthorById($id: ID!) {
                        author(id: $id) {
                            id
                            firstName
                            lastName
                            fullName
                            biography
                            avatarUrl
                            website
                            twitter
                            github
                            books {
                                id
                                title
                                isbn
                                pageCount
                                publishedDate
                                thumbnailUrl
                                shortDescription
                                status
                                categories
                                price
                                currency
                                rating
                            }
                        }
                    }",
                Variables = new { id }
            };

            // Execute the request
            var response = await _client.SendQueryAsync<AuthorResponse>(request);
            
            // Return the author
            return response.Data.Author;
        }

        /// <summary>
        /// Gets an author with their books
        /// </summary>
        public async Task<AuthorWithBooksFragment> GetAuthorWithBooksAsync(string id)
        {
            // Create a request using the generated fragment
            var request = new GraphQLHttpRequest
            {
                Query = @"
                    query GetAuthorWithBooks($id: ID!) {
                        author(id: $id) {
                            id
                            firstName
                            lastName
                            fullName
                            avatarUrl
                            books {
                                id
                                title
                                thumbnailUrl
                                status
                            }
                        }
                    }",
                Variables = new { id }
            };

            // Execute the request
            var response = await _client.SendQueryAsync<AuthorBooksResponse>(request);
            
            // Return the author with books
            return response.Data.Author;
        }

        #endregion

        #region Book Mutations

        /// <summary>
        /// Adds a new book
        /// </summary>
        public async Task<BookDetailedFragment> AddBookAsync(AddBookInput input)
        {
            // Create a request using the generated fragment
            var request = new GraphQLHttpRequest
            {
                Query = @"
                    mutation AddBook($input: AddBookInput!) {
                        addBook(input: $input) {
                            id
                            title
                            isbn
                            pageCount
                            publishedDate
                            thumbnailUrl
                            shortDescription
                            longDescription
                            status
                            categories
                            price
                            currency
                            rating
                            authors {
                                id
                                fullName
                            }
                        }
                    }",
                Variables = new { input }
            };

            // Execute the request
            var response = await _client.SendMutationAsync<AddBookResponse>(request);
            
            // Return the added book
            return response.Data.AddBook;
        }

        /// <summary>
        /// Updates an existing book
        /// </summary>
        public async Task<BookDetailedFragment> UpdateBookAsync(string id, UpdateBookInput input)
        {
            // Create a request using the generated fragment
            var request = new GraphQLHttpRequest
            {
                Query = @"
                    mutation UpdateBook($id: ID!, $input: UpdateBookInput!) {
                        updateBook(id: $id, input: $input) {
                            id
                            title
                            isbn
                            pageCount
                            publishedDate
                            thumbnailUrl
                            shortDescription
                            longDescription
                            status
                            categories
                            price
                            currency
                            rating
                            authors {
                                id
                                fullName
                            }
                        }
                    }",
                Variables = new { id, input }
            };

            // Execute the request
            var response = await _client.SendMutationAsync<UpdateBookResponse>(request);
            
            // Return the updated book
            return response.Data.UpdateBook;
        }

        /// <summary>
        /// Deletes a book
        /// </summary>
        public async Task<bool> DeleteBookAsync(string id)
        {
            // Create a request using the generated fragment
            var request = new GraphQLHttpRequest
            {
                Query = @"
                    mutation DeleteBook($id: ID!) {
                        deleteBook(id: $id)
                    }",
                Variables = new { id }
            };

            // Execute the request
            var response = await _client.SendMutationAsync<DeleteBookResponse>(request);
            
            // Return the result
            return response.Data.DeleteBook;
        }

        #endregion

        #region Author Mutations

        /// <summary>
        /// Adds a new author
        /// </summary>
        public async Task<AuthorDetailedFragment> AddAuthorAsync(AddAuthorInput input)
        {
            // Create a request using the generated fragment
            var request = new GraphQLHttpRequest
            {
                Query = @"
                    mutation AddAuthor($input: AddAuthorInput!) {
                        addAuthor(input: $input) {
                            id
                            firstName
                            lastName
                            fullName
                            biography
                            avatarUrl
                            website
                            twitter
                            github
                        }
                    }",
                Variables = new { input }
            };

            // Execute the request
            var response = await _client.SendMutationAsync<AddAuthorResponse>(request);
            
            // Return the added author
            return response.Data.AddAuthor;
        }

        /// <summary>
        /// Updates an existing author
        /// </summary>
        public async Task<AuthorDetailedFragment> UpdateAuthorAsync(string id, UpdateAuthorInput input)
        {
            // Create a request using the generated fragment
            var request = new GraphQLHttpRequest
            {
                Query = @"
                    mutation UpdateAuthor($id: ID!, $input: UpdateAuthorInput!) {
                        updateAuthor(id: $id, input: $input) {
                            id
                            firstName
                            lastName
                            fullName
                            biography
                            avatarUrl
                            website
                            twitter
                            github
                        }
                    }",
                Variables = new { id, input }
            };

            // Execute the request
            var response = await _client.SendMutationAsync<UpdateAuthorResponse>(request);
            
            // Return the updated author
            return response.Data.UpdateAuthor;
        }

        /// <summary>
        /// Deletes an author
        /// </summary>
        public async Task<bool> DeleteAuthorAsync(string id)
        {
            // Create a request using the generated fragment
            var request = new GraphQLHttpRequest
            {
                Query = @"
                    mutation DeleteAuthor($id: ID!) {
                        deleteAuthor(id: $id)
                    }",
                Variables = new { id }
            };

            // Execute the request
            var response = await _client.SendMutationAsync<DeleteAuthorResponse>(request);
            
            // Return the result
            return response.Data.DeleteAuthor;
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Disposes the client
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the client
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _client?.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion
    }

    #region Response Types

    // Response types for queries
    public class BooksResponse
    {
        public BooksData Data { get; set; }
    }

    public class BooksData
    {
        public List<BookBasicFragment> Books { get; set; }
    }

    public class BookResponse
    {
        public BookData Data { get; set; }
    }

    public class BookData
    {
        public BookCompleteFragment Book { get; set; }
    }

    public class BookReviewsResponse
    {
        public BookReviewsData Data { get; set; }
    }

    public class BookReviewsData
    {
        public BookWithReviewsFragment Book { get; set; }
    }

    public class AuthorsResponse
    {
        public AuthorsData Data { get; set; }
    }

    public class AuthorsData
    {
        public List<AuthorBasicFragment> Authors { get; set; }
    }

    public class AuthorResponse
    {
        public AuthorData Data { get; set; }
    }

    public class AuthorData
    {
        public AuthorCompleteFragment Author { get; set; }
    }

    public class AuthorBooksResponse
    {
        public AuthorBooksData Data { get; set; }
    }

    public class AuthorBooksData
    {
        public AuthorWithBooksFragment Author { get; set; }
    }

    // Response types for mutations
    public class AddBookResponse
    {
        public AddBookData Data { get; set; }
    }

    public class AddBookData
    {
        public BookDetailedFragment AddBook { get; set; }
    }

    public class UpdateBookResponse
    {
        public UpdateBookData Data { get; set; }
    }

    public class UpdateBookData
    {
        public BookDetailedFragment UpdateBook { get; set; }
    }

    public class DeleteBookResponse
    {
        public DeleteBookData Data { get; set; }
    }

    public class DeleteBookData
    {
        public bool DeleteBook { get; set; }
    }

    public class AddAuthorResponse
    {
        public AddAuthorData Data { get; set; }
    }

    public class AddAuthorData
    {
        public AuthorDetailedFragment AddAuthor { get; set; }
    }

    public class UpdateAuthorResponse
    {
        public UpdateAuthorData Data { get; set; }
    }

    public class UpdateAuthorData
    {
        public AuthorDetailedFragment UpdateAuthor { get; set; }
    }

    public class DeleteAuthorResponse
    {
        public DeleteAuthorData Data { get; set; }
    }

    public class DeleteAuthorData
    {
        public bool DeleteAuthor { get; set; }
    }

    #endregion

    #region Input Types

    // Input types for mutations
    public class AddBookInput
    {
        public string Title { get; set; }
        public string Isbn { get; set; }
        public int PageCount { get; set; }
        public string PublishedDate { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string Status { get; set; }
        public List<string> AuthorIds { get; set; }
        public List<string> Categories { get; set; }
        public float? Price { get; set; }
        public string Currency { get; set; }
    }

    public class UpdateBookInput
    {
        public string Title { get; set; }
        public string Isbn { get; set; }
        public int? PageCount { get; set; }
        public string PublishedDate { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string Status { get; set; }
        public List<string> AuthorIds { get; set; }
        public List<string> Categories { get; set; }
        public float? Price { get; set; }
        public string Currency { get; set; }
    }

    public class AddAuthorInput
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Biography { get; set; }
        public string AvatarUrl { get; set; }
        public string Website { get; set; }
        public string Twitter { get; set; }
        public string Github { get; set; }
    }

    public class UpdateAuthorInput
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Biography { get; set; }
        public string AvatarUrl { get; set; }
        public string Website { get; set; }
        public string Twitter { get; set; }
        public string Github { get; set; }
    }

    #endregion
}