using System;
using System.Collections.Generic;

namespace GraphQL.Generated.UserWithDeprecated
{
    /// <summary>
    /// Generated from GraphQL fragment 'UserWithDeprecated' on type 'User'
    /// </summary>
    public class UserWithDeprecatedFragment
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
        /// username
        /// </summary>
        [Obsolete("This field is deprecated: Use email instead")]
        public string? Username { get; set; }

        /// <summary>
        /// oldField
        /// </summary>
        [Obsolete("This field is deprecated")]
        public string? OldField { get; set; }
    }
}