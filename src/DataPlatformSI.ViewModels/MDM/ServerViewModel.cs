using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DataPlatformSI.ViewModels.MDM
{
    public class ServerViewModel
    {
        [HiddenInput]
        public string Id { set; get; }

        [Required(ErrorMessage = "服务器名必填")]
        [Display(Name = "服务器名")]
        public string Name { set; get; }
    }
}