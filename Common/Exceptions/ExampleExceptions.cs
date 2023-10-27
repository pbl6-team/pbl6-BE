using PBL6.Common.Exceptions;

namespace PBL6.Common
{
    public class ExampleExceptions : CustomException
    {
        public ExampleExceptions(string message, int statusCode = 400) : base(message, statusCode)
        {
        }
    }
}