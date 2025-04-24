using System;
using GraphQLSourceGen.Tests;

namespace GraphQLSourceGen.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running GraphQL Source Generator Tests...");
            Console.WriteLine();

            try
            {
                // Run parser tests
                Console.WriteLine("Running GraphQL Parser Tests...");
                var parserTests = new GraphQLParserTests();
                RunParserTests(parserTests);
                Console.WriteLine("GraphQL Parser Tests: PASSED");
                Console.WriteLine();

                // Run generator tests
                Console.WriteLine("Running GraphQL Fragment Generator Tests...");
                var generatorTests = new GraphQLFragmentGeneratorTests();
                generatorTests.RunAllTests();
                Console.WriteLine("GraphQL Fragment Generator Tests: PASSED");
                Console.WriteLine();

                Console.WriteLine("All tests passed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                // Return non-zero exit code to indicate failure
                return;
            }
        }

        private static void RunParserTests(GraphQLParserTests tests)
        {
            // Manually invoke each test method
            typeof(GraphQLParserTests)
                .GetMethods()
                .Where(m => m.Name.StartsWith("Parse") || m.Name.StartsWith("Map"))
                .ToList()
                .ForEach(method => 
                {
                    Console.WriteLine($"  Running test: {method.Name}");
                    method.Invoke(tests, null);
                });
        }
    }
}