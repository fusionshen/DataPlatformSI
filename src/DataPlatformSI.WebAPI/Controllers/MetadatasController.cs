using System;
using System.Linq;
using DataPlatformSI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
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
            new MetadataItem { Id = 6, Name = "6_testmetadata_测试元数据", AliasName="测试元数据", IconName="TestTube",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            //ref:https://mozilla-services.github.io/react-jsonschema-form/
            new MetadataItem { Id = 7, Name = "7_simple_Simple", AliasName="Simple", IconName="Clippy",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 8, Name = "8_nested_Nested", AliasName="Nested", IconName="NestProtect",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 9, Name = "9_arrays_Arrays", AliasName="Arrays", IconName="CodeArray",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 10, Name = "10_numbers_Numbers", AliasName="Numbers", IconName="FormatListNumbers",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 11, Name = "11_widgets_Widgets", AliasName="Widgets", IconName="Widgets",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 12, Name = "12_ordering_Ordering", AliasName="Ordering", IconName="ReorderHorizontal",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 13, Name = "13_references_References", AliasName="References", IconName="AlarmLight",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 14, Name = "14_custom_Custom", AliasName="Custom", IconName="ImageFilterCenterFocusWeak",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 15, Name = "15_errors_Errors", AliasName="Errors", IconName="Ferry",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 16, Name = "16_large_Large", AliasName="Large", IconName="GridLarge",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 17, Name = "17_date&time_Date&time", AliasName="Date & time", IconName="Timer",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 18, Name = "18_validation_Validation", AliasName="Validation", IconName="Quicktime",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 19, Name = "19_files_Files", AliasName="Files", IconName="File",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 20, Name = "20_single_Single", AliasName="Single", IconName="SkypeBusiness",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 21, Name = "21_customArray_CustomArray", AliasName="Custom Array", IconName="ViewArray",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 22, Name = "22_customObject_CustomObject", AliasName="Custom Object", IconName="Pokeball",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 23, Name = "23_alternatives_Alternatives", AliasName="Alternatives", IconName="Football",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 24, Name = "24_propertyDependencies_PropertyDependencies", AliasName="Property Dependencies", IconName="Airballoon",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            new MetadataItem { Id = 25, Name = "25_schemaDependencies_SchemaDependencies", AliasName="Schema Dependencies", IconName="CodeTagsCheck",Checksum ="82240e32a858fbfe5e77b0c920b68e2c"},
            //ref:https://github.com/RSuter/VisualJsonEditor
            new MetadataItem { Id = 26, Name = "26_jsonEditor_JsonEditor", AliasName="Json Editor", Checksum ="82240e32a858fbfe5e77b0c920b68e2c"}
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
        /// 下载JsonSchema
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //Get: api/Metadatas/1/jsContent
        [HttpGet("{id}/jscontent")]
        public IActionResult GetJsonSchemaContentById(int id)
        {
            if (metadatas.FirstOrDefault(m => m.Id == id) == null)
            {
                return NotFound();
            }
            var addrUrl = $"{_schemaDirectory}/{GetJsonSchemaFileNameFromItemName(metadatas.FirstOrDefault(m => m.Id == id).Name)}";
            if (!System.IO.File.Exists(addrUrl))
            {
                return NotFound();
            }
            var stream = System.IO.File.OpenRead(addrUrl);
            //return File(stream, "application/vnd.android.package-archive", Path.GetFileName(addrUrl));
            return Ok(stream);
        }

        private string GetJsonSchemaFileNameFromItemName(string name) => $"{name.Split(new[] { ',', ' ','_' }, StringSplitOptions.RemoveEmptyEntries)[1]}.schema.json";

        /// <summary>
        /// 下载UiSchema
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //Get: api/Metadatas/1/uisContent
        [HttpGet("{id}/uiscontent")]
        public IActionResult GetUiSchemaContentById(int id)
        {
            if (metadatas.FirstOrDefault(m => m.Id == id) == null)
            {
                return NotFound();
            }
            var addrUrl = $"{_schemaDirectory}/{GetUiSchemaFileNameFromItemName(metadatas.FirstOrDefault(m => m.Id == id).Name)}";
            if (!System.IO.File.Exists(addrUrl))
            {
                //return NotFound();
                addrUrl = $"{_schemaDirectory}/__nullable__.uischema.json";
            }
            var stream = System.IO.File.OpenRead(addrUrl);
            //return File(stream, "application/vnd.android.package-archive", Path.GetFileName(addrUrl));
            return Ok(stream);
        }

        private string GetUiSchemaFileNameFromItemName(string name) => $"{name.Split(new[] { ',', ' ', '_' }, StringSplitOptions.RemoveEmptyEntries)[1]}.uischema.json";
    }
}
