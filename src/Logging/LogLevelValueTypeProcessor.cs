using System.Linq;
using VentLib.Options.Interfaces;
using VentLib.Options.IO;

namespace VentLib.Logging;

internal class LogLevelValueTypeProcessor : IValueTypeProcessor<LogLevel>
{
    public LogLevel Read(MonoLine input)
    {
        return LogLevel.Levels.FirstOrDefault(level => level.Name == input.Content, new LogLevel(input.Content, 0U));
    }

    public MonoLine Write(LogLevel value, MonoLine output)
    {
        return output.Of(value.Name);
    }
}