using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

[ApiController]
[Route("v1/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var transactions = _transactionService.GetAllTransactions();
        return Ok(transactions);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var transaction = _transactionService.GetTransactionById(id);
        return Ok(transaction);
    }

    [HttpPost]
    public IActionResult Create(TransactionModel model)
    {
        _transactionService.CreateTransaction(model);
        return Ok(new { message = "Transaction created successfully" });
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, TransactionModel model)
    {
        _transactionService.UpdateTransaction(id, model);
        return Ok(new { message = "Transaction updated successfully" });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        _transactionService.DeleteTransaction(id);
        return Ok(new { message = "Transaction deleted successfully" });
    }
}
