using AutoMapper;
using Mango.Services.ShoppingCartAPI.DbContexts;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private IMapper _mapper;
        public CartRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public Task<bool> ClearCart(string userId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// check if product exist in database, else create it
        /// 
        /// check if header is null
        /// create header and details
        /// 
        /// if header is not null
        /// check if detail has same product
        /// if it has then update the count
        /// else create details
        /// 
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        public async Task<CartDto> CreateUpdateCart(CartDto cartDto)
        {
            Cart cart = _mapper.Map<Cart>(cartDto);
            // check if product exist in database, else create it
            var productInDb = await _dbContext.Products.FirstOrDefaultAsync(u => u.ProductId == cartDto.CartDetails.FirstOrDefault().ProductId);
            if(productInDb == null)
            {
                await _dbContext.Products.AddAsync(cart.CartDetails.FirstOrDefault().Product);
                await _dbContext.SaveChangesAsync();
            }

            // check if header is null
            var cartHeaderFromDb = _dbContext.CartHeaders.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == cart.CartHeader.UserId);
            if(cartHeaderFromDb == null)
            {
                // create header and details
                await _dbContext.CartHeaders.AddAsync(cart.CartHeader);
                await _dbContext.SaveChangesAsync();

                cart.CartDetails.FirstOrDefault().CartDetailsId = cart.CartHeader.CartHeaderId;
                cart.CartDetails.FirstOrDefault().Product = null;
                await _dbContext.CartDetails.AddAsync(cart.CartDetails.FirstOrDefault());
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                // if header is not null
                // check if detail has same product
                var cartDetailsFromDb = await _dbContext.CartDetails.AsNoTracking()
                    .FirstOrDefaultAsync(item => 
                    item.ProductId == cart.CartDetails.FirstOrDefault().ProductId &&
                    item.CartHeaderId == cart.CartHeader.CartHeaderId);
                if(cartDetailsFromDb == null)
                {
                    cart.CartDetails.FirstOrDefault().CartDetailsId = cartDetailsFromDb.CartHeaderId;
                    cart.CartDetails.FirstOrDefault().Product = null;
                    await _dbContext.CartDetails.AddAsync(cart.CartDetails.FirstOrDefault());
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // if it has then update the count
                    // else create details
                    cart.CartDetails.FirstOrDefault().Product = null;
                    cart.CartDetails.FirstOrDefault().Count += cartDetailsFromDb.Count;
                    _dbContext.CartDetails.Update(cart.CartDetails.FirstOrDefault());
                    await _dbContext.SaveChangesAsync();
                }
            }
            return _mapper.Map<CartDto>(cart);
        }

        public Task<CartDto> GetCartByUserId(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveFromCart(int cartDetailsId)
        {
            throw new NotImplementedException();
        }
    }
}
