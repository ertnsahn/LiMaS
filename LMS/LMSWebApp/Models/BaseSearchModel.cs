namespace LMSWebApp.Models
{
    public class BaseSearchModel
    {
        public BaseSearchModel()
        {
            RowsPerPage ??= 10;
        }
        public int? Page { get; set; }
        public int? RowsPerPage { get; set; }
        public SortDir SortDir { get; set; }
        public string SearchTerm { get; set; }
        public int? SortByColumn { get; set; }
    }

    public enum SortDir
    {
        ASC,
        DESC
    }
}
