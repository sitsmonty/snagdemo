using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SAJ.Data.DataContext;
using SAJ.Data.EntityModels;
using SAJ.Finder.Models;
using System;
using System.Json;
using System.Text.Json;

namespace SAJ.Finder.Pages
{
    public class ApplicationModel : PageModel
    {
        private readonly DemoContext _context;

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string DocumentInJson { get; set; }

        public ApplicationModel(DemoContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            DocumentInJson = string.Empty;
        }

        public void OnPostSubmit()
        {
            if (string.IsNullOrWhiteSpace(DocumentInJson))
            {
                ErrorMessage = "Document cannot be empty!";
                return;
            }

            if (!IsValidJson(DocumentInJson))
            {
                ErrorMessage = "Document text is not valid JSON!";
                return;
            }

            try
            {
                var document = JsonSerializer.Deserialize<InputApplication>(DocumentInJson);
                if (document == null || string.IsNullOrWhiteSpace(document.Name) || document.Questions.Count <= 0)
                {
                    ErrorMessage = "Document does not conform to expected format!";
                    return;
                }

                var newApplication = new Application { Document = DocumentInJson, Accepted = true };
                foreach (var question in document.Questions)
                {
                    var found = _context.Questions.Find(question.Id);
                    if (found == null)
                    {
                        ErrorMessage = "One or more unexpected questions encountered!";
                        return;
                    }

                    if (!question.Answer.Trim().Equals(found.AcceptedAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        newApplication.Accepted = false;
                        break;
                    }
                }

                _context.Applications.Add(newApplication);
                _context.SaveChanges();

                ResetPage();
                SuccessMessage = "Application submitted successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return;
            }
        }

        public void OnPostClear()
        {
            ResetPage();
        }

        private bool IsValidJson(string text)
        {
            try
            {
                var temp = JsonValue.Parse(text);
                return true;
            }
            catch (Exception)
            {
                //Invalid json format
                return false;
            }
        }

        private void ResetPage()
        {
            ModelState.Remove("DocumentInJson");
            DocumentInJson = string.Empty;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }
    }
}
