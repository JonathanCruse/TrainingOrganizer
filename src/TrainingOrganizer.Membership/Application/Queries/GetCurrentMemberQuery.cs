using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Membership.Application.DTOs;
using TrainingOrganizer.Membership.Application.Repositories;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Application.Queries;

public sealed record GetCurrentMemberQuery : IRequest<Result<MemberDto>>;

public sealed class GetCurrentMemberQueryHandler : IRequestHandler<GetCurrentMemberQuery, Result<MemberDto>>
{
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentMemberQueryHandler(
        IMemberRepository memberRepository,
        ICurrentUserService currentUserService)
    {
        _memberRepository = memberRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MemberDto>> Handle(GetCurrentMemberQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = new MemberId(_currentUserService.MemberId
            ?? throw new ForbiddenException("You must be authenticated."));

        var member = await _memberRepository.GetByIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Member), currentUserId.Value);

        return Result.Success(MemberDto.FromDomain(member));
    }
}
