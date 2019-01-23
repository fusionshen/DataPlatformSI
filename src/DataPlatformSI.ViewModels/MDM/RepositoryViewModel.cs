using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DataPlatformSI.ViewModels.MDM
{
    public class RepositoryViewModel
    {
        [HiddenInput]
        public string Id { set; get; }

        [Required(ErrorMessage = "资源仓库名必填")]
        [Display(Name = "资源仓库名")]
        public string Name { set; get; }

        [Required(ErrorMessage = "服务器必填")]
        [Display(Name = "服务器名")]
        public string ServerName { get; set; }

        [Required(ErrorMessage = "服务器用户名必填")]
        [Display(Name = "服务器用户名")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "服务器密码必填")]
        [Display(Name = "服务器密码")]
        public string Password { get; set; }

        [Display(Name = "端口")]
        public string Port { get; set; }
    }
}