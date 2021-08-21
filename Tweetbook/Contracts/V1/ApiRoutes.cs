using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tweetbook.Contracts.V1
{
    public static class ApiRoutes
    {
        public const string Root = "api";
        public const string Versoion = "v1";
        public const string Base = Root + "/" + Versoion;
        public static class Posts
        {
            public const string GetAll = Base + "/posts";
            public const string Update = Base + "/posts/{postId}";
            public const string Get = Base + "/posts/{postId}"; 
            public const string Create = Base + "/posts";
            public const string Delete = Base + "/posts/{postId}";
        }
    }
}
