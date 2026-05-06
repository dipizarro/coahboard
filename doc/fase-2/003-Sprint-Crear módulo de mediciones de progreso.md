Crear módulo de mediciones de progreso
Objetivo

Registrar medidas periódicas del alumno.

Esto debería ser una nueva entidad, no campos directos en Client.

Nueva entidad sugerida
ClientProgressRecord

Campos:

Id
ClientId
RecordedAt
WeightKg
HeightCm
BodyFatPercentage
ChestCm
WaistCm
HipCm
LeftArmCm
RightArmCm
LeftThighCm
RightThighCm
RestingHeartRate
Notes
CreatedAt

No todos serán obligatorios. El coach puede registrar solo peso y cintura, por ejemplo.

Endpoints sugeridos
GET /api/clients/{clientId}/progress
GET /api/clients/{clientId}/progress/{progressId}
POST /api/clients/{clientId}/progress
PUT /api/clients/{clientId}/progress/{progressId}
DELETE /api/clients/{clientId}/progress/{progressId}
Entregable

Historial de mediciones por alumno.
