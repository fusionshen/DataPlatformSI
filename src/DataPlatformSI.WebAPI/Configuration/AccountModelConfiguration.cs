using DataPlatformSI.DomainClasses;
using DataPlatformSI.WebAPI.Models;
using DataPlatformSI.WebAPI.Models.DTOs;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DataPlatformSI.WebAPI.Controllers.AccountController;

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
            builder.EntitySet<ApplicationUser>("Account");
            var userType = builder.EntityType<ApplicationUser>();

            //var account = builder.EntitySet<ApplicationUser>("Account").EntityType.HasKey(o => o.Id);
            userType.Collection.Function("Login").Returns<string>();
            //userType.Collection.Function("RefreshToken").Returns<string>();
            //userType.Collection.Function("Logout").Returns<bool>();
            //userType.Collection.Function("IsAuthenticated").Returns<bool>();
            //userType.Collection.Function("GetUserInfo").Returns<bool>();

            userType.Collection.Function("Register").Returns<string>();
            userType.Collection.Function("Protected").Returns<string>();



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
