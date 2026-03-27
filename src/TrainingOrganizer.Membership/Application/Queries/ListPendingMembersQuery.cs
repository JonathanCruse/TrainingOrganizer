using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Membership.Application.DTOs;
using TrainingOrganizer.Membership.Application.Repositories;
using TrainingOrganizer.Membership.Domain.Enums;

namespace TrainingOrganizer.Membership.Application.Queries;

public sealed record ListPendingMembersQuery(
    int Page,
    int PageSize) : IRequest<Result<PagedList<MemberDto>>>;

public sealed class ListPendingMembersQueryHandler : IRequestHandler<ListPendingMembersQuery, Result<PagedList<MemberDto>>>
{
    private readonly IMemberRepository _memberRepository;

    public ListPendingMembersQueryHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Result<PagedList<MemberDto>>> Handle(ListPendingMembersQuery request, CancellationToken cancellationToken)
    {
        var pagedMembers = await _memberRepository.GetPagedAsync(
            request.Page, request.PageSize, RegistrationStatus.Pending, null, null, cancellationToken);

        var dtos = pagedMembers.Items.Select(MemberDto.FromDomain).ToList();

        return Result.Success(new PagedList<MemberDto>(
            dtos, pagedMembers.Page, pagedMembers.PageSize, pagedMembers.TotalCount));
    }
}

public sealed class ListPendingMembersQueryValidator : AbstractValidator<ListPendingMembersQuery>
{
    public ListPendingMembersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
