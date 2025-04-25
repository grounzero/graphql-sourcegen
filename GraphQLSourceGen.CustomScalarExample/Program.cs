using GraphQL.Generated;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace GraphQLSourceGen.CustomScalarExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GraphQL Source Generator - Custom Scalar Mappings Example");
            Console.WriteLine("=======================================================");
            Console.WriteLine();
            Console.WriteLine("This example demonstrates how to map GraphQL scalar types to C# types.");
            Console.WriteLine();

            try
            {
                // Demonstrate user with basic scalar mappings
                DemonstrateUserScalars();

                // Demonstrate event with date/time scalar mappings
                DemonstrateEventScalars();

                // Demonstrate document with file-related scalar mappings
                DemonstrateDocumentScalars();

                // Demonstrate transaction with financial scalar mappings
                DemonstrateTransactionScalars();

                // Show all scalar mappings in a table
                DisplayScalarMappingsTable();
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

        static void DemonstrateUserScalars()
        {
            Console.WriteLine("1. User with Basic Scalar Mappings");
            Console.WriteLine("----------------------------------");

            // Create a user with various scalar types
            var user = new UserWithScalarsFragment
            {
                Id = Guid.NewGuid(),
                Name = "Alice Johnson",
                Email = "alice.johnson@example.com",
                Phone = "+1-555-123-4567",
                DateOfBirth = new DateOnly(1985, 3, 15),
                RegisteredAt = DateTime.Now.AddYears(-2),
                LastLoginAt = DateTime.Now.AddDays(-3),
                ProfileColor = Color.FromArgb(64, 128, 255),
                Settings = JObject.Parse(@"{
                    ""theme"": ""dark"",
                    ""notifications"": {
                        ""email"": true,
                        ""push"": false
                    },
                    ""preferences"": {
                        ""language"": ""en-US"",
                        ""timezone"": ""America/New_York""
                    }
                }"),
                ProfilePicture = new Uri("https://example.com/users/alice/profile.jpg")
            };

            // Display user information
            Console.WriteLine($"User ID (UUID): {user.Id}");
            Console.WriteLine($"Name: {user.Name}");
            Console.WriteLine($"Email (Email): {user.Email}");
            Console.WriteLine($"Phone (PhoneNumber): {user.Phone}");
            Console.WriteLine($"Date of Birth (Date): {user.DateOfBirth}");
            Console.WriteLine($"Registered At (DateTime): {user.RegisteredAt}");
            Console.WriteLine($"Last Login At (DateTime): {user.LastLoginAt}");
            Console.WriteLine($"Profile Color (Color): RGB({user.ProfileColor.R}, {user.ProfileColor.G}, {user.ProfileColor.B})");
            Console.WriteLine($"Settings (JSON): {user.Settings}");
            Console.WriteLine($"Profile Picture (URL): {user.ProfilePicture}");
            Console.WriteLine();
        }

        static void DemonstrateEventScalars()
        {
            Console.WriteLine("2. Event with Date/Time Scalar Mappings");
            Console.WriteLine("---------------------------------------");

            // Create an event with date/time scalar types
            var eventObj = new EventWithDateTimeScalarsFragment
            {
                Id = Guid.NewGuid(),
                Title = "GraphQL Conference 2025",
                Description = "Annual conference for GraphQL developers",
                StartDate = new DateTime(2025, 6, 15, 9, 0, 0),
                EndDate = new DateTime(2025, 6, 17, 17, 0, 0),
                Duration = TimeSpan.FromHours(48),
                Location = new EventWithDateTimeScalarsFragment.LocationModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Tech Convention Center",
                    Address = "123 Innovation Blvd, San Francisco, CA",
                    OpeningTime = new TimeOnly(8, 0),
                    ClosingTime = new TimeOnly(22, 0),
                    Timezone = "America/Los_Angeles"
                },
                Organizer = new EventWithDateTimeScalarsFragment.OrganizerModel
                {
                    Id = Guid.NewGuid(),
                    Name = "GraphQL Foundation"
                },
                MaxAttendees = new BigInteger(1000),
                Metadata = JObject.Parse(@"{
                    ""venue"": {
                        ""rooms"": [""Main Hall"", ""Workshop Room A"", ""Workshop Room B""],
                        ""capacity"": 1200,
                        ""amenities"": [""Wi-Fi"", ""AV Equipment"", ""Catering""]
                    },
                    ""sponsors"": [""TechCorp"", ""DevTools Inc."", ""API Solutions""]
                }")
            };

            // Display event information
            Console.WriteLine($"Event ID (UUID): {eventObj.Id}");
            Console.WriteLine($"Title: {eventObj.Title}");
            Console.WriteLine($"Description: {eventObj.Description}");
            Console.WriteLine($"Start Date (DateTime): {eventObj.StartDate}");
            Console.WriteLine($"End Date (DateTime): {eventObj.EndDate}");
            Console.WriteLine($"Duration (Duration/TimeSpan): {eventObj.Duration.TotalHours} hours");
            
            if (eventObj.Location != null)
            {
                Console.WriteLine("Location:");
                Console.WriteLine($"  Name: {eventObj.Location.Name}");
                Console.WriteLine($"  Address: {eventObj.Location.Address}");
                Console.WriteLine($"  Opening Time (Time): {eventObj.Location.OpeningTime}");
                Console.WriteLine($"  Closing Time (Time): {eventObj.Location.ClosingTime}");
                Console.WriteLine($"  Timezone: {eventObj.Location.Timezone}");
            }
            
            Console.WriteLine($"Organizer: {eventObj.Organizer?.Name}");
            Console.WriteLine($"Max Attendees (BigInt): {eventObj.MaxAttendees}");
            Console.WriteLine($"Metadata (JSON): {eventObj.Metadata}");
            Console.WriteLine();
        }

        static void DemonstrateDocumentScalars()
        {
            Console.WriteLine("3. Document with File-Related Scalar Mappings");
            Console.WriteLine("---------------------------------------------");

            // Create a document with file-related scalar types
            var document = new DocumentWithFileScalarsFragment
            {
                Id = Guid.NewGuid(),
                Title = "GraphQL Implementation Guide.pdf",
                FileSize = new BigInteger(15728640), // 15 MB
                MimeType = "application/pdf",
                UploadedAt = DateTime.Now.AddDays(-7),
                UploadedBy = new DocumentWithFileScalarsFragment.UploadedByModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Bob Williams",
                    Email = "bob.williams@example.com"
                },
                Content = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x35 }, // PDF header (partial)
                Checksum = "a1b2c3d4e5f6g7h8i9j0",
                DownloadUrl = new Uri("https://example.com/documents/graphql-guide.pdf"),
                Metadata = JObject.Parse(@"{
                    ""author"": ""GraphQL Team"",
                    ""pages"": 42,
                    ""version"": ""2.0"",
                    ""keywords"": [""GraphQL"", ""API"", ""Implementation"", ""Guide""]
                }")
            };

            // Display document information
            Console.WriteLine($"Document ID (UUID): {document.Id}");
            Console.WriteLine($"Title: {document.Title}");
            Console.WriteLine($"File Size (BigInt): {document.FileSize} bytes ({document.FileSize / 1048576.0:F2} MB)");
            Console.WriteLine($"MIME Type: {document.MimeType}");
            Console.WriteLine($"Uploaded At (DateTime): {document.UploadedAt}");
            
            if (document.UploadedBy != null)
            {
                Console.WriteLine($"Uploaded By: {document.UploadedBy.Name} ({document.UploadedBy.Email})");
            }
            
            if (document.Content != null)
            {
                Console.WriteLine($"Content (ByteArray): {BitConverter.ToString(document.Content).Replace("-", " ")} (first 8 bytes)");
            }
            
            Console.WriteLine($"Checksum: {document.Checksum}");
            Console.WriteLine($"Download URL (URL): {document.DownloadUrl}");
            Console.WriteLine($"Metadata (JSON): {document.Metadata}");
            Console.WriteLine();
        }

        static void DemonstrateTransactionScalars()
        {
            Console.WriteLine("4. Transaction with Financial Scalar Mappings");
            Console.WriteLine("--------------------------------------------");

            // Create a transaction with financial scalar types
            var transaction = new TransactionWithFinancialScalarsFragment
            {
                Id = Guid.NewGuid(),
                User = new TransactionWithFinancialScalarsFragment.UserModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Charlie Brown",
                    Email = "charlie.brown@example.com"
                },
                Amount = 299.99m,
                Currency = "USD",
                Timestamp = DateTime.Now.AddHours(-2),
                Status = "COMPLETED",
                Reference = "INV-2025-04-24-001",
                Metadata = JObject.Parse(@"{
                    ""paymentMethod"": ""Credit Card"",
                    ""cardType"": ""Visa"",
                    ""lastFour"": ""4242"",
                    ""items"": [
                        {
                            ""id"": ""prod-001"",
                            ""name"": ""GraphQL Pro License"",
                            ""quantity"": 1,
                            ""price"": 299.99
                        }
                    ]
                }")
            };

            // Display transaction information
            Console.WriteLine($"Transaction ID (UUID): {transaction.Id}");
            
            if (transaction.User != null)
            {
                Console.WriteLine($"User: {transaction.User.Name} ({transaction.User.Email})");
            }
            
            Console.WriteLine($"Amount (Decimal): {transaction.Amount} {transaction.Currency}");
            Console.WriteLine($"Timestamp (DateTime): {transaction.Timestamp}");
            Console.WriteLine($"Status: {transaction.Status}");
            Console.WriteLine($"Reference: {transaction.Reference}");
            Console.WriteLine($"Metadata (JSON): {transaction.Metadata}");
            Console.WriteLine();
        }

        static void DisplayScalarMappingsTable()
        {
            Console.WriteLine("Custom Scalar Mappings Table");
            Console.WriteLine("---------------------------");
            Console.WriteLine("| GraphQL Scalar | C# Type                     | Example Value                          |");
            Console.WriteLine("|---------------|-----------------------------|-----------------------------------------|");
            Console.WriteLine("| UUID          | System.Guid                 | 123e4567-e89b-12d3-a456-426614174000   |");
            Console.WriteLine("| Email         | System.String               | user@example.com                        |");
            Console.WriteLine("| PhoneNumber   | System.String               | +1-555-123-4567                         |");
            Console.WriteLine("| DateTime      | System.DateTime             | 2025-04-24T15:30:45Z                    |");
            Console.WriteLine("| Date          | System.DateOnly             | 2025-04-24                              |");
            Console.WriteLine("| Time          | System.TimeOnly             | 15:30:45                                |");
            Console.WriteLine("| Duration      | System.TimeSpan             | 48:00:00 (2 days)                       |");
            Console.WriteLine("| URL           | System.Uri                  | https://example.com/path                |");
            Console.WriteLine("| JSON          | Newtonsoft.Json.Linq.JObject| {\"key\": \"value\", \"nested\": {}}      |");
            Console.WriteLine("| BigInt        | System.Numerics.BigInteger  | 9007199254740992                        |");
            Console.WriteLine("| Decimal       | System.Decimal              | 299.99                                  |");
            Console.WriteLine("| Upload        | System.IO.Stream            | (Binary data stream)                    |");
            Console.WriteLine("| Byte          | System.Byte                 | 255                                     |");
            Console.WriteLine("| ByteArray     | System.Byte[]               | [0x25, 0x50, 0x44, 0x46, ...]          |");
            Console.WriteLine("| Color         | System.Drawing.Color        | RGB(64, 128, 255)                       |");
        }
    }
}