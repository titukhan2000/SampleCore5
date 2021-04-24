

namespace CCCount.Models
{
    public class Bookmark
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Owner { get; set; }
        public string CreatedOn { get; set; }
        public string ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsShared { get; set; }
        public string Body { get; set; }

        public Bookmark() { }

        public Bookmark(string Name, string Link)
        {
            this.Name = Name;
            this.Link = Link;
        }
    }
}
