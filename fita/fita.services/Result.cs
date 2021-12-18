using System.Collections.Generic;
using System.Linq;

namespace fita.services
{
    public class Result
    {
        private readonly List<string> _errors;

        internal Result(bool succeeded, List<string> errors)
        {
            Succeeded = succeeded;
            _errors = errors;
        }

        public bool Succeeded { get; }

        public List<string> Errors
            => Succeeded
                ? new List<string>()
                : _errors;

        public static Result Success { get; } = new(true, new List<string>());
        
        public static Result Fail { get; } = new(false, new List<string> {"An error occurred."});

        public static Result Failure(IEnumerable<string> errors) => new(false, errors.ToList());

        public static implicit operator Result(string error)
            => Failure(new List<string> {error});

        public static implicit operator Result(List<string> errors)
            => Failure(errors.ToList());

        public static implicit operator Result(bool success)
            => success ? Success : Failure(new[] {"Unsuccessful operation."});

        public static implicit operator bool(Result result)
            => result.Succeeded;
    }
}