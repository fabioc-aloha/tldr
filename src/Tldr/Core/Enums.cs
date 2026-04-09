namespace Tldr.Core;

public enum SummaryStyle
{
    Bullets,
    List,
    Table,
    Prose,
    Same
}

public enum DetailLevel
{
    Brief,
    Standard,
    Detailed
}

public enum Tone
{
    Neutral,
    Formal,
    Casual
}

public enum AppState
{
    Ready,
    Loaded,
    Result,
    Reading
}
