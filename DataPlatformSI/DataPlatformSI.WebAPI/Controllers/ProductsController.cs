using DataPlatformSI.DataAccess.Models;
using DataPlatformSI.DataAccess.Repositories;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataPlatformSI.WebAPI.Controllers
{
    //    [Route("odata/[controller]")]
    //    [ApiController]
    //    public class ProductsController : ODataController
    //    {
    //        private readonly ProductsRepository _repository;

    //        public ProductsController(ProductsRepository repository)
    //        {
    //            _repository = repository;
    //        }

    //        #region snippet_Get
    //        [HttpGet]
    //        public IEnumerable<Product> Get()
    //        {
    //            return _repository.GetProducts();
    //        }
    //        #endregion

    //        #region snippet_GetById
    //        [HttpGet("{id}")]
    //        [ProducesResponseType(200)]
    //        [ProducesResponseType(404)]
    //        public ActionResult<Product> GetById(int id)
    //        {
    //            if (!_repository.TryGetProduct(id, out var product))
    //            {
    //                return NotFound();
    //            }

    //            return product;
    //        }
    //        #endregion

    //        #region snippet_CreateAsync
    //        [HttpPost]
    //        [ProducesResponseType(201)]
    //        [ProducesResponseType(400)]
    //        public async Task<ActionResult<Product>> CreateAsync(Product product)
    //        {
    //            if (!ModelState.IsValid)
    //            {
    //                return BadRequest(ModelState);
    //            }

    //            await _repository.AddProductAsync(product);

    //            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    //        }
    //        #endregion
    //    }
    //}

    [Route("odata/[controller]")]
    [ApiController]
    public class ProductsController : Controller
    {
        private readonly ProductsRepository _repository;

        public ProductsController(ProductsRepository repository)
        {
            _repository = repository;
        }

        // GET: odata/Orders
        [HttpGet]
        public IActionResult GetProducts(ODataQueryOptions<Product> queryOptions)
        {
            return Ok(_repository.GetProducts());
        }

        // GET: odata/Orders(5)
        [HttpGet("id")]
        public IActionResult GetProduct([FromODataUri] int key, ODataQueryOptions<Product> queryOptions)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        // PUT: odata/Orders(5)
        [HttpPut]
        public IActionResult Put([FromODataUri] int key, Delta<Product> delta)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        // POST: odata/Orders
        [HttpPost]
        public IActionResult Post(Product pruduct)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        // PATCH: odata/Orders(5)
        [HttpPatch]
        [AcceptVerbs("PATCH", "MERGE")]
        public IActionResult Patch([FromODataUri] int key, Delta<Product> delta)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        // DELETE: odata/Orders(5)
        [HttpDelete]
        public IActionResult Delete([FromODataUri] int key)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }
    }
}
