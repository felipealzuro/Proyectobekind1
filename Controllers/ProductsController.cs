using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMongoCollection<Product> _productsCollection;

        public ProductsController(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _productsCollection = mongoDatabase.GetCollection<Product>("products");
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productsCollection.Find(_ => true).ToListAsync();
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            var userRole = Request.Headers["UserRole"].FirstOrDefault();
            if (userRole != "Admin")
            {
                return Unauthorized("Solo los administradores pueden agregar productos.");
            }

            await _productsCollection.InsertOneAsync(product);
            return Ok();
        }
    }
}