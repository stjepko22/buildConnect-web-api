using BuildConnect.Model;
using BuildConnect.Service.Common;
using Microsoft.AspNetCore.Mvc;

namespace BuildConnect.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ContractorsController : ControllerBase
{
    #region Fields

    private readonly IUserService _userService;

    #endregion Fields

    #region Constructors

    public ContractorsController(IUserService userService)
    {
        _userService = userService;
    }

    #endregion Constructors

    #region Methods

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<UserProfileResponse>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<UserProfileResponse>> Get(
        [FromQuery(Name = "q")] string? query,
        [FromQuery(Name = "category")] string[]? categories,
        [FromQuery] string? location,
        [FromQuery] string? legalType)
    {
        return Ok(_userService.GetContractors(new GetContractorsRequest(query, categories, location, legalType)));
    }

    #endregion Methods
}