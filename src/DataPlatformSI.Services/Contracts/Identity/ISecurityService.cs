using System;
using System.Collections.Generic;
using System.Text;

namespace DataPlatformSI.Services.Contracts.Identity
{
    public interface ISecurityService
    {
        string GetSha256Hash(string input);
    }

}
