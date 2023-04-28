#region Directives 

using Domain;
using EF.Models;
using IBusinessServices;
using IDataAccess;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace BusinessServices
{   
    /// <summary>
    ///  Population business service 
    /// </summary>
    public class PopulationService : IPopulationService
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        /// <summary>
        ///   IUnitOfWork private field 
        /// </summary>
        private readonly IUnitOfWork _unityOfWork;

        /// <summary>
        ///  Population ServiceConstructer
        /// </summary>
        /// <param name="unityOfWork"></param>
        public PopulationService(IUnitOfWork unityOfWork, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<PopulationService>();
            this._unityOfWork = unityOfWork;
        }

        /// <summary>
        ///  Load population details by state
        /// </summary>
        /// <param name="stateIdList"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PopulationDto>> LoadAllByStateIdAsync(List<int> stateIdList)
        {
            try
            {
                var populationList = new List<PopulationDto>();

                //Call actual data from DB 
                var actualDataList = await this._unityOfWork.ActualDataRepository().GetAsync();

                stateIdList?.ForEach(y =>
                {
                    //assume no Duplicates base on given data 
                    var actualHousehold = actualDataList?.Where(x => x.State == y).Distinct()?.FirstOrDefault();

                    if (actualHousehold != null)
                    {
                        //Convert entity model to domain object model
                        populationList.Add(ConvertToDomainActual(actualHousehold, true));
                    }

                });

                if (stateIdList.Count != populationList.Count)
                {
                    populationList = new List<PopulationDto>();
                    //Call estimation data from DB
                    var estimateDataList = await this._unityOfWork.EstimateDataRepository().GetAsync();

                    stateIdList?.ForEach(y =>
                    {

                        //If actual data not available then load estimation data 
                        var estimateHousehold = this.LoadEstimatePopulation(estimateDataList, y);

                        //If estimation data found then add into the result
                        if (estimateHousehold != null && estimateHousehold.Result.Count > 0)
                            populationList.Add(estimateHousehold.Result?.FirstOrDefault());

                    });
                }

                return populationList;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception thrown at PopulationService -> LoadAllByStateIdAsync : {0}", ex);
                throw ex.InnerException;
            }
        }

        /// <summary>
        ///  Load Estimate Population
        /// </summary>
        /// <param name="estimationList"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private async Task<List<PopulationDto>> LoadEstimatePopulation(IEnumerable<EstimateDataEntity> estimationList, int state)
        {
            ////return a sum of the value over all districts for the required state in the Estimates table
            List<PopulationDto> result = estimationList?.Where(x => x.State == state)
                 .GroupBy(l => l.State)
                 .Select(cl => new PopulationDto
                 {
                     StateId = cl.First().State,
                     IsActual = false,
                     Population = cl.Sum(c => c.Population),
                 })?.Distinct()?.ToList();

            return result;
        }

        /// <summary>
        ///  Convert Entity To Domain Actual
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isActual"></param>
        /// <returns></returns>
        private static PopulationDto ConvertToDomainActual(ActualDataEntity obj, bool isActual)
        {
            if (obj == null)
            {
                return new PopulationDto();
            }

            return new PopulationDto()
            {
                Id = obj.Id,
                StateId = obj.State,
                Population = obj.Population,
                IsActual = isActual,
            };
        }

        /// <summary>
        ///  Convert To Domain Estimate
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isActual"></param>
        /// <returns></returns>
        private static PopulationDto ConvertToDomainEstimate(EstimateDataEntity obj, bool isActual)
        {
            if (obj == null)
            {
                return new PopulationDto();
            }

            return new PopulationDto()
            {
                Id = obj.Id,
                StateId = obj.State,
                Population = obj.Population,
                IsActual = isActual,
            };
        }

    }
}
