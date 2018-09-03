using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace DataPlatformSI.WebAPI
{
    /// <summary>
    /// Provides unbound, utility functions.
    /// </summary>
    public class FunctionsController : ODataController
    {
        /// <summary>
        /// Gets the sales tax for a postal code.
        /// </summary>
        /// <param name="state">The state's short name to get the sales tax for.</param>
        /// <returns>The sales tax rate for the postal code.</returns>
        [HttpGet]
        [ODataRoute("GetSalesTaxRate(state={state})")]
        public IActionResult GetSalesTaxRate([FromODataUri] string state)
        {
            return Ok(GetRate(state));
        }

        private static double GetRate(string state)
        {
            double taxRate = 0;
            switch (state)
            {
                case "AZ": taxRate = 5.6; break;
                case "CA": taxRate = 7.5; break;
                case "CT": taxRate = 6.35; break;
                case "GA": taxRate = 4; break;
                case "IN": taxRate = 7; break;
                case "KS": taxRate = 6.15; break;
                case "KY": taxRate = 6; break;
                case "MA": taxRate = 6.25; break;
                case "NV": taxRate = 6.85; break;
                case "NJ": taxRate = 7; break;
                case "NY": taxRate = 4; break;
                case "NC": taxRate = 4.75; break;
                case "ND": taxRate = 5; break;
                case "PA": taxRate = 6; break;
                case "TN": taxRate = 7; break;
                case "TX": taxRate = 6.25; break;
                case "VA": taxRate = 4.3; break;
                case "WA": taxRate = 6.5; break;
                case "WV": taxRate = 6; break;
                case "WI": taxRate = 5; break;

                default:
                    taxRate = 0;
                    break;
            }

            return taxRate;
        }

    }
}
