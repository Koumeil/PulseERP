using Microsoft.AspNetCore.Mvc;
using Moq;
using PulseERP.Abstractions.Common.DTOs.Users.Commands;
using PulseERP.Abstractions.Common.DTOs.Users.Models;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.API.Contracts;
using PulseERP.API.Controllers;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Errors;

namespace PulseERP.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockService = new Mock<IUserService>();
        _controller = new UsersController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithPagedUsers()
    {
        var filter = new UserFilter("foo", null, null, null, 1, 5);
        var items = new List<UserSummary>
        {
            new UserSummary(Guid.NewGuid(), "first", "last", "email", "phone", "role", true, false, false, DateTime.Now,
                2, DateTime.Now)
        };

        var paged = new PagedResult<UserSummary>
        {
            Items = items,
            PageNumber = 1,
            PageSize = 5,
            TotalItems = 1
        };

        _mockService.Setup(s => s.GetAllAsync(filter)).ReturnsAsync(paged);

        var action = await _controller.GetAll(filter);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<PagedResult<UserSummary>>>(ok.Value);
        Assert.True(resp.Success);
        Assert.Equal("Users retrieved successfully", resp.Message);
        Assert.Single(resp.Data!.Items);

        _mockService.Verify(s => s.GetAllAsync(filter), Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WithUserDetails()
    {
        var id = Guid.NewGuid();

        var details = new UserDetails(id, "first", "last", "email", "phone", "role", true, false, DateTime.Now,
            DateTime.Now, 2);

        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(details);

        var action = await _controller.GetById(id);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<UserDetails>>(ok.Value);
        Assert.Equal(details.Email, resp.Data!.Email);
        Assert.Equal("User retrieved successfully", resp.Message);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ThrowsAsync(new NotFoundException("User", id));

        await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetById(id));
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithUserInfo()
    {
        var cmd = new CreateUserCommand("first", "last", "email", "phone", "role");
        var info = new UserInfo(Guid.NewGuid(), cmd.FirstName, cmd.LastName, cmd.Email, "User", "+32489200305");

        _mockService.Setup(s => s.CreateAsync(cmd)).ReturnsAsync(info);

        var action = await _controller.Create(cmd);

        var created = Assert.IsType<CreatedAtActionResult>(action.Result);
        Assert.Equal(nameof(_controller.GetById), created.ActionName);
        var resp = Assert.IsType<ApiResponse<UserInfo>>(created.Value);
        Assert.Equal(info.Id, resp.Data!.Id);
        Assert.Equal("User created successfully", resp.Message);
    }

    [Fact]
    public async Task Create_WhenValidationFails_ThrowsDomainValidationException()
    {
        var cmd = new CreateUserCommand(null!, null!, null!, null!, null!);

        _mockService.Setup(s => s.CreateAsync(cmd)).ThrowsAsync(new DomainValidationException("Invalid data"));

        await Assert.ThrowsAsync<DomainValidationException>(() => _controller.Create(cmd));
    }

    [Fact]
    public async Task Update_ReturnsOk_WithUpdatedDetails()
    {
        var id = Guid.NewGuid();
        var cmd = new UpdateUserCommand { FirstName = "New", Role = "Admin" };
        var details = new UserDetails(id, cmd.FirstName, "last", "email", "phone", cmd.Role, true, false, DateTime.Now,
            DateTime.Now, 2);

        _mockService.Setup(s => s.UpdateAsync(id, cmd)).ReturnsAsync(details);

        var action = await _controller.Update(id, cmd);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<UserDetails>>(ok.Value);
        Assert.Equal("User updated successfully", resp.Message);
        Assert.Equal("New", resp.Data!.FirstName);
        Assert.Equal("Admin", resp.Data!.Role);
    }

    [Fact]
    public async Task Update_WhenNotFound_ThrowsNotFound()
    {
        var id = Guid.NewGuid();

        _mockService.Setup(s => s.UpdateAsync(id, It.IsAny<UpdateUserCommand>()))
            .ThrowsAsync(new NotFoundException("User", id));

        await Assert.ThrowsAsync<NotFoundException>(() => _controller.Update(id, new UpdateUserCommand()));
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var id = Guid.NewGuid();

        _mockService.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ThrowsNotFound()
    {
        var id = Guid.NewGuid();

        _mockService.Setup(s => s.DeleteAsync(id)).ThrowsAsync(new NotFoundException("User", id));

        await Assert.ThrowsAsync<NotFoundException>(() => _controller.Delete(id));

        _mockService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    [Theory]
    [InlineData("activate", "User activated successfully")]
    [InlineData("deactivate", "User deactivated successfully")]
    [InlineData("restore", "User restored successfully")]
    public async Task PatchActions_ReturnsOk_WithExpectedMessage(string action, string expectedMessage)
    {
        var id = Guid.NewGuid();

        SetupPatchAction(action, id, null);

        var actionResult = await InvokePatch(action, id);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var resp = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.Equal(expectedMessage, resp.Message);

        VerifyPatchCalled(action, id);
    }

    [Theory]
    [InlineData("activate")]
    [InlineData("deactivate")]
    [InlineData("restore")]
    public async Task PatchActions_WhenNotFound_ThrowsNotFound(string action)
    {
        var id = Guid.NewGuid();

        SetupPatchAction(action, id, new NotFoundException("User", id));

        await Assert.ThrowsAsync<NotFoundException>(() => InvokePatch(action, id));

        VerifyPatchCalled(action, id);
    }

    [Theory]
    [InlineData("activate")]
    [InlineData("deactivate")]
    public async Task PatchActions_WhenInvalidOperation_ThrowsInvalidOperation(string action)
    {
        var id = Guid.NewGuid();

        SetupPatchAction(action, id, new InvalidOperationException("Invalid op"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => InvokePatch(action, id));

        VerifyPatchCalled(action, id);
    }

    [Fact]
    public async Task Activate_WhenUnhandledException_Throws()
    {
        var id = Guid.NewGuid();

        _mockService.Setup(s => s.ActivateUserAsync(id)).ThrowsAsync(new Exception("Unexpected error"));

        await Assert.ThrowsAsync<Exception>(() => _controller.Activate(id));
    }

    // === Private helpers ===

    private void SetupPatchAction(string action, Guid id, Exception? exception)
    {
        switch (action)
        {
            case "activate":
                if (exception == null)
                    _mockService.Setup(s => s.ActivateUserAsync(id)).Returns(Task.CompletedTask);
                else
                    _mockService.Setup(s => s.ActivateUserAsync(id)).ThrowsAsync(exception);
                break;

            case "deactivate":
                if (exception == null)
                    _mockService.Setup(s => s.DeactivateUserAsync(id)).Returns(Task.CompletedTask);
                else
                    _mockService.Setup(s => s.DeactivateUserAsync(id)).ThrowsAsync(exception);
                break;

            case "restore":
                if (exception == null)
                    _mockService.Setup(s => s.RestoreUserAsync(id)).Returns(Task.CompletedTask);
                else
                    _mockService.Setup(s => s.RestoreUserAsync(id)).ThrowsAsync(exception);
                break;
        }
    }

    private Task<ActionResult<ApiResponse<object>>> InvokePatch(string action, Guid id)
    {
        return action switch
        {
            "activate" => _controller.Activate(id),
            "deactivate" => _controller.Deactivate(id),
            "restore" => _controller.Restore(id),
            _ => throw new ArgumentException("Invalid patch action", nameof(action))
        };
    }

    private void VerifyPatchCalled(string action, Guid id)
    {
        switch (action)
        {
            case "activate":
                _mockService.Verify(s => s.ActivateUserAsync(id), Times.Once);
                break;
            case "deactivate":
                _mockService.Verify(s => s.DeactivateUserAsync(id), Times.Once);
                break;
            case "restore":
                _mockService.Verify(s => s.RestoreUserAsync(id), Times.Once);
                break;
        }
    }
}