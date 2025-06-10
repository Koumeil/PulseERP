using Microsoft.AspNetCore.Mvc;
using Moq;
using PulseERP.Abstractions.Common.DTOs.Inventories.Models;
using PulseERP.Abstractions.Common.DTOs.Products.Commands;
using PulseERP.Abstractions.Common.DTOs.Products.Models;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.API.Contracts;
using PulseERP.API.Controllers;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Errors;

namespace PulseERP.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _mockService;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockService = new Mock<IProductService>();
        _controller = new ProductsController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithPagedProducts()
    {
        var filter = new ProductFilter();
        var paged = new PagedResult<ProductSummary>
        {
            Items = new List<ProductSummary> { new ProductSummary { Name = "Product 1", Price = 10 } },
            TotalItems = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _mockService.Setup(s => s.GetAllProductsAsync(filter)).ReturnsAsync(paged);

        var action = await _controller.GetAll(filter);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<PagedResult<ProductSummary>>>(ok.Value);
        Assert.Equal("Products retrieved successfully", resp.Message);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WithProductDetails()
    {
        var id = Guid.NewGuid();
        var details = new ProductDetails { Id = id, Name = "Product X", Price = 99 };

        _mockService.Setup(s => s.GetProductByIdAsync(id)).ReturnsAsync(details);

        var action = await _controller.GetById(id);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<ProductDetails>>(ok.Value);
        Assert.Equal("Product retrieved successfully", resp.Message);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ThrowsNotFound()
    {
        var id = Guid.NewGuid();

        _mockService.Setup(s => s.GetProductByIdAsync(id)).ThrowsAsync(new NotFoundException("Product", id));

        await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetById(id));
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithProductDetails()
    {
        var cmd = new CreateProductCommand("Product X", "Description", "Brand", 10, 5, false);
        var details = new ProductDetails { Id = Guid.NewGuid(), Name = cmd.Name, Price = cmd.Price };

        _mockService.Setup(s => s.CreateProductAsync(cmd)).ReturnsAsync(details);

        var action = await _controller.Create(cmd);

        var created = Assert.IsType<CreatedAtActionResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<ProductDetails>>(created.Value);
        Assert.Equal("Product created successfully", resp.Message);
    }

    [Fact]
    public async Task Update_ReturnsOk_WithProductDetails()
    {
        var id = Guid.NewGuid();
        var cmd = new UpdateProductCommand { Name = "Updated", Price = 50 };
        var details = new ProductDetails { Id = id, Name = cmd.Name, Price = cmd.Price.Value };

        _mockService.Setup(s => s.UpdateProductAsync(id, cmd)).ReturnsAsync(details);

        var action = await _controller.Update(id, cmd);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<ProductDetails>>(ok.Value);
        Assert.Equal("Product updated successfully", resp.Message);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var id = Guid.NewGuid();

        _mockService.Setup(s => s.DeleteProductAsync(id)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
    }

    [Theory]
    [InlineData("activate", "Product activated successfully")]
    [InlineData("deactivate", "Product deactivated successfully")]
    [InlineData("restore", "Product restored successfully")]
    public async Task PatchActions_ReturnsOk_WithExpectedMessage(string action, string expectedMessage)
    {
        var id = Guid.NewGuid();
        SetupPatchAction(action, id, null);

        var route = InvokePatchAction(action, id);

        var actionResult = await route;
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var resp = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.Equal(expectedMessage, resp.Message);
    }

    [Fact]
    public async Task Restock_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var cmd = new RestockProductCommand { Quantity = 10 };

        _mockService.Setup(s => s.RestockProductAsync(id, cmd.Quantity)).Returns(Task.CompletedTask);

        var action = await _controller.Restock(id, cmd);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.Equal("Product restocked successfully", resp.Message);
    }

    [Fact]
    public async Task Sell_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var cmd = new SellProductCommand { Quantity = 5 };

        _mockService.Setup(s => s.SellProductAsync(id, cmd.Quantity)).Returns(Task.CompletedTask);

        var action = await _controller.Sell(id, cmd);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.Equal("Product sold successfully", resp.Message);
    }

    [Fact]
    public async Task ChangePrice_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var cmd = new ChangeProductPriceCommand { NewPrice = 25 };
        var details = new ProductDetails { Id = id, Price = cmd.NewPrice };

        _mockService.Setup(s => s.ChangeProductPriceAsync(id, cmd.NewPrice)).ReturnsAsync(details);

        var action = await _controller.ChangePrice(id, cmd);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<ProductDetails>>(ok.Value);
        Assert.Equal("Product price changed successfully", resp.Message);
    }

    [Fact]
    public async Task ApplyDiscount_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var cmd = new ApplyDiscountCommand { Percentage = 10 };
        var details = new ProductDetails { Id = id, Price = 90 };

        _mockService.Setup(s => s.ApplyDiscountAsync(id, cmd.Percentage)).ReturnsAsync(details);

        var action = await _controller.ApplyDiscount(id, cmd);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<ProductDetails>>(ok.Value);
        Assert.Equal("Discount applied successfully", resp.Message);
    }

    [Fact]
    public async Task IsLowStock_ReturnsOk()
    {
        var id = Guid.NewGuid();

        _mockService.Setup(s => s.IsProductLowStockAsync(id, 5)).ReturnsAsync(true);

        var action = await _controller.IsLowStock(id);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<bool>>(ok.Value);
        Assert.True(resp.Data);
    }

    [Fact]
    public async Task NeedsRestock_ReturnsOk()
    {
        var id = Guid.NewGuid();

        _mockService.Setup(s => s.NeedsRestockingAsync(id, 5)).ReturnsAsync(false);

        var action = await _controller.NeedsRestock(id, 5);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<bool>>(ok.Value);
        Assert.False(resp.Data);
    }

    [Fact]
    public async Task LowStockList_ReturnsOk()
    {
        _mockService.Setup(s => s.GetProductsBelowThresholdAsync(5)).ReturnsAsync(new List<ProductSummary>());

        var action = await _controller.GetLowStockList();

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<IReadOnlyCollection<ProductSummary>>>(ok.Value);
        Assert.Equal("Low stock products retrieved successfully", resp.Message);
    }

    [Fact]
    public async Task ArchiveStaleProducts_ReturnsOk()
    {
        var cmd = new ArchiveStaleProductsCommand { InactivityDays = 30, OutOfStockDays = 15 };

        _mockService.Setup(s => s.ArchiveStaleProductsAsync(TimeSpan.FromDays(cmd.InactivityDays), cmd.OutOfStockDays))
            .Returns(Task.CompletedTask);

        var action = await _controller.ArchiveStaleProducts(cmd);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.Equal("Stale products archived successfully", resp.Message);
    }

    [Fact]
    public async Task GetByBrand_ReturnsOk()
    {
        var brandName = "BrandX";
        var filter = new ProductFilter();

        _mockService.Setup(s => s.GetProductsByBrandAsync(brandName, filter)).ReturnsAsync(
            new PagedResult<ProductSummary>
            {
                Items = new List<ProductSummary>(),
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 0
            });
        var action = await _controller.GetByBrand(brandName, filter);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var resp = Assert.IsType<ApiResponse<PagedResult<ProductSummary>>>(ok.Value);
        Assert.Equal("Products by brand retrieved successfully", resp.Message);
    }

    private void SetupPatchAction(string action, Guid id, Exception? exception)
    {
        switch (action)
        {
            case "activate":
                if (exception == null)
                    _mockService.Setup(s => s.ActivateProductAsync(id)).Returns(Task.CompletedTask);
                else
                    _mockService.Setup(s => s.ActivateProductAsync(id)).ThrowsAsync(exception);
                break;
            case "deactivate":
                if (exception == null)
                    _mockService.Setup(s => s.DeactivateProductAsync(id)).Returns(Task.CompletedTask);
                else
                    _mockService.Setup(s => s.DeactivateProductAsync(id)).ThrowsAsync(exception);
                break;
            case "restore":
                if (exception == null)
                    _mockService.Setup(s => s.RestoreProductAsync(id)).Returns(Task.CompletedTask);
                else
                    _mockService.Setup(s => s.RestoreProductAsync(id)).ThrowsAsync(exception);
                break;
        }
    }

    private Task<ActionResult<ApiResponse<object>>> InvokePatchAction(string action, Guid id)
    {
        return action switch
        {
            "activate" => _controller.Activate(id),
            "deactivate" => _controller.Deactivate(id),
            "restore" => _controller.Restore(id),
            _ => throw new ArgumentException("Invalid patch action", nameof(action))
        };
    }
}