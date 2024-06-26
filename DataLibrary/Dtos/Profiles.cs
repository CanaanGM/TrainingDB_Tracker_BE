using AutoMapper;

using DataLibrary.Models;

namespace DataLibrary.Dtos;
internal class Profiles : Profile
{
    public Profiles()
    {

        CreateMap<Muscle, MuscleReadDto>()
            .ForMember(dto => dto.MuscleGroups,
            opt => opt
            .MapFrom(m => m.MuscleGroupMuscles
            .Select(mgm => mgm.MuscleGroup)
            ));

        CreateMap<Exercise, ExerciseReadDto>()
            .ForMember(dto => dto.ExerciseMuscles,
            opt => opt
                .MapFrom(m => m.ExerciseMuscles
                .Select(em => em.Muscle))
            )
            .ForMember(dto => dto.ExerciseTypes, opt => opt
                .MapFrom(t => t.ExerciseTypes.Select(ty => ty.TrainingType)));
        CreateMap<TrainingType, TypeReadDto>().ReverseMap();
        CreateMap<TrainingType, TypeWriteDto>().ReverseMap();

        CreateMap<MuscleGroup, MuscleGroupReadDto>().ReverseMap();
        CreateMap<ExerciseHowTo, ExerciseHowToDto>().ReverseMap();

    }
}
