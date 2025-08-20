MedicalItem : BaseTable
#AppointmentId
#ItemId
StatusId? MedicalItemStatus { Completed, Declined }
Dosage decimal?
DosageInfo string(255)?
DosageOverridden bool
Concentrations string(200)?
DrugRouteId?
Units string(10)?