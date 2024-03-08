// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Econolite.Ode.Domain.Configuration;
using Econolite.Ode.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Api.Configuration.Controllers;

/// <summary>
/// Api end point for working with entities
/// </summary>
[ApiController]
[Route("entities-sync")]
[AuthorizeOde(MoundRoadRole.Contributor)]
public class EntitiesSyncController : ControllerBase
{
    private readonly IEntityService _entityService;
    private readonly ILogger<EntitiesSyncController> _logger;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;

    /// <summary>
    /// Creates an instance of an entities sync api controller
    /// </summary>
    /// <param name="entityService"></param>
    /// <param name="logger"></param>
    /// <param name="auditCrudScopeFactory"></param>
    public EntitiesSyncController(IEntityService entityService, ILogger<EntitiesSyncController> logger, IAuditCrudScopeFactory auditCrudScopeFactory)
    {
        _logger = logger;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _entityService = entityService;
    }

    /// <summary>
    /// Sync entities
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<IActionResult> PostAsync([FromBody] EntitySync? value)
    {
        if (value == null) return BadRequest();
        
        var success = await _entityService.SyncAsync(value);

        if (!success)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, "Did not sync entities.");
        }
        
        return Ok();
    }
}
