using PodPilot.Application.Pods;
using PodPilot.Domain.Entities;
using PodPilot.Domain.Enums;

namespace PodPilot.Application.Tests.Pods;

public class PodMapperTests
{
    [Fact]
    public void ToResponse_MapsCreatingToBuildingPending()
    {
        var pod = new GpuPod
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Name = "training-pod",
            Status = PodStatus.Creating,
            GpuId = "gpu-1",
            Region = "US",
            ImageName = "image",
            CreatedAt = DateTime.UtcNow,
        };

        var response = PodMapper.ToResponse(pod);

        Assert.Equal(nameof(PodStatus.BuildingPending), response.Status);
    }
}
