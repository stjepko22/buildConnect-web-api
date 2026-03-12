using BuildConnect.Model.Common;

namespace BuildConnect.Model;

public sealed record ApiMetadata(string Name, string Environment, string Version) : IApplicationDescriptor;

public sealed record ApiStatus(string Name, string Environment, string Version, DateTimeOffset TimestampUtc);
