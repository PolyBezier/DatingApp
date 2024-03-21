using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
            .ForMember(
                dest => dest.PhotoUrl,
                opt => opt.MapFrom(src => src.Photos!.Any(x => x.IsMain) ? src.Photos!.First(x => x.IsMain).Url : null))
            .ForMember(
                dest => dest.Age,
                opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));

        CreateMap<Photo, PhotoDto>();
        CreateMap<MemberUpdateDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();
        CreateMap<Message, MessageDto>()
            .ForMember(
                dest => dest.SenderPhotoUrl,
                opt => opt.MapFrom(src => src.Sender!.Photos!.Any(x => x.IsMain) ? src.Sender.Photos!.First(x => x.IsMain).Url : null))
            .ForMember(
                dest => dest.RecipientPhotoUrl,
                opt => opt.MapFrom(src => src.Recipient!.Photos!.Any(x => x.IsMain) ? src.Recipient.Photos!.First(x => x.IsMain).Url : null));
    }
}
