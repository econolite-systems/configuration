// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Econolite.Ode.Domain.Configuration;
using Econolite.Ode.Domain.Entities;
using Econolite.Ode.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Configuration.Controllers;

/// <summary>
/// Api end point for working with entities
/// </summary>
[ApiController]
[Route("entities")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class EntitiesController : ControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IEntityTypeService _entityTypeService;
    private readonly ILogger<EntitiesController> _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    /// <summary>
    /// Creates an instance of an entities api controller
    /// </summary>
    /// <param name="entityService"></param>
    /// <param name="entityTypeService"></param>
    /// <param name="logger"></param>
    /// <param name="auditCrudScopeFactory"></param>
    public EntitiesController(IEntityService entityService, IEntityTypeService entityTypeService, ILogger<EntitiesController> logger, IAuditCrudScopeFactory auditCrudScopeFactory)
    {
        _logger = logger;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _entityService = entityService;
        _entityTypeService = entityTypeService;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.Entity].Event;
    }

    /// <summary>
    /// Gets a list of the entity types
    /// </summary>
    /// <returns></returns>
    [HttpGet("types")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<EntityTypeId>))]
    public async Task<IActionResult> GetTypesAsync()
    {
        var types = await _entityTypeService.GetAllAsync();
        return Ok(types);
    }

    /// <summary>
    /// Gets all of the entities
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<EntityNode>))]
    public async Task<IActionResult> GetAsync()
    {
        var allNodes = await _entityService.GetAllNodesAsync();
        return Ok(allNodes);
    }

    /// <summary>
    /// Gets the specified entities
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(EntityNode))]
    public async Task<ActionResult<EntityNode>> GetAsync([FromQuery] string[] ids)
    {
        var config = await _entityService.GetExpandedNodesAsync(ids);

        if (config == null) return NotFound();

        return Ok(config);
    }
    
    /// <summary>
    /// Gets the specified entities
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(200, Type = typeof(EntityNode))]
    public async Task<ActionResult<EntityNode>> GetAsync(Guid id)
    {
        var config = await _entityService.GetByIdAsync(id);

        if (config == null) return NotFound();

        return Ok(config);
    }
    
    /// <summary>
    /// Gets entities of a specified type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpGet("types/{type}")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<EntityNode>))]
    public async Task<ActionResult<IEnumerable<EntityNode>>> GetByTypeAsync(string type)
    {
        var config = await _entityService.GetNodesByTypeAsync(type);

        if (config == null) return NotFound();

        return Ok(config);
    }
    
    /// <summary>
    /// Gets entities of a specified type
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("intersection/{id}")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<EntityNode>))]
    public async Task<ActionResult<IEnumerable<EntityNode>>> GetByIntersectionIdAsync(Guid id)
    {
        var config = await _entityService.GetByIntersectionIdAsync(id);
        if (config == null) return NotFound();

        return Ok(config);
    }
    
    /// <summary>
    /// Find intersections within a radius distance of miles.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="miles"></param>
    /// <returns>Intersections within radius</returns>
    [HttpPost("intersections/query/radius/{miles}")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<EntityNode>))]
    [AuthorizeOde(MoundRoadRole.ReadOnly)]
    public async Task<ActionResult<IEnumerable<EntityNode>>> QueryIntersectionsWithinRadiusDistanceAsync([FromBody] GeoJsonPointFeature value, int miles)
    {
        var config = await _entityService.QueryIntersectionsWithinRadiusDistanceInMilesAsync(value, miles);
        if (config == null) return NotFound();

        return Ok(config);
    }
    
    /// <summary>
    /// Find entities with a type intersecting a route.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns>Entities of type that intersect route.</returns>
    [HttpPost("query/{type}")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<EntityNode>))]
    [AuthorizeOde(MoundRoadRole.ReadOnly)]
    public async Task<ActionResult<IEnumerable<EntityNode>>> QueryIntersectingByTypeAsync([FromBody] GeoJsonLineStringFeature value, string type)
    {
        var config = await _entityService.QueryIntersectingByTypeAsync(value, type);
        if (config == null) return NotFound();

        return Ok(config);
    }
    
    /// <summary>
    /// Find entities that point is within geofence.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Entities that point is within geofence.</returns>
    [HttpPost("query/geofence")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<EntityNode>))]
    [AuthorizeOde(MoundRoadRole.ReadOnly)]
    public async Task<ActionResult<IEnumerable<EntityNode>>> QueryIntersectingGeoFencesAsync([FromBody] GeoJsonPointFeature value)
    {
        var config = await _entityService.QueryIntersectingGeoFencesAsync(value);
        if (config == null) return NotFound();

        return Ok(config);
    }

    /// <summary>
    /// Find entities of a type that point is within geofence.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns>Entities of a type within geofence.</returns>
    [HttpPost("query/geofence/{type}")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<EntityNode>))]
    [AuthorizeOde(MoundRoadRole.ReadOnly)]
    public async Task<ActionResult<IEnumerable<EntityNode>>> QueryIntersectingGeoFencesByTypeAsync([FromBody] GeoJsonPointFeature value, string type)
    {
        var config = await _entityService.QueryIntersectingGeoFencesByTypeAsync(value, type);
        if (config == null) return NotFound();

        return Ok(config);
    }
    
    /// <summary>
    /// Gets possible entities of a specified entity type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpGet("types/{type}/parents")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<EntityNodeProjection>))]
    public async Task<ActionResult<IEnumerable<EntityNodeProjection>>> GetPossibleParentNodesByTypeIdAsync(Guid type)
    {
        var config = await _entityService.GetPossibleParentNodesByTypeAsync(type);

        if (config == null) return NotFound();

        return Ok(config);
    }
    
    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(EntityNode))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PostAsync([FromBody] EntityNode value)
    {
        if (value.Id == Guid.Empty)
        {
            value.Id = Guid.NewGuid();
        }

        if (value.Geometry.Point != null && value.Geometry.Point.Coordinates == null)
        {
            value.Geometry.Point = null;
        }
        
        if (value.Geometry.LineString != null && value.Geometry.LineString.Coordinates == null)
        {
            value.Geometry.LineString = null;
        }
        
        if (value.Geometry.Polygon!= null && value.Geometry.Polygon.Coordinates == null)
        {
            value.Geometry.Polygon = null;
        }
        
        _logger.LogDebug("Adding {@}", value);
        var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();
            
            var created = await _entityService.Add(value);
            
            return Ok(created);
        }
    }

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    [HttpPost("{parent}")]
    [ProducesResponseType(200, Type = typeof(EntityNode))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PostAsync([FromBody] EntityNode value, Guid parent)
    {
        if (value.Id == Guid.Empty)
        {
            value.Id = Guid.NewGuid();
        }
        _logger.LogDebug("Adding {@}", value);
        var scope = _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();
            
            var created = await _entityService.Add(parent, value);
            
            return Ok(created);
        }
    }

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(bool))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> PutAsync([FromBody] EntityNode value)
    {
        _logger.LogDebug("Updating {@}", value);
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, () => value);
        await using (await scope)
        {
            if (value == null) return BadRequest();
            try
            {
                var updated = await _entityService.Update(value);
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
    /// Get downstream entities
    /// </summary>
    /// <param name="value"></param>
    /// <param name="intersections"></param>
    /// <returns></returns>
    [HttpPost("downstream/{intersections}")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<EntityNode>))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> GetDownstreamAsync([FromBody] GeoJsonPointFeature value, int intersections)
    {
        _logger.LogDebug("Updating {@}", value);
        
        if (value == null) return BadRequest();
        try
        {
            var updated = await _entityService.GetDownstreamIntersectionsAsync(value, intersections);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to get downstream entities from {@}", value);
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Get upstream entities
    /// </summary>
    /// <param name="value"></param>
    /// <param name="intersections"></param>
    /// <returns></returns>
    [HttpPost("upstream/{intersections}")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<EntityNode>))]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> GetUpstreamAsync([FromBody] GeoJsonPointFeature value, int intersections)
    {
        if (value == null) return BadRequest();
        try
        {
            var updated = await _entityService.GetUpstreamIntersectionsAsync(value, intersections);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to get upstream entities from {@}", value);
            return StatusCode(500, ex.Message);
        }
    }
    
    /// <summary>
    /// Deletes an entity
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
            var deleted = await _entityService.Delete(id);
            if (!deleted) return NotFound();
            return Ok();
        }
    }
}
