namespace StoreCommerce.Application.Result;

public interface IResult<TSelf>
    where TSelf : IResult<TSelf>
{
    static abstract TSelf Validation(IEnumerable<Error> errors);
}
