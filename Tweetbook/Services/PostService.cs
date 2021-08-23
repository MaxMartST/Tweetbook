﻿using Microsoft.EntityFrameworkCore;
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

        public async Task<List<Post>> GetPostsAsync()
        {
            return await _dataCotext.Posts.ToListAsync();
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
    }
}