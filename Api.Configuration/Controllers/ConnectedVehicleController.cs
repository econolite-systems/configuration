// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Econolite.Ode.Models.ConnectedVehicle.Dto;
using Econolite.Ode.Services.ConnectedVehicle;
using Microsoft.AspNetCore.Mvc;

namespace Api.Configuration.Controllers;

/// <summary>
/// The api for managing the connected vehicle configuration.
/// </summary>
[ApiController]
[Route("connected-vehicle")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class ConnectedVehicleController : ControllerBase
{
    private readonly IConnectedVehicleConfigService _connectedVehicleService;
    private readonly ILogger<ConnectedVehicleController> _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    /// <summary>
    /// Constructes a Connected Vehicle controller to perform CRUD operations on the configuration settings
    /// </summary>
    /// <param name="connectedVehicleService">A connected vehicle service that will communicate to the repository</param>
    /// <param name="logger">A logger instance</param>
    /// <param name="auditCrudScopeFactory"></param>
    public ConnectedVehicleController(IConnectedVehicleConfigService connectedVehicleService, ILogger<ConnectedVehicleController> logger, IAuditCrudScopeFactory auditCrudScopeFactory)
    {
        _logger = logger;
        _connectedVehicleService = connectedVehicleService;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.ConnectedVehicle].Event;
    }

    /// <summary>
    /// Gets the connected vehicle configs.  There will only be one set of configs.
    /// </summary>
    /// <response code="200">Returns connected vehicle configs</response>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(ConnectedVehicleConfigDto))]
    public async Task<ActionResult<ConnectedVehicleConfigDto>> IndexAsync()
    {
        //Should only be one record
        var configs = await _connectedVehicleService.GetFirstAsync();
        return Ok(configs);
    }

    /// <summary>
    /// Adds a new connected vehicle config.  There can only be one set of configs. If configs already exists this will throw an error.
    /// </summary>
    /// <param name="value">The connected vehicle config to add</param>
    /// <response code="200">Returns connected vehicle config</response>
    /// <response code="500">Returns an error if configs already exist</response>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(ConnectedVehicleConfigDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PostAsync([FromBody] ConnectedVehicleConfigAdd value)
    {
        _logger.LogDebug("Adding {@}", value);
        var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            try
            {
                var created = await _connectedVehicleService.AddAsync(value);
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
    /// Updates the existing connected vehicle config
    /// </summary>
    /// <param name="value">The connected vehicle config to update</param>
    /// <response code="200">Returns connected vehicle config</response>
    /// <response code="500">Returns an error if the config could not be updated</response>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PutAsync([FromBody] ConnectedVehicleConfigUpdate value)
    {
        _logger.LogDebug("Updating {@}", value);
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            try
            {
                var updated = await _connectedVehicleService.UpdateAsync(value);
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
    /// Deletes the connected vehicle config.
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">Returns a success code</response>
    /// <response code="404">Returns nothing if the connected vehicle config were unable to be deleted</response>
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
            var deleted = await _connectedVehicleService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return Ok();
        }
    }
}
