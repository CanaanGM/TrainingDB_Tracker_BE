namespace DataLibrary;
public class Block
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Sets { get; set; }
    public int Rest { get; set; } // in seconds
    public string Instructions { get; set; } = null!;
    public int OrderNumber { get; set; }

    public WeekDay WeekDay { get; set; }
    public int WeekDayId { get; set; }

    public List<BlockExercise> Exercises { get; set; }
}
