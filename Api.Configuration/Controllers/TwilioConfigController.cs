// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Models.Twilio;
using Api.Configuration.Services.Twilio;
using Econolite.Ode.Auditing;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Configuration.Controllers;

/// <summary>
/// TwilioConfigController
/// </summary>
[ApiController]
[Route("twilio")]
[AuthorizeOde(MoundRoadRole.Administrator)]
public class TwilioConfigController : ControllerBase
{
    private readonly ILogger<TwilioConfigController> _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    private readonly ITwilioConfigService _twilioConfigService;

    /// <summary>
    /// TwilioConfigController
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="auditCrudScopeFactory"></param>
    /// <param name="twilioConfigService"></param>
    public TwilioConfigController(ILogger<TwilioConfigController> logger, IAuditCrudScopeFactory auditCrudScopeFactory, ITwilioConfigService twilioConfigService)
    {
        _logger = logger;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.TwilioConfiguration].Event;

        _twilioConfigService = twilioConfigService;
    }

    /// <summary>
    /// Get the Twilio Configuration
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TwilioConfigDto))]
    public async Task<IActionResult> GetAsync()
    {
        var twilioConfig = await _twilioConfigService.GetAsync();
        return Ok(twilioConfig);
    }

    /// <summary>
    /// Create the Twilio Configuration
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TwilioConfigDto))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> CreateAsync([FromBody] TwilioConfigDto twilioConfigDto)
    {
        _logger.LogDebug("Adding {@}", twilioConfigDto);
        var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => twilioConfigDto);
        await using (await scope)
        {
            if (twilioConfigDto == null)
                return BadRequest();

            try
            {
                await _twilioConfigService.CreateAsync(twilioConfigDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create the Twilio Configuration");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create the Twilio Configuration");
            }
        }

        return Ok(twilioConfigDto);
    }

    /// <summary>
    /// Update the Twilio Configuration
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TwilioConfigDto))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> UpdateAsync([FromBody] TwilioConfigDto twilioConfigDto)
    {
        _logger.LogDebug("Updating {@}", twilioConfigDto);
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => twilioConfigDto);
        await using (await scope)
        {
            if (twilioConfigDto == null)
                return BadRequest();

            try
            {
                await _twilioConfigService.UpdateAsync(twilioConfigDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update the Twilio Configuration");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update the Twilio Configuration");
            }
        }

        return Ok(twilioConfigDto);
    }

    /// <summary>
    /// Delete the Twilio Configuration
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TwilioConfigDto))]
    [AuthorizeOde(MoundRoadRole.Administrator)]
    public async Task<IActionResult> DeleteAsync([FromBody] TwilioConfigDto twilioConfigDto)
    {
        _logger.LogDebug("Deleting {@}", twilioConfigDto);
        var scope = _auditCrudScopeFactory.CreateDeleteAsync(_auditEventType, () => twilioConfigDto);
        await using (await scope)
        {
            try
            {
                await _twilioConfigService.DeleteAsync(twilioConfigDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete the Twilio Configuration");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete the Twilio Configuration");
            }
        }

        return Ok(twilioConfigDto);
    }
}
