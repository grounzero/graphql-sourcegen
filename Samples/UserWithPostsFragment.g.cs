using System;
using System.Collections.Generic;

namespace GraphQL.Generated.UserWithPosts
{
    /// <summary>
    /// Generated from GraphQL fragment 'UserWithPosts' on type 'User'
    /// </summary>
    public class UserWithPostsFragment
    {
        /// <summary>
        /// UserBasic fragment spread
        /// </summary>
        public GraphQL.Generated.UserBasic.UserBasicFragment? UserBasic { get; set; }

        /// <summary>
        /// posts
        /// </summary>
        public List<PostData>? Posts { get; set; }

        /// <summary>
        /// Nested class for posts field
        /// </summary>
        public class PostData
        {
            /// <summary>
            /// id
            /// </summary>
            public string? Id { get; set; }

            /// <summary>
            /// title
            /// </summary>
            public string? Title { get; set; }

            /// <summary>
            /// publishedAt
            /// </summary>
            public DateTime? PublishedAt { get; set; }

            /// <summary>
            /// comments
            /// </summary>
            public List<CommentData>? Comments { get; set; }

            /// <summary>
            /// Nested class for comments field
            /// </summary>
            public class CommentData
            {
                /// <summary>
                /// id
                /// </summary>
                public string? Id { get; set; }

                /// <summary>
                /// text
                /// </summary>
                public string? Text { get; set; }

                /// <summary>
                /// author
                /// </summary>
                public AuthorData? Author { get; set; }

                /// <summary>
                /// Nested class for author field
                /// </summary>
                public class AuthorData
                {
                    /// <summary>
                    /// id
                    /// </summary>
                    public string? Id { get; set; }

                    /// <summary>
                    /// name
                    /// </summary>
                    public string? Name { get; set; }
                }
            }
        }
    }
}