using Newtonsoft.Json;

namespace Gamelab.Dialogue;

public record DialogueLine(
    [property: JsonProperty("speakerName")] string SpeakerName,
    [property: JsonProperty("text")] string Text
);
