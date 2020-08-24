using System.Linq;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace CSE.Helium.Validation
{
    internal static class ValidationProcessor
    {
        /// <summary>
        /// creates JSON response using StringBuilder given inputs
        /// </summary>
        /// <param name="context"></param>
        /// <param name="target"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static string ProcessBadParameter(ActionContext context, string target, ILogger logger)
        {
            var code = "NullValue";

            var modelStateEntries = context.ModelState.Where(
                e => e.Value.Errors.Count > 0).ToArray();
            var last = modelStateEntries.Last();

            var sb = new StringBuilder();

            sb.Append("{\n");
            sb.Append("\"error\": {\n");
            sb.Append($"\"code\": \"BadParameter\",\n");
            sb.Append($"\"message\": \"Parameter validation failed\",\n");
            sb.Append($"\"target\": \"{target}\",\n");
            sb.Append("\"details\": [\n");
            
            // write and log details collection
            foreach (var state in modelStateEntries)
            {
                logger.LogWarning($"InvalidParameter|{target}|{state.Value.Errors[0].ErrorMessage}");
                sb.Append("{");
                sb.Append(string.IsNullOrWhiteSpace(state.Value.AttemptedValue)
                    ? $"\"code\": \"{code}\",\n"
                    : $"\"code\": \"Invalid parameter\",\n");

                sb.Append($"\"target\": \"{state.Key}\",\n");
                sb.Append($"\"message\": \"{state.Value.Errors[0].ErrorMessage}\"\n");

                if (state.Equals(last))
                {
                    sb.Append("}\n");
                    sb.Append("]");
                }
                else
                {
                    // more results
                    sb.Append("},\n");
                }
            }

            sb.Append("}\n");
            sb.Append("}");

            return sb.ToString();
        }
    }
}
