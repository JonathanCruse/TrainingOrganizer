using MediatR;
using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Membership.Application.DTOs;
using TrainingOrganizer.Membership.Application.Repositories;

namespace TrainingOrganizer.Membership.Application.Queries;

public sealed record ListTrainersQuery : IRequest<Result<List<MemberDto>>>;

public sealed class ListTrainersQueryHandler : IRequestHandler<ListTrainersQuery, Result<List<MemberDto>>>
{
    private readonly IMemberRepository _memberRepository;

    public ListTrainersQueryHandler(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Result<List<MemberDto>>> Handle(ListTrainersQuery request, CancellationToken cancellationToken)
    {
        var trainers = await _memberRepository.GetTrainersAsync(cancellationToken);
        var dtos = trainers.Select(MemberDto.FromDomain).ToList();
        return Result.Success(dtos);
    }
}
