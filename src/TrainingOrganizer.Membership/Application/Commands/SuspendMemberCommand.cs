using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Membership.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Application.Commands;

public sealed record SuspendMemberCommand(Guid MemberId, string Reason) : IRequest<Result>;

public sealed class SuspendMemberCommandHandler : IRequestHandler<SuspendMemberCommand, Result>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SuspendMemberCommandHandler(
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SuspendMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var memberId = new MemberId(request.MemberId);
            var member = await _memberRepository.GetByIdAsync(memberId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Member), request.MemberId);

            member.Suspend(request.Reason);

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

public sealed class SuspendMemberCommandValidator : AbstractValidator<SuspendMemberCommand>
{
    public SuspendMemberCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
