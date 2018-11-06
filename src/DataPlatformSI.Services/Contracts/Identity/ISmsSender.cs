using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.Contracts.Identity
{
    public interface ISmsSender
    {
        #region BaseClass

        Task SendSmsAsync(string number, string message);

        #endregion

        #region CustomMethods

        #endregion
    }
}
