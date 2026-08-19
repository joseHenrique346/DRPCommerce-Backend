namespace StoreCommerce.Application.Result
{
    public class Result<TContent>
    {
        public bool IsSuccess { get; private set; } = true;
        public TContent Content { get; private set; }
        public List<string> ListMessageErrors { get; private set; } = new();

        public static Result<TContent> Failure(string message)
        {
            return new Result<TContent>
            {
                IsSuccess = false,
                ListMessageErrors = new() { message }
            };
        }

        public static Result<TContent> Success(TContent content)
        {
            return new Result<TContent>
            {
                Content = content,
                IsSuccess = true
            };
        }

        public static Result<TContent> FailureFromList(List<string> errors)
        {
            return new Result<TContent>
            {
                IsSuccess = false,
                ListMessageErrors = new(errors)
            };
        }
    }
}
