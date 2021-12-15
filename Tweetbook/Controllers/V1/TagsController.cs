﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
    //
    //// ограничение 
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    public class TagsController : Controller
    {
        private readonly IPostService _postService;
        public TagsController(IPostService postService)
        {
            _postService = postService;
        }

        /// <summary>
        /// Returns all the tags in the system
        /// </summary>

        [HttpGet(ApiRoutes.Tags.GetAll)]
        //// ограничение по политике Policy = "TagViewer"
        // [Authorize(Policy = "TagViewer")]
        //
        //// ограничение по политике Policy = "MustWorkForChapsas"
        // [Authorize(Policy = "MustWorkForChapsas")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _postService.GetAllTagsAsync());
        }

        /// <summary>
        /// Ceates a tag in the system
        /// </summary>
        /// <response code="201">Ceates a tag in the system</response>
        /// <response code="400">Unable to create tag due to validation error</response>

        [HttpPost(ApiRoutes.Tags.Create)]
        [ProducesResponseType(typeof(TagResponse), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        //// ограничение по политике Policy = "TagViewer"
        // [Authorize(Policy = "TagViewer")]
        // 
        //// ораничение по ролям Roles = "Admin"
        // [Authorize(Roles = "Admin")]
        //// через запятую можно указать несколько ролей Roles = "Poster,Admin"
        // [Authorize(Roles = "Poster,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTagRequest tagRequest)
        {
            var newTag = new Tag
            {
                Name = tagRequest.TagName,
                CreatedBy = DateTime.Now
            };

            var created = await _postService.CreateTagAsync(newTag);

            if (!created)
            {
                return BadRequest(
                    new ErrorResponse 
                    { 
                        Errors = new List<ErrorModel> { new ErrorModel { Message = "Unable to create tag"} } 
                    });
            }

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            var locationUri = baseUrl + "/" + ApiRoutes.Tags.Create.Replace("{tagName}", newTag.Name);

            var response = new TagResponse { Name = newTag.Name };

            return Created(locationUri, response);
        }
    }
}
