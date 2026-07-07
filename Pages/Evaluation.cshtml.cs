using FineArts.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace FineArts.Web.Pages;

public class EvaluationModel : PageModel
{
    private readonly EvaluatorRepository _repo = new();
    private readonly VolunteerRepository _volRepo = new();

    public List<EvaluatorRecord> Evaluators { get; set; } = new();

    public void OnGet()
    {
        Evaluators = _repo.GetEvaluators();
    }

    // AJAX: Get divisions for an evaluator
    public IActionResult OnGetDivisions(int evaluatorId)
    {
        var divisions = _repo.GetDivisionsForEvaluator(evaluatorId);
        return new JsonResult(divisions.Select(d => new { value = d.DivisionID, text = d.DivisionName }));
    }

    // AJAX: Get categories for an evaluator in a division
    public IActionResult OnGetCategories(int evaluatorId, int divisionId)
    {
        var categories = _repo.GetCategoriesForEvaluatorInDivision(evaluatorId, divisionId);
        return new JsonResult(categories.Select(c => new { value = c.CategoryID, text = c.CategoryName }));
    }

    // AJAX: Get unevaluated events for a category
    public IActionResult OnGetEvents(int categoryId, int evaluatorId)
    {
        var events = _repo.GetEventsForCategory(categoryId, evaluatorId);
        return new JsonResult(events.Select(e => new { value = e.EventID, text = e.DisplayName }));
    }

    // AJAX: Get all churches (for unlisted participant)
    public IActionResult OnGetChurches()
    {
        var churches = _repo.GetChurches();
        return new JsonResult(churches.Select(c => new { value = c.ChurchID, text = c.ChurchName }));
    }

    // AJAX: Get all divisions (for Add Evaluator modal)
    public IActionResult OnGetAllDivisions()
    {
        var divisions = _volRepo.GetAllDivisions();
        return new JsonResult(divisions.Select(d => new { value = d.DivisionID, text = d.DivisionName }));
    }

    // AJAX: Get categories for a division (for Add Evaluator modal)
    public IActionResult OnGetCategoriesForDivision(int divisionId)
    {
        var categories = _volRepo.GetCategoriesForDivisionID(divisionId);
        return new JsonResult(categories.Select(c => new { value = c.CategoryID, text = c.CategoryName }));
    }

    // POST: Submit an evaluation
    public IActionResult OnPostSubmit([FromBody] SubmitEvaluationRequest req)
    {
        try
        {
            _repo.SaveEvaluation(
                req.EvaluatorId,
                req.EventId,
                req.SelectionScore,
                req.CommunicationScore,
                req.PresentationScore,
                req.MinistryScore,
                req.TotalScore,
                req.IsNoShow,
                req.IsEvaluatorsChoice,
                req.Comments,
                req.ViolationDescription
            );
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }

    // POST: Add an unlisted participant and return their EventID
    public IActionResult OnPostAddParticipant([FromBody] AddParticipantRequest req)
    {
        try
        {
            int? existingParticipantId = _repo.FindParticipant(req.FirstName, req.LastName, req.ChurchId);
            if (existingParticipantId.HasValue)
            {
                return new JsonResult(new { success = false, duplicate = true, error = "This participant is already registered." });
            }

            var result = _repo.CreateParticipantAndEvent(req.FirstName, req.LastName, req.ChurchId, req.CategoryId);
            string displayName = $"{req.FirstName} {req.LastName}";
            return new JsonResult(new { success = true, eventId = result.eventID, displayName });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }

    // POST: Add a new evaluator, or assign a category to an existing evaluator matched by email
    public IActionResult OnPostAddEvaluator([FromBody] AddEvaluatorRequest req)
    {
        try
        {
            var result = _volRepo.InsertEvaluator(
                req.FirstName,
                req.LastName,
                "Evaluator",
                req.Email,
                req.CellPhone,
                false,
                req.CategoryIds
            );
            string fullName = $"{result.FirstName} {result.LastName}";
            return new JsonResult(new
            {
                success = true,
                evaluatorId = result.EvaluatorID,
                fullName,
                wasExisting = result.WasExisting
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }
}

public class SubmitEvaluationRequest
{
    public int EvaluatorId { get; set; }
    public int EventId { get; set; }
    public int SelectionScore { get; set; }
    public int CommunicationScore { get; set; }
    public int PresentationScore { get; set; }
    public int MinistryScore { get; set; }
    public int TotalScore { get; set; }
    public bool IsNoShow { get; set; }
    public bool IsEvaluatorsChoice { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string ViolationDescription { get; set; } = string.Empty;
}

public class AddParticipantRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int ChurchId { get; set; }
    public int CategoryId { get; set; }
}

public class AddEvaluatorRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CellPhone { get; set; } = string.Empty;
    public List<int> CategoryIds { get; set; } = new();
}
