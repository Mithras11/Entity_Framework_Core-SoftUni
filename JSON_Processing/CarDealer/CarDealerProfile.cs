using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using CarDealer.Models;
using CarDealer.DTO;
using System.Linq;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            this.CreateMap<Car, ImportCarDTO>();

            this.CreateMap<Car, CarDTO>();

            this.CreateMap<Supplier, SupplierDTO>();

            this.CreateMap<Customer, CustomerTotalSalesDTO>()
                .ForMember(x => x.FullName, y => y.MapFrom(z => z.Name))
                .ForMember(x => x.BoughtCars, y => y.MapFrom(z => z.Sales.Count))
                .ForMember(x => x.SpentMoney, y => y.MapFrom(z => z.Sales
                                                           .SelectMany(s => s.Car.PartCars
                                                                            .Select(pc => pc.Part.Price))
                                                           .Sum()));



        }
    }
}
