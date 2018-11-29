using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DataPlatformSI.Common.IdentityToolkit
{
    public static class ModelStateExtensions
    {
        /// <summary>
        /// IdentityResult errors list to string
        /// </summary>
        public static string DumpErrors(this ModelStateDictionary result, bool useHtmlNewLine = false)
        {
            var results = new StringBuilder();
            foreach (var state in result)
            {
                foreach (var err in state.Value.Errors)
                {
                    var errorDescription = err.ErrorMessage;
                    if (string.IsNullOrWhiteSpace(errorDescription))
                    {
                        continue;
                    }

                    if (!useHtmlNewLine)
                    {
                        results.AppendLine(errorDescription);
                    }
                    else
                    {
                        results.AppendLine($"{errorDescription}<br/>");
                    }
                }
                
            }
            return results.ToString();
        }
    }
}
