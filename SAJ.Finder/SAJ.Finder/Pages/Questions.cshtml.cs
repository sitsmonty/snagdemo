using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SAJ.Data.DataContext;
using SAJ.Data.EntityModels;
using System.Collections.Generic;
using System.Linq;

namespace SAJ.Finder.Pages
{
    public class QuestionsModel : PageModel
    {
        private readonly DemoContext _context;

        [BindProperty(SupportsGet = true)]
        public List<Question> Questions { get; set; }

        public QuestionsModel(DemoContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            Questions = _context.Questions.OrderBy(x => x.QuestionText).ToList();
        }
    }
}
