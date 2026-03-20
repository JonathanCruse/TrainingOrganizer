using FluentValidation;
using MediatR;
using TrainingOrganizer.SharedKernel.Application.Interfaces;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Facility.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Facility.Domain;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Application.Commands;

public sealed record CreateLocationCommand(
    string Name,
    string Street,
    string City,
    string PostalCode,
    string Country) : IRequest<Result<Guid>>;

public sealed class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, Result<Guid>>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLocationCommandHandler(
        ILocationRepository locationRepository,
        IUnitOfWork unitOfWork)
    {
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var name = new LocationName(request.Name);
            var address = new Address(request.Street, request.City, request.PostalCode, request.Country);

            var location = Location.Create(name, address);

            await _locationRepository.AddAsync(location, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(location.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>("Location.DomainError", ex.Message);
        }
    }
}

public sealed class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(500);
        RuleFor(x => x.City).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}
