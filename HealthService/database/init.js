/* =====================================================================
   HealthService | Проєкт №3 | MongoDB
   Створення колекцій з JSON Schema Validation та індексів. Ідемпотентно.
   Запуск:  mongosh "mongodb://localhost:27017" --file init.js
   Посилання на інші бази (dogId) — лише логічні, без перевірки існування в DogsDb.
   ===================================================================== */
const health = db.getSiblingDB('HealthDb');

const NUM = ['int', 'long', 'double', 'decimal'];
const UUID = '^[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$';

function ensureCollection(name, validator) {
  const opts = { validator, validationLevel: 'strict', validationAction: 'error' };
  if (health.getCollectionInfos({ name }).length === 0) {
    health.createCollection(name, opts);
    print(`created: ${name}`);
  } else {
    health.runCommand({ collMod: name, ...opts });
    print(`updated validator: ${name}`);
  }
}

/* ---------- medicalRecords: одна картка на собаку (embedding масивів) ---------- */
ensureCollection('medicalRecords', {
  $jsonSchema: {
    bsonType: 'object',
    required: ['dogId', 'dogName', 'createdAt'],
    properties: {
      dogId:   { bsonType: 'string', pattern: UUID, description: 'Id собаки з DogsDb (без FK)' },
      dogName: { bsonType: 'string', minLength: 1, description: 'денормалізовано з DogsDb' },
      bloodType: { bsonType: 'string' },
      allergies: {
        bsonType: 'array',
        items: {
          bsonType: 'object',
          required: ['substance', 'severity'],
          properties: {
            substance: { bsonType: 'string' },
            severity:  { enum: ['Mild', 'Moderate', 'Severe'] },
            notedAt:   { bsonType: 'date' }
          }
        }
      },
      chronicConditions: {
        bsonType: 'array',
        items: {
          bsonType: 'object',
          required: ['name'],
          properties: { name: { bsonType: 'string' }, diagnosedAt: { bsonType: 'date' }, notes: { bsonType: 'string' } }
        }
      },
      notes: {
        bsonType: 'array',
        items: {
          bsonType: 'object',
          required: ['text', 'createdAt'],
          properties: { text: { bsonType: 'string' }, author: { bsonType: 'string' }, createdAt: { bsonType: 'date' } }
        }
      },
      createdAt: { bsonType: 'date' },
      updatedAt: { bsonType: 'date' }
    }
  }
});

/* ---------- vaccinations: вкладений масив vaccines + reference на картку (гібрид) ---------- */
ensureCollection('vaccinations', {
  $jsonSchema: {
    bsonType: 'object',
    required: ['dogId', 'dogName', 'medicalRecordId', 'vaccinatedAt', 'vaccines'],
    properties: {
      dogId:           { bsonType: 'string', pattern: UUID },
      dogName:         { bsonType: 'string', minLength: 1 },
      medicalRecordId: { bsonType: 'objectId', description: 'reference на medicalRecords._id' },
      vaccinatedAt:    { bsonType: 'date' },
      nextDueAt:       { bsonType: 'date' },
      veterinarian:    { bsonType: 'object', properties: { id: { bsonType: 'string' }, name: { bsonType: 'string' } } },
      vaccines: {
        bsonType: 'array', minItems: 1,
        items: {
          bsonType: 'object',
          required: ['name'],
          properties: {
            name:        { bsonType: 'string' },
            manufacturer:{ bsonType: 'string' },
            batchNumber: { bsonType: 'string' },
            doseMl:      { bsonType: NUM }
          }
        }
      }
    }
  }
});

/* ---------- veterinaryVisits: гнучка схема (поля залежать від visitType) ---------- */
ensureCollection('veterinaryVisits', {
  $jsonSchema: {
    bsonType: 'object',
    required: ['dogId', 'dogName', 'medicalRecordId', 'visitDate', 'visitType'],
    properties: {
      dogId:           { bsonType: 'string', pattern: UUID },
      dogName:         { bsonType: 'string', minLength: 1 },
      medicalRecordId: { bsonType: 'objectId' },
      relatedVisitId:  { bsonType: 'objectId', description: 'reference на попередній візит (контрольний)' },
      dogSnapshot: {
        bsonType: 'object',
        description: 'знімок на момент візиту — не оновлюється',
        properties: { name: { bsonType: 'string' }, breed: { bsonType: 'string' }, weightKg: { bsonType: NUM } }
      },
      visitDate: { bsonType: 'date' },
      visitType: { enum: ['Checkup', 'Surgery', 'Emergency', 'FollowUp'] },
      veterinarian: { bsonType: 'object', properties: { id: { bsonType: 'string' }, name: { bsonType: 'string' } } },
      symptoms:        { bsonType: 'array', items: { bsonType: 'string' } },
      diagnosis:       { bsonType: 'string' },
      recommendations: { bsonType: 'array', items: { bsonType: 'string' } }
      // Додаткові поля (weightKg, temperatureC, anesthesia, operationNotes, triageLevel ...) дозволені:
      // additionalProperties не обмежено — це демонстрація гнучкої схеми.
    }
  }
});

/* ============================== ІНДЕКСИ ============================== */
// Одна картка на собаку; запит: картка за dogId
health.medicalRecords.createIndex({ dogId: 1 }, { unique: true, name: 'ux_medicalRecords_dogId' });

// Запит: історія вакцинацій собаки (нові першими)
health.vaccinations.createIndex({ dogId: 1, vaccinatedAt: -1 }, { name: 'ix_vaccinations_dogId_vaccinatedAt' });
// Запит: "прострочені вакцинації" ($match за nextDueAt) — partial: лише документи з nextDueAt
health.vaccinations.createIndex({ nextDueAt: 1 }, { name: 'ix_vaccinations_nextDueAt', partialFilterExpression: { nextDueAt: { $exists: true } } });

// Запит: візити собаки за часом
health.veterinaryVisits.createIndex({ dogId: 1, visitDate: -1 }, { name: 'ix_visits_dogId_visitDate' });
// Запит: статистика/пошук за типом візиту
health.veterinaryVisits.createIndex({ visitType: 1, visitDate: -1 }, { name: 'ix_visits_type_date' });
// Запит: пошук за симптомом (multikey)
health.veterinaryVisits.createIndex({ symptoms: 1 }, { name: 'ix_visits_symptoms' });

print('HealthDb initialized');
