namespace Pandora
{
    public interface IMemoryOperation
    {
        bool IsApplied { get; }

        bool Apply();

        bool Remove();
    }
}
