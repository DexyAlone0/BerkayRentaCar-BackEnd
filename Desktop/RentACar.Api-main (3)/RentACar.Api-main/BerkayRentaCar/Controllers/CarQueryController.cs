﻿using BerkayRentaCar.Application.CarQueryService;
using BerkayRentaCar.Contract.Request.CarRequest;
using BerkayRentaCar.Contract.Response.CarResponse;
using BerkayRentaCar.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BerkayRentaCar.Controllers
{
    
    public class CarQueryController : ControllerBase
    {
        private readonly ICarQueryService carQueryService;
        private static List<Car> cars;
        public CarQueryController(ICarQueryService carQueryService)
        {
            this.carQueryService = carQueryService;
        }
        

        [Route("carQuery")]
        [HttpGet]
        public IReadOnlyCollection<CarDetailQueryResponse> Get(CarQueryRequest carQueryRequest)
        {
           return this.carQueryService.GetCarQuery(carQueryRequest);
        }

        [Route("carQuery/{carId}")]
        [HttpGet]
        public async Task<CarDetailQueryResponse?> GetDetail(CarDetailQueryRequest request) 
        {
            return await this.carQueryService.GetCarDetailQuery(request);
        }
        
    }
}
