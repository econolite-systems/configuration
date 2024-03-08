// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Econolite.Ode.Models.WrongWayDriver.Dto;
using Econolite.Ode.Services.WrongWayDriver;
using Microsoft.AspNetCore.Mvc;

namespace Api.Configuration.Controllers;

/// <summary>
/// A controller for wrong way driver configuration
/// </summary>
[ApiController]
[Route("wrong-way-driver")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class WrongWayDriverController : ControllerBase
{
    private readonly IWrongWayDriverConfigService _wrongWayDriverService;
    private readonly ILogger<WrongWayDriverController> _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    /// <summary>
    /// Constructs a Wrong Way Driver controller to perform CRUD operations on the configuration settings
    /// </summary>
    /// <param name="wrongWayDriverService">A wrong way driver service that will communicate to the repository</param>
    /// <param name="logger">A logger instance</param>
    /// <param name="auditCrudScopeFactory"></param>
    public WrongWayDriverController(IWrongWayDriverConfigService wrongWayDriverService, ILogger<WrongWayDriverController> logger, IAuditCrudScopeFactory auditCrudScopeFactory)
    {
        _logger = logger;
        _wrongWayDriverService = wrongWayDriverService;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.WrongWayDriver].Event;
    }

    /// <summary>
    /// Gets the wrong way driver configs.  There will only be one set of configs.
    /// </summary>
    /// <response code="200">Returns wrong way driver configs</response>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(WrongWayDriverConfigDto))]
    public async Task<ActionResult<WrongWayDriverConfigDto>> IndexAsync()
    {
        //Should only be one record
        var configs = await _wrongWayDriverService.GetFirstAsync();
        if (configs == null)
        {
            configs = await _wrongWayDriverService.Add(new WrongWayDriverConfigAdd() {ActiveDays = 7});
        }
        return Ok(configs);
    }

    /// <summary>
    /// Adds a new wrong way driver config.  There can only be one set of configs. If configs already exists this will throw an error.
    /// </summary>
    /// <param name="value">The wrong way driver config to add</param>
    /// <response code="200">Returns wrong way driver config</response>
    /// <response code="500">Returns an error if configs already exist</response>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(WrongWayDriverConfigDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PostAsync([FromBody] WrongWayDriverConfigAdd value)
    {
        _logger.LogDebug("Adding {@}", value);
        var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            try
            {
                var created = await _wrongWayDriverService.Add(value);
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
    /// Updates the existing wrong way driver config
    /// </summary>
    /// <param name="value">The wrong way driver config to update</param>
    /// <response code="200">Returns wrong way driver config</response>
    /// <response code="500">Returns an error if the config could not be updated</response>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PutAsync([FromBody] WrongWayDriverConfigUpdate value)
    {
        _logger.LogDebug("Updating {@}", value);
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            try
            {
                var updated = await _wrongWayDriverService.Update(value);
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
    /// Deletes the wrong way driver config.
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">Returns a success code</response>
    /// <response code="404">Returns nothing if the wrong way driver config were unable to be deleted</response>
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
            var deleted = await _wrongWayDriverService.Delete(id);
            if (!deleted) return NotFound();
            return Ok();
        }
    }
}
