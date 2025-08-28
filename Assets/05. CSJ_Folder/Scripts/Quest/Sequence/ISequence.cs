namespace _05._CSJ_Folder.Scripts.Quest.Sequence
{
    public interface ISequence<out T>
    {
        T[] GetSequence();
    }
}