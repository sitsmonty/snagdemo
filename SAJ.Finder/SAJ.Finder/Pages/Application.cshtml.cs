using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SAJ.Data.DataContext;
using SAJ.Data.EntityModels;
using SAJ.Finder.Models;
using System;
using System.IO;
using System.Json;
using System.Text;
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

        [BindProperty(SupportsGet = true)]
        public IFormFile DocumentUpload { get; set; }

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
            var json = ValidateAndReadJson();

            try
            {
                var document = JsonSerializer.Deserialize<InputApplication>(json);
                if (document == null || string.IsNullOrWhiteSpace(document.Name) || document.Questions.Count <= 0)
                {
                    ErrorMessage = "Document does not conform to expected format!";
                    return;
                }

                var newApplication = new Application { Document = json, Accepted = true };
                foreach (var question in document.Questions)
                {
                    // find question and accepted answer from data store
                    var found = _context.Questions.Find(question.Id);
                    if (found == null)
                    {
                        ErrorMessage = "One or more unexpected questions encountered!";
                        return;
                    }

                    // if question is found and the answer does not match, reject the application and break the loop
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

        private string ValidateAndReadJson()
        {
            var json = string.Empty;

            if (string.IsNullOrWhiteSpace(DocumentInJson) && DocumentUpload == null)
            {
                ErrorMessage = "Document cannot be empty!";
                return json;
            }

            if (!string.IsNullOrWhiteSpace(DocumentInJson))
            {
                json = DocumentInJson;
            }
            else
            {
                var text = new StringBuilder();
                using (var reader = new StreamReader(DocumentUpload.OpenReadStream()))
                {
                    while (reader.Peek() >= 0)
                        text.AppendLine(reader.ReadLine());
                }

                json = text.ToString();
            }
            

            if (!IsValidJson(json))
            {
                ErrorMessage = "Document text is not valid JSON!";
                return string.Empty;
            }

            return json;
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
