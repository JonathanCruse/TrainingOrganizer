using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Membership.DTOs;
using TrainingOrganizer.Application.Membership.Repositories;

namespace TrainingOrganizer.Application.Membership.Queries;

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
        var currentUserId = _currentUserService.MemberId
            ?? throw new ForbiddenException("You must be authenticated.");

        var member = await _memberRepository.GetByIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Membership.Member), currentUserId.Value);

        return Result.Success(MemberDto.FromDomain(member));
    }
}
