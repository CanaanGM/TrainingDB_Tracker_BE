using DataLibrary.Dtos;

namespace DataLibrary.Interfaces;
public interface IExerciseService
{
    List<ExerciseReadDto> Get();
}
