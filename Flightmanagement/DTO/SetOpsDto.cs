namespace Flightmanagement.DTOs
{
    public class SetOpsDto
    {
        public IReadOnlyList<string> UnionSample { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> IntersectSample { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> ExceptSample { get; init; } = Array.Empty<string>();
    }
}
