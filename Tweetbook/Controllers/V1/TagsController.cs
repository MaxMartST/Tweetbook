using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Domain;
using Tweetbook.Services;

namespace Tweetbook.Controllers.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TagsController : Controller
    {
        private readonly IPostService _postService;
        public TagsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet(ApiRoutes.Tags.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _postService.GetAllTagsAsync());
        }

        [HttpPost(ApiRoutes.Tags.Create)]
        public async Task<IActionResult> Create([FromBody] CreateTagRequest tagRequest)
        {
            var tag = new Tag
            {
                Name = tagRequest.Name,
                CreatedBy = DateTime.Now
            };

            await _postService.CreateTagAsync(tag);

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            var locationUri = baseUrl + "/" + ApiRoutes.Posts.Get.Replace("{postId}", tag.Id.ToString());

            var response = new TagResponse { Id = tag.Id };

            return Created(locationUri, response);
        }
    }
}
