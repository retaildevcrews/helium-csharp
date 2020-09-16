namespace CSE.Helium
{
    public class ValidationError
    {
        public string Code { get; }

        public string Target { get; }

        public string Message { get; }

        public ValidationError(string code, string target, string message)
        {
            Code = code;
            Target = target;
            Message = message;
        }
    }
}
