namespace DropCommerce.Application.Result
{
    internal class Result<TContent>
    {
        public bool IsSuccess { get; private set; } = true;
        public TContent Content { get; private set; }
        public List<string> ListMessageErrors { get; private set; }
        
        public void Failure(string message)
        {
            IsSuccess = false;
            ListMessageErrors.Add(message);
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
            var result = new Result<TContent>();
            result.IsSuccess = false;
            result.ListMessageErrors.AddRange(errors);
            return result;
        }
    }
}
