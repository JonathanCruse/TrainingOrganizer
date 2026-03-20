using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Exceptions;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Membership.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Membership.Application.Commands;

public sealed record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone) : IRequest<Result>;

public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result>
{
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(
        IMemberRepository memberRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = new MemberId(_currentUserService.MemberId
                ?? throw new ForbiddenException("You must be authenticated to update your profile."));

            var member = await _memberRepository.GetByIdAsync(currentUserId, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Member), currentUserId.Value);

            var name = new PersonName(request.FirstName, request.LastName);
            var email = new Email(request.Email);
            var phone = request.Phone is not null ? new PhoneNumber(request.Phone) : null;

            member.UpdateProfile(name, email, phone);

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

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).MaximumLength(50).When(x => x.Phone is not null);
    }
}
