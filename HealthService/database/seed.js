/* =====================================================================
   HealthDb — тестові дані. ІДЕМПОТЕНТНО: replaceOne з upsert за фіксованим _id.
   dogId збігаються з seed-даними DogsService (SeedIds.cs).
   Запуск:  mongosh "mongodb://localhost:27017" --file seed.js
   ===================================================================== */
const health = db.getSiblingDB('HealthDb');

const DOG = {
  rex:     'd0000000-0000-0000-0000-000000000001',
  bella:   'd0000000-0000-0000-0000-000000000002',
  max:     'd0000000-0000-0000-0000-000000000003',
  luna:    'd0000000-0000-0000-0000-000000000004'
};
const MR = {
  rex:   ObjectId('65f000000000000000000001'),
  bella: ObjectId('65f000000000000000000002'),
  max:   ObjectId('65f000000000000000000003'),
  luna:  ObjectId('65f000000000000000000004')
};

function upsert(coll, doc) {
  health.getCollection(coll).replaceOne({ _id: doc._id }, doc, { upsert: true });
}

/* ---------- medicalRecords ---------- */
upsert('medicalRecords', {
  _id: MR.rex, dogId: DOG.rex, dogName: 'Rex', bloodType: 'DEA 1.1+',
  allergies: [], chronicConditions: [],
  notes: [{ text: 'Здоровий, активний.', author: 'Dr. Petrenko', createdAt: ISODate('2026-02-01T10:00:00Z') }],
  createdAt: ISODate('2026-01-10T09:00:00Z')
});
upsert('medicalRecords', {
  _id: MR.bella, dogId: DOG.bella, dogName: 'Bella',
  allergies: [{ substance: 'Курка', severity: 'Moderate', notedAt: ISODate('2026-03-05T00:00:00Z') }],
  chronicConditions: [{ name: 'Дисплазія кульшових суглобів', diagnosedAt: ISODate('2025-11-20T00:00:00Z'), notes: 'Контроль раз на пів року' }],
  notes: [], createdAt: ISODate('2026-01-10T09:05:00Z')
});
upsert('medicalRecords', {
  _id: MR.max, dogId: DOG.max, dogName: 'Max',
  allergies: [{ substance: 'Пилок', severity: 'Mild' }],
  chronicConditions: [], notes: [], createdAt: ISODate('2026-01-11T11:00:00Z')
});
upsert('medicalRecords', {
  _id: MR.luna, dogId: DOG.luna, dogName: 'Luna',
  allergies: [], chronicConditions: [{ name: 'Післяопераційна реабілітація' }],
  notes: [{ text: 'Потребує спокою 3 тижні.', author: 'Dr. Petrenko', createdAt: ISODate('2026-10-20T12:00:00Z') }],
  createdAt: ISODate('2026-01-11T11:30:00Z')
});

/* ---------- vaccinations (embedded vaccines[], reference medicalRecordId) ---------- */
upsert('vaccinations', {
  _id: ObjectId('65f100000000000000000001'), dogId: DOG.rex, dogName: 'Rex', medicalRecordId: MR.rex,
  vaccinatedAt: ISODate('2026-02-01T00:00:00Z'), nextDueAt: ISODate('2027-02-01T00:00:00Z'),
  veterinarian: { id: 'vet-1', name: 'Dr. Petrenko' },
  vaccines: [
    { name: 'Rabies',  manufacturer: 'Nobivac', batchNumber: 'R-2201', doseMl: 1.0 },
    { name: 'DHPPi',   manufacturer: 'Eurican', batchNumber: 'E-9921', doseMl: 1.0 }
  ]
});
upsert('vaccinations', {
  _id: ObjectId('65f100000000000000000002'), dogId: DOG.bella, dogName: 'Bella', medicalRecordId: MR.bella,
  vaccinatedAt: ISODate('2025-05-10T00:00:00Z'), nextDueAt: ISODate('2026-05-10T00:00:00Z'),   // ПРОСТРОЧЕНА
  veterinarian: { id: 'vet-1', name: 'Dr. Petrenko' },
  vaccines: [{ name: 'Rabies', manufacturer: 'Nobivac', batchNumber: 'R-1888' }]
});
upsert('vaccinations', {
  _id: ObjectId('65f100000000000000000003'), dogId: DOG.max, dogName: 'Max', medicalRecordId: MR.max,
  vaccinatedAt: ISODate('2026-03-15T00:00:00Z'), nextDueAt: ISODate('2027-03-15T00:00:00Z'),
  vaccines: [{ name: 'DHPPi', batchNumber: 'E-7710', doseMl: 0.5 }, { name: 'Leptospirosis' }]
});
upsert('vaccinations', {
  _id: ObjectId('65f100000000000000000004'), dogId: DOG.luna, dogName: 'Luna', medicalRecordId: MR.luna,
  vaccinatedAt: ISODate('2025-09-01T00:00:00Z'), nextDueAt: ISODate('2026-09-01T00:00:00Z'),   // ПРОСТРОЧЕНА
  vaccines: [{ name: 'Rabies', manufacturer: 'Rabisin' }]
});

/* ---------- veterinaryVisits (різні типи візитів = різні набори полів) ---------- */
upsert('veterinaryVisits', {   // Checkup: weightKg, temperatureC
  _id: ObjectId('65f200000000000000000001'), dogId: DOG.rex, dogName: 'Rex', medicalRecordId: MR.rex,
  dogSnapshot: { name: 'Rex', breed: 'Labrador Retriever', weightKg: 29.5 },
  visitDate: ISODate('2026-02-01T10:00:00Z'), visitType: 'Checkup',
  veterinarian: { id: 'vet-1', name: 'Dr. Petrenko' },
  symptoms: [], diagnosis: 'Здоровий', recommendations: ['Щорічний огляд'],
  weightKg: 29.5, temperatureC: 38.4
});
upsert('veterinaryVisits', {   // Surgery: anesthesia {...}, operationNotes
  _id: ObjectId('65f200000000000000000002'), dogId: DOG.luna, dogName: 'Luna', medicalRecordId: MR.luna,
  dogSnapshot: { name: 'Luna', breed: 'Mixed Breed', weightKg: 9.8 },
  visitDate: ISODate('2026-10-15T08:30:00Z'), visitType: 'Surgery',
  veterinarian: { id: 'vet-2', name: 'Dr. Ivanchuk' },
  symptoms: ['кульгання', 'біль у задній лапі'], diagnosis: 'Перелом кісточки',
  recommendations: ['Спокій 3 тижні', 'Антибіотики 7 днів'],
  anesthesia: { type: 'General', durationMin: 90 }, operationNotes: 'Остеосинтез, фіксація пластиною.'
});
upsert('veterinaryVisits', {   // FollowUp: relatedVisitId (reference на попередній візит)
  _id: ObjectId('65f200000000000000000003'), dogId: DOG.luna, dogName: 'Luna', medicalRecordId: MR.luna,
  relatedVisitId: ObjectId('65f200000000000000000002'),
  dogSnapshot: { name: 'Luna', breed: 'Mixed Breed' },
  visitDate: ISODate('2026-10-29T09:00:00Z'), visitType: 'FollowUp',
  symptoms: [], diagnosis: 'Загоєння без ускладнень', recommendations: ['Контроль через 2 тижні']
});
upsert('veterinaryVisits', {   // Emergency: triageLevel
  _id: ObjectId('65f200000000000000000004'), dogId: DOG.max, dogName: 'Max', medicalRecordId: MR.max,
  dogSnapshot: { name: 'Max', breed: 'Beagle', weightKg: 12.3 },
  visitDate: ISODate('2026-07-12T21:15:00Z'), visitType: 'Emergency',
  symptoms: ['блювання', 'апатія'], diagnosis: 'Харчове отруєння',
  recommendations: ['Дієта 2 дні', 'Багато води'], triageLevel: 2
});
upsert('veterinaryVisits', {
  _id: ObjectId('65f200000000000000000005'), dogId: DOG.bella, dogName: 'Bella', medicalRecordId: MR.bella,
  dogSnapshot: { name: 'Bella', breed: 'German Shepherd', weightKg: 27.0 },
  visitDate: ISODate('2026-04-02T11:00:00Z'), visitType: 'Checkup',
  symptoms: ['кульгання після прогулянки'], diagnosis: 'Загострення дисплазії',
  recommendations: ['Обмежити навантаження', 'Хондропротектори'], weightKg: 27.0, temperatureC: 38.6
});

print('HealthDb seeded: ' + health.medicalRecords.countDocuments() + ' records, '
  + health.vaccinations.countDocuments() + ' vaccinations, '
  + health.veterinaryVisits.countDocuments() + ' visits');
