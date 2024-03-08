// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Excel;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Econolite.Ode.Models.PavementCondition.Config;
using Econolite.Ode.Models.PavementCondition.Status;
using Econolite.Ode.Services.PavementCondition;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Econolite.Ode.Auditing;
using Econolite.Ode.Models.PavementCondition.Db;

// TODO: Move this out of the Configuration API
namespace Api.Configuration.Controllers;

/// <summary>
/// Pavement Condition Status
/// </summary>
[ApiController]
[Route("pavement-condition-status")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class PavementConditionStatusController : ControllerBase
{
    private readonly ILogger<PavementConditionStatusController> _logger;
    private readonly IPavementConditionStatusService _pavementConditionStatusService;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    /// <summary>
    /// Pavement Condition Status
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="pavementConditionStatusService"></param>
    /// <param name="auditCrudScopeFactory"></param>
    public PavementConditionStatusController(ILogger<PavementConditionStatusController> logger, IPavementConditionStatusService pavementConditionStatusService, IAuditCrudScopeFactory auditCrudScopeFactory)
    {
        _logger = logger;
        _pavementConditionStatusService = pavementConditionStatusService;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.PavementConditionStatus].Event;
    }

    /// <summary>
    /// Find pavement condition statuses
    /// </summary>
    /// <response code="200">Returns pavement condition statuses</response>
    [HttpGet("find")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PavementConditionStatusDto>))]
    public async Task<IActionResult> FindAsync([FromQuery] bool? active)
    {
        var pcStatusDtos = await _pavementConditionStatusService.FindAsync(active);

        return Ok(pcStatusDtos);
    }

    /// <summary>
    /// Parse File
    /// </summary>
    /// <param name="file"></param>
    /// <response code="200">Returns pavement condition statuses</response>
    [HttpPost("parse")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PavementConditionStatusDto>))]
    public IActionResult ParseFile(IFormFile file)
    {
        // XLSM files only
        //if (!(file.ContentType == "application/vnd.ms-excel.sheet.macroEnabled.12"))
        //    return BadRequest("Invalid file content type");

        try
        {
            using var csv = new CsvReader(new ExcelParser(file.OpenReadStream(), "PASER Ratings", CultureInfo.InvariantCulture));

            csv.Context.RegisterClassMap<PaserRatingsMap>();

            var records = csv.GetRecords<PaserRatings>().ToList()
                .Select(pcs =>
                {
                    var splitStr = pcs.Start.TrimStart('[').TrimEnd(']').Split(',');
                    return new PavementConditionStatusDto
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        Location = pcs.Segment.ToString(),
                        Latitude = double.Parse(splitStr[0]),
                        Longitude = double.Parse(splitStr[1]),
                        Severity = PavementConditionStatusSeverity.Medium,
                        Type = PavementConditionStatusType.None,
                        IsActive = false,
                    };
                });

            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to parse file");
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Import pavement condition statuses
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="pcStatusDtos"></param>
    /// <returns></returns>
    [HttpPost("import")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PavementConditionStatusDto>))]
    public async Task<IActionResult> ImportAsync([FromQuery] string filename, [FromBody] IEnumerable<PavementConditionStatusDto> pcStatusDtos)
    {
        _logger.LogDebug("Adding {@}", pcStatusDtos);
        var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => {
            return new PavementConditionStatusAddAuditDocument
            {
                Filename = filename,
                PavementConditionStatuses = pcStatusDtos.Select(pcs => pcs.ToDoc())
            };
        });
        await using (await scope)
        {
            try
            {
                await _pavementConditionStatusService.InsertManyAsync(pcStatusDtos);
                return Ok(pcStatusDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to import data");
                return StatusCode(500, ex.Message);
            }
        }
    }

    /// <summary>
    /// Update pavement condition statuses
    /// </summary>
    /// <param name="pcStatusDtos"></param>
    /// <returns></returns>
    [HttpPut("update")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PavementConditionStatusDto>))]
    public async Task<IActionResult> UpdateAsync([FromBody] IEnumerable<PavementConditionStatusDto> pcStatusDtos)
    {
        _logger.LogDebug("Updating {@}", pcStatusDtos);
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => {
            return new PavementConditionStatusUpdateAuditDocument
            {
                PavementConditionStatuses = pcStatusDtos.Select(pcs => pcs.ToDoc())
            };
        });
        await using (await scope)
        {
            try
            {
                await _pavementConditionStatusService.UpdateManyAsync(pcStatusDtos);
                return Ok(pcStatusDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update data");
                return StatusCode(500, ex.Message);
            }
        }
    }

    /// <summary>
    /// Delete pavement condition statuses
    /// </summary>
    /// <param name="pcStatusDtos"></param>
    /// <returns></returns>
    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PavementConditionStatusDto>))]
    public async Task<IActionResult> DeleteAsync([FromBody] IEnumerable<PavementConditionStatusDto> pcStatusDtos)
    {
        _logger.LogDebug("Deleting {@}", pcStatusDtos);
        var scope = _auditCrudScopeFactory.CreateDeleteAsync(_auditEventType, () => {
            return new PavementConditionStatusDeleteAuditDocument
            {
                PavementConditionStatuses = pcStatusDtos.Select(pcs => pcs.ToDoc())
            };
        });
        await using (await scope)
        {
            try
            {
                await _pavementConditionStatusService.DeleteManyAsync(pcStatusDtos);
                return Ok(pcStatusDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to delete data");
                return StatusCode(500, ex.Message);
            }
        }
    }

    private class PaserRatings
    {
        public int Segment { get; set; }
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
        public double Length { get; set; }
        public double RoadboticsRating { get; set; }
        public double PaserConversion { get; set; }
        public string DistressDescription { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }

    private class PaserRatingsMap : ClassMap<PaserRatings>
    {
        public PaserRatingsMap()
        {
            Map(m => m.Segment).Name("Segment #");
            Map(m => m.Start).Name("Start");
            Map(m => m.End).Name("End");
            Map(m => m.Length).Name("Length (ft)");
            Map(m => m.RoadboticsRating).Name("Roadbotics Rating");
            Map(m => m.PaserConversion).Name("PASER Conversion");
            Map(m => m.DistressDescription).Name("Distress Description");
            Map(m => m.Recommendation).Name("Recommendation");
        }
    }
}
