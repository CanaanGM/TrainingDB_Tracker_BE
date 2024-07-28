namespace DataLibrary.Models;

public partial class Measurement
{
    public int Id { get; set; }

    public int? Hip { get; set; }

    public int? Chest { get; set; }

    public int? Waist { get; set; }

    public int? LeftThigh { get; set; }

    public int? RightThigh { get; set; }

    public int? LeftCalf { get; set; }

    public int? RightCalf { get; set; }

    public int? LeftUpperArm { get; set; }

    public int? LeftLowerArm { get; set; }

    public int? RightUpperArm { get; set; }

    public int? RightLowerArm { get; set; }

    public int? Neck { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? UserId { get; set; }

    public virtual User? User { get; set; }
}
