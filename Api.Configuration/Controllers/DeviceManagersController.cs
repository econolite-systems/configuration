// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Econolite.Ode.Domain.Configuration;
using Econolite.Ode.Models.DeviceManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Configuration.Controllers;

/// <summary>
/// Api end points to communicate with the device manager
/// </summary>
[ApiController]
[Route("device-managers")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class DeviceManagersController : ControllerBase
{
    private readonly IDeviceManagerService _dmService;
    private readonly ILogger<DeviceManagersController> _logger;
    private readonly Guid _tenantId;
    private readonly Guid _userId;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventTypeDmConfig;
    private readonly string _auditEventTypeDmChannel;

    /// <summary>
    /// Creates an instance of a device manager api controller
    /// </summary>
    /// <param name="config"></param>
    /// <param name="dmService"></param>
    /// <param name="logger"></param>
    /// <param name="auditCrudScopeFactory"></param>
    public DeviceManagersController(
        IConfiguration config,
        IDeviceManagerService dmService,
        ILogger<DeviceManagersController> logger,
        IAuditCrudScopeFactory auditCrudScopeFactory
    )
    {
        _logger = logger;
        _dmService = dmService;
        _tenantId = Guid.Parse(config["TenantId"] ?? throw new NullReferenceException("TenantId missing in config")); //Guid.Parse(httpContextAccessor.HttpContext.User.Claims.Single(claim => claim.Type == "tenantId").Value);
        _userId = Guid
            .Empty; //Guid.Parse(httpContextAccessor.HttpContext.User.Claims.Single(claim => claim.Type == "userId").Value);
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventTypeDmConfig = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.DeviceManagerConfiguration].Event;
        _auditEventTypeDmChannel = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.DeviceManagerChannel].Event;
    }

    /// <summary>
    /// Get All device managers
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<DmConfigDto>))]
    public async Task<ActionResult<IEnumerable<DmConfigDto>>> IndexAsync()
    {
        var configs = await _dmService.GetAllAsync(_tenantId);
        return Ok(configs);
    }

    /// <summary>
    /// Get a specified device manager
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(200, Type = typeof(DmConfigDto))]
    public async Task<ActionResult<DmConfigDto>> GetAsync(Guid id)
    {
        var config = await _dmService.GetByIdAsync(id);

        if (config == null) return NotFound();

        return Ok(config);
    }

    /// <summary>
    /// Add a device manager
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(DmConfigDto))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PostAsync([FromBody] DmConfigAdd value)
    {
        _logger.LogDebug("Adding {@}", value);
        var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventTypeDmConfig, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            try
            {
                var created = await _dmService.AddDeviceManager(_tenantId, value);
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
    /// Update a device manager config
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(bool))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PutAsync([FromBody] DmConfigUpdate value)
    {
        _logger.LogDebug("Updating {@}", value);
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventTypeDmConfig, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            try
            {
                var updated = await _dmService.UpdateDeviceManager(value);
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
    /// Delete a device manager config
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(200, Type = typeof(bool))]
    [AuthorizeOde(MoundRoadRole.Administrator)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        _logger.LogDebug("Deleting {@}", id);
        var scope = _auditCrudScopeFactory.CreateDeleteAsync(_auditEventTypeDmConfig, id.ToString);
        await using (await scope)
        {
            var deleted = false;
            try
            {
                deleted = await _dmService.DeleteDeviceManager(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to delete {@}", id);
            }
            if (!deleted) return NotFound();
            return Ok();
        }
    }

    /// <summary>
    /// Add a channel to a device manager
    /// </summary>
    /// <param name="id">device manager id</param>
    /// <param name="value">channel config</param>
    /// <returns></returns>
    [HttpPost("{id}/channel")]
    [ProducesResponseType(200, Type = typeof(ChannelDto))]
    public async Task<IActionResult> PostAsync(Guid id, [FromBody] ChannelAdd value)
    {
        _logger.LogDebug("Adding {@}", value);
        var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventTypeDmChannel, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            try
            {
                var created = await _dmService.AddChannelAsync(id, value);
                return Ok(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to add channel {@}", new { DeviceManagerId = id, Channel = value });
                return StatusCode(500, ex.Message);
            }
        }
    }

    /// <summary>
    /// Update a channel on a device manager
    /// </summary>
    /// <param name="id">device manager id</param>
    /// <param name="value">channel config</param>
    /// <returns></returns>
    [HttpPut("{id}/channel")]
    [ProducesResponseType(200, Type = typeof(ChannelDto))]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] ChannelUpdate value)
    {
        _logger.LogDebug("Updating {@}", value);
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventTypeDmChannel, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            try
            {
                var updated = await _dmService.UpdateChannelAsync(id, value);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to update channel {@}", new { DeviceManagerId = id, Channel = value });
                return StatusCode(500, ex.Message);
            }
        }
    }

    /// <summary>
    /// Delete a channel from a device manager
    /// </summary>
    /// <param name="id">device manager id</param>
    /// <param name="chId">channel id</param>
    /// <returns></returns>
    [HttpDelete("{id}/channel/{chId}")]
    [ProducesResponseType(200, Type = typeof(bool))]
    public async Task<IActionResult> DeleteAsync(Guid id, Guid chId)
    {
        _logger.LogDebug("Deleting {@}", chId);
        var scope = _auditCrudScopeFactory.CreateDeleteAsync(_auditEventTypeDmChannel, chId.ToString);
        await using (await scope)
        {
            var deleted = false;
            try
            {
                deleted = await _dmService.DeleteChannelAsync(id, chId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to delete channel {@}", new { DeviceManagerId = id, ChannelId = chId });
            }
            if (!deleted) return NotFound();
            return Ok();
        }
    }
}
