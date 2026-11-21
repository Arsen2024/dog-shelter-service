/* HealthDb — приклади запитів і aggregation pipeline.  mongosh --file queries.js */
const health = db.getSiblingDB('HealthDb');

print('--- 1) Візити собаки за часом (ix_visits_dogId_visitDate) ---');
printjson(health.veterinaryVisits.find({ dogId: 'd0000000-0000-0000-0000-000000000004' }).sort({ visitDate: -1 }).toArray());

print('--- 2) Документи різної структури в одній колекції: візити з operationNotes (лише Surgery) ---');
printjson(health.veterinaryVisits.find({ operationNotes: { $exists: true } }, { dogName: 1, visitType: 1, anesthesia: 1 }).toArray());

print('--- 3) Собаки з ПРОСТРОЧЕНОЮ вакцинацією (остання вакцинація кожної собаки) ---');
printjson(health.vaccinations.aggregate([
  { $sort: { vaccinatedAt: -1 } },
  { $group: { _id: '$dogId', dogName: { $first: '$dogName' }, lastVaccinatedAt: { $first: '$vaccinatedAt' }, nextDueAt: { $first: '$nextDueAt' } } },
  { $match: { nextDueAt: { $lt: new Date() } } },
  { $sort: { nextDueAt: 1 } }
]).toArray());

print('--- 4) Кількість візитів за типом ---');
printjson(health.veterinaryVisits.aggregate([
  { $group: { _id: '$visitType', visits: { $sum: 1 }, lastVisit: { $max: '$visitDate' } } },
  { $sort: { visits: -1 } }
]).toArray());

print('--- 5) Візит + алергії з картки ($lookup по reference medicalRecordId) ---');
printjson(health.veterinaryVisits.aggregate([
  { $match: { visitType: 'Emergency' } },
  { $lookup: { from: 'medicalRecords', localField: 'medicalRecordId', foreignField: '_id', as: 'record' } },
  { $unwind: '$record' },
  { $project: { dogName: 1, diagnosis: 1, allergies: '$record.allergies' } }
]).toArray());

print('--- 6) Найчастіші вакцини ($unwind вкладеного масиву) ---');
printjson(health.vaccinations.aggregate([
  { $unwind: '$vaccines' },
  { $group: { _id: '$vaccines.name', doses: { $sum: 1 } } },
  { $sort: { doses: -1 } }
]).toArray());

print('--- 7) Собаки з хронічними захворюваннями ---');
printjson(health.medicalRecords.find({ 'chronicConditions.0': { $exists: true } }, { dogName: 1, chronicConditions: 1 }).toArray());

print('--- 8) Перевірка валідації: некоректний документ має бути відхилений ---');
try {
  health.veterinaryVisits.insertOne({ dogId: 'not-a-uuid', dogName: 'X', visitDate: new Date(), visitType: 'Unknown' });
  print('ПОМИЛКА: валідація не спрацювала');
} catch (e) { print('OK, відхилено: ' + e.codeName); }
