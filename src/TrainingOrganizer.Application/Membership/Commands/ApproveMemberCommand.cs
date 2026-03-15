using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Membership.Repositories;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Application.Membership.Commands;

public sealed record ApproveMemberCommand(Guid MemberId) : IRequest<Result>;

public sealed class ApproveMemberCommandHandler : IRequestHandler<ApproveMemberCommand, Result>
{
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveMemberCommandHandler(
        IMemberRepository memberRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ApproveMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = _currentUserService.MemberId
                ?? throw new ForbiddenException("You must be authenticated to approve members.");

            var memberId = new MemberId(request.MemberId);
            var member = await _memberRepository.GetByIdAsync(memberId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Membership.Member), request.MemberId);

            member.Approve(currentUserId);

            await _memberRepository.UpdateAsync(member, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure("Member.DomainError", ex.Message);
        }
    }
}

public sealed class ApproveMemberCommandValidator : AbstractValidator<ApproveMemberCommand>
{
    public ApproveMemberCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
    }
}
