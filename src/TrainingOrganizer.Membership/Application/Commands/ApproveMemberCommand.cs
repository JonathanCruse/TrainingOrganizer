using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Membership.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Application.Commands;

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
            var currentUserId = new MemberId(_currentUserService.MemberId
                ?? throw new ForbiddenException("You must be authenticated to approve members."));

            var memberId = new MemberId(request.MemberId);
            var member = await _memberRepository.GetByIdAsync(memberId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Member), request.MemberId);

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
