
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PBL6.Common.Functions
{
    public static class ModelStateErrorHandler
    {

        public static Dictionary<string, string> GetModelErrors(this ModelStateDictionary errDictionary)
        {
            var errors = new Dictionary<string, string>();
            var listError = errDictionary.Where(k => k.Value.Errors.Count > 0);
            foreach (var err in listError)
            {
                var er = string.Join(", ", err.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                errors.Add(err.Key, er);
            }

            return errors;
        }

        public static string StringifyModelErrors(this ModelStateDictionary errDictionary)
        {
            var errorsBuilder = new StringBuilder();
            var errors = errDictionary.GetModelErrors();
            foreach (var err in errors)
            {
                errorsBuilder.AppendFormat("{0}: {1} - ", err.Key, err.Value);
            }

            return errorsBuilder.ToString();
        }
    }
}