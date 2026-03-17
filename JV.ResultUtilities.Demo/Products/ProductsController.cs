using JV.ResultUtilities.Demo.Infrastructure;
using JV.ResultUtilities.Demo.Products.Models;
using JV.ResultUtilities.Demo.Translations;
using JV.ResultUtilities.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JV.ResultUtilities.Demo.Products;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(
    ProductService productService,
    ITranslator translator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await productService.GetAllAsync();
        return result.ToActionResult(translator, products => Ok(products));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await productService.GetByIdAsync(id);

        return result.Match(
            onSuccess: product => (IActionResult)Ok(product),
            onFailure: messages =>
            {
                if (messages.Any(m => m.KeyDefinition?.Key == "product.not_found"))
                    return NotFound();
                return result.ToActionResult(translator);
            });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        return await productService.CreateAsync(request)
            .ToActionResultAsync(translator,
                product => CreatedAtAction(nameof(GetById), new { id = product.Id }, product));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        return await productService.UpdateAsync(id, request)
            .ToActionResultAsync(translator);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await productService.DeactivateAsync(id);

        var (isSuccess, messages) = result;
        if (!isSuccess)
            return result.ToActionResult(translator);

        return NoContent();
    }

    [HttpPost("import")]
    public IActionResult BulkImport([FromBody] List<CreateProductRequest> requests)
    {
        var result = productService.BulkImport(requests);
        return result.ToActionResult(translator);
    }

    [HttpPost("validate-batch")]
    public IActionResult ValidateBatch([FromBody] List<CreateProductRequest> requests)
    {
        var result = productService.ValidateBatch(requests);
        return result.ToActionResult(translator);
    }

    [HttpGet("categories/{category}")]
    public IActionResult GetByCategory(string category)
    {
        var result = productService.GetByCategoryCached(category);
        return result.ToActionResult(translator);
    }

    [HttpGet("cache-stats")]
    public IActionResult GetCacheStats()
    {
        return Ok(productService.GetCacheStats());
    }
}
