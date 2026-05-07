Ampliar modelo de ejercicio
Objetivo

Agregar campos nuevos al ejercicio sin meternos todavía con fotos reales ni carga de archivos.

Primero haría una mejora de datos.

Campos sugeridos
Description
Instructions
VideoUrl
ReferenceUrl
DifficultyLevel
MovementPattern
Equipment
TargetMuscleGroup
SecondaryMuscleGroups
ExerciseType
Environment
Tags
IsActive

Ejemplos:

DifficultyLevel:

- Principiante
- Intermedio
- Avanzado

MovementPattern:

- Empuje
- Tirón
- Sentadilla
- Bisagra de cadera
- Core
- Locomoción
- Movilidad

Equipment:

- Peso corporal
- Mancuernas
- Barra
- Máquina
- Polea
- Banda elástica
- Kettlebell
- TRX

TargetMuscleGroup:

- Piernas
- Glúteos
- Pecho
- Espalda
- Hombros
- Brazos
- Core
- Full body

ExerciseType:

- Fuerza
- Cardio
- Movilidad
- Técnica
- Rehabilitación
- Calentamiento

Environment:

- Gimnasio
- Casa
- Exterior
- Calistenia
  Decisión técnica

Para partir simple, usaría strings separados por coma para tags y grupos secundarios:

public string? Tags { get; set; }
public string? SecondaryMuscleGroups { get; set; }

Más adelante, si queremos algo más robusto, lo llevamos a tablas normalizadas:

ExerciseTags
ExerciseMuscleGroups
ExerciseEquipment

Pero para MVP, no sobrecomplicaría.
