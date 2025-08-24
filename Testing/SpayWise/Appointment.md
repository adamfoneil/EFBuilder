Appointment : BaseTable
#LocationId <Appointments
#TypeId AppointmentType <Appointments
#PatientId <Appointments
#Date DateOnly
DropOff TimeOnly
Pickup DateTime
StatusId AppointmentStatus
VolumeClientId Client? <VolumeAppointments // shelter/rescue/group
CopayClientId Client? <CopayAppointments // additional payer (grant)
TransportClientId Client? <TransportAppointments
HasRabiesVaccineProof bool
KennelSizeId? <Appointments
Points int
PointsClientId Client? <PointsAppointments // if null points go to clinic