using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SAJ.Data.DataContext;
using SAJ.Finder.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SAJ.Finder.Pages
{
    public class EmployerModel : PageModel
    {
        private readonly DemoContext _context;

        public string ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<InputApplication> Applications { get; set; }

        public EmployerModel(DemoContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            var acceptedApplications = _context.Applications.Where(x => x.Accepted).ToList();
            if (acceptedApplications.Count <= 0)
            {
                ErrorMessage = "No applications were accepted.";
                return;
            }

            Applications = acceptedApplications.Select(x => JsonSerializer.Deserialize<InputApplication>(x.Document)).ToList();
        }
    }
}
