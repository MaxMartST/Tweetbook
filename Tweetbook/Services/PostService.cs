using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Domain;

namespace Tweetbook.Services
{
    public class PostService : IPostService
    {
        private readonly List<Post> _posts;

        public PostService()
        {
            _posts = new List<Post>();

            for (int i = 0; i < 5; i++)
            {
                _posts.Add(new Post
                {
                    Id = Guid.NewGuid(),
                    Name = $"Post Name {i}"
                });
            }
        }

        public bool DeletePost(Guid postId)
        {
            var post = GetPostById(postId);

            if (post == null)
            {
                return false;
            }

            _posts.Remove(post);
            return true;
        }

        public Post GetPostById(Guid postId)
        {
            return _posts.SingleOrDefault(x => x.Id == postId);
        }

        public List<Post> GetPosts()
        {
            return _posts;
        }

        public bool UpdatePost(Post postUpdate)
        {
            var exists = GetPostById(postUpdate.Id) != null;

            if (!exists)
            {
                return false;
            }

            var index = _posts.FindLastIndex(x => x.Id == postUpdate.Id);
            _posts[index] = postUpdate;

            return true;
        }
    }
}
