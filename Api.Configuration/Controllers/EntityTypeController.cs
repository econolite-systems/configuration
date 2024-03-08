// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Econolite.Ode.Domain.Entities;
using Econolite.Ode.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Configuration.Controllers;

/// <summary>
/// Api end point for working with entity types
/// </summary>
[ApiController]
[Route("entity-type")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class EntityTypeController : ControllerBase
{
    private readonly ILogger<EntityTypeController> _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly IEntityTypeService _entityTypeService;
    private readonly string _auditEventType;

    /// <summary>
    /// Creates an instance of a entity type api controller
    /// </summary>
    /// <param name="entityTypeService"></param>
    /// <param name="logger"></param>
    /// <param name="auditCrudScopeFactory"></param>
    public EntityTypeController(IEntityTypeService entityTypeService, ILogger<EntityTypeController> logger, IAuditCrudScopeFactory auditCrudScopeFactory)
    {
        _logger = logger;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _entityTypeService = entityTypeService;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.EntityType].Event;
    }

    /// <summary>
    /// Gets a list of all entity type sections
    /// </summary>
    /// <returns></returns>
    [HttpGet("sections")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<EntityType>))]
    public async Task<ActionResult<IEnumerable<EntityTypeSection>>> GetSectionsAsync()
    {
        var configs = await _entityTypeService.GetConfigSections();
        return Ok(configs);
    }
    
    /// <summary>
    /// Gets a list of all entity types
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<EntityType>))]
    public async Task<ActionResult<IEnumerable<EntityType>>> IndexAsync()
    {
        var configs = await _entityTypeService.GetAllAsync();
        return Ok(configs);
    }

    /// <summary>
    /// Gets the specified type
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(200, Type = typeof(EntityType))]
    public async Task<ActionResult<EntityType>> GetAsync(Guid id)
    {
        var config = await _entityTypeService.GetByIdAsync(id);

        if (config == null) return NotFound();

        return Ok(config);
    }

    /// <summary>
    /// Adds a new entity type
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(EntityType))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PostAsync([FromBody] EntityTypeAdd value)
    {
        _logger.LogDebug("Adding {@}", value);
        var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();
            
            var created = await _entityTypeService.Add(value);
            
            return Ok(created);
        }
    }

    /// <summary>
    /// Updates an existing entity type
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(bool))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PutAsync([FromBody] EntityType value)
    {
        _logger.LogDebug("Updating {@}", value);
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();
            try
            {
                var updated = await _entityTypeService.Update(value);
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
    /// Deletes an entity type
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
            var deleted = await _entityTypeService.Delete(id);
            if (!deleted) return NotFound();
            return Ok();
        }
    }
}
