using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Data;
using Tweetbook.Domain;

namespace Tweetbook.Services
{
    public class PostService : IPostService
    {
        private readonly DataContext _dataCotext;

        public PostService(DataContext dataCotext)
        {
            _dataCotext = dataCotext;
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            var post = await GetPostByIdAsync(postId);

            if (post == null)
            {
                return false;
            }

            _dataCotext.Posts.Remove(post);
            var deleted = await _dataCotext.SaveChangesAsync();

            return deleted > 0;
        }

        public async Task<Post> GetPostByIdAsync(Guid postId)
        {
            return await _dataCotext.Posts.SingleOrDefaultAsync(x => x.Id == postId);
        }

        public async Task<List<Post>> GetPostsAsync(GetAllPostsFilter filter = null, PaginationFilter paginationFilter = null)
        {
            var queryable = _dataCotext.Posts.AsQueryable();

            if (paginationFilter == null)
            {
                return await queryable.ToListAsync();
            }

            queryable = AddFiltersOnQuery(filter, queryable);

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;

            return await queryable.Skip(skip).Take(paginationFilter.PageSize).ToListAsync();
        }

        public async Task<bool> UpdatePostAsync(Post postUpdate)
        {
            _dataCotext.Posts.Update(postUpdate);
            var updated = await _dataCotext.SaveChangesAsync();

            return updated > 0;
        }

        public async Task<bool> CreatePostAsync(Post post)
        {
            await _dataCotext.Posts.AddAsync(post);
            var created = await _dataCotext.SaveChangesAsync();

            return created > 0; 
        }

        public async Task<bool> UserOwnsPostAsync(Guid postId, string userId)
        {
            var post = await _dataCotext.Posts
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == postId);

            if (post == null)
            {
                return false;
            }

            if (post.UserId != userId)
            {
                return false;
            }

            return true;
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _dataCotext.Tags.ToListAsync();
        }

        public async Task<bool> CreateTagAsync(Tag tag)
        {
            await _dataCotext.Tags.AddAsync(tag);
            var created = await _dataCotext.SaveChangesAsync();

            return created > 0;
        }

        private static IQueryable<Post> AddFiltersOnQuery(GetAllPostsFilter filter, IQueryable<Post> queryable)
        {
            if (!string.IsNullOrEmpty(filter?.UserId))
            {
                queryable = queryable.Where(x => x.UserId == filter.UserId);
            }

            return queryable;
        }
    }
}
