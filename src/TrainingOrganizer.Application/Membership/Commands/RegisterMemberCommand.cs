using FluentValidation;
using MediatR;
using TrainingOrganizer.Application.Common.Interfaces;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Membership.Repositories;
using TrainingOrganizer.Application.Membership.Services;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Services;

namespace TrainingOrganizer.Application.Membership.Commands;

public sealed record RegisterMemberCommand(
    string FirstName,
    string LastName,
    string Email) : IRequest<Result<Guid>>;

public sealed class RegisterMemberCommandHandler : IRequestHandler<RegisterMemberCommand, Result<Guid>>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberUniquenessService _memberUniquenessService;
    private readonly IKeycloakAdminClient _keycloakAdminClient;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterMemberCommandHandler(
        IMemberRepository memberRepository,
        IMemberUniquenessService memberUniquenessService,
        IKeycloakAdminClient keycloakAdminClient,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _memberUniquenessService = memberUniquenessService;
        _keycloakAdminClient = keycloakAdminClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(RegisterMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var email = new Email(request.Email);
            var isUnique = await _memberUniquenessService.IsEmailUniqueAsync(email, cancellationToken: cancellationToken);
            if (!isUnique)
                return Result.Failure<Guid>("Member.DuplicateEmail", "A member with this email address already exists.");

            var keycloakUserId = await _keycloakAdminClient.CreateOrGetUserAsync(
                request.Email, request.FirstName, request.LastName, cancellationToken);

            await _keycloakAdminClient.AssignRealmRoleAsync(keycloakUserId, "Member", cancellationToken);

            var externalIdentity = new ExternalIdentity("keycloak", keycloakUserId);
            var name = new PersonName(request.FirstName, request.LastName);

            var member = Member.Register(externalIdentity, name, email);

            await _memberRepository.AddAsync(member, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(member.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>("Member.DomainError", ex.Message);
        }
    }
}

public sealed class RegisterMemberCommandValidator : AbstractValidator<RegisterMemberCommand>
{
    public RegisterMemberCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
