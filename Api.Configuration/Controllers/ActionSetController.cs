// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Econolite.Ode.Models.LogicStatement.Dto;
using Econolite.Ode.Service.LogicStatement;
using Microsoft.AspNetCore.Mvc;

namespace Api.Configuration.Controllers;

/// <summary>
/// Api end point for working with signals
/// </summary>
[ApiController]
[Route("action-set")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class ActionSetController : ControllerBase
{
    private readonly ILogger<ActionSetController> _logger;
    private readonly IActionSetService _actionSetService;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    /// <summary>
    /// Creates an instance of a action set api controller
    /// </summary>
    /// <param name="actionSetService"></param>
    /// <param name="logger"></param>
    /// <param name="auditCrudScopeFactory"></param>
    public ActionSetController(IActionSetService actionSetService, ILogger<ActionSetController> logger, IAuditCrudScopeFactory auditCrudScopeFactory)
    {
        _logger = logger;
        _actionSetService = actionSetService;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.ActionSet].Event;
    }

    /// <summary>
    /// Gets a list of all action sets
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ActionSet>))]
    public async Task<ActionResult<IEnumerable<ActionSet>>> IndexAsync()
    {
        var configs = await _actionSetService.GetAllAsync();
        return Ok(configs);
    }

    /// <summary>
    /// Gets the specified action set
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(200, Type = typeof(ActionSet))]
    public async Task<ActionResult<ActionSet>> GetAsync(Guid id)
    {
        var config = await _actionSetService.GetByIdAsync(id);

        if (config == null) return NotFound();

        return Ok(config);
    }
    
    /// <summary>
    /// Gets a list of action sets that have no
    /// </summary>
    /// <returns></returns>
    [HttpGet("entity/{type}/{id}")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ActionSet>))]
    public async Task<ActionResult<IEnumerable<ActionSet>>> GetByEntityIdAsync(Guid id, string type)
    {
        var configs = await _actionSetService.GetByEntityIdAndTypeAsync(id, type);
        return Ok(configs);
    }

    /// <summary>
    /// Adds a new action set
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(ActionSet))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PostAsync([FromBody] ActionSet value)
    {
        _logger.LogDebug("Adding {@}", value);
        var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            var created = await _actionSetService.Add(value);

            return Ok(created);
        }
    }

    /// <summary>
    /// Updates an existing action set
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(bool))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PutAsync([FromBody] ActionSet value)
    {
        _logger.LogDebug("Updating {@}", value);
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();

            try
            {
                var updated = await _actionSetService.Update(value);
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
    /// Deletes an action set
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
            var deleted = await _actionSetService.Delete(id);
            if (!deleted) return NotFound();
            return Ok();
        }
    }
}
