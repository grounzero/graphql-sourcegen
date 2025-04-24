using System;
using System.Collections.Generic;

namespace GraphQL.Generated.PostWithStats
{
    /// <summary>
    /// Generated from GraphQL fragment 'PostWithStats' on type 'Post'
    /// </summary>
    public class PostWithStatsFragment
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; } // ID! maps to non-nullable string

        /// <summary>
        /// title
        /// </summary>
        public string Title { get; set; } // String! maps to non-nullable string

        /// <summary>
        /// viewCount
        /// </summary>
        public int? ViewCount { get; set; } // Int maps to nullable int

        /// <summary>
        /// rating
        /// </summary>
        public double? Rating { get; set; } // Float maps to nullable double

        /// <summary>
        /// isPublished
        /// </summary>
        public bool IsPublished { get; set; } // Boolean! maps to non-nullable bool

        /// <summary>
        /// publishedAt
        /// </summary>
        public DateTime? PublishedAt { get; set; } // DateTime maps to nullable DateTime

        /// <summary>
        /// tags
        /// </summary>
        public List<string>? Tags { get; set; } // [String] maps to nullable List<string>

        /// <summary>
        /// categories
        /// </summary>
        public List<string> Categories { get; set; } // [String!]! maps to non-nullable List<string>
    }
}