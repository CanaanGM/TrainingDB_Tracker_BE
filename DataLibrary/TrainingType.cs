namespace DataLibrary;

// my main db is SQLITE, there are no enums in it, hence this table
/*
     Cardio
    Strength
    BodyBuilding 
    ... etc
 */
public class TrainingType
{
    public int Id { get; set; }
    public string Type { get; set; } = null!;

}
