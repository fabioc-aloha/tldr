namespace Tldr.Core;

public interface IClipboardService
{
    string? GetText();
    void SetText(string plainText, string? htmlText = null);
    bool HasText();
}
