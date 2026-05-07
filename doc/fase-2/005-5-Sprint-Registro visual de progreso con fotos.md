Registro visual de progreso con fotos
Objetivo

Permitir subir fotos de progreso del alumno.

Decisión técnica importante

Para MVP, podemos hacerlo simple:

Guardar metadata en BD.
Guardar imagen en carpeta local o storage externo después.

Pero como esto después será SaaS, lo mejor es preparar una abstracción:

IFileStorageService

Implementación inicial:

LocalFileStorageService

Futuro:

AzureBlobStorageService
S3StorageService
CloudinaryStorageService
Nueva entidad
ClientProgressPhoto

Campos:

Id
ClientId
ProgressRecordId nullable
PhotoUrl
PhotoType: Front / Side / Back / Other
TakenAt
Notes
CreatedAt
