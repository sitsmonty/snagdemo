using System.Collections.Generic;

namespace SAJ.Finder.Models
{
    public class InputApplication
    {
        public string Name { get; set; }
        public List<InputQuestion> Questions { get; set; }

        public InputApplication()
        {
            Questions = new List<InputQuestion>();
        }
    }
}
