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
    //// ограничение всех конечных точек по роли Roles = "Poster"
    //// через запятую можно указать другие роли Roles = "Poster,Admin"
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Poster,Admin")]
    public class TagsController : Controller
    {
        private readonly IPostService _postService;
        public TagsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet(ApiRoutes.Tags.GetAll)]
        //// ограничение по политике Policy = "TagViewer"
        // [Authorize(Policy = "TagViewer")]
        //
        //// ограничение по политике Policy = "MustWorkForChapsas"
        [Authorize(Policy = "MustWorkForChapsas")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _postService.GetAllTagsAsync());
        }

        [HttpPost(ApiRoutes.Tags.Create)]
        //// ограничение по политике Policy = "TagViewer"
        // [Authorize(Policy = "TagViewer")]
        // 
        //// ораничение по ролям Roles = "Admin"
        [Authorize(Roles = "Admin")]
        //// через запятую можно указать несколько ролей Roles = "Poster,Admin"
        // [Authorize(Roles = "Poster,Admin")]
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
