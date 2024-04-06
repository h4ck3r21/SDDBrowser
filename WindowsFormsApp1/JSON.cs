using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDDBrowser
{
    public class BookmarkFolderJson 
    {
        public List<BookmarkFolderJson> folders { get; set; }
        public List<BookmarkJson> bookmarks { get; set; }
        public string name { get; set; }
        public int position { get; set; }
    }

    public class BookmarkJson
    {
        public string name { get; set; }
        public string url { get; set; }
        public int position { get; set; }
    }


}
