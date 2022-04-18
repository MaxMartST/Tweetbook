using AutoMapper;
using System.Linq;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Requests.Queries;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Domain;

namespace Tweetbook.MappingProfiles
{
    public class RequestToDomainProfile : Profile
    {
        public RequestToDomainProfile()
        {
            CreateMap<PaginationQuery, PaginationFilter>();
            CreateMap<GetAllPostsQuery, GetAllPostsFilter>();

            CreateMap<Post, PostResponse>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(x => new TagResponse { Name = x.TagName })));
            CreateMap<Tag, TagResponse>();
        }
    }
}
