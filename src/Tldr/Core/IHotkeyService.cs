namespace Tldr.Core;

public interface IHotkeyService
{
    bool Register(string id, string keys, Action callback);
    void Unregister(string id);
    void UnregisterAll();
}
