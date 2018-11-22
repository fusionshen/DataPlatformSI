using DataPlatformSI.Services.Contracts.Identity;
using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace DataPlatformSI.WebAPI.Controllers
{
    /// <summary>
    /// 系统日志模块
    /// </summary>
    [Authorize(Roles = ConstantRoles.Admin)]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class LogsController : Controller
    {
        private readonly IAppLogItemsService _appLogItemsService;

        public LogsController(
            IAppLogItemsService appLogItemsService)
        {
            _appLogItemsService = appLogItemsService;
            _appLogItemsService.CheckArgumentIsNull(nameof(_appLogItemsService));
        }

        /// <summary>
        /// restful获得日志列表
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get(
                        string logLevel = "",
                        int pageNumber = 1,
                        int pageSize = -1,
                        string sort = "desc")
        {
            var itemsPerPage = 10;
            if (pageSize > 0)
            {
                itemsPerPage = pageSize;
            }

            var model = await _appLogItemsService.GetPagedAppLogItemsAsync(
                pageNumber, itemsPerPage, sort == "desc" ? SortOrder.Descending : SortOrder.Ascending, logLevel);
            model.LogLevel = logLevel;
            model.Paging.CurrentPage = pageNumber;
            model.Paging.ItemsPerPage = itemsPerPage;
            return Json(model);
        }

        /// <summary>
        /// restful删除日志
        /// </summary>
        /// <param name="id">日志id</param>
        /// <returns>期望返回</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _appLogItemsService.DeleteAsync(id);
            return Ok();
        }

        /// <summary>
        /// 按照级别批量删除
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteAll(string logLevel = "")
        {
            await _appLogItemsService.DeleteAllAsync(logLevel);
            return Ok();
        }

        /// <summary>
        /// 按照条件批量删除
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        /// <param name="days">几天前</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteOlderThan(string logLevel = "", int days = 5)
        {
            var cutoffUtc = DateTimeOffset.UtcNow.AddDays(-days);
            await _appLogItemsService.DeleteOlderThanAsync(cutoffUtc, logLevel);
            return Ok();
        }
    }
}
