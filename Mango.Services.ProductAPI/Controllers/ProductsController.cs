using Mango.Services.ProductAPI.Models.Dtos;
using Mango.Services.ProductAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ProductAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        protected ResponseDto _response;
        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _response = new ResponseDto();
        }
        
        [HttpGet]
        public async Task<object> Get()
        {
            try
            {
                var productDtos = await _productRepository.GetProducts();
                _response.Result = productDtos;
            }
            catch(Exception ex)
            {
                _response.HandleException(ex);
            }
            return _response;
        }
        
        [HttpGet("{id}")]
        [Authorize]
        public async Task<object> Get(int id)
        {
            try
            {
                var productDto = await _productRepository.GetProductById(id);
                _response.Result = productDto;
            }
            catch (Exception ex)
            {
                _response.HandleException(ex);
            }
            return _response;
        }
        
        [HttpPost]
        [Authorize]
        public async Task<object> Post([FromBody] ProductDto productDto)
        {
            try
            {
                _response.Result = await _productRepository.SaveProduct(productDto);
            }
            catch (Exception ex)
            {
                _response.HandleException(ex);
            }
            return _response;
        }
        
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("{id}")]
        public async Task<object> Delete(int id)
        {
            try
            {
                _response.IsSuccess = await _productRepository.DeleteProduct(id);
            }
            catch (Exception ex)
            {
                _response.HandleException(ex);
            }
            return _response;
        }

        [HttpPut]
        [Authorize]
        public async Task<object> Put([FromBody] ProductDto productDto)
        {
            try
            {
                _response.Result = await _productRepository.SaveProduct(productDto);
            }
            catch (Exception ex)
            {
                _response.HandleException(ex);
            }
            return _response;
        }
    }
}
