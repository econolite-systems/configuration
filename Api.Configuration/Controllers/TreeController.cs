// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
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
[Route("tree")]
[AuthorizeOde(MoundRoadRole.ReadOnly)]
public class TreeController : ControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IEntityTypeService _entityTypeService;
    private readonly ILogger<TreeController> _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;

    /// <summary>
    /// Creates an instance of an entities api controller
    /// </summary>
    /// <param name="entityService"></param>
    /// <param name="entityTypeService"></param>
    /// <param name="logger"></param>
    /// <param name="auditCrudScopeFactory"></param>
    public TreeController(IEntityService entityService, IEntityTypeService entityTypeService, ILogger<TreeController> logger, IAuditCrudScopeFactory auditCrudScopeFactory)
    {
        _logger = logger;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _entityService = entityService;
        _entityTypeService = entityTypeService;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<EntityNodeProjection>>> GetAllAsync()
    {
        var results = await _entityService.GetAllNodesAsync();
        results = await _entityService.RemoveInvisibleTypesAsync(results.ToArray());
        return Ok(results);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EntityNodeProjection>>> GetAsync()
    {
        var results = await _entityService.GetRootNodesAsync();
        results = await _entityService.RemoveInvisibleTypesAsync(results.ToArray());
        return Ok(results);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="term"></param>
    /// <returns></returns>
    [HttpGet("search/{term}")]
    public async Task<ActionResult<IEnumerable<EntityNode>>> GetSearchResultsAsync(string term)
    {
        var results = await _entityService.GetSearchNodesAsync(term);
        results = await _entityService.RemoveInvisibleTypesAsync(results.ToArray());
        return Ok(results);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    [HttpGet("{instanceId}")]
    public async Task<ActionResult<EntityNodeProjection?>> GetEntityAsync(string instanceId)
    {
        return await _entityService.GetNodeAsync(instanceId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(IEnumerable<EntityNodeProjection>), 200)]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> IndexAsync([FromBody] IndexRequest request)
    {
        var result = await _entityService.GetExpandedNodesAsync(request.ExpandedEntityIds);
        result = await _entityService.RemoveInvisibleTypesAsync(result.ToArray());
        return Ok(result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    [HttpGet("{instanceId}/children")]
    public async Task<ActionResult<EntityNodeProjection[]>> GetChildrenAsync(string instanceId)
    {
        var node = await _entityService.GetNodeAsync(instanceId);
        node = await _entityService.RemoveInvisibleTypesAsync(node);
        return node?.Children?.ToArray() ?? Array.Empty<EntityNodeProjection>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("children")]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<ActionResult<EntityNodeProjection[]>> GetChildrenPostAsync([FromBody] InstanceRequest request)
    {
        var node = await _entityService.GetNodeAsync(request.InstanceId);
        node = await _entityService.RemoveInvisibleTypesAsync(node);
        return node?.Children?.ToArray() ?? Array.Empty<EntityNodeProjection>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    [HttpPut("{instanceId}/move-to/{parent}")]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<ActionResult<EntityNode>> MoveNodeAsync(string instanceId, Guid parent)
    {
        var node = await GetEntityAsync(instanceId);
        var (result, err) = await _entityService.MoveAsync(node.Value!, parent);
        if (err != null)
        {
            return BadRequest(err);
        }

        return Ok(result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    [HttpPut("{instanceId}/copy-to/{parent}")]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<ActionResult<EntityNodeProjection>> CopyNodeAsync(string instanceId, Guid parent)
    {
        var node = await GetEntityAsync(instanceId);
        var (result, err) = await _entityService.CopyAsync(node.Value!, parent);
        if (err != null)
        {
            return BadRequest(err);
        }

        return Ok(result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    [HttpPut("{instanceId}/move-up")]
    public async Task<ActionResult<EntityNodeProjection>> MoveUpAsync(string instanceId)
    {
        var (result, err) = await _entityService.MoveUpAsync(instanceId);
        if (err != null)
        {
            return BadRequest(err);
        }

        return Ok(result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    [HttpPut("{instanceId}/move-down")]
    public async Task<ActionResult<EntityNodeProjection>> MoveDownAsync(string instanceId)
    {
        var (result, err) = await _entityService.MoveDownAsync(instanceId);
        if (err != null)
        {
            return BadRequest(err);
        }

        return Ok(result);
    }
}

/// <summary>
/// 
/// </summary>
public class IndexRequest
{
    /// <summary>
    /// 
    /// </summary>
    public IEnumerable<string> ExpandedEntityIds { get; set; } = Array.Empty<string>();
}

/// <summary>
/// 
/// </summary>
public class InstanceRequest
{
    /// <summary>
    /// 
    /// </summary>
    public string InstanceId { get; set; } = string.Empty;
}
