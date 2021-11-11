using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vektonn.ApiContracts;
using Vektonn.DataSource;
using Vektonn.SharedImpl.ApiContracts;
using Vektonn.SharedImpl.ApiContracts.Validation;
using Vektonn.SharedImpl.Configuration;
using Vektonn.SharedImpl.Contracts;
using Vostok.Logging.Abstractions;

namespace Vektonn.ApiService.Controllers
{
    [ApiController]
    [Route("api/v1/upload/{dataSourceName}/{dataSourceVersion}")]
    public class UploadApiController : ControllerBase
    {
        private readonly ILog log;
        private readonly IIndexMetaProvider indexMetaProvider;
        private readonly IDataSourceProducer dataSourceProducer;

        public UploadApiController(
            ILog log,
            IIndexMetaProvider indexMetaProvider,
            IDataSourceProducer dataSourceProducer)
        {
            this.log = log;
            this.indexMetaProvider = indexMetaProvider;
            this.dataSourceProducer = dataSourceProducer;
        }

        [HttpPost]
        public async Task<ActionResult> Upload([Required] string dataSourceName, [Required] string dataSourceVersion, [FromBody] InputDataPointDto[] uploadQuery)
        {
            var dataSourceId = new DataSourceId(dataSourceName, dataSourceVersion);
            var dataSourceMeta = indexMetaProvider.TryGetDataSourceMeta(dataSourceId);
            if (dataSourceMeta == null)
                return NotFound(new ErrorDto(ErrorMessages: new[] {$"Data source {dataSourceId} does not exist"}));

            var inputDataPointOrTombstones = new List<InputDataPointOrTombstone>();
            var inputDataPointValidator = new InputDataPointValidator(dataSourceMeta);
            foreach (var inputDataPoint in uploadQuery)
            {
                var validationResult = await inputDataPointValidator.ValidateAsync(inputDataPoint);
                if (!validationResult.IsValid)
                    return BadRequest(new ErrorDto(ErrorMessages: validationResult.Errors.Select(x => x.ErrorMessage).ToArray()));

                inputDataPointOrTombstones.Add(ToInputDataPointOrTombstone(inputDataPoint));
            }

            await dataSourceProducer.ProduceAsync(dataSourceMeta, inputDataPointOrTombstones);
            log.Info($"Successfully produced {inputDataPointOrTombstones.Count} items to data source: {dataSourceId}");

            return Ok();
        }

        private static InputDataPointOrTombstone ToInputDataPointOrTombstone(InputDataPointDto inputDataPoint)
        {
            var attributes = inputDataPoint.Attributes.ToDictionary(x => x.Key, x => x.Value.ToAttributeValue());

            if (inputDataPoint.IsDeleted)
                return new InputDataPointOrTombstone(new Tombstone(attributes));

            var vectorCoordinateIndices = inputDataPoint.Vector is SparseVectorDto sparseVectorDto
                ? sparseVectorDto.CoordinateIndices
                : null;

            return new InputDataPointOrTombstone(new InputDataPoint(attributes, inputDataPoint.Vector!.Coordinates, vectorCoordinateIndices));
        }
    }
}
