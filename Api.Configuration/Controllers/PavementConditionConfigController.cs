// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Econolite.Ode.Models.PavementCondition.Dto;
using Econolite.Ode.Services.PavementCondition;
using Microsoft.AspNetCore.Mvc;

namespace Api.Configuration.Controllers;

/// <summary>
/// The api for managing the pavement condition configuration.
/// </summary>
[ApiController]
[Route("pavement-condition")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class PavementConditionConfigController : ControllerBase
{
    private readonly IPavementConditionConfigService _pavementConditionService;
    private readonly ILogger<PavementConditionConfigController> _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    /// <summary>
    /// Constructs a Pavement Condition controller to perform CRUD operations on the configuration settings
    /// </summary>
    /// <param name="pavementConditionService">A pavement condition service that will communicate to the repository</param>
    /// <param name="logger">A logger instance</param>
    /// <param name="auditCrudScopeFactory"></param>
    public PavementConditionConfigController(IPavementConditionConfigService pavementConditionService, ILogger<PavementConditionConfigController> logger, IAuditCrudScopeFactory auditCrudScopeFactory)
    {
        _logger = logger;
        _pavementConditionService = pavementConditionService;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.PavementConditionConfiguration].Event;
    }

    /// <summary>
    /// Gets the pavement condition configs.  There will only be one set of configs.
    /// </summary>
    /// <response code="200">Returns pavement condition configs</response>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(PavementConditionConfigDto))]
    public async Task<ActionResult<PavementConditionConfigDto>> IndexAsync()
    {
        //Should only be one record
        var configs = await _pavementConditionService.GetFirstAsync();
        if (configs == null)
        {
            configs = await _pavementConditionService.Add(new PavementConditionConfigAdd() { ActiveDays = 7 });
        }

        return Ok(configs);
    }

    /// <summary>
    /// Adds a new pavement condition config.  There can only be one set of configs. If configs already exists this will throw an error.
    /// </summary>
    /// <param name="value">The pavement condition config to add</param>
    /// <response code="200">Returns pavement condition config</response>
    /// <response code="500">Returns an error if configs already exist</response>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(PavementConditionConfigDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PostAsync([FromBody] PavementConditionConfigAdd value)
    {
        _logger.LogDebug("Adding {@}", value);
        var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            try
            {
                var created = await _pavementConditionService.Add(value);
                return Ok(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to add {@}", value);
                return StatusCode(500, ex.Message);
            }
        }
    }

    /// <summary>
    /// Updates the existing pavement condition config
    /// </summary>
    /// <param name="value">The pavement condition config to update</param>
    /// <response code="200">Returns pavement condition config</response>
    /// <response code="500">Returns an error if the config could not be updated</response>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PutAsync([FromBody] PavementConditionConfigUpdate value)
    {
        _logger.LogDebug("Updating {@}", value);
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            try
            {
                var updated = await _pavementConditionService.Update(value);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update {@}", value);
                return StatusCode(500, ex.Message);
            }
        }
    }

    /// <summary>
    /// Deletes the pavement condition config.
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">Returns a success code</response>
    /// <response code="404">Returns nothing if the pavement condition config were unable to be deleted</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(200, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AuthorizeOde(MoundRoadRole.Administrator)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        _logger.LogDebug("Deleting {@}", id);
        var scope = _auditCrudScopeFactory.CreateDeleteAsync(_auditEventType, id.ToString);
        await using (await scope)
        {
            var deleted = await _pavementConditionService.Delete(id);
            if (!deleted) return NotFound();
            return Ok();
        }
    }
}
