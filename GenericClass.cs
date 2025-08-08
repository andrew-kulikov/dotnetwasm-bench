public class GenericClass<T>
{
    public T Value { get; set; }
}

public class GenericClass1<T>
{
    public bool Compare<R>()
    {
        return typeof(R) == typeof(T);
    }
}