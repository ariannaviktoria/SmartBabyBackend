using AutoMapper;
using SmartBaby.Core.DTOs;
using SmartBaby.Core.Entities;

namespace SmartBaby.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<Baby, BabyDto>().ReverseMap();
        CreateMap<Baby, CreateBabyDto>().ReverseMap();
        CreateMap<Baby, UpdateBabyDto>().ReverseMap();
        CreateMap<SleepPeriod, SleepPeriodDto>().ReverseMap();
        CreateMap<Feeding, FeedingDto>().ReverseMap();
        CreateMap<Crying, CryingDto>().ReverseMap();
        CreateMap<Note, NoteDto>().ReverseMap();
    }
} 