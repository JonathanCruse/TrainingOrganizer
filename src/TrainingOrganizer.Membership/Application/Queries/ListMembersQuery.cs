using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Membership.Application.DTOs;
using TrainingOrganizer.Membership.Application.Repositories;
using TrainingOrganizer.Membership.Domain.Enums;

namespace TrainingOrganizer.Membership.Application.Queries;

public sealed record ListMembersQuery(
    int Page,
    int PageSize,
    RegistrationStatus? Status,
    string? Search,
    MemberRole? Role) : IRequest<Result<PagedList<MemberDto>>>;

public sealed class ListMembersQueryHandler : IRequestHandler<ListMembersQuery, Result<PagedList<MemberDto>>>
{
    private readonly IMemberRepository _memberRepository;

    public ListMembersQueryHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Result<PagedList<MemberDto>>> Handle(ListMembersQuery request, CancellationToken cancellationToken)
    {
        var pagedMembers = await _memberRepository.GetPagedAsync(
            request.Page, request.PageSize, request.Status, request.Search, request.Role, cancellationToken);

        var dtos = pagedMembers.Items.Select(MemberDto.FromDomain).ToList();

        return Result.Success(new PagedList<MemberDto>(
            dtos, pagedMembers.Page, pagedMembers.PageSize, pagedMembers.TotalCount));
    }
}

public sealed class ListMembersQueryValidator : AbstractValidator<ListMembersQuery>
{
    public ListMembersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
