using AutoMapper;

using DataLibrary.Dtos;
using DataLibrary.Models;

namespace DataLibrary.Core;
internal class Profiles : Profile
{
    public Profiles()
    {
        CreateMap<Muscle, MuscleReadDto>()
            .ForMember(x => x.MuscleName, src => src.MapFrom(w => w.Name))
            .ReverseMap();


        CreateMap<Muscle, MuscleWriteDto>().ReverseMap();
        CreateMap<ExerciseMuscle, MuscleExerciseReadDto>()
            .ForMember(dt => dt.Id, opt => opt.MapFrom(src => src.Muscle.Id))
            .ForMember(dt => dt.Name, opt => opt.MapFrom(src => src.Muscle.Name))
            .ForMember(dt => dt.Function, opt => opt.MapFrom(src => src.Muscle.Function))
            .ForMember(dt => dt.MuscleGroup, opt => opt.MapFrom(src => src.Muscle.MuscleGroup))
            .ForMember(dt => dt.WikiPageUrl, opt => opt.MapFrom(src => src.Muscle.WikiPageUrl));


        CreateMap<TrainingType, TrainingTypeReadDto>().ReverseMap();
        CreateMap<TrainingType, TrainingTypeWriteDto>().ReverseMap();


        CreateMap<Exercise, ExerciseReadDto>()
            .ForMember(s => s.HowTos, opt => opt.MapFrom(src => src.ExerciseHowTos));
        CreateMap<Exercise, ExerciseWriteDto>().ReverseMap();

        CreateMap<ExerciseHowTo, ExerciseHowToReadDto>();
        CreateMap<ExerciseHowTo, ExerciseHowToWriteDto>().ReverseMap();


        CreateMap<TrainingSession, TrainingSessionReadDto>()
            //.ForMember(dt => dt.TrainingTypes, src => src.MapFrom(o => o.))
            ;
        CreateMap<ExerciseRecord, ExerciseRecordReadDto>()
            .ForMember(dt => dt.ExcerciseName
                , src => src
                    .MapFrom(o => o.Exercise.Name))
            .ForMember(dt => dt.MuscleGroup
                , src => src
                    .MapFrom(o => o.Exercise.ExerciseMuscles
                        .Select(x => x.Muscle.Name)));



    }
}
