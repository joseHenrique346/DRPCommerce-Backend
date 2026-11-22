using SharedCommerce.Application.Result;

namespace SharedCommerce.Application.Interfaces.Results;

public interface IResult<TSelf>
    where TSelf : IResult<TSelf>
{
    static abstract TSelf Validation(IEnumerable<Error> errors);
}
