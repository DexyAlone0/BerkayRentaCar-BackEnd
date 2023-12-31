﻿using BerkayRentaCar.Contract.Request.CarRequest;
using BerkayRentaCar.Contract.Response.CarResponse;
using BerkayRentaCar.Data.Context;
using BerkayRentaCar.Data.Repositories.Abstract;
using BerkayRentaCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using TnActivationCore.Repository.Mssql.GenericRepository;
using TnActivationRandevu.Common.Extensions;

namespace BerkayRentaCar.Data.Repositories.Concrete
{
    public class CarRepository : ICarRepository
    {
        private readonly IGenericRepository genericRepository;
        private readonly BerkayRentaCarContext dbContext;
        public CarRepository(IGenericRepository genericRepository, BerkayRentaCarContext dbContext)
        {
            this.genericRepository = genericRepository;
            this.dbContext = dbContext;
        }

        public async Task<CarDetailQueryResponse?> GetCarDetailQueryAsync(CarDetailQueryRequest request)
        {
            return await this.genericRepository.GetQueryable<Car>().Where(x => x.Id == request.CarId).Select(x => new CarDetailQueryResponse
            {
                CarId = request.CarId,
                ModelId = x.ModelId,
                Year = x.Year,
                HesAirConditioning = x.HesAirConditioning,
                BrandName = x.Model.Brand.Name,
                EngineCapacityName = x.EngineCapacity,
                FuelTypeName = x.Model.FuelType.Name,
                GearTypeName = x.Model.GearType.Name,
                FileId = x.Model.FileId,

            }).FirstOrDefaultAsync();
        }

        public async Task UpdateCarAsync(UpdateCarRequest request)
        {
            var carUpdate = this.genericRepository.GetQueryable<Car>().FirstOrDefault(x => x.Id == request.Id);
            if (carUpdate is null)
            {
                throw new Exception("Araba bulunamadı");
            }

            carUpdate.Year = request.Year;
            carUpdate.HesAirConditioning = request.HesAirConditioning;
            carUpdate.ModelId = request.ModelId;


            // var carUpdate = new Model
            // {
            //     Name = model.Name,
            //     BrandId = model.BrandId,
            //     FuelTypeId = model.FuelTypeId,
            //     GearTypeId = model.GearTypeId,
            //     CountOfSeats = model.CountOfSeats,
            //     CaseTypeId = model.CaseTypeId,
            //     Id = model.Id,
            //     FileId = 1
            // };
            this.genericRepository.Update(carUpdate);
            await this.genericRepository.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<CarDetailQueryResponse>> GetCarListAsync(CarQueryRequest request)
        {
            Expression<Func<Car, bool>> expression = LinqExpressionBuilder.New<Car>(x => x.IsActive);


            if (request.FuelTypeId is not null)
            {
                expression = expression.And(x => x.Model.FuelTypeId == request.FuelTypeId);
            }

            if (request.GearTypeId is not null)
            {
                expression = expression.And(x => x.Model.GearTypeId == request.GearTypeId);
            }


            //EagerLoading
            Func<IQueryable<Car>, IIncludableQueryable<Car, object>> includes = x => x.Include(x => x.Model).Include(x => x.Model.FuelType).Include(x => x.Model.GearType).Include(x => x.Model.Brand);
            var result = await this.genericRepository.GetListAsync(expression, includes);

            ////LazyLoading
            //this.genericRepository.GetQueryable<Car>().Where(expression).Select(x => new CarQueryResponse
            //{
            //    Id = x.Id,
            //    BrandName = x.Model.Brand.Name,
            //    FuelTypeName = x.Model.FuelType.Name,
            //    GearTypeName = x.Model.GearType.Name,
            //    ModelName = x.Model.Name,
            //});

            return result.Select(x => new CarDetailQueryResponse
            {
                CarId = x.Id,
                BrandName = x.Model.Brand.Name,
                FuelTypeName = x.Model.FuelType.Name,
                GearTypeName = x.Model.GearType.Name,
                ModelName = x.Model.Name,
                CreatedDate = x.CreatedDate,
                FileId = x.Model.FileId

            }).ToList();
        }

        public async Task<CarQuantityResponse> CarQuantity()
        {
            var carsWithModels = dbContext.Cars
                .Join(
                    inner: dbContext.Models,
                    outerKeySelector: c => c.ModelId,
                    innerKeySelector: m => m.Id,
                    resultSelector: (car, model) => new { Car = car, Model = model })
                .AsEnumerable()
                .GroupBy(c => new
                {
                    ModelName = c.Model.Name, 
                    Year = c.Car.Year,
                })
                .Select(group => new CarQuantityItem
                {
                    ModelName = group.Key.ModelName, 
                    Year = group.Key.Year,
                    Count = group.Count()
                })
                .ToList();

            var response = new CarQuantityResponse
            {
                CarQuantities = carsWithModels
            };

            return response;
        }

    }
}