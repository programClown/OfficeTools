namespace OfficeTools.Core.Helper;

public class PropertyComparer<T> : IEqualityComparer<T> where T : class
{
    public PropertyComparer(Func<T, object> expr)
    {
        Expr = expr;
    }

    private Func<T, object> Expr { get; }

    public bool Equals(T? x, T? y)
    {
        if (x == null || y == null) return false;

        var first = Expr.Invoke(x);
        var second = Expr.Invoke(y);

        return first.Equals(second);
    }

    public int GetHashCode(T obj) => obj.GetHashCode();
}