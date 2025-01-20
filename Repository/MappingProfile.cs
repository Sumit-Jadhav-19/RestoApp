using AutoMapper;
using RestoApp.Models.Entities;
using RestoApp.Models.ViewModels;

namespace RestoApp.Repository
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CategoryViewModel, Categories>();  
            CreateMap<Categories, CategoryViewModel>();
            CreateMap<Menu, MenuViewModel>();
            CreateMap<MenuViewModel, Menu>();
            CreateMap<Hall, HallViewModel>();
            CreateMap<HallViewModel, Hall>();
            CreateMap<Table, TableViewModel>();
            CreateMap<TableViewModel, Table>();
            CreateMap<OrderMaster, OrderMasterViewModel>();
            CreateMap<OrderMasterViewModel, OrderMaster>();
            CreateMap<OrderDetails, OrderDetailsViewModel>();
            CreateMap<OrderDetailsViewModel, OrderDetails>();
        }
    }
}
