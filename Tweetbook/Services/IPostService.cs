using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Domain;

namespace Tweetbook.Services
{
    public interface IPostService
    {
        Task<List<Tag>> GetAllTagsAsync();
        Task<bool> CreateTagAsync(Tag tag);
        Task<List<Post>> GetPostsAsync();
        Task<Post> GetPostByIdAsync(Guid postId);
        Task<bool> UpdatePostAsync(Post postUpdate);
        Task<bool> DeletePostAsync(Guid postId);
        Task<bool> CreatePostAsync(Post post);
        Task<bool> UserOwnsPostAsync(Guid postId, string userId);
    }
}
