using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Cache;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Requests.Queries;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Domain;
using Tweetbook.Extensions;
using Tweetbook.Helpers;
using Tweetbook.Services;

namespace Tweetbook.Controllers.V1
{
    //// ограничение по авторизации
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;

        public PostsController(IPostService postService, IMapper mapper, IUriService uriService)
        {
            _postService = postService;
            _mapper = mapper;
            _uriService = uriService;
        }

        [HttpGet(ApiRoutes.Posts.GetAll)]
        // кешируем ответ с таймером
        // [Cached(600)]
        [Authorize(Roles = "Poster")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllPostsQuery query, [FromQuery] PaginationQuery paginationOuery)
        {
            var pagination = _mapper.Map<PaginationFilter>(paginationOuery);
            var filter = _mapper.Map<GetAllPostsFilter>(query);

            var posts = await _postService.GetPostsAsync(filter, pagination);
            var postsResponseList = _mapper.Map<List<PostResponse>>(posts);

            if (pagination is null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(postsResponseList);
            }

            var paginationResponse = PaginationHelpers.CreatePaginatedResponse(_uriService, pagination, postsResponseList);

            return Ok(paginationResponse);
        }

        [HttpPut(ApiRoutes.Posts.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid postId, [FromBody] UpdatePostRequest request)
        {
            // HttpContext.GetUserId() -> получить id конкретного пользователя из HttpContext
            var userOwnsPost = await _postService.UserOwnsPostAsync(postId, HttpContext.GetUserId());

            if (!userOwnsPost)
            {
                return BadRequest(new { error = "You do not own this post"});
            }

            var post = await _postService.GetPostByIdAsync(postId);
            post.Name = request.Name;

            var updated = await _postService.UpdatePostAsync(post);

            if (updated)
            {
                var postsResponse = _mapper.Map<PostResponse>(post);

                return Ok(new Response<PostResponse>(postsResponse));
            }

            return NotFound();
        }

        [HttpDelete(ApiRoutes.Posts.Delete)]
        [Authorize(Roles = "Poster")]
        public async Task<IActionResult> Delete([FromRoute] Guid postId)
        {
            // HttpContext.GetUserId() -> получить id конкретного пользователя из HttpContext
            var userOwnsPost = await _postService.UserOwnsPostAsync(postId, HttpContext.GetUserId());

            if (!userOwnsPost)
            {
                return BadRequest(new { error = "You do not own this post" });
            }

            var delete = await _postService.DeletePostAsync(postId);

            if (delete)
            {
                return NoContent();
            }

            return NotFound();
        }

        [HttpGet(ApiRoutes.Posts.Get)]
        // кешируем ответ с таймером
        // [Cached(600)]
        public async Task<IActionResult> Get([FromRoute] Guid postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);

            if (post == null)
            {
                return NotFound();
            }

            var postsResponse = _mapper.Map<PostResponse>(post);

            return Ok(new Response<PostResponse>(postsResponse));
        }

        [HttpPost(ApiRoutes.Posts.Create)]
        [Authorize(Roles = "Poster")]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest postRequest)
        {
            var newPostId = Guid.NewGuid();

            var post = new Post 
            { 
                Id = newPostId,
                Name = postRequest.Name,
                UserId = HttpContext.GetUserId(),// получить id конкретного пользователя из HttpContext
                Tags = postRequest.Tags
                    .Select(t => new PostTag { PostId = newPostId, TagName = t.TagName })
                    .ToList(),
            };

            await _postService.CreatePostAsync(post);

            //var response = new PostResponse { Id = post.Id };

            var locationUri = _uriService.GetPostUri(post.Id.ToString());
            var postsResponse = _mapper.Map<PostResponse>(post);

            return Created(locationUri, Ok(new Response<PostResponse>(postsResponse)));
        }
    }
}
