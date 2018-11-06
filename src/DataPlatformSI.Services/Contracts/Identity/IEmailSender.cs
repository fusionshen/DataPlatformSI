﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.Contracts.Identity
{
    public interface IEmailSender
    {
        #region BaseClass

        Task SendEmailAsync(string email, string subject, string message);

        #endregion

        #region CustomMethods

        Task SendEmailAsync<T>(string email, string subject, string viewNameOrPath, T model);

        #endregion
    }
}