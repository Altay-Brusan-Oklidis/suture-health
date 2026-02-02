namespace SutureHealth
{
    public interface IParentRelationship<T>
    {
        long ParentId { get; set; }
        T Parent { get; set; }
    }
}
