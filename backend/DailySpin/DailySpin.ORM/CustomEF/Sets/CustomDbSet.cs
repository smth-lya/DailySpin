namespace DailySpin.ORM;

public class CustomDbSet<T> : CustomQueryable<T>
{
    public CustomDbSet(IAsyncQueryProvider provider) : base(provider)
    {

    }
}
