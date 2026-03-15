using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Membership.Repositories;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Application.Membership.Commands;

public sealed record RejectMemberCommand(Guid MemberId, string Reason) : IRequest<Result>;

public sealed class RejectMemberCommandHandler : IRequestHandler<RejectMemberCommand, Result>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectMemberCommandHandler(
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RejectMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var memberId = new MemberId(request.MemberId);
            var member = await _memberRepository.GetByIdAsync(memberId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Membership.Member), request.MemberId);

            member.Reject(request.Reason);

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

public sealed class RejectMemberCommandValidator : AbstractValidator<RejectMemberCommand>
{
    public RejectMemberCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
