using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPlatformSI.DomainClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataPlatformSI.WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class MetadatasController : Controller
    {
        private readonly string _schemaDirectory = $"{System.AppContext.BaseDirectory}/Downloads/Metadatas";

        MetadataItem[] metadatas = new MetadataItem[]
        {
            new MetadataItem { Id = 1, Name = "1_itemtype_物料分类", AliasName="物料分类", IconName="FormatListBulletedType", Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 2, Name = "2_item_物料", AliasName="物料",IconName="ChemicalWeapon", Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 3, Name = "3_supplier_供应商", AliasName="供应商",IconName="AccountStar", Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 4, Name = "4_customer_客户", AliasName="客户",IconName="AccountSwitch", Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 5, Name = "5_contact_往来单位", AliasName="往来单位",IconName="Contacts", Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 6, Name = "6_testmetadata_测试元数据", AliasName="测试元数据", IconName="TestTube",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"}
        };

        // GET: api/Metadatas
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(metadatas);
        }

        // GET: api/Metadatas/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var metadata = metadatas.FirstOrDefault((m) => m.Id == id);
            if (metadata == null)
            {
                return NotFound();
            }
            return Ok(metadata);
        }

        // POST: api/Metadatas
        [HttpPost]
        public void Post([FromBody] MetadataItem value)
        {
        }

        // PUT: api/Metadatas/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] MetadataItem value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        /// <summary>
        /// 下载jsonschema
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //Get: api/Metadatas/1/Content
        [HttpGet("{id}/Content")]
        public IActionResult GetContentById(int id)
        {
            if (metadatas.FirstOrDefault(m => m.Id == id) == null)
            {
                return NotFound();
            }
            var addrUrl = $"{_schemaDirectory}/{GetFileNameFromItemName(metadatas.FirstOrDefault(m => m.Id == id).Name)}";
            if (!System.IO.File.Exists(addrUrl))
            {
                return NotFound();
            }
            var stream = System.IO.File.OpenRead(addrUrl);
            //return File(stream, "application/vnd.android.package-archive", Path.GetFileName(addrUrl));
            return Ok(stream);
        }

        private string GetFileNameFromItemName(string name) => $"{name.Split(new[] { ',', ' ','_' }, StringSplitOptions.RemoveEmptyEntries)[1]}.schema.json";
    }
}
