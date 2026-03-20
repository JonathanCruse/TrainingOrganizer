using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Membership.Application.DTOs;
using TrainingOrganizer.Membership.Application.Repositories;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Application.Queries;

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
            ?? throw new NotFoundException(nameof(Domain.Member), request.MemberId);

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
