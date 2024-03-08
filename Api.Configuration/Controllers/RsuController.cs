// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Econolite.Ode.Configuration.Rsu;
using Econolite.Ode.Models.Rsu.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Api.Configuration.Controllers;

/// <summary>
/// Api end point for working with rsus
/// </summary>
[ApiController]
[Route("rsu")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class RsuController : ControllerBase
{
    private readonly ILogger<RsuController> _logger;
    private readonly IRsuService _rsuService;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    /// <summary>
    /// Creates an instance of a rsu api controller
    /// </summary>
    /// <param name="rsuService"></param>
    /// <param name="logger"></param>
    /// <param name="auditCrudScopeFactory"></param>
    public RsuController(IRsuService rsuService, ILogger<RsuController> logger, IAuditCrudScopeFactory auditCrudScopeFactory)
    {
        _logger = logger;
        _rsuService = rsuService;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.RoadsideUnit].Event;
    }

    /// <summary>
    /// Gets a list of all rsus
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Rsu>))]
    public async Task<ActionResult<IEnumerable<Rsu>>> IndexAsync()
    {
        var configs = await _rsuService.GetAllAsync();
        return Ok(configs);
    }

    /// <summary>
    /// Gets the specified rsu
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(200, Type = typeof(Rsu))]
    public async Task<ActionResult<Rsu>> GetAsync(Guid id)
    {
        var config = await _rsuService.GetByIdAsync(id);

        if (config == null) return NotFound();

        return Ok(config);
    }

    /// <summary>
    /// Adds a new rsu
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(Rsu))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PostAsync([FromBody] Rsu value)
    {
        _logger.LogDebug("Adding {@}", value);
        var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            var created = await _rsuService.Add(value);

            return Ok(created);
        }
    }

    /// <summary>
    /// Updates an existing rsu
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(bool))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PutAsync([FromBody] Rsu value)
    {
        _logger.LogDebug("Updating {@}", value);
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            try
            {
                var updated = await _rsuService.Update(value);
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
    /// Deletes a rsu
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(200, Type = typeof(bool))]
    [AuthorizeOde(MoundRoadRole.Administrator)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        _logger.LogDebug("Deleting {@}", id);
        var scope = _auditCrudScopeFactory.CreateDeleteAsync(_auditEventType, id.ToString);
        await using (await scope)
        {
            var deleted = await _rsuService.Delete(id);
            if (!deleted) return NotFound();
            return Ok();
        }
    }
}
