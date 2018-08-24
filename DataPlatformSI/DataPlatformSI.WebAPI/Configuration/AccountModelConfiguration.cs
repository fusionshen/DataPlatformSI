using DataPlatformSI.WebAPI.Models;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataPlatformSI.WebAPI.Configuration
{
    public class AccountModelConfiguration : IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion)
        {
            var account = builder.EntitySet<ApplicationUser>("Account").EntityType.HasKey(o => o.Id);
            builder.Function("Login").Returns<string>();
            builder.Function("Register").Returns<string>();
            //account.Action("Login").Returns<string>();
            //if (apiVersion < ApiVersions.V2)
            //{
            //    order.Ignore(o => o.EffectiveDate);
            //}

            //if (apiVersion < ApiVersions.V3)
            //{
            //    order.Ignore(o => o.Description);
            //}

            //if (apiVersion >= ApiVersions.V1)
            //{
            //    order.Collection.Function("MostExpensive").ReturnsFromEntitySet<Order>("Orders");
            //}

            //if (apiVersion >= ApiVersions.V2)
            //{
            //    order.Action("Rate").Parameter<int>("rating");
            //}
        }
    }
}
