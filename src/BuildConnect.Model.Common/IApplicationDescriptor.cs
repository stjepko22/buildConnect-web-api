namespace BuildConnect.Model.Common;

public interface IApplicationDescriptor
{
    string Name { get; }

    string Environment { get; }

    string Version { get; }
}
