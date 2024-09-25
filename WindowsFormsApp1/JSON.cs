using System.Collections.Generic;

namespace SDDBrowser
{
    public class BookmarkFolderJson
    {
        public List<BookmarkFolderJson> Folders { get; set; }
        public List<BookmarkJson> Bookmarks { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
    }

    public class BookmarkJson
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int Position { get; set; }
    }


}
