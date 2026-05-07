using AutoMapper;
using CoachBoard.Application.DTOs;
using CoachBoard.Domain.Entities;

namespace CoachBoard.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CoachCreateDto, Coach>();
        CreateMap<CoachUpdateDto, Coach>();
        CreateMap<Coach, CoachReadDto>();

        CreateMap<ClientCreateDto, Client>();
        CreateMap<ClientUpdateDto, Client>();
        CreateMap<Client, ClientReadDto>();

        CreateMap<ClientProgressCreateDto, ClientProgressRecord>();
        CreateMap<ClientProgressUpdateDto, ClientProgressRecord>()
            .ForMember(d => d.ClientId, m => m.Ignore())
            .ForMember(d => d.CreatedAt, m => m.Ignore());
        CreateMap<ClientProgressRecord, ClientProgressReadDto>();

        CreateMap<ClientProgressPhotoCreateDto, ClientProgressPhoto>();
        CreateMap<ClientProgressPhoto, ClientProgressPhotoReadDto>();

        // Exercises
        CreateMap<ExerciseCreateDto, Exercise>();
        CreateMap<ExerciseUpdateDto, Exercise>();
        CreateMap<Exercise, ExerciseReadDto>();

        // Routines
        CreateMap<RoutineCreateDto, Routine>();
        CreateMap<RoutineUpdateDto, Routine>();

        // RoutineExercise <-> DTOs
        CreateMap<RoutineItemDto, RoutineExercise>();
        CreateMap<RoutineExercise, RoutineReadItemDto>()
            .ForMember(d => d.ExerciseName, m => m.MapFrom(s => s.Exercise.Name))
            .ForMember(d => d.Category, m => m.MapFrom(s => s.Exercise.Category));

        CreateMap<Routine, RoutineReadDto>()
            .ForMember(d => d.Items, m => m.MapFrom(s => s.RoutineExercises.OrderBy(x => x.Order)));

        // Sessions
        CreateMap<SessionCreateDto, Session>()
            .ForMember(d => d.Status, m => m.MapFrom(_ => "Planned"))
            .ForMember(d => d.CoachId, m => m.Ignore()); // se setea en el controller según el usuario

        CreateMap<SessionUpdateDto, Session>()
            .ForMember(d => d.CoachId, m => m.Ignore())  // nunca se cambia el coach desde aquí
            .ForMember(d => d.CreatedAt, m => m.Ignore()); // no tocamos CreatedAt

        CreateMap<Session, SessionReadDto>()
            .ForMember(d => d.ClientName, m => m.MapFrom(s => s.Client != null ? s.Client.FullName : null))
            .ForMember(d => d.RoutineTitle, m => m.MapFrom(s => s.Routine != null ? s.Routine.Title : null));
    }
}
