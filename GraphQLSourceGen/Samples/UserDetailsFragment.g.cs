using System;
using System.Collections.Generic;

namespace GraphQL.Generated.UserDetails
{
    /// <summary>
    /// Generated from GraphQL fragment 'UserDetails' on type 'User'
    /// </summary>
    public class UserDetailsFragment
    {
        /// <summary>
        /// id
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// isActive
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// profile
        /// </summary>
        public ProfileData? Profile { get; set; }

        /// <summary>
        /// posts
        /// </summary>
        public List<PostData>? Posts { get; set; }

        /// <summary>
        /// followers
        /// </summary>
        public List<FollowerData>? Followers { get; set; }

        /// <summary>
        /// Nested class for profile field
        /// </summary>
        public class ProfileData
        {
            /// <summary>
            /// bio
            /// </summary>
            public string? Bio { get; set; }

            /// <summary>
            /// avatarUrl
            /// </summary>
            public string? AvatarUrl { get; set; }

            /// <summary>
            /// joinDate
            /// </summary>
            public DateTime? JoinDate { get; set; }
        }

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
            /// content
            /// </summary>
            public string? Content { get; set; }

            /// <summary>
            /// createdAt
            /// </summary>
            public DateTime? CreatedAt { get; set; }
        }

        /// <summary>
        /// Nested class for followers field
        /// </summary>
        public class FollowerData
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