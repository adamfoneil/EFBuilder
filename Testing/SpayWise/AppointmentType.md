AppointmentType : BaseTable
#ClinicId
#Name string(50)
BuiltInType BuiltInAppointmentType { SpayNeuter, Wellness, Recheck }
BackColor string(20)?
TextColor string(20)?
IsActive bool = true
