namespace PBL6.Common.Exceptions
{
    public class NotFoundException<T> : CustomException where T : class
    {
        public NotFoundException(string id) : base($"Couldn't find {typeof(T).Name} with id: {id}.", 404)
        {
        }
    }
}