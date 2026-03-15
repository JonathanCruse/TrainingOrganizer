using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Membership.DTOs;
using TrainingOrganizer.Application.Membership.Repositories;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Application.Membership.Queries;

public sealed record GetMemberQuery(Guid MemberId) : IRequest<Result<MemberDto>>;

public sealed class GetMemberQueryHandler : IRequestHandler<GetMemberQuery, Result<MemberDto>>
{
    private readonly IMemberRepository _memberRepository;

    public GetMemberQueryHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Result<MemberDto>> Handle(GetMemberQuery request, CancellationToken cancellationToken)
    {
        var memberId = new MemberId(request.MemberId);
        var member = await _memberRepository.GetByIdAsync(memberId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Membership.Member), request.MemberId);

        return Result.Success(MemberDto.FromDomain(member));
    }
}

public sealed class GetMemberQueryValidator : AbstractValidator<GetMemberQuery>
{
    public GetMemberQueryValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
    }
}
