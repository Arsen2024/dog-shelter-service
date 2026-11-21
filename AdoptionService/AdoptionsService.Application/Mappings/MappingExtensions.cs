using AdoptionsService.Application.DTOs;
using AdoptionsService.Domain.Entities;
using AdoptionsService.Domain.Models;

namespace AdoptionsService.Application.Mappings;

public static class MappingExtensions
{
    public static AdopterDto ToDto(this AdopterEntity e) =>
        new(e.Id, e.FullName, e.Email, e.Phone, e.Address, e.CreatedAt);

    public static ApplicationListItemDto ToDto(this ApplicationListItemModel m) =>
        new(m.Id, m.Status, m.SubmittedAt, m.AdopterId, m.AdopterName);

    public static RequirementDto ToDto(this RequirementEntity e) =>
        new(e.Id, e.Code, e.Title, e.Description, e.IsMandatory);

    public static DogDto ToDto(this DogEntity e) =>
        new(e.Id, e.Name, e.Status, e.SyncedAt);

    public static ApplicationDetailsDto ToDto(this ApplicationDetailsModel m) =>
        new(
            m.Header.Id,
            m.Header.Status,
            m.Header.SubmittedAt,
            m.Header.DecisionNote,
            m.Header.AdopterId,
            m.Header.FullName,
            m.Header.Email,
            m.Header.AgreementNumber is null || m.Header.SignedAt is null
                ? null
                : new AgreementDto(m.Header.AgreementNumber, m.Header.SignedAt.Value),
            m.Dogs.Select(d => new ApplicationDogDto(d.DogId, d.DogNameAtSubmission, d.CurrentName, d.CurrentStatus, d.Priority)).ToList(),
            m.Requirements.Select(r => new ApplicationRequirementDto(r.Code, r.Title, r.IsMandatory, r.IsFulfilled, r.CheckedAt, r.CheckedBy)).ToList(),
            m.History.Select(h => new StatusHistoryDto(h.OldStatus, h.NewStatus, h.ChangedAt, h.ChangedBy, h.Comment)).ToList());
}
