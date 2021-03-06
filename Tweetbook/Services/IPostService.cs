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
        Task<Tag> GetTagByNameAsync(string tagName);
        Task<bool> CreateTagAsync(Tag tag);
        Task<bool> DeleteTagAsync(string tagName);
        Task<List<Post>> GetPostsAsync(GetAllPostsFilter filter = null, PaginationFilter paginationFilter = null);
        Task<Post> GetPostByIdAsync(Guid postId);
        Task<bool> UpdatePostAsync(Post postUpdate);
        Task<bool> DeletePostAsync(Guid postId);
        Task<bool> CreatePostAsync(Post post);
        Task<bool> UserOwnsPostAsync(Guid postId, string userId);
    }
}
