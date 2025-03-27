using EvaluationCrm.Models.entity;
using EvaluationCrm.service;
using Microsoft.AspNetCore.Mvc;

namespace EvaluationCrm.Controllers;

public class DashboardController(HttpClient httpClient) : Controller
{
	
	// private readonly TicketService _ticketService;
	// private readonly LeadService _leadService;
	// private readonly ExpenseService _expenseService;
	// private readonly BudgetService _budgetService;

	public IActionResult Index()
	{
		var viewModel = httpClient.GetFromJsonAsync<Dashboard>("http://localhost:8080/api/dashboard").Result;

		return View(viewModel);
	}
}