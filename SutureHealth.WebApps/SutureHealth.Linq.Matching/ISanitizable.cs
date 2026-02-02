namespace SutureHealth.Linq
{
    //TODO: Default Implementation with C#8?
    public interface ISanitizable<T>
    {
        T Sanitize();
    }
}
