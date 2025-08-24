Patient : BaseTable
#ClinicId
#Number int
Name string(50)
OwnerClientId Client? <OwnerPatients
VolumeClientId Client? <VolumePatients