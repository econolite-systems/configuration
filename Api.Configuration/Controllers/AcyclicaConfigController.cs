// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Models.Acyclica;
using Api.Configuration.Services.Acyclica;
using Econolite.Ode.Auditing;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Configuration.Controllers;

/// <summary>
/// AcyclicaConfigController
/// </summary>
[ApiController]
[Route("acyclica")]
[AuthorizeOde(MoundRoadRole.Administrator)]
public class AcyclicaConfigController : ControllerBase
{
    private readonly ILogger<AcyclicaConfigController> _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    private readonly IAcyclicaConfigService _acyclicaConfigService;

    /// <summary>
    /// AcyclicaConfigController
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="auditCrudScopeFactory"></param>
    /// <param name="acyclicaConfigService"></param>
    public AcyclicaConfigController(ILogger<AcyclicaConfigController> logger, IAuditCrudScopeFactory auditCrudScopeFactory, IAcyclicaConfigService acyclicaConfigService)
    {
        _logger = logger;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.AcyclicaConfiguration].Event;

        _acyclicaConfigService = acyclicaConfigService;
    }

    /// <summary>
    /// Get the Acyclica Configuration
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AcyclicaConfigDto))]
    public async Task<IActionResult> GetAsync()
    {
        var acyclicaConfig = await _acyclicaConfigService.GetAsync();
        return Ok(acyclicaConfig);
    }

    /// <summary>
    /// Create the Acyclica Configuration
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AcyclicaConfigDto))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> CreateAsync([FromBody] AcyclicaConfigDto acyclicaConfigDto)
    {
        _logger.LogDebug("Adding {@}", acyclicaConfigDto);
        var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => acyclicaConfigDto);
        await using (await scope)
        {
            if (acyclicaConfigDto == null)
                return BadRequest();

            try
            {
                await _acyclicaConfigService.CreateAsync(acyclicaConfigDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create the Acyclica Configuration");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create the Acyclica Configuration");
            }
        }

        return Ok(acyclicaConfigDto);
    }

    /// <summary>
    /// Update the Acyclica Configuration
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AcyclicaConfigDto))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> UpdateAsync([FromBody] AcyclicaConfigDto acyclicaConfigDto)
    {
        _logger.LogDebug("Updating {@}", acyclicaConfigDto);
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => acyclicaConfigDto);
        await using (await scope)
        {
            if (acyclicaConfigDto == null)
                return BadRequest();

            try
            {
                await _acyclicaConfigService.UpdateAsync(acyclicaConfigDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update the Acyclica Configuration");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update the Acyclica Configuration");
            }
        }

        return Ok(acyclicaConfigDto);
    }

    /// <summary>
    /// Delete the Acyclica Configuration
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AcyclicaConfigDto))]
    [AuthorizeOde(MoundRoadRole.Administrator)]
    public async Task<IActionResult> DeleteAsync([FromBody] AcyclicaConfigDto acyclicaConfigDto)
    {
        _logger.LogDebug("Deleting {@}", acyclicaConfigDto);
        var scope = _auditCrudScopeFactory.CreateDeleteAsync(_auditEventType, () => acyclicaConfigDto);
        await using (await scope)
        {
            try
            {
                await _acyclicaConfigService.DeleteAsync(acyclicaConfigDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete the Acyclica Configuration");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete the Acyclica Configuration");
            }
        }

        return Ok(acyclicaConfigDto);
    }
}
