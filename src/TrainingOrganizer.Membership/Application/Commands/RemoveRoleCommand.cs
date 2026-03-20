using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Membership.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Membership.Domain.Enums;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Application.Commands;

public sealed record RemoveRoleCommand(Guid MemberId, MemberRole Role) : IRequest<Result>;

public sealed class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, Result>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRoleCommandHandler(
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var memberId = new MemberId(request.MemberId);
            var member = await _memberRepository.GetByIdAsync(memberId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Member), request.MemberId);

            member.RemoveRole(request.Role);

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

public sealed class RemoveRoleCommandValidator : AbstractValidator<RemoveRoleCommand>
{
    public RemoveRoleCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.Role).IsInEnum();
    }
}
