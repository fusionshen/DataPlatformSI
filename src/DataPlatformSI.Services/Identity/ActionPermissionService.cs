using DataPlatformSI.Entities.Identity;
using DataPlatformSI.Services.Authorization;
using DataPlatformSI.Services.Contracts.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.Identity
{
    public class ActionPermissionService : IActionPermissionService
    {
        public IEnumerable<AppPermission> GetAllActionByAssembly(string assemblyName)
        {
            var result = new List<AppPermission>();
            //取程序集中的全部类型
            var types = Assembly.Load(assemblyName).GetTypes();

            foreach (var type in types)
            {
                if (type.BaseType.Name == "Controller")//如果是BaseController                
                {
                    //取类型的方法，类型为实例方法，公共方法
                    var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                    foreach (var method in methods)
                    {
                        //统一返回PermissionAuthorize的方法
                        if (method.GetCustomAttributes(typeof(PermissionAuthorizeAttribute),true).Length > 0)
                        {
                            var paa = method.GetCustomAttributes(typeof(PermissionAuthorizeAttribute), true)[0];
                            var ap = new AppPermission()
                            {
                                Permission = (paa as PermissionAuthorizeAttribute).Permission,
                                Action = method.Name,
                                //Controller = method.DeclaringType.Name,
                                Scope = method.DeclaringType.Name,
                                //Params = FormatParams(method)
                            };
                            //去Controller的描述
                            var attrs = method.DeclaringType.GetCustomAttributes(typeof(DescriptionAttribute), true);
                            if (attrs.Length > 0)
                                ap.ScopeDescription = (attrs[0] as DescriptionAttribute).Description;

                            //取Action的描述
                            attrs = method.GetCustomAttributes(typeof(DescriptionAttribute), true);
                            if (attrs.Length > 0)
                                ap.Description = (attrs[0] as DescriptionAttribute).Description;

                            result.Add(ap);
                        }
                        ////因存在异步与同步方法，所以统一用返回类型的tostring方法
                        //var returnType = method.ReturnType.ToString();
                        //if (!method.DeclaringType.Name.Contains("ControllerBase") && returnType.Contains("ActionResult"))
                        //{
                        //    var ap = new AppPermission()
                        //    {
                        //        Action = method.Name,
                        //        Controller = method.DeclaringType.Name,
                        //        //Params = FormatParams(method)
                        //    };
                        //    //去Controller的描述
                        //    var attrs = method.DeclaringType.GetCustomAttributes(typeof(DescriptionAttribute), true);
                        //    if (attrs.Length > 0)
                        //        ap.ControllerDescription = (attrs[0] as DescriptionAttribute).Description;

                        //    //取Action的描述
                        //    attrs = method.GetCustomAttributes(typeof(DescriptionAttribute), true);
                        //    if (attrs.Length > 0)
                        //        ap.Description = (attrs[0] as DescriptionAttribute).Description;

                        //    result.Add(ap);
                        //}
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 格式化Method的参数字符串
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private string FormatParams(MethodInfo method)
        {
            var param = method.GetParameters();
            var result = new StringBuilder();
            if (param.Length > 0)
            {
                foreach (var item in param)
                {
                    result.AppendLine(String.Format("Type:{0}, Name:{1}; ", item.ParameterType, item.Name));
                }
                return result.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}