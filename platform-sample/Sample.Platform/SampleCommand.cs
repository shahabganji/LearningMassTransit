using System;

namespace Sample.Platform
{
    public interface SampleCommand
    {
        Guid Id { get; set; }
        string Command { get; set; }
    }
}
