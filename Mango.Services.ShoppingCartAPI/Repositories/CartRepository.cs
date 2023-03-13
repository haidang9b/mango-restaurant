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

        public async Task<bool> ClearCart(string userId)
        {
            var cartHeaderFromDb = await _dbContext.CartHeaders.FirstOrDefaultAsync(item => item.UserId == userId);
            if (cartHeaderFromDb != null)
            {
                _dbContext.CartDetails.RemoveRange(_dbContext.CartDetails.Where(u => u.CartHeaderId == cartHeaderFromDb.CartHeaderId));
                _dbContext.CartHeaders.Remove(cartHeaderFromDb);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
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
                var productFirst = cartDto.CartDetails.FirstOrDefault().Product;
                _dbContext.Add(_mapper.Map<Product>(productFirst));
                await _dbContext.SaveChangesAsync();
            }

            // check if header is null
            var cartHeaderFromDb = await _dbContext.CartHeaders
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(u => u.UserId == cart.CartHeader.UserId);
            if(cartHeaderFromDb == null)
            {
                // create header and details
                _dbContext.CartHeaders.Add(cart.CartHeader);
                await _dbContext.SaveChangesAsync();

                cart.CartDetails.FirstOrDefault().CartDetailsId = cart.CartHeader.CartHeaderId;
                cart.CartDetails.FirstOrDefault().Product = null;
                _dbContext.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                // if header is not null
                // check if detail has same product
                var cartDetailsFromDb = await _dbContext.CartDetails
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(item => 
                                            item.ProductId == cart.CartDetails.FirstOrDefault().ProductId &&
                                            item.CartHeaderId == cart.CartHeader.CartHeaderId);
                if(cartDetailsFromDb == null)
                {
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                    cart.CartDetails.FirstOrDefault().Product = null;
                    _dbContext.CartDetails.Add(cart.CartDetails.FirstOrDefault());
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

        public async Task<CartDto> GetCartByUserId(string userId)
        {
            Cart cart = new Cart()
            {
                CartHeader = await _dbContext.CartHeaders.FirstOrDefaultAsync(item => item.UserId == userId)
            };
            cart.CartDetails = _dbContext.CartDetails
                .Where(item => item.CartDetailsId == cart.CartHeader.CartHeaderId).Include(u => u.Product);
            return _mapper.Map<CartDto>(cart);
        }

        public async Task<bool> RemoveFromCart(int cartDetailsId)
        {
            try
            {
                CartDetails cartDetails = await _dbContext.CartDetails
                .FirstOrDefaultAsync(item => item.CartDetailsId == cartDetailsId);
                int totalCountOfCartItems = _dbContext.CartDetails
                    .Where(item => item.CartHeaderId == cartDetails.CartHeaderId).Count();
                _dbContext.CartDetails.Remove(cartDetails);
                if (totalCountOfCartItems == 1)
                {
                    var cartHeaderForRemove = await _dbContext.CartHeaders.FirstOrDefaultAsync(item => item.CartHeaderId == cartDetails.CartHeaderId);
                    _dbContext.Remove(cartHeaderForRemove);
                }
                await _dbContext.SaveChangesAsync();
                return true;

            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
