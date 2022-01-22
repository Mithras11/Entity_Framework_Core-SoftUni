using AutoMapper;
using ProductShop.Dtos.Import;
using ProductShop.Dtos.Export;
using ProductShop.Models;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            this.CreateMap<ImportUserDto, User>();
            this.CreateMap<ImportProductDto, Product>();
            this.CreateMap<ImportCategoryDto, Product>();
            this.CreateMap<ImportCategoryProductDto, CategoryProduct>();

            this.CreateMap<Product, ProductInRangeDto>()
                .ForMember(x => x.Buyer, y => y.MapFrom(z => z.Buyer.FirstName + " " + z.Buyer.LastName));

            this.CreateMap<Product, ProductSoldDTO>();
            this.CreateMap<User, ExportUserDto>()
                .ForMember(x => x.SoldProducts, y => y.MapFrom(z => z.ProductsSold));
        }
    }
}
