using Me.Services.Catalog.API;
using Me.Services.Catalog.API.Data;
using Me.Services.Catalog.API.Model;
using Me.Services.Catalog.API.Controllers;
using Me.Services.Catalog.API.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit.Sdk;
using Microsoft.AspNetCore.Builder;
using Xunit.Abstractions;

namespace UnitTest.Catalog;

public class CatalogControllerTest
{
    private readonly DbContextOptions<CatalogContext> _dbOptions;
    private readonly ITestOutputHelper output;

    public CatalogControllerTest(ITestOutputHelper output)
    {
        this.output = output;
        _dbOptions = new DbContextOptionsBuilder<CatalogContext>()
            .UseInMemoryDatabase("InMemDb")
            .Options;

        using var catalogContext = new CatalogContext(_dbOptions);
        _ = PrepDb(catalogContext);
    }


    private async Task PrepDb(CatalogContext catalogContext)
    {
        await catalogContext.CatalogTypes.AddRangeAsync(CatalogContextSeed.GetPreconfiguredCatalogTypes());
        await catalogContext.CatalogBrands.AddRangeAsync(CatalogContextSeed.GetPreconfiguredCatalogBrands());

        foreach (CatalogItem item in GetFakeCatalogItems())
        {
            item.CatalogType = await catalogContext.CatalogTypes.Where(t => t.Id == item.CatalogTypeId).FirstOrDefaultAsync();
            item.CatalogBrand = await catalogContext.CatalogBrands.Where(b => b.Id == item.CatalogBrandId).FirstOrDefaultAsync();
            await catalogContext.CatalogItems.AddAsync(item);
            await catalogContext.SaveChangesAsync();
        }
    }


    [Fact]
    public async Task Get_catalog_items_success()
    {
        //Arrange
        var typesFilterApplied = 2;
        var brandFilterApplied = 1;
        var pageSize = 4;
        var pageIndex = 1;

        var expectedItemsInPage1 = 2;
        var expectedTotalItems = 6;

        var catalogContext = new CatalogContext(_dbOptions);
        var catalogSettings = new TestCatalogSettings();

        //var integrationServicesMock = new Mock<ICatalogIntegrationEventService>();

        //Act
        var orderController = new CatalogController(catalogContext, catalogSettings);
        //, integrationServicesMock.Object);
        var actionResult = await orderController.ItemsByTypeIdAndBrandIdAsync(
                typesFilterApplied,
                brandFilterApplied,
                pageSize,
                pageIndex);

        var page = Assert.IsAssignableFrom<PaginatedItemsViewModel<CatalogItem>>(actionResult.Value);
        var data = page.Data.ToList();

        //Assert
        Assert.IsType<ActionResult<PaginatedItemsViewModel<CatalogItem>>>(actionResult);
        Assert.Equal(expectedTotalItems, page.Count);
        Assert.Equal(pageIndex, page.PageIndex);
        Assert.Equal(pageSize, page.PageSize);
        Assert.Equal(expectedItemsInPage1, page.Data.Count());
        Assert.NotNull(data[0].CatalogType);
        Assert.NotNull(data[0].CatalogType.Type);
        Assert.NotEmpty(data[0].CatalogType.Type);
        // try
        // {
        //     Assert.Fail($"Catalog item type: {data[0].CatalogType.Type}");           
        // }
        // catch (XunitException e)
        // {
        //     output.WriteLine($"{e.Message}: Unexpected page index");
        //     throw;
        // }
    }


    private static List<CatalogItem> GetFakeCatalogItems()
    {
        return new List<CatalogItem>()
        {
            new()
            {
                //Id = 1,
                Name = "fakeItemA",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemA.png"
            },
            new()
            {
                //Id = 2,
                Name = "fakeItemB",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemB.png"
            },
            new()
            {
                //Id = 3,
                Name = "fakeItemC",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemC.png"
            },
            new()
            {
                //Id = 4,
                Name = "fakeItemD",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemD.png"
            },
            new()
            {
                //Id = 5,
                Name = "fakeItemE",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemE.png"
            },
            new()
            {
                //Id = 6,
                Name = "fakeItemF",
                CatalogTypeId = 2,
                CatalogBrandId = 1,
                PictureFileName = "fakeItemF.png"
            }
        };
    }

}


public class TestCatalogSettings : IOptionsSnapshot<CatalogSettings>
{
    public CatalogSettings Value => new()
    {
        PicBaseUrl = "http://image-server.com/",
        AzureStorageEnabled = true
    };

    public CatalogSettings Get(string name) => Value;
}

