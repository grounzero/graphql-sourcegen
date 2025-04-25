using GraphQL.Generated;
using System;
using System.Collections.Generic;

namespace GraphQLSourceGen.SchemaAwareExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GraphQL Source Generator - Schema-Aware Example");
            Console.WriteLine("===============================================");
            Console.WriteLine();
            Console.WriteLine("This example demonstrates schema-aware fragment generation with type inference.");
            Console.WriteLine();

            try
            {
                // Demonstrate SimpleUser fragment
                DemonstrateSimpleUser();

                // Run the schema-aware demo
                SchemaAwareDemo.RunDemo();
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

        static void DemonstrateSimpleUser()
        {
            Console.WriteLine("1. SimpleUser Fragment Example");
            Console.WriteLine("-----------------------------");

            // Create a SimpleUserFragment instance
            var user = new SimpleUserFragment
            {
                Id = "user-123",
                Name = "Jane Smith",
                Email = "jane.smith@example.com",
                IsActive = "true",
                Role = "EDITOR",
                Bio = "Technical writer and GraphQL enthusiast",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            // Display user information
            Console.WriteLine($"User: {user.Name} ({user.Email})");
            Console.WriteLine($"Role: {user.Role}");
            Console.WriteLine($"Active: {user.IsActive}");
            Console.WriteLine($"Bio: {user.Bio}");
            Console.WriteLine($"Avatar: {user.AvatarUrl}");
            
            Console.WriteLine();
        }
    }
}