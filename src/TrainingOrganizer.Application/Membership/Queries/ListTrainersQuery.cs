using MediatR;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Membership.DTOs;
using TrainingOrganizer.Application.Membership.Repositories;

namespace TrainingOrganizer.Application.Membership.Queries;

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
