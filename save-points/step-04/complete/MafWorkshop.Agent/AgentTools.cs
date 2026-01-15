using System.ComponentModel;

namespace MafWorkshop.Agent;

public class AgentTools
{
    [Description("Formats the story for publication, revealing its title.")]
    public static string FormatStory(string title, string story) => $"""
        **Title**: {title}

        {story}
        """;
}