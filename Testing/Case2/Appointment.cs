using Testing.Conventions;

namespace Testing.Case2;

public class Appointment : BaseTable
{
	public int LocationId { get; set; }
	public int TypeId { get; set; }
	public int PatientId { get; set; }
	public DateOnly Date { get; set; }
	public TimeOnly DropOff { get; set; }
	public DateTime Pickup { get; set; }
	public int StatusId { get; set; }
	public int? VolumeClientId { get; set; }
	public int? CopayClientId { get; set; }
	public int? TransportClientId { get; set; }
	public bool HasRabiesVaccineProof { get; set; }
	public int? KennelSizeId { get; set; }
	public int Points { get; set; }
	public int? PointsClientId { get; set; }

	public Location? Location { get; set; }
	public AppointmentType? Type { get; set; }
	public Patient? Patient { get; set; }
	public AppointmentStatus? Status { get; set; }
	public Client? VolumeClient { get; set; }
	public Client? CopayClient { get; set; }
	public Client? TransportClient { get; set; }
	public KennelSize? KennelSize { get; set; }
	public Client? PointsClient { get; set; }


}
